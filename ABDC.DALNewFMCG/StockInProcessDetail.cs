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
    
    public partial class StockInProcessDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StockInProcessDetail()
        {
            this.StockSeperatedDetails = new HashSet<StockSeperatedDetail>();
        }
    
        public long Id { get; set; }
        public long SPId { get; set; }
        public int ProductId { get; set; }
        public int UOMId { get; set; }
        public double Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal Amount { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual StockInProcess StockInProcess { get; set; }
        public virtual UOM UOM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockSeperatedDetail> StockSeperatedDetails { get; set; }
    }
}
