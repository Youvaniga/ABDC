//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDB.DALOldFMCG
{
    using System;
    using System.Collections.Generic;
    
    public partial class PurchaseReturn
    {
        public decimal PRId { get; set; }
        public string PRCode { get; set; }
        public Nullable<System.DateTime> PRDate { get; set; }
        public string LedgerCode { get; set; }
        public string PRType { get; set; }
        public string InvoiceNo { get; set; }
        public Nullable<double> Extra { get; set; }
        public Nullable<double> DiscountAmount { get; set; }
        public string Narration { get; set; }
    }
}
