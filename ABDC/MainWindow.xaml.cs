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
using ABDC.DALNew;
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

        public MainWindow()
        {
            InitializeComponent();
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
                    DataKeyValue.CompanyId = fm.Id;
                    WriteLog(string.Format("Stored Fund : {0}, Id : {1}", f,fm.Id));
                    pbrFund.Value += 1;
                    lblFund.Text = string.Format("Stored Fund : {0}, Id : {1}", f, fm.Id);
                    DALNew.UserType ut = new DALNew.UserType() { TypeOfUser= DataKeyValue.Administrator_Key };
                    fm.UserTypes.Add(ut);
                    dbNew.SaveChanges();
                    WriteLog(string.Format("Stored User Type : {0}, Id : {1}", ut.TypeOfUser, ut.Id));
                    DataKeyValue.Write(ut.TypeOfUser, ut.Id);
                    
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
                        dbNew.SaveChanges();
                        WriteLog(string.Format("Stored User Type Detail : {0}, Id : {1}", utfd.FormName, ud.Id));
                    }

                    DALNew.UserAccount ua = new DALNew.UserAccount() { LoginId = "Admin", UserName = "Admin", Password = "Admin" };
                    ut.UserAccounts.Add(ua);
                    dbNew.SaveChanges();
                    WriteLog(string.Format("Stored User Account : {0}, Id : {1}", ua.UserName, ua.Id));
                    DataKeyValue.UserId = ua.Id;

                    DALNew.CustomFormat cf = new DALNew.CustomFormat()
                    {
                        CurrencyNegativeSymbolPrefix = "[RM] ",
                        CurrencyPositiveSymbolPrefix = "RM ",
                        CurrencyToWordPrefix = "Ringgit Malaysia", 
                        DecimalToWordPrefix="Cents", 
                        DecimalSymbol=".",
                        DigitGroupingSymbol=",",
                        IsDisplayWithOnlyOnSuffix=true,
                        NoOfDigitAfterDecimal=2,
                        FundMasterId=fm.Id                                         
                    };
                    dbNew.CustomFormats.Add(cf);
                    dbNew.SaveChanges();

                    LogDetailStore(new DALNew.FundMaster() {Id= fm.Id, FundName=fm.FundName, IsActive=fm.IsActive }, LogDetailType.INSERT,DataKeyValue.UserId);
                    LogDetailStore(new DALNew.ACYearMaster() { Id = acym.Id, ACYear=acym.ACYear, ACYearStatusId=acym.ACYearStatusId, FundMasterId=acym.FundMasterId}, LogDetailType.INSERT, DataKeyValue.UserId);
                    LogDetailStore(new DALNew.UserType() { Id = ut.Id, FundMasterId=ut.FundMasterId, TypeOfUser=ut.TypeOfUser, Description=ut.Description }, LogDetailType.INSERT, DataKeyValue.UserId);
                    foreach(var utd in ut.UserTypeDetails)
                    {
                        LogDetailStore(new DALNew.UserTypeDetail() {Id=utd.Id, UserTypeId=utd.UserTypeId, UserTypeFormDetailId=utd.UserTypeFormDetailId, IsViewForm=utd.IsViewForm, AllowInsert=utd.AllowInsert,AllowUpdate=utd.AllowUpdate,AllowDelete=utd.AllowDelete }, LogDetailType.INSERT, DataKeyValue.UserId);
                    }
                    LogDetailStore(new DALNew.UserAccount() { Id = ua.Id,  UserTypeId=ua.UserTypeId, LoginId=ua.LoginId, Password=ua.Password, UserName=ua.UserName}, LogDetailType.INSERT, DataKeyValue.UserId);
                    LogDetailStore(new DALNew.CustomFormat() { Id = cf.Id, CurrencyCaseSensitive=cf.CurrencyCaseSensitive, CurrencyNegativeSymbolPrefix=cf.CurrencyNegativeSymbolPrefix, CurrencyNegativeSymbolSuffix=cf.CurrencyNegativeSymbolSuffix, CurrencyPositiveSymbolPrefix=cf.CurrencyPositiveSymbolPrefix, CurrencyPositiveSymbolSuffix=cf.CurrencyPositiveSymbolSuffix, CurrencyToWordPrefix=cf.CurrencyToWordPrefix, CurrencyToWordSuffix=cf.CurrencyToWordSuffix, DecimalSymbol=cf.DecimalSymbol, DecimalToWordPrefix = cf.DecimalToWordPrefix, DecimalToWordSuffix=cf.DecimalToWordSuffix, DigitGroupingBy=cf.DigitGroupingBy, DigitGroupingSymbol=cf.DigitGroupingSymbol, FundMasterId=cf.FundMasterId, IsDisplayWithOnlyOnSuffix = cf.IsDisplayWithOnlyOnSuffix, NoOfDigitAfterDecimal= cf.NoOfDigitAfterDecimal}, LogDetailType.INSERT, DataKeyValue.UserId);


                    WriteAccountGroup(fm,1,null,acym);
                    WritePayment(fm);
                    WriteReceipt(fm);
                    WriteJournal(fm);
         
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
            }
            
            WriteLog("ABDC End");
            MessageBox.Show("Finished");            
        }

        void WriteAccountGroup(DALNew.FundMaster cm,decimal AGId,int? UAGId,DALNew.ACYearMaster acym)
        {
            WriteLog("Start to store the Accounts Group");

            var lstAccountsGroup = dbOld.AccountGroups.Where(x=> x.Under==AGId && x.AccountGroupId!=AGId).ToList();
            foreach(var ag in lstAccountsGroup)
            {
                DALNew.AccountGroup d = new DALNew.AccountGroup() {
                    GroupName = ag.GroupName,
                    GroupCode = ag.GroupCode,
                    FundMasterId = cm.Id,
                    UnderGroupId = UAGId                   
                };
                dbNew.AccountGroups.Add(d);
                dbNew.SaveChanges();
                LogDetailStore(new DALNew.AccountGroup() {Id = d.Id, GroupCode = d.GroupCode, GroupName = d.GroupName, FundMasterId=d.FundMasterId, UnderGroupId=d.UnderGroupId }, LogDetailType.INSERT, DataKeyValue.UserId);

                DataKeyValue.Write(d.GroupName, d.Id);
                WriteLog(string.Format("Stored Account Group : {0}, Id : {1}", d.GroupName, d.Id));
                var lstLedger = dbOld.Ledgers.Where(x => x.AccountGroupId == ag.AccountGroupId).ToList();

                foreach(var l in lstLedger)
                {
                    
                    DALOld.LedgerOP lop = l.LedgerOPs.Where(x => x.Fund == cm.FundName).FirstOrDefault();
                    if (lop == null) lop = new DALOld.LedgerOP();

                    DALNew.Ledger dl = new DALNew.Ledger()
                    {
                        LedgerName = l.LedgerName,
                        LedgerCode = l.AccountCode                                               
                    };

                    d.Ledgers.Add(dl);
                    dbNew.SaveChanges();
                    LogDetailStore(new DALNew.Ledger() { Id=dl.Id, AccountGroupId = dl.AccountGroupId, LedgerCode = dl.LedgerCode, LedgerName=dl.LedgerName }, LogDetailType.INSERT, DataKeyValue.UserId);

                    decimal OPDr = Convert.ToDecimal(lop.DrAmt);
                    decimal OPCr = Convert.ToDecimal(lop.CrAmt);
                    if(OPDr !=0 || OPCr != 0)
                    {
                        var acylb = new DALNew.ACYearLedgerBalance()
                        {
                            DrAmt = OPDr,
                            CrAmt = OPCr,
                            LedgerId = dl.Id,

                        };
                        acym.ACYearLedgerBalances.Add(acylb);
                        dbNew.SaveChanges();
                        LogDetailStore(new DALNew.ACYearLedgerBalance() { Id = acylb.Id, ACYearMasterId=acylb.ACYearMasterId, CrAmt=acylb.CrAmt,DrAmt=acylb.DrAmt, LedgerId=acylb.LedgerId }, LogDetailType.INSERT, DataKeyValue.UserId);
                    }

                    WriteLog(string.Format("Stored Ledger : {0}, Id : {1}", dl.LedgerName, dl.Id));
                }

                WriteAccountGroup(cm, ag.AccountGroupId, d.Id, acym);

            }
            WriteLog("End to store the Accounts Group");
        }

        void WritePayment(DALNew.FundMaster cm)
        {
            WriteLog("Start to store the Payment");
            try
            {
                
                var lstPayment = dbOld.PaymentMasters.Where(x => x.Fund == cm.FundName && x.PaymentDate>=new DateTime(2016,4,1)).ToList();
                pbrPayment.Maximum = lstPayment.Count();
                pbrPayment.Value = 0;                
                foreach (var p in lstPayment)
                {
                    try
                    {
                        DALNew.Payment pm = new DALNew.Payment()
                        {
                            LedgerId = GetLedgerId(p.Ledger.LedgerName, cm.Id),
                            Amount = Convert.ToDecimal(p.PayAmount),
                            ChequeDate = p.chequeDate,
                            ChequeNo = p.ChequeNo,
                            ClearDate = p.ClearDate,
                            VoucherNo = p.VoucherNo,
                            EntryNo = Payment_NewRefNo(cm.Id,p.PaymentDate.Value),
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
                                LedgerId = GetLedgerId(pd.Ledger.LedgerName, cm.Id),
                                Amount = Convert.ToDecimal(pd.Amount),
                                Particular = pd.Narration,
                            };
                            pm.PaymentDetails.Add(pmd);
                          }

                        dbNew.Payments.Add(pm);
                        dbNew.SaveChanges();
                        LogDetailStore(new DALNew.Payment() { Id = pm.Id, LedgerId = pm.LedgerId, Amount= pm.Amount, ChequeDate=pm.ChequeDate, ChequeNo= pm.ChequeNo, ClearDate=pm.ClearDate, EntryNo=pm.EntryNo, ExtraCharge=pm.ExtraCharge, Particulars=pm.Particulars, PaymentDate=pm.PaymentDate, PaymentMode=pm.PaymentMode, PayTo=pm.PayTo, RefCode=pm.RefCode, RefNo= pm.RefNo, Status= pm.Status, VoucherNo = pm.VoucherNo }, LogDetailType.INSERT, DataKeyValue.UserId);
                        foreach(var l1 in pm.PaymentDetails)
                        {
                            LogDetailStore(new DALNew.PaymentDetail() { Id = l1.Id, Amount=l1.Amount, LedgerId=l1.LedgerId, Particular= l1.Particular, PaymentId = l1.PaymentId }, LogDetailType.INSERT, DataKeyValue.UserId);
                        }
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
                var lstReceipt = dbOld.ReceiptMasters.Where(x => x.Fund == cm.FundName &&  x.ReceiptDate >= new DateTime(2016, 4, 1)).ToList();
                pbrReceipt.Maximum = lstReceipt.Count();
                pbrReceipt.Value = 0;
                foreach (var r in lstReceipt)
                {

                    try
                    {
                        DALNew.Receipt rm = new DALNew.Receipt()
                        {
                            LedgerId = GetLedgerId(r.Ledger.LedgerName, cm.Id),
                            Amount = Convert.ToDecimal(r.ReceiptAmount),
                            ChequeDate = r.ChequeDate,
                            ChequeNo = r.ChequeNo,
                            CleareDate = r.ClrDate,
                            VoucherNo = r.VoucherNo,
                            EntryNo = Receipt_NewRefNo(cm.Id,r.ReceiptDate.Value),
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
                                LedgerId = GetLedgerId(rd.Ledger.LedgerName, cm.Id),
                                Amount = Convert.ToDecimal(rd.Amount),
                                Particulars = rd.Narration,
                            };
                            rm.ReceiptDetails.Add(pmd);
                        }

                        dbNew.Receipts.Add(rm);
                        dbNew.SaveChanges();
                        LogDetailStore(new DALNew.Receipt() { Id = rm.Id, LedgerId = rm.LedgerId, Amount=rm.Amount, ChequeDate=rm.ChequeDate, ChequeNo=rm.ChequeNo, CleareDate=rm.CleareDate, EntryNo=rm.EntryNo, Extracharge=rm.Extracharge, Particulars=rm.Particulars, ReceiptDate=rm.ReceiptDate, ReceiptMode=rm.ReceiptMode, ReceivedFrom=rm.ReceivedFrom, RefCode=rm.RefCode, RefNo=rm.RefNo, Status=rm.Status, VoucherNo=rm.VoucherNo }, LogDetailType.INSERT, DataKeyValue.UserId);
                        foreach(var l1 in rm.ReceiptDetails)
                        {
                            LogDetailStore(new DALNew.ReceiptDetail() {Id=l1.Id, Amount=l1.Amount, LedgerId=l1.LedgerId, Particulars=l1.Particulars, ReceiptId=l1.ReceiptId  }, LogDetailType.INSERT, DataKeyValue.UserId);
                        }
                        
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
                var lstJournal = dbOld.JournalMasters.Where(x => x.Fund == cm.FundName && x.JournalDate >= new DateTime(2016, 4, 1)).ToList();
                pbrJournal.Maximum = lstJournal.Count();
                pbrJournal.Value = 0;
                foreach (var j in lstJournal)
                {
                    try
                    {
                        DALNew.Journal jm = new DALNew.Journal()
                        {
                            VoucherNo = j.VoucherNo,
                            EntryNo = Journal_NewRefNo(cm.Id,j.JournalDate.Value),
                            HQNo = j.HQNo,
                            JournalDate = j.JournalDate.Value,
                            Status = j.Status,
                            RefCode = j.JournalId.ToString()
                        };

                        foreach (var jd in j.JournalDetails)
                        {
                            DALNew.JournalDetail pmd = new DALNew.JournalDetail()
                            {
                                LedgerId = GetLedgerId(jd.Ledger.LedgerName, cm.Id),
                                CrAmt = Convert.ToDecimal(jd.CrAmt),
                                DrAmt = Convert.ToDecimal(jd.DrAmt),
                                Particulars = jd.Narration,
                            };
                            jm.JournalDetails.Add(pmd);
                        }

                        dbNew.Journals.Add(jm);
                        dbNew.SaveChanges();
                        LogDetailStore(new DALNew.Journal() { Id=jm.Id, Amount= jm.Amount, EntryNo=jm.EntryNo, HQNo=jm.HQNo, JournalDate=jm.JournalDate, Particular=jm.Particular, RefCode=jm.RefCode, Status=jm.Status, VoucherNo=jm.VoucherNo }, LogDetailType.INSERT, DataKeyValue.UserId);
                        foreach(var l1 in jm.JournalDetails)
                        {
                            LogDetailStore(new DALNew.JournalDetail() {Id=l1.Id, CrAmt=l1.CrAmt, DrAmt=l1.DrAmt, JournalId=l1.JournalId, Particulars=l1.Particulars, LedgerId=l1.LedgerId }, LogDetailType.INSERT, DataKeyValue.UserId);
                        }
                        
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

        int GetLedgerId(string LedgerName,int FundId)
        {
            DALNew.Ledger l = dbNew.Ledgers.Where(x => x.LedgerName == LedgerName && x.AccountGroup.FundMasterId == FundId).FirstOrDefault();
            if (l == null)
            {
                return 0;
            }else
            {
                return l.Id;
            }
        }

        public string Payment_NewRefNo(int FundId, DateTime dt)
        {
            //DateTime dt = DateTime.Now;
            string Prefix = string.Format("{0}{1:yy}{2:X}", FormPrefix.Payment, dt, dt.Month);
            long No = 0;

            var d = dbNew.Payments.Where(x => x.Ledger.AccountGroup.FundMasterId == FundId && x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 16);

            return string.Format("{0}{1:X5}", Prefix, No + 1);
        }

        public string Receipt_NewRefNo(int FundId, DateTime dt)
        {
            //DateTime dt = DateTime.Now;
            string Prefix = string.Format("{0}{1:yy}{2:X}", FormPrefix.Receipt, dt, dt.Month);
            long No = 0;

            var d = dbNew.Receipts.Where(x => x.Ledger.AccountGroup.FundMasterId == FundId && x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 16);

            return string.Format("{0}{1:X5}", Prefix, No + 1);
        }

        public string Journal_NewRefNo(int FundId, DateTime dt)
        {
            //DateTime dt = DateTime.Now;
            string Prefix = string.Format("{0}{1:yy}{2:X}", FormPrefix.Journal, dt, dt.Month);
            long No = 0;

            var d = dbNew.Journals.Where(x => x.JournalDetails.FirstOrDefault().Ledger.AccountGroup.FundMasterId == FundId && x.EntryNo.StartsWith(Prefix))
                                     .OrderByDescending(x => x.EntryNo)
                                     .FirstOrDefault();

            if (d != null) No = Convert.ToInt64(d.EntryNo.Substring(Prefix.Length), 16);

            return string.Format("{0}{1:X5}", Prefix, No + 1);
        }

        public  void WriteLog(String str)
        {
            using (StreamWriter writer = new StreamWriter(System.IO.Path.GetTempPath() + "ABDC_log.txt", true))
            {
                writer.WriteLine(string.Format("{0:dd/MM/yyyy hh:mm:ss} => {1}", DateTime.Now, str));
            }
            dtEnd = DateTime.Now;
            TimeSpan ts = dtEnd - dtStart;
            lblStatus.Text = string.Format("Start Time : {0:hh:mm:ss}, End Time : {1:hh:mm:ss}, Work on Mins : {2}\r\nMessage : {3}",dtStart,dtEnd, ts.TotalMinutes,str);
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
        private int EntityTypeId(string Type)
        {
            DALNew.EntityType et = EntityTypeList.Where(x => x.Entity == Type).FirstOrDefault();
            if (et == null)
            {
                et = new DALNew.EntityType();
                dbNew.EntityTypes.Add(et);
                EntityTypeList.Add(et);
                et.Entity = Type;
                dbNew.SaveChanges();
            }
            return et.Id;
        }

        private int LogDetailTypeId(LogDetailType Type)
        {
            DALNew.LogDetailType ldt = LogDetailTypeList.Where(x => x.Type == Type.ToString()).FirstOrDefault();
            return ldt.Id;
        }

        private void LogDetailStore(object Data, LogDetailType Type, int userId)
        {
            try
            {
                Type t = Data.GetType();
                long EntityId = Convert.ToInt64(t.GetProperty("Id").GetValue(Data));
                int ETypeId = EntityTypeId(t.Name);

                DALNew.LogMaster l = dbNew.LogMasters.Where(x => x.EntityId == EntityId && x.EntityTypeId == ETypeId).FirstOrDefault();
                DALNew.LogDetail ld = new DALNew.LogDetail();
                DateTime dt = DateTime.Now;

                if (l == null)
                {
                    l = new DALNew.LogMaster();
                    dbNew.LogMasters.Add(l);
                    l.EntityId = EntityId;
                    l.EntityTypeId = ETypeId;
                    l.CreatedAt = dt;
                    l.CreatedBy = userId;
                }

                if (Type == LogDetailType.UPDATE)
                {
                    l.UpdatedAt = dt;
                    l.UpdatedBy = userId;
                }
                else if (Type == LogDetailType.DELETE)
                {
                    l.DeletedAt = dt;
                    l.DeletedBy = userId;
                }

                dbNew.SaveChanges();

                dbNew.LogDetails.Add(ld);
                ld.LogMasterId = l.Id;
                ld.RecordDetail = new JavaScriptSerializer().Serialize(Data);
                ld.EntryBy =userId;
                ld.EntryAt = dt;
                ld.LogDetailTypeId = LogDetailTypeId(Type);
                dbNew.SaveChanges();
            }
            catch (Exception ex) { }
        }
    }
}
