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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Web.Script.Serialization;

namespace ABDC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DALOld.nubebfsv1Entities dbOld = new DALOld.nubebfsv1Entities();
        DALNew.nube_newEntities dbNew = new DALNew.nube_newEntities();
        DateTime dtStart, dtEnd;
        private List<DALNew.EntityType> _entityTypeList;
        private  List<DALNew.LogDetailType> _logDetailTypeList;

        List<DALOld.AccountGroup> lstAccountGroup = new List<DALOld.AccountGroup>();
        List<DALOld.Ledger> lstLedger = new List<DALOld.Ledger>();
        List<DALOld.PaymentMaster> lstPayment = new List<DALOld.PaymentMaster>();
        List<DALOld.ReceiptMaster> lstReceipt = new List<DALOld.ReceiptMaster>();
        List<DALOld.JournalMaster> lstJournal = new List<DALOld.JournalMaster>();

        List<DALNew.Ledger> lstLedgerNew = new List<DALNew.Ledger>();
        List<DALNew.Payment> lstPaymentNew = new List<DALNew.Payment>();
        List<DALNew.Receipt> lstReceiptNew = new List<DALNew.Receipt>();
        List<DALNew.Journal> lstJournalNew = new List<DALNew.Journal>();
        public MainWindow()
        {
            InitializeComponent();
            lstAccountGroup = dbOld.AccountGroups.ToList();
            lstLedger = dbOld.Ledgers.ToList();
            lstPayment = dbOld.PaymentMasters.ToList();
            lstReceipt = dbOld.ReceiptMasters.ToList();
            lstJournal = dbOld.JournalMasters.ToList();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            dtStart = DateTime.Now;
            WriteLog("ABDC Start");

            try
            {                
                WriteLog("Fetching Fund List");
                var lstFund = dbOld.ViewLedgerGroups.Select(x => x.Fund).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
                var lstUserTypeFormDetail = dbNew.UserTypeFormDetails.ToList();
                pbrFund.Maximum = lstFund.Count();
                pbrFund.Value = 0;
                WriteLog("Creating Company from Fund List");
                foreach (var f in lstFund)
                {
                    pbrPayment.Value = 0;
                    pbrReceipt.Value = 0;
                    pbrJournal.Value = 0;

                    DALNew.FundMaster fm = new DALNew.FundMaster() { FundName = f, IsActive = true };
                    DALNew.ACYearMaster acym = new DALNew.ACYearMaster() {ACYear="2016 - 2017", ACYearStatusId=1 };
                   

                    fm.ACYearMasters.Add(acym);
                    dbNew.FundMasters.Add(fm);
                    dbNew.SaveChanges();

                    pbrFund.Value += 1;
                    DALNew.UserType ut = new DALNew.UserType() { TypeOfUser= DataKeyValue.Administrator_Key };
                    fm.UserTypes.Add(ut);                    
                    
                    foreach(var utfd in lstUserTypeFormDetail)
                    {
                        var ud = new DALNew.UserTypeDetail() {
                            UserTypeFormDetailId = utfd.Id,
                            IsViewForm = true,
                            AllowInsert = true,
                            AllowUpdate = true,
                            AllowDelete = true
                        };
                        ut.UserTypeDetails.Add(ud);                                            
                    }

                    DALNew.UserAccount ua = new DALNew.UserAccount() { LoginId = "Admin", UserName = "Admin", Password = "Admin" };
                    ut.UserAccounts.Add(ua);                    
                    WriteLog(string.Format("Stored User Account : {0}, Id : {1}", ua.UserName, ua.Id));

                    DALNew.CustomFormat cf = new DALNew.CustomFormat()
                    {
                        CurrencyNegativeSymbolPrefix = "[RM] ",
                        CurrencyPositiveSymbolPrefix = "RM ",
                        CurrencyToWordPrefix = "Ringgit Malaysia ", 
                        DecimalToWordPrefix="Cents ", 
                        DecimalSymbol=".",
                        DigitGroupingSymbol=",",
                        IsDisplayWithOnlyOnSuffix=true,
                        NoOfDigitAfterDecimal=2,
                        FundMasterId=fm.Id                                         
                    };
                    dbNew.CustomFormats.Add(cf);
                    WriteAccountGroup(fm,1,null,acym); dbNew.SaveChanges();

                    lstLedgerNew = dbNew.Ledgers.Where(x => x.AccountGroup.FundMasterId == fm.Id).ToList();
                    lstPaymentNew = new List<DALNew.Payment>();
                    lstReceiptNew = new List<DALNew.Receipt>();
                    lstJournalNew = new List<DALNew.Journal>();

                    WritePayment(fm);
                    dbNew.Payments.AddRange(lstPaymentNew);
                    dbNew.SaveChanges();

                    WriteReceipt(fm);
                    dbNew.Receipts.AddRange(lstReceiptNew);
                    dbNew.SaveChanges();

                    WriteJournal(fm);
                    dbNew.Journals.AddRange(lstJournalNew);
                    dbNew.SaveChanges();

                    WriteDataKey(fm);
                    WriteLogData(fm);
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
            }
            
            WriteLog("ABDC End");
            MessageBox.Show("Finished");            
        }

        void WriteAccountGroup(DALNew.FundMaster fm,decimal AGId,DALNew.AccountGroup UAG,DALNew.ACYearMaster acym)
        {
            WriteLog("Start to store the Accounts Group");
            
            foreach(var ag in lstAccountGroup.Where(x => x.Under == AGId && x.AccountGroupId != AGId).ToList())
            {
                DALNew.AccountGroup d = new DALNew.AccountGroup() {
                    GroupName = ag.GroupName,
                    GroupCode = ag.GroupCode,
                    AccountGroup2=UAG                                       
                };
                fm.AccountGroups.Add(d);
                
                WriteLog(string.Format("Stored Account Group : {0}", d.GroupName));

                foreach (var l in lstLedger.Where(x => x.AccountGroupId == ag.AccountGroupId).ToList())
                {

                    DALOld.LedgerOP lop = l.LedgerOPs.Where(x => x.Fund == fm.FundName).FirstOrDefault();
                    if (lop == null) lop = new DALOld.LedgerOP();

                    DALNew.Ledger dl = new DALNew.Ledger()
                    {
                        LedgerName = l.LedgerName,
                        LedgerCode = l.AccountCode                       
                    };

                    d.Ledgers.Add(dl);

                    decimal OPDr = Convert.ToDecimal(lop.DrAmt);
                    decimal OPCr = Convert.ToDecimal(lop.CrAmt);
                    if (OPDr != 0 || OPCr != 0)
                    {
                        var acylb = new DALNew.ACYearLedgerBalance()
                        {
                            DrAmt = OPDr,
                            CrAmt = OPCr,
                            ACYearMaster = acym
                        };
                        dl.ACYearLedgerBalances.Add(acylb);
                    }

                    WriteLog(string.Format("Stored Ledger : {0}, Id : {1}", dl.LedgerName, dl.Id));
                }

                WriteAccountGroup(fm, ag.AccountGroupId, d, acym);

            }
            WriteLog("End to store the Accounts Group");            
        }

        void WritePayment(DALNew.FundMaster fm)
        {
            WriteLog("Start to store the Payment");
            try
            {
                
                var l1 = lstPayment.Where(x => x.Fund == fm.FundName && x.PaymentDate>=new DateTime(2016,4,1)).ToList();
                pbrPayment.Maximum = l1.Count();
                pbrPayment.Value = 0;                
                foreach (var p in l1)
                {
                    try
                    {
                        
                        DALNew.Payment pm = new DALNew.Payment()
                        {
                            LedgerId =  GetLedgerId(p.Ledger.LedgerName),
                            Amount = Convert.ToDecimal(p.PayAmount),
                            ChequeDate = p.chequeDate,
                            ChequeNo = p.ChequeNo,
                            ClearDate = p.ClearDate,
                            VoucherNo = p.VoucherNo,
                            EntryNo = Payment_NewRefNo(p.PaymentDate.Value),
                            ExtraCharge = Convert.ToDecimal(p.ExtraCharge),
                            Particulars = p.Narration,
                            PaymentDate = p.PaymentDate.Value,
                            PaymentMode = p.PaymentMode,
                            PayTo = p.PayTo,
                            Status = p.Status,
                            RefNo = p.RefNo,
                            RefCode = p.PaymentId.ToString()
                        };

                        foreach (var pd in p.PaymentDetails)
                        {
                            DALNew.PaymentDetail pmd = new DALNew.PaymentDetail()
                            {
                                LedgerId = GetLedgerId(pd.Ledger.LedgerName),
                                Amount = Convert.ToDecimal(pd.Amount),
                                Particular = pd.Narration,
                            };
                            pm.PaymentDetails.Add(pmd);
                          }

                        lstPaymentNew.Add(pm);
                        
                        WriteLog(string.Format("Stored Payment => Date : {0}, Entry No : {1}, Voucher No : {2}", pm.PaymentDate, pm.EntryNo, pm.VoucherNo));
                        pbrPayment.Value += 1;                        
                    }
                    catch(Exception ex)
                    {
                        WriteLog(string.Format("Error on Stored Payment => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", p.PaymentDate, p.EntryNo, p.VoucherNo,ex.Message));
                    }                    
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error on Payment: {0}",ex.Message));
            }
            WriteLog("End to Store the Payment");
        }

        void WriteReceipt(DALNew.FundMaster cm)
        {
            WriteLog("Start to store the Receipt");
            try
            {
                var l1 = lstReceipt.Where(x => x.Fund == cm.FundName &&  x.ReceiptDate >= new DateTime(2016, 4, 1)).ToList();
                pbrReceipt.Maximum = l1.Count();
                pbrReceipt.Value = 0;
                foreach (var r in l1)
                {

                    try
                    {
                        DALNew.Receipt rm = new DALNew.Receipt()
                        {
                            LedgerId = GetLedgerId(r.Ledger.LedgerName),
                            Amount = Convert.ToDecimal(r.ReceiptAmount),
                            ChequeDate = r.ChequeDate,
                            ChequeNo = r.ChequeNo,
                            CleareDate = r.ClrDate,
                            VoucherNo = r.VoucherNo,
                            EntryNo = Receipt_NewRefNo(r.ReceiptDate.Value),
                            Extracharge = Convert.ToDecimal(r.ExtraCharge),
                            Particulars = r.Narration,
                            ReceiptDate = r.ReceiptDate.Value,
                            ReceiptMode = r.ReceiptMode,
                            ReceivedFrom = r.ReceivedFrom,
                            Status = r.Status,
                            RefNo = r.RefNo,
                            RefCode = r.ReceiptId.ToString()
                        };

                        foreach (var rd in r.ReceiptDetails)
                        {
                            DALNew.ReceiptDetail pmd = new DALNew.ReceiptDetail()
                            {
                                LedgerId = GetLedgerId(rd.Ledger.LedgerName),
                                Amount = Convert.ToDecimal(rd.Amount),
                                Particulars = rd.Narration,
                            };
                            rm.ReceiptDetails.Add(pmd);
                        }

                        lstReceiptNew.Add(rm);
                        WriteLog(string.Format("Stored Receipt => Date : {0}, Entry No : {1}, Voucher No : {2}", rm.ReceiptDate, rm.EntryNo, rm.VoucherNo));
                        pbrReceipt.Value += 1;                        
                    }
                    catch(Exception ex)
                    {
                        WriteLog(string.Format("Error on Stored Receipt => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", r.ReceiptDate, r.EntryNo, r.VoucherNo, ex.Message));
                    }
                    
                }
            }
            catch(Exception ex)
            {
                WriteLog(string.Format("Error on Receipt: {0}", ex.Message));
            }
            
            WriteLog("End to Store the Receipt");
        }

        void WriteJournal(DALNew.FundMaster cm)
        {
            WriteLog("Start to store the Journal");
            try
            {
                var l1 = lstJournal.Where(x => x.Fund == cm.FundName && x.JournalDate >= new DateTime(2016, 4, 1)).ToList();
                pbrJournal.Maximum = l1.Count();
                pbrJournal.Value = 0;
                foreach (var j in l1)
                {
                    try
                    {
                        DALNew.Journal jm = new DALNew.Journal()
                        {
                            VoucherNo = j.VoucherNo,
                            EntryNo = Journal_NewRefNo(j.JournalDate.Value),
                            HQNo = j.HQNo,
                            JournalDate = j.JournalDate.Value,
                            Status = j.Status,
                            RefCode = j.JournalId.ToString()
                        };

                        foreach (var jd in j.JournalDetails)
                        {
                            DALNew.JournalDetail pmd = new DALNew.JournalDetail()
                            {
                                LedgerId = GetLedgerId(jd.Ledger.LedgerName),
                                CrAmt = Convert.ToDecimal(jd.CrAmt),
                                DrAmt = Convert.ToDecimal(jd.DrAmt),
                                Particulars = jd.Narration,
                            };
                            jm.JournalDetails.Add(pmd);
                        }

                        lstJournalNew.Add(jm);
                        
                        WriteLog(string.Format("Stored Journal => Date : {0}, Entry No : {1}, Voucher No : {2}", jm.JournalDate, jm.EntryNo, jm.VoucherNo));
                        pbrJournal.Value += 1;                        
                    }
                    catch(Exception ex)
                    {
                        WriteLog(string.Format("Error Stored Journal => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", j.JournalDate, j.EntryNo, j.VoucherNo,ex.Message));
                    }
                    
                }
            }
            catch(Exception ex)
            {
                WriteLog(string.Format("Error on Journal: {0}", ex.Message));
            }
            
            WriteLog("End to Store the Journal");
        }

        int GetLedgerId(string LedgerName)
        {
            return lstLedgerNew.Where(x => x.LedgerName == LedgerName).Select(x => x.Id).FirstOrDefault();
        }

        public string Payment_NewRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Payment, dt);
            long No = 0;

            var d = lstPaymentNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d3}", Prefix, No + 1);
        }

        public string Receipt_NewRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Receipt, dt);
            long No = 0;

            var d = lstReceiptNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d3}", Prefix, No + 1);
        }

        public string Journal_NewRefNo(DateTime dt)
        {
            string Prefix = string.Format("{0}{1:yyMM}", FormPrefix.Journal, dt);
            long No = 0;

            var d = lstJournalNew.Where(x => x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 10);

            return string.Format("{0}{1:d3}", Prefix, No + 1);
        }

        void WriteDataKey(DALNew.FundMaster fm)
        {            
            var ut = fm.UserTypes.FirstOrDefault();
            fm.DataKeyValues.Add(new DALNew.DataKeyValue() { DataKey=ut.TypeOfUser,DataValue=ut.Id });
            foreach(var ag in fm.AccountGroups)
            {
                fm.DataKeyValues.Add(new DALNew.DataKeyValue() { DataKey = ag.GroupName, DataValue = ag.Id });
            }
            dbNew.SaveChanges();
        }
        void WriteLogData(DALNew.FundMaster fm)
        {
            var ua = fm.UserTypes.FirstOrDefault().UserAccounts.FirstOrDefault();
            var ua1 = new DALNew.FundMaster() { Id = fm.Id, FundName = fm.FundName, IsActive = fm.IsActive };

            var cf = fm.CustomFormats.FirstOrDefault();
            var cf1 = new DALNew.CustomFormat() { Id = cf.Id, CurrencyCaseSensitive = cf.CurrencyCaseSensitive, CurrencyNegativeSymbolPrefix = cf.CurrencyNegativeSymbolPrefix, CurrencyNegativeSymbolSuffix = cf.CurrencyNegativeSymbolSuffix, CurrencyPositiveSymbolPrefix = cf.CurrencyPositiveSymbolPrefix, CurrencyPositiveSymbolSuffix = cf.CurrencyPositiveSymbolSuffix, CurrencyToWordPrefix = cf.CurrencyToWordPrefix, CurrencyToWordSuffix = cf.CurrencyToWordSuffix, DecimalSymbol = cf.DecimalSymbol, DecimalToWordPrefix = cf.DecimalToWordPrefix, DecimalToWordSuffix = cf.DecimalToWordSuffix, DigitGroupingBy = cf.DigitGroupingBy, DigitGroupingSymbol = cf.DigitGroupingSymbol, FundMasterId = cf.FundMasterId, IsDisplayWithOnlyOnSuffix = cf.IsDisplayWithOnlyOnSuffix, NoOfDigitAfterDecimal = cf.NoOfDigitAfterDecimal };

            var ut = fm.UserTypes.FirstOrDefault();           
            var ut1 = new DALNew.UserType() { Id = ut.Id, FundMasterId = ut.FundMasterId, TypeOfUser = ut.TypeOfUser, Description = ut.Description };
            foreach (var utd in ut.UserTypeDetails)
            {
                ut1.UserTypeDetails.Add(new DALNew.UserTypeDetail() { Id = utd.Id, UserTypeId = utd.UserTypeId, UserTypeFormDetailId = utd.UserTypeFormDetailId, IsViewForm = utd.IsViewForm, AllowInsert = utd.AllowInsert, AllowUpdate = utd.AllowUpdate, AllowDelete = utd.AllowDelete });
            }

            var acym = fm.ACYearMasters.FirstOrDefault();
            var acym1 = new DALNew.ACYearMaster() { Id = acym.Id, ACYear = acym.ACYear, ACYearStatusId = acym.ACYearStatusId, FundMasterId = acym.FundMasterId };
            foreach(var acymlb in acym.ACYearLedgerBalances)
            {
                acym1.ACYearLedgerBalances.Add(new DALNew.ACYearLedgerBalance() {Id=acymlb.Id, ACYearMasterId=acym.Id, LedgerId = acymlb.LedgerId, DrAmt= acymlb.DrAmt, CrAmt=acymlb.CrAmt  });
            }

            var fm1 = new DALNew.FundMaster() { Id = fm.Id, FundName = fm.FundName, IsActive = fm.IsActive };

            LogDetailStore(fm1, ua.Id);
            LogDetailStore(cf1, ua.Id);
            LogDetailStore(ut1, ua.Id);
            LogDetailStore(ua1, ua.Id);
            LogDetailStore(acym1, ua.Id);
            


            dbNew.SaveChanges();
        }

        public  void WriteLog(String str)
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
        private  List<DALNew.EntityType> EntityTypeList
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
        private  List<DALNew.LogDetailType> LogDetailTypeList
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
        private DALNew.EntityType EntityType(string Type)
        {
            DALNew.EntityType et = EntityTypeList.Where(x => x.Entity == Type).FirstOrDefault();
            if (et == null)
            {
                et = new DALNew.EntityType();
                dbNew.EntityTypes.Add(et);
                EntityTypeList.Add(et);
                et.Entity = Type;
            }
            return et;
        }

        private int LogDetailTypeId(LogDetailType Type)
        {
            DALNew.LogDetailType ldt = LogDetailTypeList.Where(x => x.Type == Type.ToString()).FirstOrDefault();
            return ldt.Id;
        }

        private void LogDetailStore(object Data,int userId)
        {
            try
            {
                Type t = Data.GetType();
                long EntityId = Convert.ToInt64(t.GetProperty("Id").GetValue(Data));

                DALNew.LogMaster l = new DALNew.LogMaster();
                DALNew.LogDetail ld = new DALNew.LogDetail();
                DateTime dt = DateTime.Now;
                dbNew.LogMasters.Add(l);
                l.EntityId = EntityId;
                l.EntityType = EntityType(t.Name);
                l.CreatedAt = dt;
                l.CreatedBy = userId;
                l.LogDetails.Add(ld);
                
                ld.RecordDetail = new JavaScriptSerializer().Serialize(Data);
                ld.EntryBy =userId;
                ld.EntryAt = dt;
                ld.LogDetailTypeId = LogDetailTypeId(LogDetailType.INSERT);
            }
            catch (Exception ex) { }
        }
    }
}
