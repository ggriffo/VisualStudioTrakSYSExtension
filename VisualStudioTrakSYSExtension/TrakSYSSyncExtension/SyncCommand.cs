using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrakSYSSyncExtension.Business;
using TrakSYSSyncExtension.Helper;
using TrakSYSSyncExtension.Options;
using Task = System.Threading.Tasks.Task;

namespace TrakSYSSyncExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SyncCommand
    {
        private const string _solutionItemsProjectName = "Solution Items";
        private static readonly Regex _reservedFileNamePattern = new Regex($@"(?i)^(PRN|AUX|NUL|CON|COM\d|LPT\d)(\.|$)");
        private static readonly HashSet<char> _invalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        public static System.IServiceProvider serviceProvider;
        
        public static DTE2 _dte;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c03828fb-9b6a-4ea9-92c0-938da2258e98");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SyncCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SyncCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SyncCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SyncCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;

            _dte = (DTE2)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

            SyncFromVisualStudio();

            TrakSYSSyncOptions.Instance.LastSyncFromTrakSYS = date;
            TrakSYSSyncOptions.Instance.Save();
        }

        private void SyncFromVisualStudio()
        {
            SyncFromVisualStudio(0, "Global");
            SyncFromVisualStudio(2, "LogicScripts");
            SyncFromVisualStudio(3, "WebScripts");
        }

        private void SyncFromVisualStudio(int scriptType, string scriptMajorGroup)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = _dte.SelectedItems.Item(1).Project;

            Business.Synchronization sync = new Business.Synchronization(project);
            var result = sync.SyncClassesToVisualStudio(scriptType);
            foreach (var resultItem in result)
            {
                ProcessResult(resultItem, scriptMajorGroup);
            }
        }

        private void ProcessResult(TrakSYSSynchronization.Synchronization.ResultItem resultItem, string trakSYSScriptType)
        {
            string[] parsedInputs = GetParsedInput(resultItem.ScriptName + ".cs");

            NewItemTarget target = NewItemTarget.Create(_dte, trakSYSScriptType, resultItem.GroupName);

            foreach (string name in parsedInputs)
            {
                try
                {
                    AddItem(name, resultItem.Script, target, trakSYSScriptType);
                }
                catch (Exception ex) when (!ErrorHandler.IsCriticalException(ex))
                {
                    Logger.Log(ex);
                    MessageBox.Show(
                            $"Error creating file '{name}':{Environment.NewLine}{ex.Message}",
                            "TrakSYS Sync Extension",
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                }
            }
        }

        private void AddItem(string name, string script, NewItemTarget target, string folder)
        {
            // The naming rules that apply to files created on disk also apply to virtual solution folders,
            // so regardless of what type of item we are creating, we need to validate the name.
            ValidatePath(name);

            if (name.EndsWith("\\", StringComparison.Ordinal))
            {
                AddProjectFolder(name, target);
            }
            else
            {
                AddFile(name, script, target);
            }
        }

        private void AddFile(string name, string script, NewItemTarget target)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            FileInfo file;

            // If the file is being added to a solution folder, but that
            // solution folder doesn't have a corresponding directory on
            // disk, then write the file to the root of the solution instead.
            if (target.IsSolutionFolder && !Directory.Exists(target.Directory))
            {
                file = new FileInfo(Path.Combine(Path.GetDirectoryName(_dte.Solution.FullName), Path.GetFileName(name)));
            }
            else if (name.StartsWith("sln\\"))
            {
                file = new FileInfo(Path.Combine(Path.GetDirectoryName(_dte.Solution.FullName), Path.GetFileName(name.Substring(4))));
            }
            else if (name.StartsWith("prj\\") && target.Project != null)
            {
                file = new FileInfo(Path.Combine(Path.GetDirectoryName(target.Project.FileName), Path.GetFileName(name.Substring(4))));
            }
            else
            {
                file = new FileInfo(Path.Combine(target.Directory, name));
            }

            // Make sure the directory exists before we create the file. Don't use
            // `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            Directory.CreateDirectory(file.DirectoryName);

            if (!file.Exists)
            {
                Project project;

                //if (target.IsSolutionOrSolutionFolder)
                //{
                    //project = GetOrAddSolutionFolder(Path.GetDirectoryName(name), target);
                //}
                //else
                //{
                    project = target.Project;
                //}

                int position = WriteFile(project, file.FullName, script);
                if (target.ProjectItem != null && target.ProjectItem.IsKind(EnvDTE.Constants.vsProjectItemKindVirtualFolder))
                {
                    target.ProjectItem.ProjectItems.AddFromFile(file.FullName);
                }
                else
                {
                    project.AddFileToProject(file);
                }

                //VsShellUtilities.OpenDocument(serviceProvider, file.FullName);

                // Move cursor into position.
                if (position > 0)
                {
                    Microsoft.VisualStudio.Text.Editor.IWpfTextView view = ProjectHelpers.GetCurentTextView();

                    if (view != null)
                    {
                        view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, position));
                    }
                }

                //ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument");
                //_dte.ActiveDocument.Activate();
            }
            else
            {
                MessageBox.Show($"The file '{file}' already exists.", "TrakSYS File Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExecuteCommandIfAvailable(string commandName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			Command command;

			try
			{
				command = _dte.Commands.Item(commandName);
			}
			catch (ArgumentException)
			{
				// The command does not exist, so we can't execute it.
				return;
			}

			if (command.IsAvailable)
			{
				_dte.ExecuteCommand(commandName);
			}
		}

        private static int WriteFile(Project project, string file, string script)
        {
            WriteToDisk(file, script);

            return 0;
        }

        private static void WriteToDisk(string file, string content)
        {
            using (StreamWriter writer = new StreamWriter(file, false, GetFileEncoding(file)))
            {
                writer.Write(content);
            }
        }

        private static Encoding GetFileEncoding(string file)
        {
            string[] noBom = { ".cmd", ".bat", ".json" };
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (noBom.Contains(ext))
            {
                return new UTF8Encoding(false);
            }

            return new UTF8Encoding(true);
        }

        private void AddProjectFolder(string name, NewItemTarget target)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Make sure the directory exists before we add it to the project. Don't
            // use `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            string targetFolder = Path.Combine(target.Directory, name);
            Directory.CreateDirectory(targetFolder);
            ProjectHelpers.AddFolders(target.Project, targetFolder);
        }

        private void ValidatePath(string path)
        {
            do
            {
                string name = Path.GetFileName(path);

                if (_reservedFileNamePattern.IsMatch(name))
                {
                    throw new InvalidOperationException($"The name '{name}' is a system reserved name.");
                }

                if (name.Any(c => _invalidFileNameChars.Contains(c)))
                {
                    throw new InvalidOperationException($"The name '{name}' contains invalid characters.");
                }

                path = Path.GetDirectoryName(path);
            } while (!string.IsNullOrEmpty(path));
        }

        private static string[] GetParsedInput(string input)
        {
            // var tests = new string[] { "file1.txt", "file1.txt, file2.txt", ".ignore", ".ignore.(old,new)", "license", "folder/",
            //    "folder\\", "folder\\file.txt", "folder/.thing", "page.aspx.cs", "widget-1.(html,js)", "pages\\home.(aspx, aspx.cs)",
            //    "home.(html,js), about.(html,js,css)", "backup.2016.(old, new)", "file.(txt,txt,,)", "file_@#d+|%.3-2...3^&.txt" };
            Regex pattern = new Regex(@"[,]?([^(,]*)([\.\/\\]?)[(]?((?<=[^(])[^,]*|[^)]+)[)]?");
            List<string> results = new List<string>();
            Match match = pattern.Match(input);

            while (match.Success)
            {
                // Always 4 matches w. Group[3] being the extension, extension list, folder terminator ("/" or "\"), or empty string
                string path = match.Groups[1].Value.Trim() + match.Groups[2].Value;
                string[] extensions = match.Groups[3].Value.Split(',');

                foreach (string ext in extensions)
                {
                    string value = path + ext.Trim();

                    // ensure "file.(txt,,txt)" or "file.txt,,file.txt,File.TXT" returns as just ["file.txt"]
                    if (value != "" && !value.EndsWith(".", StringComparison.Ordinal) && !results.Contains(value, StringComparer.OrdinalIgnoreCase))
                    {
                        results.Add(value);
                    }
                }
                match = match.NextMatch();
            }
            return results.ToArray();
        }

    }
}
