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
    
    public partial class StockSeparated
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StockSeparated()
        {
            this.StockSeperatedDetails = new HashSet<StockSeperatedDetail>();
        }
    
        public long Id { get; set; }
        public System.DateTime Date { get; set; }
        public string RefNo { get; set; }
        public string RefCode { get; set; }
        public int StaffId { get; set; }
        public decimal ItemAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Narration { get; set; }
    
        public virtual Staff Staff { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockSeperatedDetail> StockSeperatedDetails { get; set; }
    }
}
