using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ABDB.DALNewFMCG;
using ABDB.DALOldFMCG;

namespace ABDC
{

    public partial class frmFMCG : Window
    {
        FMCG_Old01Entities dbOld = new ABDB.DALOldFMCG.FMCG_Old01Entities();
        FMCG_New01Entities dbNew = new ABDB.DALNewFMCG.FMCG_New01Entities();
        List<DataKeyValueFMCG> lstLedgerIds = new List<DataKeyValueFMCG>();
        List<DataKeyValueFMCG> lstProductIds = new List<DataKeyValueFMCG>();


        List<DALNewFMCG.Purchase> lstPurchaseNew = new List<DALNewFMCG.Purchase>();
        List<DALNewFMCG.Sale> lstSaleNew = new List<DALNewFMCG.Sale>();
        List<DALNewFMCG.PurchaseReturn> lstPurchaseReturnNew = new List<DALNewFMCG.PurchaseReturn>();
        List<DALNewFMCG.SalesReturn> lstSaleReturnNew = new List<DALNewFMCG.SalesReturn>();
        List<DALNewFMCG.Journal> lstJournalNew = new List<DALNewFMCG.Journal>();
        List<DALNewFMCG.Payment> lstPaymentNew = new List<DALNewFMCG.Payment>();
        List<DALNewFMCG.Receipt> lstReceiptNew = new List<DALNewFMCG.Receipt>();


        List<DALNewFMCG.Journal> lstJournal_Tr = new List<DALNewFMCG.Journal>();

        DateTime dtStart, dtEnd;
        private List<DALNewFMCG.EntityType> _entityTypeList;
        private List<DALNewFMCG.LogDetailType> _logDetailTypeList;

        int UOMId;
        int TAXId;

