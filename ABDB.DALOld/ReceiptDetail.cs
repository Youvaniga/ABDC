//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDC.DALOld
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReceiptDetail
    {
        public decimal RDID { get; set; }
        public Nullable<decimal> ReceiptId { get; set; }
        public Nullable<decimal> LedgerId { get; set; }
        public string Narration { get; set; }
        public Nullable<double> Amount { get; set; }
    
        public virtual Ledger Ledger { get; set; }
        public virtual ReceiptMaster ReceiptMaster { get; set; }
    }
}