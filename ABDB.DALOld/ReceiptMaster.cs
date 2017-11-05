//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ABDC.DALOldNUBE
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReceiptMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReceiptMaster()
        {
            this.ReceiptDetails = new HashSet<ReceiptDetail>();
        }
    
        public decimal ReceiptId { get; set; }
        public Nullable<System.DateTime> ReceiptDate { get; set; }
        public Nullable<decimal> LedgerId { get; set; }
        public string ReceiptMode { get; set; }
        public Nullable<double> ReceiptAmount { get; set; }
        public string RefNo { get; set; }
        public string Status { get; set; }
        public Nullable<double> ExtraCharge { get; set; }
        public string ChequeNo { get; set; }
        public Nullable<System.DateTime> ChequeDate { get; set; }
        public string Narration { get; set; }
        public Nullable<decimal> EntryNo { get; set; }
        public string Fund { get; set; }
        public Nullable<System.DateTime> ClrDate { get; set; }
        public string ReceivedFrom { get; set; }
        public string VoucherNo { get; set; }
    
        public virtual Ledger Ledger { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReceiptDetail> ReceiptDetails { get; set; }
    }
}
