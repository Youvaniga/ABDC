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
    
    public partial class ReceiptDetail
    {
        public long Id { get; set; }
        public long ReceiptId { get; set; }
        public int LedgerId { get; set; }
        public decimal Amount { get; set; }
        public string Particulars { get; set; }
    
        public virtual Ledger Ledger { get; set; }
        public virtual Receipt Receipt { get; set; }
    }
}
