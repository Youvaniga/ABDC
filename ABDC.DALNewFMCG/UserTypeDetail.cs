//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDC.DALNewFMCG
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserTypeDetail
    {
        public int Id { get; set; }
        public int UserTypeId { get; set; }
        public int UserTypeFormDetailId { get; set; }
        public bool IsViewForm { get; set; }
        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    
        public virtual UserType UserType { get; set; }
        public virtual UserTypeFormDetail UserTypeFormDetail { get; set; }
    }
}
