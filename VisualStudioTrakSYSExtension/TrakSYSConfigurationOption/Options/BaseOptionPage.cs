using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrakSYSConfigurationOption.Options
{
    internal class BaseOptionPage<T> : DialogPage where T : BaseOptionModel<T>, new()
    {
        private BaseOptionModel<T> _model;

        public BaseOptionPage()
        {
            _model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<T>.CreateAsync);
        }

        public override object AutomationObject => _model;

        public override void LoadSettingsFromStorage()
        {
            _model.Load();
        }

        public override void SaveSettingsToStorage()
        {
            _model.Save();
        }
    }
}
