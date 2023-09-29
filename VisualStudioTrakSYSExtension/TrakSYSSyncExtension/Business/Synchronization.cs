using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrakSYSSyncExtension.Options;

namespace TrakSYSSyncExtension.Business
{
    internal class Synchronization
    {
        public Project Project { get; set; }

        public Synchronization(Project project)
        {
            Project = project;
        }

        public List<TrakSYSSynchronization.Synchronization.ResultItem> SyncClassesToVisualStudio(int scriptType)
        {
            TrakSYSSynchronization.Synchronization sync = new TrakSYSSynchronization.Synchronization();
            return sync.GetScripts(TrakSYSSyncOptions.Instance.ConnectionString, scriptType, TrakSYSSyncOptions.Instance.LastSyncFromVisualStudio);
        }

        private void SyncGlobalScripts()
        {
            //TrakSYSSynchronization.Synchronization sync = new TrakSYSSynchronization.Synchronization();
            //var results = sync.GetScripts(TrakSYSSyncOptions.Instance.ConnectionString, 0);
            
            //foreach ( var result in results )
            //{
            //    AddFile(result);
            //}
        }

        //private void AddFile(TrakSYSSynchronization.Synchronization.ResultItem resultItem)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    if (Project != null)
        //    {
        //        // Create a code file and add it to the project
        //        string className = "MyClass"; // Replace with the desired class name
        //        string fileName = path + @"\" + className + ".cs";

        //        Project.ProjectItems.AddFolder(path);

        //        // Create a new code file item
        //        ProjectItem newCodeFile = Project.ProjectItems.AddFromFile(fileName);

        //        // Set the content of the code file
        //        if (newCodeFile != null)
        //        {
        //            // Get the path to the newly created code file
        //            string filePath = Path.Combine(newCodeFile.FileNames[1]);

        //            // Write the class code to the file
        //            File.WriteAllText(filePath, classCode);
        //        }
        //    }
        //}

        private void SyncWebScripts()
        {

        }
    }
}
