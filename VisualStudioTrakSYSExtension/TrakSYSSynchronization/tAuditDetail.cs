//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrakSYSSynchronization.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tAuditDetail
    {
        public long ID { get; set; }
        public Nullable<int> AuditHeaderID { get; set; }
        public System.DateTimeOffset AuditDateTime { get; set; }
        public string Login { get; set; }
        public string TableName { get; set; }
        public string EntityName { get; set; }
        public Nullable<int> PrimaryKey01 { get; set; }
        public Nullable<int> PrimaryKey02 { get; set; }
        public int Operation { get; set; }
        public string Data { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDateTime { get; set; }
        public Nullable<System.DateTimeOffset> UploadedDateTime { get; set; }
    
        public virtual tAuditHeader tAuditHeader { get; set; }
    }
}
