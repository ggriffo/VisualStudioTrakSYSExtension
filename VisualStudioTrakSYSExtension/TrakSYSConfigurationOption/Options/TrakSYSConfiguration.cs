using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrakSYSConfigurationOption.Options
{
    internal class TrakSYSConfiguration : BaseOptionModel<TrakSYSConfiguration>
    {
        [Category("Database Configuration")]
        [DisplayName("Connection String")]
        [Description("Specifies the connection string to the TrakSYS Database. TO BE USED ON DEV environment only")]
        [DefaultValue("My message")]
        public string ConnectionString { get; set; } = "";

        [Category("Sync Properties category")]
        [DisplayName("Last Sync from VS")]
        [Description("Last time the code was synced from Visual Studio to TrakSYS")]
        //[Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public DateTime LastSyncFromVisualStudio { get; set; }


        [Category("Sync Properties category")]
        [DisplayName("Last Sync from TrakSYS")]
        [Description("Last time the code was synced from TrakSYS to Visual Studio")]
        //[Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public DateTime LastSyncFromTrakSYS { get; set; }
    }
}
