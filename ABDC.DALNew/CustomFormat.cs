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
    
    public partial class CustomFormat
    {
        public int Id { get; set; }
        public string CurrencyPositiveSymbolPrefix { get; set; }
        public string CurrencyPositiveSymbolSuffix { get; set; }
        public string CurrencyNegativeSymbolPrefix { get; set; }
        public string CurrencyNegativeSymbolSuffix { get; set; }
        public string CurrencyToWordPrefix { get; set; }
        public string CurrencyToWordSuffix { get; set; }
        public string DecimalToWordPrefix { get; set; }
        public string DecimalToWordSuffix { get; set; }
        public string DecimalSymbol { get; set; }
        public Nullable<int> NoOfDigitAfterDecimal { get; set; }
        public string DigitGroupingSymbol { get; set; }
        public Nullable<int> DigitGroupingBy { get; set; }
        public Nullable<int> CurrencyCaseSensitive { get; set; }
        public Nullable<bool> IsDisplayWithOnlyOnSuffix { get; set; }
        public Nullable<int> FundMasterId { get; set; }
    
        public virtual FundMaster FundMaster { get; set; }
    }
}
