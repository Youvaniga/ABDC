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
    
    public partial class PurchaseReturnDetail
    {
        public long Id { get; set; }
        public long PRId { get; set; }
        public Nullable<long> PDId { get; set; }
        public int ProductId { get; set; }
        public int UOMId { get; set; }
        public double Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal Amount { get; set; }
        public Nullable<bool> IsResale { get; set; }
        public string Particulars { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual PurchaseDetail PurchaseDetail { get; set; }
        public virtual PurchaseReturn PurchaseReturn { get; set; }
        public virtual UOM UOM { get; set; }
    }
}
