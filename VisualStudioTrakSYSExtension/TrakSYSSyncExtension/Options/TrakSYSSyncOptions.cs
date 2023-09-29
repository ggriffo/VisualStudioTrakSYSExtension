using System;
using System.ComponentModel;

namespace TrakSYSSyncExtension.Options
{
    internal class TrakSYSSyncOptions : BaseOptionModel<TrakSYSSyncOptions>
    {
        [Category("TrakSYS Configuration")]
        [DisplayName("Connection String")]
        [Description("Specifies the connection string to the TrakSYS Database. TO BE USED ON DEV environment only")]
        [DefaultValue("My message")]
        public string ConnectionString { get; set; } = "";

        [Category("Sync Properties")]
        [DisplayName("Last Sync from VS")]
        [Description("Last time the code was synced from Visual Studio to TrakSYS")]
        //[Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public DateTime LastSyncFromVisualStudio { get; set; } = DateTime.MinValue;


        [Category("Sync Properties")]
        [DisplayName("Last Sync from TrakSYS")]
        [Description("Last time the code was synced from TrakSYS to Visual Studio")]
        //[Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public DateTime LastSyncFromTrakSYS { get; set; } = DateTime.MinValue;
    }
}
