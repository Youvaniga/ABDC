//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDC.DALNewNUBE
{
    using System;
    using System.Collections.Generic;
    
    public partial class LogDetail
    {
        public long Id { get; set; }
        public Nullable<long> LogMasterId { get; set; }
        public string RecordDetail { get; set; }
        public Nullable<int> EntryBy { get; set; }
        public Nullable<System.DateTime> EntryAt { get; set; }
        public Nullable<int> LogDetailTypeId { get; set; }
    
        public virtual LogDetailType LogDetailType { get; set; }
        public virtual LogMaster LogMaster { get; set; }
        public virtual UserAccount UserAccount { get; set; }
    }
}
