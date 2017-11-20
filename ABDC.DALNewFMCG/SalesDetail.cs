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
    
    public partial class SalesDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SalesDetail()
        {
            this.SalesReturnDetails = new HashSet<SalesReturnDetail>();
        }
    
        public long Id { get; set; }
        public long SalesId { get; set; }
        public Nullable<long> SODId { get; set; }
        public int ProductId { get; set; }
        public int UOMId { get; set; }
        public double Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal Amount { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Sale Sale { get; set; }
        public virtual SalesOrderDetail SalesOrderDetail { get; set; }
        public virtual UOM UOM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesReturnDetail> SalesReturnDetails { get; set; }
    }
}