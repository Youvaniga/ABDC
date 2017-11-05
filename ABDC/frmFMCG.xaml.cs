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
        DALOldNUBE.nubebfsv1Entities dbOld = new DALOldNUBE.nubebfsv1Entities();
        DALNewNUBE.nube_newEntities dbNew = new DALNewNUBE.nube_newEntities();
        DateTime dtStart, dtEnd;
        private List<DALNewNUBE.EntityType> _entityTypeList;
        private List<DALNewNUBE.LogDetailType> _logDetailTypeList;

        List<DALOldNUBE.AccountGroup> lstAccountGroup = new List<DALOldNUBE.AccountGroup>();
        List<DALOldNUBE.Ledger> lstLedger = new List<DALOldNUBE.Ledger>();
        List<DALOldNUBE.PaymentMaster> lstPayment = new List<DALOldNUBE.PaymentMaster>();
        List<DALOldNUBE.ReceiptMaster> lstReceipt = new List<DALOldNUBE.ReceiptMaster>();
        List<DALOldNUBE.JournalMaster> lstJournal = new List<DALOldNUBE.JournalMaster>();

        List<DALNewNUBE.Ledger> lstLedgerNew = new List<DALNewNUBE.Ledger>();
        List<DALNewNUBE.Payment> lstPaymentNew = new List<DALNewNUBE.Payment>();
        List<DALNewNUBE.Receipt> lstReceiptNew = new List<DALNewNUBE.Receipt>();
        List<DALNewNUBE.Journal> lstJournalNew = new List<DALNewNUBE.Journal>();

        public frmFMCG()
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

                    DALNewNUBE.FundMaster fm = new DALNewNUBE.FundMaster() { FundName = f, IsActive = true };
                    DALNewNUBE.ACYearMaster acym = new DALNewNUBE.ACYearMaster() { ACYear = "2016 - 2017", ACYearStatusId = 1 };


                    fm.ACYearMasters.Add(acym);
                    dbNew.FundMasters.Add(fm);
                    dbNew.SaveChanges();

                    pbrFund.Value += 1;
                    DALNewNUBE.UserType ut = new DALNewNUBE.UserType() { TypeOfUser = DataKeyValue.Administrator_Key };
                    fm.UserTypes.Add(ut);

                    foreach (var utfd in lstUserTypeFormDetail)
                    {
                        var ud = new DALNewNUBE.UserTypeDetail()
                        {
                            UserTypeFormDetailId = utfd.Id,
                            IsViewForm = true,
                            AllowInsert = true,
                            AllowUpdate = true,
                            AllowDelete = true
                        };
                        ut.UserTypeDetails.Add(ud);
                    }

                    DALNewNUBE.UserAccount ua = new DALNewNUBE.UserAccount() { LoginId = "Admin", UserName = "Admin", Password = "Admin" };
                    ut.UserAccounts.Add(ua);
                    WriteLog(string.Format("Stored User Account : {0}, Id : {1}", ua.UserName, ua.Id));

                    DALNewNUBE.CustomFormat cf = new DALNewNUBE.CustomFormat()
                    {
                        CurrencyNegativeSymbolPrefix = "[RM] ",
                        CurrencyPositiveSymbolPrefix = "RM ",
                        CurrencyToWordPrefix = "Ringgit Malaysia ",
                        DecimalToWordPrefix = "Cents ",
                        DecimalSymbol = ".",
                        DigitGroupingSymbol = ",",
                        IsDisplayWithOnlyOnSuffix = true,
                        NoOfDigitAfterDecimal = 2,
                        FundMasterId = fm.Id
                    };
                    dbNew.CustomFormats.Add(cf);
                    WriteAccountGroup(fm, 1, null, acym); dbNew.SaveChanges();

                    lstLedgerNew = dbNew.Ledgers.Where(x => x.AccountGroup.FundMasterId == fm.Id).ToList();
                    lstPaymentNew = new List<DALNewNUBE.Payment>();
                    lstReceiptNew = new List<DALNewNUBE.Receipt>();
                    lstJournalNew = new List<DALNewNUBE.Journal>();

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
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }

            WriteLog("ABDC End");
            MessageBox.Show("Finished");
        }

        void WriteAccountGroup(DALNewNUBE.FundMaster fm, decimal AGId, DALNewNUBE.AccountGroup UAG, DALNewNUBE.ACYearMaster acym)
        {
            WriteLog("Start to store the Accounts Group");

            foreach (var ag in lstAccountGroup.Where(x => x.Under == AGId && x.AccountGroupId != AGId).ToList())
            {
                DALNewNUBE.AccountGroup d = new DALNewNUBE.AccountGroup()
                {
                    GroupName = ag.GroupName,
                    GroupCode = ag.GroupCode,
                    AccountGroup2 = UAG
                };
                fm.AccountGroups.Add(d);

                WriteLog(string.Format("Stored Account Group : {0}", d.GroupName));

                foreach (var l in lstLedger.Where(x => x.AccountGroupId == ag.AccountGroupId).ToList())
                {

                    DALOldNUBE.LedgerOP lop = l.LedgerOPs.Where(x => x.Fund == fm.FundName).FirstOrDefault();
                    if (lop == null) lop = new DALOldNUBE.LedgerOP();

                    DALNewNUBE.Ledger dl = new DALNewNUBE.Ledger()
                    {
                        LedgerName = l.LedgerName,
                        LedgerCode = l.AccountCode
                    };

                    d.Ledgers.Add(dl);

                    decimal OPDr = Convert.ToDecimal(lop.DrAmt);
                    decimal OPCr = Convert.ToDecimal(lop.CrAmt);
                    if (OPDr != 0 || OPCr != 0)
                    {
                        var acylb = new DALNewNUBE.ACYearLedgerBalance()
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

        void WritePayment(DALNewNUBE.FundMaster fm)
        {
            WriteLog("Start to store the Payment");
            try
            {

                var l1 = lstPayment.Where(x => x.Fund == fm.FundName && x.PaymentDate >= new DateTime(2016, 4, 1)).ToList();
                pbrPayment.Maximum = l1.Count();
                pbrPayment.Value = 0;
                foreach (var p in l1)
                {
                    try
                    {

                        DALNewNUBE.Payment pm = new DALNewNUBE.Payment()
                        {
                            LedgerId = GetLedgerId(p.Ledger.LedgerName),
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
                            DALNewNUBE.PaymentDetail pmd = new DALNewNUBE.PaymentDetail()
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
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("Error on Stored Payment => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", p.PaymentDate, p.EntryNo, p.VoucherNo, ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error on Payment: {0}", ex.Message));
            }
            WriteLog("End to Store the Payment");
        }

        void WriteReceipt(DALNewNUBE.FundMaster cm)
        {
            WriteLog("Start to store the Receipt");
            try
            {
                var l1 = lstReceipt.Where(x => x.Fund == cm.FundName && x.ReceiptDate >= new DateTime(2016, 4, 1)).ToList();
                pbrReceipt.Maximum = l1.Count();
                pbrReceipt.Value = 0;
                foreach (var r in l1)
                {

                    try
                    {
                        DALNewNUBE.Receipt rm = new DALNewNUBE.Receipt()
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
                            DALNewNUBE.ReceiptDetail pmd = new DALNewNUBE.ReceiptDetail()
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
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("Error on Stored Receipt => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", r.ReceiptDate, r.EntryNo, r.VoucherNo, ex.Message));
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error on Receipt: {0}", ex.Message));
            }

            WriteLog("End to Store the Receipt");
        }

        void WriteJournal(DALNewNUBE.FundMaster cm)
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
                        DALNewNUBE.Journal jm = new DALNewNUBE.Journal()
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
                            DALNewNUBE.JournalDetail pmd = new DALNewNUBE.JournalDetail()
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
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("Error Stored Journal => Date : {0}, Entry No : {1}, Voucher No : {2}, Error : {3}", j.JournalDate, j.EntryNo, j.VoucherNo, ex.Message));
                    }

                }
            }
            catch (Exception ex)
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

        void WriteDataKey(DALNewNUBE.FundMaster fm)
        {
            var ut = fm.UserTypes.FirstOrDefault();
            fm.DataKeyValues.Add(new DALNewNUBE.DataKeyValue() { DataKey = ut.TypeOfUser, DataValue = ut.Id });
            foreach (var ag in fm.AccountGroups)
            {
                fm.DataKeyValues.Add(new DALNewNUBE.DataKeyValue() { DataKey = ag.GroupName, DataValue = ag.Id });
            }
            dbNew.SaveChanges();
        }
        void WriteLogData(DALNewNUBE.FundMaster fm)
        {
            var ua = fm.UserTypes.FirstOrDefault().UserAccounts.FirstOrDefault();
            var ua1 = new DALNewNUBE.FundMaster() { Id = fm.Id, FundName = fm.FundName, IsActive = fm.IsActive };

            var cf = fm.CustomFormats.FirstOrDefault();
            var cf1 = new DALNewNUBE.CustomFormat() { Id = cf.Id, CurrencyCaseSensitive = cf.CurrencyCaseSensitive, CurrencyNegativeSymbolPrefix = cf.CurrencyNegativeSymbolPrefix, CurrencyNegativeSymbolSuffix = cf.CurrencyNegativeSymbolSuffix, CurrencyPositiveSymbolPrefix = cf.CurrencyPositiveSymbolPrefix, CurrencyPositiveSymbolSuffix = cf.CurrencyPositiveSymbolSuffix, CurrencyToWordPrefix = cf.CurrencyToWordPrefix, CurrencyToWordSuffix = cf.CurrencyToWordSuffix, DecimalSymbol = cf.DecimalSymbol, DecimalToWordPrefix = cf.DecimalToWordPrefix, DecimalToWordSuffix = cf.DecimalToWordSuffix, DigitGroupingBy = cf.DigitGroupingBy, DigitGroupingSymbol = cf.DigitGroupingSymbol, FundMasterId = cf.FundMasterId, IsDisplayWithOnlyOnSuffix = cf.IsDisplayWithOnlyOnSuffix, NoOfDigitAfterDecimal = cf.NoOfDigitAfterDecimal };

            var ut = fm.UserTypes.FirstOrDefault();
            var ut1 = new DALNewNUBE.UserType() { Id = ut.Id, FundMasterId = ut.FundMasterId, TypeOfUser = ut.TypeOfUser, Description = ut.Description };
            foreach (var utd in ut.UserTypeDetails)
            {
                ut1.UserTypeDetails.Add(new DALNewNUBE.UserTypeDetail() { Id = utd.Id, UserTypeId = utd.UserTypeId, UserTypeFormDetailId = utd.UserTypeFormDetailId, IsViewForm = utd.IsViewForm, AllowInsert = utd.AllowInsert, AllowUpdate = utd.AllowUpdate, AllowDelete = utd.AllowDelete });
            }

            var acym = fm.ACYearMasters.FirstOrDefault();
            var acym1 = new DALNewNUBE.ACYearMaster() { Id = acym.Id, ACYear = acym.ACYear, ACYearStatusId = acym.ACYearStatusId, FundMasterId = acym.FundMasterId };
            foreach (var acymlb in acym.ACYearLedgerBalances)
            {
                acym1.ACYearLedgerBalances.Add(new DALNewNUBE.ACYearLedgerBalance() { Id = acymlb.Id, ACYearMasterId = acym.Id, LedgerId = acymlb.LedgerId, DrAmt = acymlb.DrAmt, CrAmt = acymlb.CrAmt });
            }

            var fm1 = new DALNewNUBE.FundMaster() { Id = fm.Id, FundName = fm.FundName, IsActive = fm.IsActive };

            LogDetailStore(fm1, ua.Id);
            LogDetailStore(cf1, ua.Id);
            LogDetailStore(ut1, ua.Id);
            LogDetailStore(ua1, ua.Id);
            var x1 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of Master");
            LogDetailStore(acym1, ua.Id);
            var x2 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of AccountYearMaster");
            foreach (var ag in fm.AccountGroups)
            {
                LogDetailStore(new DALNewNUBE.AccountGroup() { Id = ag.Id, FundMasterId = fm.Id, GroupCode = ag.GroupCode, UnderGroupId = ag.UnderGroupId, GroupName = ag.GroupName }, ua.Id);
            }

            var x3 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of AccountGroup");
            foreach (var ld in lstLedgerNew)
            {
                LogDetailStore(new DALNewNUBE.Ledger() { Id = ld.Id, AccountGroupId = ld.AccountGroupId, LedgerCode = ld.LedgerCode, LedgerName = ld.LedgerName }, ua.Id);
            }
            var n0 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of Ledger");
            int i = 0;
            foreach (var p in lstPaymentNew)
            {
                var p1 = new DALNewNUBE.Payment() { Id = p.Id, Amount = p.Amount, ChequeDate = p.ChequeDate, ChequeNo = p.ChequeNo, ClearDate = p.ClearDate, EntryNo = p.EntryNo, ExtraCharge = p.ExtraCharge, LedgerId = p.LedgerId, Particulars = p.Particulars, PaymentDate = p.PaymentDate, PaymentMode = p.PaymentMode, PayTo = p.PayTo, RefCode = p.RefCode, RefNo = p.RefNo, Status = p.Status, VoucherNo = p.VoucherNo };
                foreach (var pd in p.PaymentDetails)
                {
                    p1.PaymentDetails.Add(new DALNewNUBE.PaymentDetail() { Id = pd.Id, Amount = pd.Amount, LedgerId = pd.LedgerId, Particular = pd.Particular, PaymentId = pd.PaymentId });
                }

                LogDetailStore(p1, ua.Id);
                if (i++ % 1000 == 0) dbNew.SaveChanges();

            }
            var n1 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of Payment");
            foreach (var r in lstReceiptNew)
            {
                var r1 = new DALNewNUBE.Receipt() { Id = r.Id, Amount = r.Amount, ChequeDate = r.ChequeDate, ChequeNo = r.ChequeNo, CleareDate = r.CleareDate, EntryNo = r.EntryNo, Extracharge = r.Extracharge, LedgerId = r.LedgerId, Particulars = r.Particulars, ReceiptDate = r.ReceiptDate, ReceiptMode = r.ReceiptMode, ReceivedFrom = r.ReceivedFrom, RefCode = r.RefCode, RefNo = r.RefNo, Status = r.Status, VoucherNo = r.VoucherNo };
                foreach (var rd in r.ReceiptDetails)
                {
                    r1.ReceiptDetails.Add(new DALNewNUBE.ReceiptDetail() { Id = rd.Id, Amount = rd.Amount, LedgerId = rd.LedgerId, Particulars = rd.Particulars, ReceiptId = rd.ReceiptId });
                }
                LogDetailStore(r1, ua.Id);

                if (i++ % 1000 == 0) dbNew.SaveChanges();
            }
            var n2 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of Receipt");
            foreach (var j in lstJournalNew)
            {
                var j1 = new DALNewNUBE.Journal() { Id = j.Id, Amount = j.Amount, EntryNo = j.EntryNo, HQNo = j.HQNo, JournalDate = j.JournalDate, Particular = j.Particular, RefCode = j.RefCode, Status = j.Status, VoucherNo = j.VoucherNo };
                foreach (var jd in j.JournalDetails)
                {
                    j1.JournalDetails.Add(new DALNewNUBE.JournalDetail() { Id = jd.Id, CrAmt = jd.CrAmt, DrAmt = jd.DrAmt, JournalId = jd.JournalId, LedgerId = jd.LedgerId, Particulars = jd.Particulars });
                }
                LogDetailStore(j1, ua.Id);
                if (i++ % 1000 == 0) dbNew.SaveChanges();
            }

            var n3 = dbNew.SaveChanges();
            WriteLog("Log Data Finished of Journal");
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
        private List<DALNewNUBE.EntityType> EntityTypeList
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
        private List<DALNewNUBE.LogDetailType> LogDetailTypeList
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
        private DALNewNUBE.EntityType EntityType(string Type)
        {
            DALNewNUBE.EntityType et = EntityTypeList.Where(x => x.Entity == Type).FirstOrDefault();
            if (et == null)
            {
                et = new DALNewNUBE.EntityType();
                dbNew.EntityTypes.Add(et);
                EntityTypeList.Add(et);
                et.Entity = Type;
            }
            return et;
        }

        private int LogDetailTypeId(LogDetailType Type)
        {
            DALNewNUBE.LogDetailType ldt = LogDetailTypeList.Where(x => x.Type == Type.ToString()).FirstOrDefault();
            return ldt.Id;
        }

        private void LogDetailStore(object Data, int userId)
        {
            try
            {
                Type t = Data.GetType();
                long EntityId = Convert.ToInt64(t.GetProperty("Id").GetValue(Data));

                DALNewNUBE.LogMaster l = new DALNewNUBE.LogMaster();
                DALNewNUBE.LogDetail ld = new DALNewNUBE.LogDetail();
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
