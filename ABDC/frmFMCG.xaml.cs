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

namespace ABDC
{
    
    public partial class frmFMCG : Window
    {
        DALOldFMCG.FMCG_Old01Entities dbOld = new DALOldFMCG.FMCG_Old01Entities();
        DALNewFMCG.FMCG_New01Entities dbNew = new DALNewFMCG.FMCG_New01Entities();

        DateTime dtStart, dtEnd;
        private List<DALNewFMCG.EntityType> _entityTypeList;
        private List<DALNewFMCG.LogDetailType> _logDetailTypeList;
        
        int UOMId = 0;

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
                var lstCompany = dbOld.CompanyDetails.Where(x => x.CompanyId>1).ToList();
                var lstUserTypeFormDetail = dbNew.UserTypeFormDetails.ToList();
                pbrCompany.Maximum = lstCompany.Count();
                pbrCompany.Value = 0;
                WriteLog("Creating Company");
                foreach (var c in lstCompany)
                {
                    pbrPayment.Value = 0;
                    pbrReceipt.Value = 0;
                    pbrJournal.Value = 0;

                    DALNewFMCG.CompanyDetail cm = new DALNewFMCG.CompanyDetail() { CompanyName = c.CompanyName, AddressLine1=c.Address, GSTNo=c.Cst, EMailId=c.MailId,MobileNo=c.MobileNo, TelephoneNo=c.PhNo, CompanyType="Company", IsActive = true };
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
            UOMId = dbNew.UOMs.FirstOrDefault().Id;
            WriteAccountGroup(cm, "ACG0001", null); dbNew.SaveChanges();
            WriteStockGroup(cm, "STG001", null); dbNew.SaveChanges();

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
                if(d.GroupName== "Sundry Debtors")
                {
                    foreach(var c in dbOld.Customers.ToList())
                    {
                        DALNewFMCG.Ledger l = new DALNewFMCG.Ledger() {
                            LedgerName=c.LedgerName,
                            PersonIncharge=c.CustomerName,                            
                            AddressLine1=c.AddressLine,
                            TelephoneNo=c.TelePhoneNo,
                            MobileNo=c.MobileNo,
                            EMailId=c.EMailId,
                            GSTNo=c.TinNo,                           
                        };
                        d.Ledgers.Add(l);
                        dbNew.Customers.Add(new DALNewFMCG.Customer() {Ledger=l });
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
                        dbNew.Banks.Add(new DALNewFMCG.Bank() { Ledger = l,BankAccountName=b.BankName });
                    }
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
                foreach(var pt in dbOld.Products.ToList().Where(x=> x.GroupCode==ag.StockGroupCode || (ag.GroupName== "OTHERS" && (String.IsNullOrEmpty(x.GroupCode) || string.IsNullOrWhiteSpace(x.GroupCode)))).ToList())
                {
                    d.Products.Add(new DALNewFMCG.Product() {
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

        

        //void WritePayment(DALNewFMCG.CompanyDetail cm)
        //{
        //    WriteLog("Start to store the Payment");
        //    try
        //    {

        //       pbrPayment.Maximum = lstPayment.Count();
        //        pbrPayment.Value = 0;
        //        foreach (var p in lstPayment)
        //        {
        //            try
        //            {

        //                DALNewFMCG.Payment pm = new DALNewFMCG.Payment()
        //                {
        //                    LedgerId = GetLedgerId(p.l),
        //                    Amount = Convert.ToDecimal(p.PayAmount),
        //                    ChequeDate = p.chequeDate,
        //                    ChequeNo = p.ChequeNo,
        //                    ClearDate = p.ClearDate,
        //                    VoucherNo = p.VoucherNo,
        //                    EntryNo = Payment_NewRefNo(p.PaymentDate.Value),
        //                    ExtraCharge = Convert.ToDecimal(p.ExtraCharge),
        //                    Particulars = p.Narration,
        //                    PaymentDate = p.PaymentDate.Value,
        //                    PaymentMode = p.PaymentMode,
        //                    PayTo = p.PayTo,
        //                    Status = p.Status,
        //                    RefNo = p.RefNo,
        //                    RefCode = p.PaymentId.ToString()
        //                };

        //                foreach (var pd in p.PaymentDetails)
        //                {
        //                    DALNewFMCG.PaymentDetail pmd = new DALNewFMCG.PaymentDetail()
        //                    {
        //                        LedgerId = GetLedgerId(pd.Ledger.LedgerName),
        //                        Amount = Convert.ToDecimal(pd.Amount),
        //                        Particular = pd.Narration,
        //                    };
        //                    pm.PaymentDetails.Add(pmd);
        //                }

        //                lstPaymentNew.Add(pm);

        //                WriteLog(string.Format("Stored Payment => Date : {0}, Entry No : {1}, Voucher No : {2}", pm.PaymentDate, pm.EntryNo, pm.VoucherNo));
        //                pbrPayment.Value += 1;
        //            }
        //            catch (Exception ex)
        //            {
        //                WriteLog(string.Format("Error on Stored Payment => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", p.PaymentDate, p.EntryNo, p.VoucherNo, ex.Message));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(string.Format("Error on Payment: {0}", ex.Message));
        //    }
        //    WriteLog("End to Store the Payment");
        //}

        //void WriteReceipt(DALNewFMCG.CompanyDetail cm)
        //{
        //    WriteLog("Start to store the Receipt");
        //    try
        //    {
        //        var l1 = lstReceipt.Where(x => x.Fund == cm.FundName && x.ReceiptDate >= new DateTime(2016, 4, 1)).ToList();
        //        pbrReceipt.Maximum = l1.Count();
        //        pbrReceipt.Value = 0;
        //        foreach (var r in l1)
        //        {

        //            try
        //            {
        //                DALNewFMCG.Receipt rm = new DALNewFMCG.Receipt()
        //                {
        //                    LedgerId = GetLedgerId(r.Ledger.LedgerName),
        //                    Amount = Convert.ToDecimal(r.ReceiptAmount),
        //                    ChequeDate = r.ChequeDate,
        //                    ChequeNo = r.ChequeNo,
        //                    CleareDate = r.ClrDate,
        //                    VoucherNo = r.VoucherNo,
        //                    EntryNo = Receipt_NewRefNo(r.ReceiptDate.Value),
        //                    Extracharge = Convert.ToDecimal(r.ExtraCharge),
        //                    Particulars = r.Narration,
        //                    ReceiptDate = r.ReceiptDate.Value,
        //                    ReceiptMode = r.ReceiptMode,
        //                    ReceivedFrom = r.ReceivedFrom,
        //                    Status = r.Status,
        //                    RefNo = r.RefNo,
        //                    RefCode = r.ReceiptId.ToString()
        //                };

        //                foreach (var rd in r.ReceiptDetails)
        //                {
        //                    DALNewFMCG.ReceiptDetail pmd = new DALNewFMCG.ReceiptDetail()
        //                    {
        //                        LedgerId = GetLedgerId(rd.Ledger.LedgerName),
        //                        Amount = Convert.ToDecimal(rd.Amount),
        //                        Particulars = rd.Narration,
        //                    };
        //                    rm.ReceiptDetails.Add(pmd);
        //                }

        //                lstReceiptNew.Add(rm);
        //                WriteLog(string.Format("Stored Receipt => Date : {0}, Entry No : {1}, Voucher No : {2}", rm.ReceiptDate, rm.EntryNo, rm.VoucherNo));
        //                pbrReceipt.Value += 1;
        //            }
        //            catch (Exception ex)
        //            {
        //                WriteLog(string.Format("Error on Stored Receipt => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", r.ReceiptDate, r.EntryNo, r.VoucherNo, ex.Message));
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(string.Format("Error on Receipt: {0}", ex.Message));
        //    }

        //    WriteLog("End to Store the Receipt");
        //}

        //void WriteJournal(DALNewFMCG.CompanyDetail cm)
        //{
        //    WriteLog("Start to store the Journal");
        //    try
        //    {
        //        var l1 = lstJournal.Where(x => x.Fund == cm.FundName && x.JournalDate >= new DateTime(2016, 4, 1)).ToList();
        //        pbrJournal.Maximum = l1.Count();
        //        pbrJournal.Value = 0;
        //        foreach (var j in l1)
        //        {
        //            try
        //            {
        //                DALNewFMCG.Journal jm = new DALNewFMCG.Journal()
        //                {
        //                    VoucherNo = j.VoucherNo,
        //                    EntryNo = Journal_NewRefNo(j.JournalDate.Value),
        //                    HQNo = j.HQNo,
        //                    JournalDate = j.JournalDate.Value,
        //                    Status = j.Status,
        //                    RefCode = j.JournalId.ToString()
        //                };

        //                foreach (var jd in j.JournalDetails)
        //                {
        //                    DALNewFMCG.JournalDetail pmd = new DALNewFMCG.JournalDetail()
        //                    {
        //                        LedgerId = GetLedgerId(jd.Ledger.LedgerName),
        //                        CrAmt = Convert.ToDecimal(jd.CrAmt),
        //                        DrAmt = Convert.ToDecimal(jd.DrAmt),
        //                        Particulars = jd.Narration,
        //                    };
        //                    jm.JournalDetails.Add(pmd);
        //                }

        //                lstJournalNew.Add(jm);

        //                WriteLog(string.Format("Stored Journal => Date : {0}, Entry No : {1}, Voucher No : {2}", jm.JournalDate, jm.EntryNo, jm.VoucherNo));
        //                pbrJournal.Value += 1;
        //            }
        //            catch (Exception ex)
        //            {
        //                WriteLog(string.Format("Error Stored Journal => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", j.JournalDate, j.EntryNo, j.VoucherNo, ex.Message));
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(string.Format("Error on Journal: {0}", ex.Message));
        //    }

        //    WriteLog("End to Store the Journal");
        //}

        //int GetLedgerId(string LedgerName)
        //{
        //    return lstLedgerNew.Where(x => x.LedgerName == LedgerName).Select(x => x.Id).FirstOrDefault();
        //}

        

        //public string Payment_NewRefNo(DateTime dt)
        //{
        //    string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Payment, dt);
        //    long No = 0;

        //    var d = lstPaymentNew.Where(x => x.EntryNo.StartsWith(Prefix))
        //                             .OrderByDescending(x => x.EntryNo)
        //                             .FirstOrDefault();

        //    if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

        //    return string.Format("{0}{1:d3}", Prefix, No + 1);
        //}

        //public string Receipt_NewRefNo(DateTime dt)
        //{
        //    string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Receipt, dt);
        //    long No = 0;

        //    var d = lstReceiptNew.Where(x => x.EntryNo.StartsWith(Prefix))
        //                             .OrderByDescending(x => x.EntryNo)
        //                             .FirstOrDefault();

        //    if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

        //    return string.Format("{0}{1:d3}", Prefix, No + 1);
        //}

        //public string Journal_NewRefNo(DateTime dt)
        //{
        //    string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Journal, dt);
        //    long No = 0;

        //    var d = lstJournalNew.Where(x => x.EntryNo.StartsWith(Prefix))
        //                             .OrderByDescending(x => x.EntryNo)
        //                             .FirstOrDefault();

        //    if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

        //    return string.Format("{0}{1:d3}", Prefix, No + 1);
        //}

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
        //void WriteLogData(DALNewFMCG.CompanyDetail cm)
        //{
        //    var ua = cm.UserTypes.FirstOrDefault().UserAccounts.FirstOrDefault();
        //    var ua1 = new DALNewFMCG.FundMaster() { Id = cm.Id, FundName = cm.FundName, IsActive = cm.IsActive };

        //    var cf = cm.CustomFormats.FirstOrDefault();
        //    var cf1 = new DALNewFMCG.CustomFormat() { Id = cf.Id, CurrencyCaseSensitive = cf.CurrencyCaseSensitive, CurrencyNegativeSymbolPrefix = cf.CurrencyNegativeSymbolPrefix, CurrencyNegativeSymbolSuffix = cf.CurrencyNegativeSymbolSuffix, CurrencyPositiveSymbolPrefix = cf.CurrencyPositiveSymbolPrefix, CurrencyPositiveSymbolSuffix = cf.CurrencyPositiveSymbolSuffix, CurrencyToWordPrefix = cf.CurrencyToWordPrefix, CurrencyToWordSuffix = cf.CurrencyToWordSuffix, DecimalSymbol = cf.DecimalSymbol, DecimalToWordPrefix = cf.DecimalToWordPrefix, DecimalToWordSuffix = cf.DecimalToWordSuffix, DigitGroupingBy = cf.DigitGroupingBy, DigitGroupingSymbol = cf.DigitGroupingSymbol, FundMasterId = cf.FundMasterId, IsDisplayWithOnlyOnSuffix = cf.IsDisplayWithOnlyOnSuffix, NoOfDigitAfterDecimal = cf.NoOfDigitAfterDecimal };

        //    var ut = cm.UserTypes.FirstOrDefault();
        //    var ut1 = new DALNewFMCG.UserType() { Id = ut.Id, FundMasterId = ut.FundMasterId, TypeOfUser = ut.TypeOfUser, Description = ut.Description };
        //    foreach (var utd in ut.UserTypeDetails)
        //    {
        //        ut1.UserTypeDetails.Add(new DALNewFMCG.UserTypeDetail() { Id = utd.Id, UserTypeId = utd.UserTypeId, UserTypeFormDetailId = utd.UserTypeFormDetailId, IsViewForm = utd.IsViewForm, AllowInsert = utd.AllowInsert, AllowUpdate = utd.AllowUpdate, AllowDelete = utd.AllowDelete });
        //    }

        //    var acym = cm.ACYearMasters.FirstOrDefault();
        //    var acym1 = new DALNewFMCG.ACYearMaster() { Id = acym.Id, ACYear = acym.ACYear, ACYearStatusId = acym.ACYearStatusId, FundMasterId = acym.FundMasterId };
        //    foreach (var acymlb in acym.ACYearLedgerBalances)
        //    {
        //        acym1.ACYearLedgerBalances.Add(new DALNewFMCG.ACYearLedgerBalance() { Id = acymlb.Id, ACYearMasterId = acym.Id, LedgerId = acymlb.LedgerId, DrAmt = acymlb.DrAmt, CrAmt = acymlb.CrAmt });
        //    }

        //    var fm1 = new DALNewFMCG.FundMaster() { Id = cm.Id, FundName = cm.FundName, IsActive = cm.IsActive };

        //    LogDetailStore(fm1, ua.Id);
        //    LogDetailStore(cf1, ua.Id);
        //    LogDetailStore(ut1, ua.Id);
        //    LogDetailStore(ua1, ua.Id);
        //    var x1 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of Master");
        //    LogDetailStore(acym1, ua.Id);
        //    var x2 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of AccountYearMaster");
        //    foreach (var ag in cm.AccountGroups)
        //    {
        //        LogDetailStore(new DALNewFMCG.AccountGroup() { Id = ag.Id, FundMasterId = cm.Id, GroupCode = ag.GroupCode, UnderGroupId = ag.UnderGroupId, GroupName = ag.GroupName }, ua.Id);
        //    }

        //    var x3 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of AccountGroup");
        //    foreach (var ld in lstLedgerNew)
        //    {
        //        LogDetailStore(new DALNewFMCG.Ledger() { Id = ld.Id, AccountGroupId = ld.AccountGroupId, LedgerCode = ld.LedgerCode, LedgerName = ld.LedgerName }, ua.Id);
        //    }
        //    var n0 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of Ledger");
        //    int i = 0;
        //    foreach (var p in lstPaymentNew)
        //    {
        //        var p1 = new DALNewFMCG.Payment() { Id = p.Id, Amount = p.Amount, ChequeDate = p.ChequeDate, ChequeNo = p.ChequeNo, ClearDate = p.ClearDate, EntryNo = p.EntryNo, ExtraCharge = p.ExtraCharge, LedgerId = p.LedgerId, Particulars = p.Particulars, PaymentDate = p.PaymentDate, PaymentMode = p.PaymentMode, PayTo = p.PayTo, RefCode = p.RefCode, RefNo = p.RefNo, Status = p.Status, VoucherNo = p.VoucherNo };
        //        foreach (var pd in p.PaymentDetails)
        //        {
        //            p1.PaymentDetails.Add(new DALNewFMCG.PaymentDetail() { Id = pd.Id, Amount = pd.Amount, LedgerId = pd.LedgerId, Particular = pd.Particular, PaymentId = pd.PaymentId });
        //        }

        //        LogDetailStore(p1, ua.Id);
        //        if (i++ % 1000 == 0) dbNew.SaveChanges();

        //    }
        //    var n1 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of Payment");
        //    foreach (var r in lstReceiptNew)
        //    {
        //        var r1 = new DALNewFMCG.Receipt() { Id = r.Id, Amount = r.Amount, ChequeDate = r.ChequeDate, ChequeNo = r.ChequeNo, CleareDate = r.CleareDate, EntryNo = r.EntryNo, Extracharge = r.Extracharge, LedgerId = r.LedgerId, Particulars = r.Particulars, ReceiptDate = r.ReceiptDate, ReceiptMode = r.ReceiptMode, ReceivedFrom = r.ReceivedFrom, RefCode = r.RefCode, RefNo = r.RefNo, Status = r.Status, VoucherNo = r.VoucherNo };
        //        foreach (var rd in r.ReceiptDetails)
        //        {
        //            r1.ReceiptDetails.Add(new DALNewFMCG.ReceiptDetail() { Id = rd.Id, Amount = rd.Amount, LedgerId = rd.LedgerId, Particulars = rd.Particulars, ReceiptId = rd.ReceiptId });
        //        }
        //        LogDetailStore(r1, ua.Id);

        //        if (i++ % 1000 == 0) dbNew.SaveChanges();
        //    }
        //    var n2 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of Receipt");
        //    foreach (var j in lstJournalNew)
        //    {
        //        var j1 = new DALNewFMCG.Journal() { Id = j.Id, Amount = j.Amount, EntryNo = j.EntryNo, HQNo = j.HQNo, JournalDate = j.JournalDate, Particular = j.Particular, RefCode = j.RefCode, Status = j.Status, VoucherNo = j.VoucherNo };
        //        foreach (var jd in j.JournalDetails)
        //        {
        //            j1.JournalDetails.Add(new DALNewFMCG.JournalDetail() { Id = jd.Id, CrAmt = jd.CrAmt, DrAmt = jd.DrAmt, JournalId = jd.JournalId, LedgerId = jd.LedgerId, Particulars = jd.Particulars });
        //        }
        //        LogDetailStore(j1, ua.Id);
        //        if (i++ % 1000 == 0) dbNew.SaveChanges();
        //    }

        //    var n3 = dbNew.SaveChanges();
        //    WriteLog("Log Data Finished of Journal");
        //}

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
