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
    
    public partial class ErrorLog
    {
        public int Id { get; set; }
        public Nullable<int> EntityTypeId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> ErrorBy { get; set; }
        public Nullable<System.DateTime> ErrorAt { get; set; }
        public string ErrorMessage { get; set; }
    }
}