        public frmFMCG()
        {
            InitializeComponent();
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            dtStart = DateTime.Now;
            WriteLog("ABDC FMCG Start");

            try
            {
                WriteLog("Fetching Company List");
                var lstCompany = dbOld.CompanyDetails.Where(x => x.CompanyId > 1).ToList();
                var lstUserTypeFormDetail = dbNew.UserTypeFormDetails.ToList();
                pbrCompany.Maximum = lstCompany.Count();
                pbrCompany.Value = 0;
                WriteLog("Creating Company");
                foreach (var c in lstCompany)
                {
                    pbrPayment.Value = 0;
                    pbrReceipt.Value = 0;
                    pbrJournal.Value = 0;

                    DALNewFMCG.CompanyDetail cm = new DALNewFMCG.CompanyDetail() { CompanyName = c.CompanyName, AddressLine1 = c.Address, GSTNo = c.Cst, EMailId = c.MailId, MobileNo = c.MobileNo, TelephoneNo = c.PhNo, CompanyType = "Company", IsActive = true };
                    dbNew.CompanyDetails.Add(cm);
                    dbNew.SaveChanges();

                    pbrCompany.Value += 1;
                    DALNewFMCG.UserType ut = new DALNewFMCG.UserType() { TypeOfUser = DataKeyValueNUBE.Administrator_Key };
                    cm.UserTypes.Add(ut);

                    foreach (var utfd in lstUserTypeFormDetail)
                    {
                        var ud = new DALNewFMCG.UserTypeDetail()
                        {
                            UserTypeFormDetailId = utfd.Id,
                            IsViewForm = true,
                            AllowInsert = true,
                            AllowUpdate = true,
                            AllowDelete = true
                        };
                        ut.UserTypeDetails.Add(ud);
                    }

                    DALNewFMCG.UserAccount ua = new DALNewFMCG.UserAccount() { LoginId = "Admin", UserName = "Admin", Password = "Admin" };
                    ut.UserAccounts.Add(ua);
                    WriteLog(string.Format("Stored User Account : {0}, Id : {1}", ua.UserName, ua.Id));

                    DALNewFMCG.CustomFormat cf = new DALNewFMCG.CustomFormat()
                    {
                        CurrencyNegativeSymbolPrefix = "[RM] ",
                        CurrencyPositiveSymbolPrefix = "RM ",
                        CurrencyToWordPrefix = "Ringgit Malaysia ",
                        DecimalToWordPrefix = "Cents ",
                        DecimalSymbol = ".",
                        DigitGroupingSymbol = ",",
                        IsDisplayWithOnlyOnSuffix = true,
                        NoOfDigitAfterDecimal = 2,
                        CompanyId = cm.Id
                    };
                    dbNew.CustomFormats.Add(cf);
                    dbNew.SaveChanges();
                    WriteMasterData(cm);
                    WriteTransactionData(cm);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }

            WriteLog("ABDC End");
            MessageBox.Show("Finished");
        }
        void WriteMasterData(DALNewFMCG.CompanyDetail cm)
        {
            WriteUnitOfMeasurement(cm); ; dbNew.SaveChanges();
            WriteTaxMaster(cm); ; dbNew.SaveChanges();

            UOMId = dbNew.UOMs.ToList().LastOrDefault().Id;
            TAXId = dbNew.TaxMasters.ToList().LastOrDefault().Id;
            WriteAccountGroup(cm, "ACG0001", null); dbNew.SaveChanges();
            WriteStockGroup(cm, "STG001", null); dbNew.SaveChanges();

            var l1 = from LedgerOld in dbOld.Ledgers.ToList()
                     join LedgerNew in dbNew.Ledgers.ToList() on LedgerOld.LedgerName equals LedgerNew.LedgerName
                     select new DataKeyValueFMCG() { DataKey = LedgerOld.LedgerCode, DataValue = LedgerNew.Id };
            lstLedgerIds = l1.ToList();

            var l2 = from ProductOld in dbOld.Products.ToList()
                     join ProductNew in dbNew.Products.ToList() on ProductOld.ProductName equals ProductNew.ProductName
                     select new DataKeyValueFMCG() { DataKey = ProductOld.ProductCode, DataValue = ProductNew.Id };
            lstProductIds = l2.ToList();

        }

        private void WriteTaxMaster(CompanyDetail cm)
        {
            WriteLog("Start to store the TAX");

            foreach (var dOld in dbOld.TaxMasters.ToList())
            {
                DALNewFMCG.TaxMaster dNew = new DALNewFMCG.TaxMaster()
                {
                    TaxName = dOld.Narration,
                    TaxPercentage = dOld.TaxValue,
                    CompanyId = cm.Id
                };
                cm.TaxMasters.Add(dNew);

                WriteLog(string.Format("Stored TAX : {0}", dNew.TaxName));

            }
            WriteLog("End to store the UOM");
        }

        void WriteTransactionData(DALNewFMCG.CompanyDetail cm)
        {

            lstPurchaseNew = new List<Purchase>();
            lstSaleNew = new List<Sale>();
            lstPurchaseReturnNew = new List<PurchaseReturn>();
            lstSaleReturnNew = new List<SalesReturn>();

            WritePurchase();
            dbNew.Purchases.AddRange(lstPurchaseNew);
            dbNew.SaveChanges();

            WriteSale();
            dbNew.Sales.AddRange(lstSaleNew);
            dbNew.SaveChanges();

            WritePurchaseReturn();
            dbNew.PurchaseReturns.AddRange(lstPurchaseReturnNew);
            dbNew.SaveChanges();

            WriteSaleReturn();
            dbNew.SalesReturns.AddRange(lstSaleReturnNew);
            dbNew.SaveChanges();


        }
        void WriteAccountGroup(DALNewFMCG.CompanyDetail cm, string AGId, DALNewFMCG.AccountGroup UAG)
        {
            WriteLog("Start to store the Accounts Group");

            foreach (var ag in dbOld.AccountGroups.Where(x => x.Under == AGId && x.AccountGroupCode != AGId).ToList())
            {
                DALNewFMCG.AccountGroup d = new DALNewFMCG.AccountGroup()
                {
                    GroupName = ag.GroupName,
                    GroupCode = ag.GroupCode,
                    AccountGroup2 = UAG
                };
                cm.AccountGroups.Add(d);
                if (d.GroupName == "Sundry Debtors")
                {
                    foreach (var c in dbOld.Customers.ToList())
                    {
                        DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                        {
                            LedgerName = c.LedgerName,
                            PersonIncharge = c.CustomerName,
                            AddressLine1 = c.AddressLine,
                            TelephoneNo = c.TelePhoneNo,
                            MobileNo = c.MobileNo,
                            EMailId = c.EMailId,
                            GSTNo = c.TinNo,
                        };
                        d.Ledgers.Add(l);
                        dbNew.Customers.Add(new DALNewFMCG.Customer() { Ledger = l });
                    }
                }
                else if (d.GroupName == "Sundry Creditors")
                {
                    foreach (var s in dbOld.Suppliers.ToList())
                    {
                        DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                        {
                            LedgerName = s.LedgerName,
                            PersonIncharge = s.SupplierName,
                            AddressLine1 = s.AddressLine,
                            TelephoneNo = s.TelePhoneNo,
                            MobileNo = s.MobileNo,
                            EMailId = s.EMailId,
                            GSTNo = s.TinNo,
                        };
                        d.Ledgers.Add(l);
                        dbNew.Suppliers.Add(new DALNewFMCG.Supplier() { Ledger = l });
                    }
                }
                else if (d.GroupName == "Bank Accounts")
                {
                    foreach (var b in dbOld.Banks.ToList())
                    {
                        DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                        {
                            LedgerName = b.LedgerName,
                            PersonIncharge = b.CPerson1,
                            AddressLine1 = b.Address,
                            TelephoneNo = b.Phone,
                            MobileNo = b.MobileNo
                        };
                        d.Ledgers.Add(l);
                        dbNew.Banks.Add(new DALNewFMCG.Bank() { Ledger = l, BankAccountName = b.BankName });
                    }
                }
                if (d.GroupName == "Cash-in-hand")
                {
                    DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Cash Ledger",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    dbNew.SaveChanges();
                }
                if (d.GroupName == "Duties & Taxes")
                {
                    DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Input Tax",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    dbNew.SaveChanges();
                }
                if (d.GroupName == "Duties & Taxes")
                {
                    DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Output Tax",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    dbNew.SaveChanges();
                }
                if (d.GroupName == "Sales Accounts")
                {
                    DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Sales A/C",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Sales Return A/C",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    dbNew.SaveChanges();
                }
                if (d.GroupName == "Purchase Account")
                {
                    DALNewFMCG.Ledger l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Purchase A/C",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    l = new DALNewFMCG.Ledger()
                    {
                        LedgerName = "Purchase Return A/C",
                        PersonIncharge = "",
                        AddressLine1 = "",
                        TelephoneNo = "",
                        MobileNo = ""
                    };
                    d.Ledgers.Add(l);
                    dbNew.Ledgers.Add(l);
                    dbNew.SaveChanges();
                }
                WriteLog(string.Format("Stored Account Group : {0}", d.GroupName));
                WriteAccountGroup(cm, ag.AccountGroupCode, d);
            }
            WriteLog("End to store the Accounts Group");
        }

        void WriteStockGroup(DALNewFMCG.CompanyDetail cm, string SGId, DALNewFMCG.StockGroup USG)
        {
            WriteLog("Start to store the Stock Group");

            foreach (var ag in dbOld.StockGroups.Where(x => x.Under == SGId && x.StockGroupCode != SGId).ToList())
            {
                DALNewFMCG.StockGroup d = new DALNewFMCG.StockGroup()
                {
                    StockGroupName = ag.GroupName,
                    GroupCode = ag.GroupCode,
                    StockGroup2 = USG
                };
                cm.StockGroups.Add(d);
                foreach (var pt in dbOld.Products.ToList().Where(x => x.GroupCode == ag.StockGroupCode || (ag.GroupName == "OTHERS" && (String.IsNullOrEmpty(x.GroupCode) || string.IsNullOrWhiteSpace(x.GroupCode)))).ToList())
                {
                    d.Products.Add(new DALNewFMCG.Product()
                    {
                        ProductName = pt.ProductName,
                        ItemCode = pt.ItemCode,
                        UOMId = UOMId
                    });

                }
                WriteLog(string.Format("Stored Account Group : {0}", d.StockGroupName));
                WriteStockGroup(cm, ag.StockGroupCode, d);

            }
            WriteLog("End to store the Stock Group");
        }

        void WriteUnitOfMeasurement(DALNewFMCG.CompanyDetail cm)
        {
            WriteLog("Start to store the UOM");

            foreach (var dOld in dbOld.UnitsOfMeasurements.ToList())
            {
                DALNewFMCG.UOM dNew = new DALNewFMCG.UOM()
                {
                    FormalName = dOld.formalname,
                    CompanyId = cm.Id,
                    Symbol = dOld.UOMSymbol
                };
                cm.UOMs.Add(dNew);

                WriteLog(string.Format("Stored UOM : {0}", dNew.Symbol));

            }
            WriteLog("End to store the UOM");
        }

        void WritePurchase()
        {
            var lstPurchase = dbOld.Purchases.ToList();
            var lstpurchaseDetails = dbOld.PurchaseDetails.ToList();

            foreach (var p in lstPurchase)
            {
                try
                {
                    var lstDetails = lstpurchaseDetails.Where(x => x.PurchaseCode == p.PurchaseCode).ToList();
                    var ItemAmt = Convert.ToDecimal(lstDetails.Sum(x => x.Rate * x.Quantity));
                    var disAmt = Convert.ToDecimal(lstDetails.Sum(x => (x.Rate * x.Quantity) * x.DisPer / 100));
                    var GSTAmt = Convert.ToDecimal(lstDetails.Sum(x => ((x.Rate * x.Quantity) - ((x.Rate * x.Quantity) * x.DisPer / 100)) * Convert.ToDouble(x.TaxPer != "" ? "6" : "0") / 100));
                    var ExAmt = Convert.ToDecimal(p.Extra);
                    var TotAmt = ItemAmt - disAmt + GSTAmt + ExAmt;

                    DALNewFMCG.Purchase d = new DALNewFMCG.Purchase()
                    {
                        LedgerId = GetLedgerId(p.LedgerCode),
                        PurchaseDate = p.PurchaseDate.Value,
                        TransactionTypeId = p.PurchaseType == "Cash" ? 1 : 2,
                        RefCode = p.InvoiceNo,
                        RefNo = GetPurchaseRefNo(p.PurchaseDate.Value),
                        ItemAmount = ItemAmt,
                        ExtraAmount = ExAmt,
                        DiscountAmount = disAmt,
                        GSTAmount = GSTAmt,
                        TotalAmount = TotAmt
                    };

                    foreach (var pd in lstDetails)
                    {
                        try
                        {
                            d.PurchaseDetails.Add(new DALNewFMCG.PurchaseDetail()
                            {
                                ProductId = GetProductId(pd.ProductCode),
                                UnitPrice = Convert.ToDecimal(pd.Rate),
                                Quantity = pd.Quantity.Value,
                                Amount = Convert.ToDecimal(pd.Rate * pd.Quantity.Value),
                                DiscountAmount = Convert.ToDecimal((pd.Rate * pd.Quantity.Value) * pd.DisPer),
                                GSTAmount = Convert.ToDecimal(((pd.Rate * pd.Quantity.Value) * pd.DisPer) * Convert.ToDouble(pd.TaxPer != "" ? "6" : "0") / 100),
                                UOMId = UOMId
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }

                    }
                    lstPurchaseNew.Add(d);
                    WriteJournal_Purchase(d);
                    dbNew.Journals.AddRange(lstJournal_Tr);
                    dbNew.SaveChanges();
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }

            }
        }
        void WriteSale()
        {
            var lstSale = dbOld.Sales.ToList();
            var lstsaleDetails = dbOld.SalesDetails.ToList();

            foreach (var p in lstSale)
            {
                try
                {
                    var lstDetails = lstsaleDetails.Where(x => x.SalesCode == p.SalesCode).ToList();
                    var ItemAmt = Convert.ToDecimal(lstDetails.Sum(x => x.Rate * x.Quantity));
                    var disAmt = Convert.ToDecimal(lstDetails.Sum(x => (x.Rate * x.Quantity) * x.DisPer / 100));
                    var GSTAmt = Convert.ToDecimal(lstDetails.Sum(x => ((x.Rate * x.Quantity) - ((x.Rate * x.Quantity) * x.DisPer / 100)) * Convert.ToDouble(x.TaxPer != "" ? "6" : "0") / 100));
                    var ExAmt = Convert.ToDecimal(p.Extra);
                    var TotAmt = ItemAmt - disAmt + GSTAmt + ExAmt;

                    DALNewFMCG.Sale d = new DALNewFMCG.Sale()
                    {
                        LedgerId = GetLedgerId(p.LedgerCode),
                        SalesDate = p.SalesDate.Value,
                        TransactionTypeId = p.SalesType == "Cash" ? 1 : 2,
                        RefCode = p.InvoiceNo,
                        RefNo = GetSaleRefNo(p.SalesDate.Value),
                        ItemAmount = ItemAmt,
                        ExtraAmount = ExAmt,
                        DiscountAmount = disAmt,
                        GSTAmount = GSTAmt,
                        TotalAmount = TotAmt
                    };

                    foreach (var pd in lstDetails)
                    {
                        try
                        {
                            d.SalesDetails.Add(new DALNewFMCG.SalesDetail()
                            {
                                ProductId = GetProductId(pd.ProductCode),
                                UnitPrice = Convert.ToDecimal(pd.Rate),
                                Quantity = pd.Quantity.Value,
                                Amount = Convert.ToDecimal(pd.Rate * pd.Quantity.Value),
                                DiscountAmount = Convert.ToDecimal((pd.Rate * pd.Quantity.Value) * pd.DisPer),
                                GSTAmount = Convert.ToDecimal(((pd.Rate * pd.Quantity.Value) * pd.DisPer) * Convert.ToDouble(pd.TaxPer != "" ? "6" : "0") / 100),
                                UOMId = UOMId
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }

                    }
                    lstSaleNew.Add(d);
                    WriteJournal_Sales(d);
                    dbNew.Journals.AddRange(lstJournal_Tr);
                    dbNew.SaveChanges();
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }

            }
        }

        void WritePurchaseReturn()
        {
            var lstPurchaseReturn = dbOld.PurchaseReturns.ToList();
            var lstPurchaseReturnDetails = dbOld.PurchaseReturnDetails.ToList();

            foreach (var p in lstPurchaseReturn)
            {
                try
                {
                    var lstDetails = lstPurchaseReturnDetails.Where(x => x.PRCode == p.PRCode).ToList();
                    var ItemAmt = Convert.ToDecimal(lstDetails.Sum(x => x.Rate * x.Quantity));
                    var disAmt = Convert.ToDecimal(lstDetails.Sum(x => (x.Rate * x.Quantity) * x.DisPer / 100));
                    var GSTAmt = Convert.ToDecimal(lstDetails.Sum(x => ((x.Rate * x.Quantity) - ((x.Rate * x.Quantity) * x.DisPer / 100)) * Convert.ToDouble(x.TaxPer != "" ? "6" : "0") / 100));
                    var ExAmt = Convert.ToDecimal(p.Extra);
                    var TotAmt = ItemAmt - disAmt + GSTAmt + ExAmt;

                    DALNewFMCG.PurchaseReturn d = new DALNewFMCG.PurchaseReturn()
                    {
                        LedgerId = GetLedgerId(p.LedgerCode),
                        PRDate = p.PRDate.Value,
                        TransactionTypeId = p.PRType == "Cash" ? 1 : 2,
                        RefCode = p.InvoiceNo,
                        RefNo = GetPurchaseReturnRefNo(p.PRDate.Value),
                        ItemAmount = ItemAmt,
                        ExtraAmount = ExAmt,
                        DiscountAmount = disAmt,
                        GSTAmount = GSTAmt,
                        TotalAmount = TotAmt
                    };

                    foreach (var pd in lstDetails)
                    {
                        try
                        {
                            d.PurchaseReturnDetails.Add(new DALNewFMCG.PurchaseReturnDetail()
                            {
                                ProductId = GetProductId(pd.ProductCode),
                                UnitPrice = Convert.ToDecimal(pd.Rate),
                                Quantity = pd.Quantity.Value,
                                Amount = Convert.ToDecimal(pd.Rate * pd.Quantity.Value),
                                DiscountAmount = Convert.ToDecimal((pd.Rate * pd.Quantity.Value) * pd.DisPer),
                                GSTAmount = Convert.ToDecimal(((pd.Rate * pd.Quantity.Value) * pd.DisPer) * Convert.ToDouble(pd.TaxPer != "" ? "6" : "0") / 100),
                                UOMId = UOMId
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }

                    }
                    lstPurchaseReturnNew.Add(d);
                    WriteJournal_Purchase_Return(d);
                    dbNew.Journals.AddRange(lstJournal_Tr);
                    dbNew.SaveChanges();
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }

            }
        }

        void WriteSaleReturn()
        {
            var lstSaleReturn = dbOld.SalesReturns.ToList();
            var lstsaleReturnDetails = dbOld.SalesReturnDetails.ToList();

            foreach (var p in lstSaleReturn)
            {
                try
                {
                    var lstDetails = lstsaleReturnDetails.Where(x => x.SRCode == p.SRCode).ToList();
                    var ItemAmt = Convert.ToDecimal(lstDetails.Sum(x => x.Rate * x.Quantity));
                    var disAmt = Convert.ToDecimal(lstDetails.Sum(x => (x.Rate * x.Quantity) * x.DisPer / 100));
                    var GSTAmt = Convert.ToDecimal(lstDetails.Sum(x => ((x.Rate * x.Quantity) - ((x.Rate * x.Quantity) * x.DisPer / 100)) * Convert.ToDouble(x.TaxPer != "" ? "6" : "0") / 100));
                    var ExAmt = Convert.ToDecimal(p.Extra);
                    var TotAmt = ItemAmt - disAmt + GSTAmt + ExAmt;

                    DALNewFMCG.SalesReturn d = new DALNewFMCG.SalesReturn()
                    {
                        LedgerId = GetLedgerId(p.LedgerCode),
                        SRDate = p.SRDate.Value,
                        TransactionTypeId = p.SRType == "Cash" ? 1 : 2,
                        RefNo = GetSaleReturnRefNo(p.SRDate.Value),
                        ItemAmount = ItemAmt,
                        ExtraAmount = ExAmt,
                        DiscountAmount = disAmt,
                        GSTAmount = GSTAmt,
                        TotalAmount = TotAmt
                    };

                    foreach (var pd in lstDetails)
                    {
                        try
                        {
                            d.SalesReturnDetails.Add(new DALNewFMCG.SalesReturnDetail()
                            {
                                ProductId = GetProductId(pd.ProductCode),
                                UnitPrice = Convert.ToDecimal(pd.Rate),
                                Quantity = pd.Quantity.Value,
                                Amount = Convert.ToDecimal(pd.Rate * pd.Quantity.Value),
                                DiscountAmount = Convert.ToDecimal((pd.Rate * pd.Quantity.Value) * pd.DisPer),
                                GSTAmount = Convert.ToDecimal(((pd.Rate * pd.Quantity.Value) * pd.DisPer) * Convert.ToDouble(pd.TaxPer != "" ? "6" : "0") / 100),
                                UOMId = UOMId
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }

                    }
                    lstSaleReturnNew.Add(d);
                    WriteJournal_Sales_Return(d);
                    dbNew.Journals.AddRange(lstJournal_Tr);
                    dbNew.SaveChanges();
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }

            }
        }

        void WritePayment()
        {
            var lstP = dbOld.Payments.ToList();
            var lstPDetails = dbOld.PaymentDetails.ToList();

            foreach (var p in lstP)
            {
                try
                {
                    var lstDetails = lstPDetails.Where(x => x.PaymentCode == p.PaymentCode).ToList();
                    DALNewFMCG.Payment d = new DALNewFMCG.Payment()
                    {
                        RefCode = p.PaymentCode,
                        EntryNo = GetJournalEntryNo(p.PaymentDate.Value),
                        Amount = (decimal)p.PayAmount,
                        PaymentDate = p.PaymentDate.Value,
                        Particulars = p.Narration,
                        Status = p.Status,
                        ChequeDate = p.ChequeDate,
                        ChequeNo = p.ChequeNo,
                        ExtraCharge = (decimal)p.ExtraCharge.Value,
                        PaymentMode = p.PaymentMode,
                        PayTo = p.PaymentTo, RefNo = p.RefNo,
                        LedgerId = GetLedgerId(p.LedgerCodeFrom)
                    };

                    foreach (var pd in lstPDetails)
                    {
                        try
                        {
                            d.PaymentDetails.Add(new DALNewFMCG.PaymentDetail()
                            {
                                LedgerId = GetLedgerId(p.LedgerCodeTo),
                                Amount = (decimal)pd.Amount,
                                PaymentId = Convert.ToInt64(pd.PaymentCode.ToString()),                           
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                    lstPaymentNew.Add(d);
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }
            }
        }

        void WriteReceipt()
        {
            var lstR = dbOld.Receipts.ToList();
            var lstRDetails = dbOld.ReceiptDetails.ToList();

            foreach (var p in lstR)
            {
                try
                {
                    var lstDetails = lstRDetails.Where(x => x.ReceiptCode == p.ReceiptCode).ToList();
                    DALNewFMCG.Receipt d = new DALNewFMCG.Receipt()
                    {
                        RefCode = p.ReceiptCode,
                        EntryNo = GetReceiptEntryNo(p.ReceiptDate.Value),
                        Amount = (decimal)p.ReceiptAmount,
                        ReceiptDate = p.ReceiptDate.Value,
                        Particulars = p.Narration,
                        Status = p.Status,
                        ChequeDate = p.ChequeDate,
                        ChequeNo = p.ChequeNo,
                        Extracharge = (decimal)p.ExtraCharge.Value,
                        ReceiptMode = p.ReceiptMode,
                        ReceivedFrom = p.ReceiptFrom,
                        RefNo = p.RefNo,
                        LedgerId = GetLedgerId(p.LedgerCodeFrom)
                    };

                    foreach (var pd in lstRDetails)
                    {
                        try
                        {
                            d.ReceiptDetails.Add(new DALNewFMCG.ReceiptDetail()
                            {
                                LedgerId = GetLedgerId(p.ReceiptTo),
                                Amount = (decimal)pd.Amount,
                                ReceiptId = Convert.ToInt64(pd.ReceiptCode.ToString()),
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                    lstPaymentNew.Add(d);
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }
            }
        }

        #region journal

        void WriteJournal()
        {
            var lstJournal = dbOld.JournalMasters.ToList();
            var lstJournalDetails = dbOld.JournalDetails.ToList();

            foreach (var p in lstJournal)
            {
                try
                {
                    var lstDetails = lstJournalDetails.Where(x => x.JournalCode == p.JournalCode).ToList();

                    DALNewFMCG.Journal d = new DALNewFMCG.Journal()
                    {
                        RefCode = p.JournalCode,
                        EntryNo = GetJournalEntryNo(p.JournalDate.Value),

                    };

                    foreach (var pd in lstDetails)
                    {
                        try
                        {
                            d.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                            {
                                LedgerId = GetLedgerId(pd.LedgerCode),
                                CrAmt = (decimal)pd.CrAmt,
                                DrAmt = (decimal)pd.DrAmt,
                                Particulars = pd.Narration,

                            });
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }

                    }
                    lstJournalNew.Add(d);

                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }


            }
        }


            #region Journal Sales
            void WriteJournal_Sales(Sale S)
        {
                string Mode, status = null;

                DALNewFMCG.Journal j = new DALNewFMCG.Journal()
                {
                    JournalDate = S.SalesDate,
                    RefCode = S.RefCode,
                    EntryNo = GetJournalEntryNo(S.SalesDate),
                };
                if (S.TransactionTypeId == 1)
                {
                    Mode = "Cash";

                }
                else if (S.TransactionTypeId == 1)
                {
                    Mode = "Credit";
                }
                else
                {
                    Mode = "Cheque";
                    status = "Process";
                }
                foreach (var pd in S.SalesDetails)
                {
                    try
                    {
                        if (S.TransactionTypeId == 1)
                        {
                            j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                            {
                                LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Cash Ledger").Select(x => x.Id).FirstOrDefault(),
                                CrAmt = S.TotalAmount,
                                Particulars = S.Narration,
                                TransactionMode = "Cash"
                            });
                        }
                        else if (S.TransactionTypeId == 2)
                        {
                            j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                            {
                                LedgerId = S.LedgerId,
                                CrAmt = S.TotalAmount,
                                Particulars = S.Narration,
                                TransactionMode = "Credit"
                            });
                        }
                        else
                        {
                            j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                            {
                                LedgerId = dbNew.Banks.FirstOrDefault().LedgerId,
                                CrAmt = S.TotalAmount,
                                TransactionMode = "Cheque",
                                Particulars = S.Narration,
                                ChequeDate = S.ChequeDate,
                                ChequeNo = S.ChequeNo,
                                Status = "Process"

                            });
                        }
                        j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                        {
                            LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Sales Return A/C").Select(x => x.Id).FirstOrDefault(),
                            DrAmt = S.ItemAmount - S.DiscountAmount + S.ExtraAmount,
                            Particulars = S.Narration,
                            TransactionMode = Mode,
                            ChequeDate = S.ChequeDate,
                            ChequeNo = S.ChequeNo,
                            Status = status

                        });

                        j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                        {
                            LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Output Tax").Select(x => x.Id).FirstOrDefault(),
                            DrAmt = S.GSTAmount,
                            Particulars = S.Narration,
                            TransactionMode = Mode,
                            ChequeDate = S.ChequeDate,
                            ChequeNo = S.ChequeNo,
                            Status = status
                        });

                        lstJournal_Tr.Add(j);

                    }

                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
            }
            #endregion

            #region Journal Purchase Return
            void WriteJournal_Purchase_Return(PurchaseReturn P)
        {
                string Mode, status = null;

                DALNewFMCG.Journal j = new DALNewFMCG.Journal()
                {
                    JournalDate = P.PRDate,
                    RefCode = P.RefCode,
                    EntryNo = GetJournalEntryNo(P.PRDate),
                };
                if (P.TransactionTypeId == 1)
                {
                    Mode = "Cash";

                }
                else if (P.TransactionTypeId == 1)
                {
                    Mode = "Credit";
                }
                else
                {
                    Mode = "Cheque";
                    status = "Process";
                }

                if (P.TransactionTypeId == 1)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Cash Ledger").Select(x => x.Id).FirstOrDefault(),
                        DrAmt = P.TotalAmount,
                        Particulars = P.Narration,
                        TransactionMode = "Cash"
                    });
                }
                else if (P.TransactionTypeId == 2)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = P.LedgerId,
                        DrAmt = P.TotalAmount,
                        Particulars = P.Narration,
                        TransactionMode = "Credit"
                    });
                }
                else
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Banks.FirstOrDefault().LedgerId,
                        DrAmt = P.TotalAmount,
                        TransactionMode = "Cheque",
                        Particulars = P.Narration,
                        ChequeDate = P.ChequeDate,
                        ChequeNo = P.ChequeNo,
                        Status = "Process"

                    });
                }
                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Purchase Return A/C").Select(x => x.Id).FirstOrDefault(),

                    CrAmt = P.ItemAmount - P.DiscountAmount + P.ExtraAmount,
                    Particulars = P.Narration,
                    TransactionMode = Mode,
                    ChequeDate = P.ChequeDate,
                    ChequeNo = P.ChequeNo,
                    Status = status

                });

                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Input Tax").Select(x => x.Id).FirstOrDefault(),

                    CrAmt = P.GSTAmount,
                    Particulars = P.Narration,
                    TransactionMode = Mode,
                    ChequeDate = P.ChequeDate,
                    ChequeNo = P.ChequeNo,
                    Status = status
                });


            }
            #endregion

