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
                WriteLog("Futching Fund List");
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

                    DALNew.FundMaster cm = new DALNew.FundMaster() { FundName = f, IsActive = true };
                    dbNew.FundMasters.Add(cm);
                    dbNew.SaveChanges();
                    DataKeyValue.CompanyId = cm.Id;
                    WriteLog(string.Format("Stored Fund : {0}, Id : {1}", f,cm.Id));
                    pbrFund.Value += 1;
                    lblFund.Text = string.Format("Stored Fund : {0}, Id : {1}", f, cm.Id);
                    DALNew.UserType ut = new DALNew.UserType() { TypeOfUser= DataKeyValue.Administrator_Key };
                    cm.UserTypes.Add(ut);
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

                    WriteAccountGroup(cm,1,null);
                    WritePayment(cm);
                    WriteReceipt(cm);
                    WriteJournal(cm);
         
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
            }
            
            WriteLog("ABDC End");
            MessageBox.Show("Finished");            
        }

        void WriteAccountGroup(DALNew.FundMaster cm,decimal AGId,int? UAGId)
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
                DataKeyValue.Write(d.GroupName, d.Id);
                WriteLog(string.Format("Stored Account Group : {0}, Id : {1}", d.GroupName, d.Id));
                var lstLedger = dbOld.Ledgers.Where(x => x.AccountGroupId == ag.AccountGroupId).ToList();

                foreach(var l in lstLedger)
                {
                    //if(l.PaymentMasters.Where(x=> x.Fund==cm.CompanyName).Count()>0 || 
                    //   l.PaymentDetails.Where(x => x.PaymentMaster.Fund == cm.CompanyName).Count() > 0 || 
                    //   l.ReceiptMasters.Where(x => x.Fund == cm.CompanyName).Count()>0 || 
                    //   l.ReceiptDetails.Where(x => x.ReceiptMaster.Fund == cm.CompanyName).Count()>0 
                    //   || l.JournalDetails.Where(x => x.JournalMaster.Fund == cm.CompanyName).Count()>0)
                    //{
                       
                    //}

                    DALOld.LedgerOP lop = l.LedgerOPs.Where(x => x.Fund == cm.FundName).FirstOrDefault();
                    if (lop == null) lop = new DALOld.LedgerOP();

                    DALNew.Ledger dl = new DALNew.Ledger()
                    {
                        LedgerName = l.LedgerName,
                        LedgerCode = l.AccountCode, 
                       
                        //OPDr = Convert.ToDecimal(lop.DrAmt),
                        //OPCr = Convert.ToDecimal(lop.CrAmt)
                    };

                    d.Ledgers.Add(dl);
                    dbNew.SaveChanges();
                    WriteLog(string.Format("Stored Ledger : {0}, Id : {1}", dl.LedgerName, dl.Id));
                }

                WriteAccountGroup(cm, ag.AccountGroupId, d.Id);

            }
            WriteLog("End to store the Accounts Group");
        }

        void WritePayment(DALNew.FundMaster cm)
        {
            WriteLog("Start to store the Payment");
            try
            {
                var lstPayment = dbOld.PaymentMasters.Where(x => x.Fund == cm.FundName).ToList();
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
                var lstReceipt = dbOld.ReceiptMasters.Where(x => x.Fund == cm.FundName).ToList();
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
                var lstJournal = dbOld.JournalMasters.Where(x => x.Fund == cm.FundName).ToList();
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
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }
    }
}
