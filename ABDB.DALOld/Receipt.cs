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
    
    public partial class Receipt
    {
        public decimal ReceiptId { get; set; }
        public Nullable<System.DateTime> ReceiptDate { get; set; }
        public string ReceiptFrom { get; set; }
        public decimal LedgerId { get; set; }
        public string ReceiptMode { get; set; }
        public Nullable<double> ReceiptAmount { get; set; }
        public string Narration { get; set; }
    }
}