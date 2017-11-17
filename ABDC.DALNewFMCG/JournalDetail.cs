//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDB.DALNewFMCG
{
    using System;
    using System.Collections.Generic;
    
    public partial class JournalDetail
    {
        public long Id { get; set; }
        public long JournalId { get; set; }
        public int LedgerId { get; set; }
        public decimal DrAmt { get; set; }
        public decimal CrAmt { get; set; }
        public string Particulars { get; set; }
        public string TransactionMode { get; set; }
        public string RefNo { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> ExtraCharge { get; set; }
        public string ChequeNo { get; set; }
        public Nullable<System.DateTime> ChequeDate { get; set; }
        public Nullable<System.DateTime> ClearDate { get; set; }
    
        public virtual Journal Journal { get; set; }
        public virtual Ledger Ledger { get; set; }
    }
}