            #region Journal Purchase
            void WriteJournal_Purchase(Purchase P)
        {
                string Mode, status = null;

                DALNewFMCG.Journal j = new DALNewFMCG.Journal()
                {
                    JournalDate = P.PurchaseDate,
                    RefCode = P.RefCode,
                    EntryNo = GetJournalEntryNo(P.PurchaseDate),
                };
                if (P.TransactionTypeId == 1)
                {
                    Mode = "Cash";

                }
                else if (P.TransactionTypeId == 1)
                {
                    Mode = "Credit";
                }
                else
                {
                    Mode = "Cheque";
                    status = "Process";
                }

                if (P.TransactionTypeId == 1)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Cash Ledger").Select(x => x.Id).FirstOrDefault(),
                        CrAmt = P.TotalAmount,
                        Particulars = P.Narration,
                        TransactionMode = "Cash"
                    });
                }
                else if (P.TransactionTypeId == 2)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = P.LedgerId,
                        CrAmt = P.TotalAmount,
                        Particulars = P.Narration,
                        TransactionMode = "Credit"
                    });
                }
                else
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Banks.FirstOrDefault().LedgerId,
                        CrAmt = P.TotalAmount,
                        TransactionMode = "Cheque",
                        Particulars = P.Narration,
                        ChequeDate = P.ChequeDate,
                        ChequeNo = P.ChequeNo,
                        Status = "Process"

                    });
                }
                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Purchase Return A/C").Select(x => x.Id).FirstOrDefault(),

                    DrAmt = P.ItemAmount - P.DiscountAmount + P.ExtraAmount,
                    Particulars = P.Narration,
                    TransactionMode = Mode,
                    ChequeDate = P.ChequeDate,
                    ChequeNo = P.ChequeNo,
                    Status = status

                });

                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Output Tax").Select(x => x.Id).FirstOrDefault(),

                    DrAmt = P.GSTAmount,
                    Particulars = P.Narration,
                    TransactionMode = Mode,
                    ChequeDate = P.ChequeDate,
                    ChequeNo = P.ChequeNo,
                    Status = status
                });


            }
            #endregion
            #region Journal Sales Return
            void WriteJournal_Sales_Return(SalesReturn SR)
        {
                string Mode, status = null;

                DALNewFMCG.Journal j = new DALNewFMCG.Journal()
                {
                    JournalDate = SR.SRDate,
                    RefCode = SR.RefCode,
                    EntryNo = GetJournalEntryNo(SR.SRDate),
                };
                if (SR.TransactionTypeId == 1)
                {
                    Mode = "Cash";

                }
                else if (SR.TransactionTypeId == 1)
                {
                    Mode = "Credit";
                }
                else
                {
                    Mode = "Cheque";
                    status = "Process";
                }

                if (SR.TransactionTypeId == 1)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Cash Ledger").Select(x => x.Id).FirstOrDefault(),
                        CrAmt = SR.TotalAmount,
                        Particulars = SR.Narration,
                        TransactionMode = "Cash"
                    });
                }
                else if (SR.TransactionTypeId == 2)
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = SR.LedgerId,
                        CrAmt = SR.TotalAmount,
                        Particulars = SR.Narration,
                        TransactionMode = "Credit"
                    });
                }
                else
                {
                    j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                    {
                        LedgerId = dbNew.Banks.FirstOrDefault().LedgerId,
                        CrAmt = SR.TotalAmount,
                        TransactionMode = "Cheque",
                        Particulars = SR.Narration,
                        ChequeDate = SR.ChequeDate,
                        ChequeNo = SR.ChequeNo,
                        Status = "Process"

                    });
                }
                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Sales Return A/C").Select(x => x.Id).FirstOrDefault(),

                    DrAmt = SR.ItemAmount - SR.DiscountAmount + SR.ExtraAmount,
                    Particulars = SR.Narration,
                    TransactionMode = Mode,
                    ChequeDate = SR.ChequeDate,
                    ChequeNo = SR.ChequeNo,
                    Status = status

                });

                j.JournalDetails.Add(new DALNewFMCG.JournalDetail()
                {
                    LedgerId = dbNew.Ledgers.Where(x => x.LedgerName == "Output Tax").Select(x => x.Id).FirstOrDefault(),

                    DrAmt = SR.GSTAmount,
                    Particulars = SR.Narration,
                    TransactionMode = Mode,
                    ChequeDate = SR.ChequeDate,
                    ChequeNo = SR.ChequeNo,
                    Status = status
                });


            }
        #endregion

            #endregion
        private string GetPurchaseRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Purchase, dt);
            long No = 0;

            var d = lstPurchaseNew.Where(x => x.RefNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.RefNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.RefNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        private string GetSaleRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Sales, dt);
            long No = 0;

            var d = lstSaleNew.Where(x => x.RefNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.RefNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.RefNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        private string GetPurchaseReturnRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.PurchaseReturn, dt);
            long No = 0;

            var d = lstPurchaseReturnNew.Where(x => x.RefNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.RefNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.RefNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        private string GetSaleReturnRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.SalesReturn, dt);
            long No = 0;

            var d = lstSaleReturnNew.Where(x => x.RefNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.RefNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.RefNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }
        private string GetJournalEntryNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Journal, dt);
            long No = 0;

            var d = lstJournalNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        private string GetPaymentEntryNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Payment, dt);
            long No = 0;

            var d = lstPaymentNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        private string GetReceiptEntryNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Receipt, dt);
            long No = 0;

            var d = lstReceiptNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d4}", Prefix, No + 1);
        }

        void WriteDataKey(DALNewFMCG.CompanyDetail cm)
        {
            var ut = cm.UserTypes.FirstOrDefault();
            cm.DataKeyValues.Add(new DALNewFMCG.DataKeyValue() { DataKey = ut.TypeOfUser, DataValue = ut.Id });
            foreach (var ag in cm.AccountGroups)
            {
                cm.DataKeyValues.Add(new DALNewFMCG.DataKeyValue() { DataKey = ag.GroupName, DataValue = ag.Id });
            }
            dbNew.SaveChanges();
        }

        public void WriteLog(String str)
        {
            using (StreamWriter writer = new StreamWriter(System.IO.Path.GetTempPath() + "ABDC_log.txt", true))
            {
                writer.WriteLine(string.Format("{0:dd/MM/yyyy hh:mm:ss} => {1}", DateTime.Now, str));
            }
            dtEnd = DateTime.Now;
            TimeSpan ts = dtEnd - dtStart;
            lblStatus.Text = string.Format("Start Time : {0:hh:mm:ss}, End Time : {1:hh:mm:ss}, Work on Mins : {2}\r\nMessage : {3}", dtStart, dtEnd, ts.TotalMinutes, str);
            DoEvents();
        }

        public int GetLedgerId(string LedgerCode)
        {
            return lstLedgerIds.Where(x => x.DataKey == LedgerCode).FirstOrDefault().DataValue;
        }

        public int GetProductId(string ProductCode)
        {
            return lstProductIds.Where(x => x.DataKey == ProductCode).FirstOrDefault().DataValue;
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        enum LogDetailType
        {
            INSERT,
            UPDATE,
            DELETE
        }
        private List<DALNewFMCG.EntityType> EntityTypeList
        {
            get
            {
                if (_entityTypeList == null)
                {
                    _entityTypeList = dbNew.EntityTypes.ToList();
                }
                return _entityTypeList;
            }
            set
            {
                _entityTypeList = value;
            }
        }
        private List<DALNewFMCG.LogDetailType> LogDetailTypeList
        {
            get
            {
                if (_logDetailTypeList == null) _logDetailTypeList = dbNew.LogDetailTypes.ToList();
                return _logDetailTypeList;
            }
            set
            {
                _logDetailTypeList = value;
            }
        }
        private DALNewFMCG.EntityType EntityType(string Type)
        {
            DALNewFMCG.EntityType et = EntityTypeList.Where(x => x.Entity == Type).FirstOrDefault();
            if (et == null)
            {
                et = new DALNewFMCG.EntityType();
                dbNew.EntityTypes.Add(et);
                EntityTypeList.Add(et);
                et.Entity = Type;
            }
            return et;
        }

        private int LogDetailTypeId(LogDetailType Type)
        {
            DALNewFMCG.LogDetailType ldt = LogDetailTypeList.Where(x => x.Type == Type.ToString()).FirstOrDefault();
            return ldt.Id;
        }

        private void LogDetailStore(object Data, int userId)
        {
            try
            {
                Type t = Data.GetType();
                long EntityId = Convert.ToInt64(t.GetProperty("Id").GetValue(Data));

                DALNewFMCG.LogMaster l = new DALNewFMCG.LogMaster();
                DALNewFMCG.LogDetail ld = new DALNewFMCG.LogDetail();
                DateTime dt = DateTime.Now;
                dbNew.LogMasters.Add(l);
                l.EntityId = EntityId;
                l.EntityType = EntityType(t.Name);
                l.CreatedAt = dt;
                l.CreatedBy = userId;
                l.LogDetails.Add(ld);

                ld.RecordDetail = Newtonsoft.Json.JsonConvert.SerializeObject(Data); //new JavaScriptSerializer().Serialize(Data);
                ld.EntryBy = userId;
                ld.EntryAt = dt;
                ld.LogDetailTypeId = LogDetailTypeId(LogDetailType.INSERT);
                WriteLog(string.Format("Name={0},Id={1}", t.Name, EntityId));
            }
            catch (Exception ex) { }
        }
    }
}
