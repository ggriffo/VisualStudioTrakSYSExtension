using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrakSYSSyncExtension.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime LastAuditTime { get; set; }
    }
}
