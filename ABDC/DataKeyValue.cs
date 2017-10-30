using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABDC
{
    public class DataKeyValue : INotifyPropertyChanged
    {


        #region fields
        private static List<DataKeyValue> _toList;
        private int _Id;
        private string _DataKey;
        private int _DataValue;
        private static int _CompanyId;
        private static int _UserId;
        #endregion

        #region Property

        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    NotifyPropertyChanged(nameof(Id));
                }
            }
        }

        public string DataKey
        {
            get
            {
                return _DataKey;
            }
            set
            {
                if (_DataKey != value)
                {
                    _DataKey = value;
                    NotifyPropertyChanged(nameof(DataKey));
                }
            }
        }

        public int DataValue
        {
            get
            {
                return _DataValue;
            }
            set
            {
                if (_DataValue != value)
                {
                    _DataValue = value;
                    NotifyPropertyChanged(nameof(DataValue));
                }

            }
        }
       
        #endregion

        #region Static Property

        public static int CompanyId
        {
            get
            {
                return _CompanyId;
            }

            set
            {
                _CompanyId = value;
                _toList = null;
            }
        }

        public static int UserId
        {
            get
            {
                return _UserId;
            }

            set
            {
                _UserId = value;
            }
        }

        public static string Administrator_Key
        {
            get
            {
                return "Administrator";
            }
        }
        public static int Administrator_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Administrator_Key).FirstOrDefault().DataValue;
            }
        }
        
        #region AccountsGroup
              

        public static string Assets_Key
        {
            get
            {
                return "Assets";
            }
        }
        public static int Assets_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Assets_Key).FirstOrDefault().DataValue;
            }
        }

        public static string Liabilities_Key
        {
            get
            {
                return "Liabilities";
            }

        }
        public static int Liabilities_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Liabilities_Key).FirstOrDefault().DataValue;
            }
        }

        public static string Income_Key
        {
            get
            {
                return "Income";
            }

        }
        public static int Income_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Income_Key).FirstOrDefault().DataValue;
            }
        }

        public static string Expenses_Key
        {
            get
            {
                return "Expenses";
            }

        }
        public static int Expenses_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Expenses_Key).FirstOrDefault().DataValue;
            }
        }

        public static string SundryCreditors_Key
        {
            get
            {
                return "Sundry Creditors";
            }

        }
        public static int SundryCreditors
        {
            get
            {
                return toList.Where(x => x.DataKey == SundryCreditors_Key).FirstOrDefault().DataValue;
            }
        }
        public static string SundryDebtors_Key
        {
            get
            {
                return "Sundry Debtors";
            }

        }
        public static int SundryDebtors
        {
            get
            {
                return toList.Where(x => x.DataKey == SundryDebtors_Key).FirstOrDefault().DataValue;
            }
        }

        public static string CurrentAssets_Key
        {
            get
            {
                return "Current Assets";
            }

        }
        public static int CurrentAssets_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == CurrentAssets_Key).FirstOrDefault().DataValue;
            }
        }
        public static string CashInHand_Key
        {
            get
            {
                return "Cash-in-Hand";
            }

        }
        public static int CashInHand_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == CashInHand_Key).FirstOrDefault().DataValue;
            }
        }
        public static string CashLedger_Key
        {
            get
            {
                return "Cash Ledger";
            }

        }
        public static int CashLedger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == CashLedger_Key).FirstOrDefault().DataValue;
            }
        }

        public static string Deposits_Key
        {
            get
            {
                return "Deposits";
            }

        }
        public static int Deposits_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Deposits_Key).FirstOrDefault().DataValue;
            }
        }
        public static string LoansandAdvances_Key
        {
            get
            {
                return "Loans and Advances";
            }

        }
        public static int LoansandAdvances_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == LoansandAdvances_Key).FirstOrDefault().DataValue;
            }
        }
        public static string BankAccounts_Key
        {
            get
            {
                return "Bank Accounts";
            }

        }
        public static int BankAccounts_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == BankAccounts_Key).FirstOrDefault().DataValue;
            }
        }
        public static string StockInHand_Key
        {
            get
            {
                return "Stock-In-Hand";
            }

        }
        public static int StockInHand_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == StockInHand_Key).FirstOrDefault().DataValue;
            }
        }

        #region Stock_In_Hand Ledger
        public static string Stock_In_Hand_Ledger_Key
        {
            get
            {
                return "Stock-In-Hand";
            }

        }
        public static int Stock_In_Hand_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Stock_In_Hand_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Stock_Inward_Ledger
        public static string Stock_Inward_Ledger_Key
        {
            get
            {
                return "Stock Inward";
            }

        }
        public static int Stock_Inward_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Stock_Inward_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Stock_Outward_Ledger
        public static string Stock_Outward_Ledger_Key
        {
            get
            {
                return "Stock Outward";
            }

        }
        public static int Stock_Outward_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Stock_Outward_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        public static string FixedAssets_Key
        {
            get
            {
                return "Fixed Assets";
            }

        }
        public static int FixedAssets_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == FixedAssets_Key).FirstOrDefault().DataValue;
            }
        }
        public static string MiscExpenses_Key
        {
            get
            {
                return "Misc. Expenses";
            }

        }
        public static int MiscExpenses_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == MiscExpenses_Key).FirstOrDefault().DataValue;
            }
        }
        public static string Investments_Key
        {
            get
            {
                return "Investments";
            }

        }
        public static int Investments_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Investments_Key).FirstOrDefault().DataValue;
            }
        }

        public static string CurrentLiabilities_Key
        {
            get
            {
                return "Current Liabilities";
            }

        }
        public static int CurrentLiabilities_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == CurrentLiabilities_Key).FirstOrDefault().DataValue;
            }
        }
        public static string DutiesTaxes_Key
        {
            get
            {
                return "Duties & Taxes";
            }

        }
        public static int DutiesTaxes_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == DutiesTaxes_Key).FirstOrDefault().DataValue;
            }
        }
        public static string Provisions_Key
        {
            get
            {
                return "Provisions";
            }

        }
        public static int Provisions_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Provisions_Key).FirstOrDefault().DataValue;
            }
        }
        public static string Loans_Key
        {
            get
            {
                return "Loans";
            }

        }
        public static int Loans_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Loans_Key).FirstOrDefault().DataValue;
            }
        }
        public static string BankODAc_Key
        {
            get
            {
                return "Bank OD A/c";
            }

        }
        public static int BankODAc_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == BankODAc_Key).FirstOrDefault().DataValue;
            }
        }
        public static string SecuredLoans_Key
        {
            get
            {
                return "Secured Loans";
            }

        }
        public static int SecuredLoans_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == SecuredLoans_Key).FirstOrDefault().DataValue;
            }
        }
        public static string UnSecuredLoans_Key
        {
            get
            {
                return "UnSecured Loans";
            }

        }
        public static int UnSecuredLoans_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == UnSecuredLoans_Key).FirstOrDefault().DataValue;
            }
        }
        public static string BranchDivisions_Key
        {
            get
            {
                return "Branch /Divisions";
            }

        }
        public static int BranchDivisions_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == BranchDivisions_Key).FirstOrDefault().DataValue;
            }
        }
        public static string CapitalAccount_Key
        {
            get
            {
                return "Capital Account";
            }

        }
        public static int CapitalAccount_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == CapitalAccount_Key).FirstOrDefault().DataValue;
            }
        }
        public static string ReservesSurplus_Key
        {
            get
            {
                return "Reserves & Surplus";
            }

        }
        public static int ReservesSurplus_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == ReservesSurplus_Key).FirstOrDefault().DataValue;
            }
        }
        public static string SuspenseAc_Key
        {
            get
            {
                return "Suspense A/c";
            }

        }
        public static int SuspenseAc_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == SuspenseAc_Key).FirstOrDefault().DataValue;
            }
        }
        public static string DirectIncome_Key
        {
            get
            {
                return "Direct Income";
            }

        }
        public static int DirectIncome_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == DirectIncome_Key).FirstOrDefault().DataValue;
            }
        }
        public static string IndirectIncome_Key
        {
            get
            {
                return "Indirect Income";
            }

        }
        public static int IndirectIncome_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == IndirectIncome_Key).FirstOrDefault().DataValue;
            }
        }
        public static string SalesAccount_Key
        {
            get
            {
                return "Sales Account";
            }

        }
        public static int SalesAccount_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == SalesAccount_Key).FirstOrDefault().DataValue;
            }
        }

        #region Sales A/C Ledger
        public static string SalesAccount_Ledger_Key
        {
            get
            {
                return "Sales A/C";
            }

        }
        public static int SalesAccount_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == SalesAccount_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Sales_Return A/C Ledger
        public static string Sales_Return_Ledger_Key
        {
            get
            {
                return "Sales Return A/C";
            }

        }
        public static int Sales_Return_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Sales_Return_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion


        #region Profit_Loss Ledger
        public static string Profit_Loss_Ledger_Key
        {
            get
            {
                return "Profit & Loss A/c";
            }

        }
        public static int Profit_Loss_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Profit_Loss_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Input_Tax Ledger
        public static string Input_Tax_Ledger_Key
        {
            get
            {
                return "Input Tax";
            }

        }
        public static int Input_Tax_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Input_Tax_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Output_Tax Ledger
        public static string Output_Tax_Ledger_Key
        {
            get
            {
                return "Output Tax";
            }

        }
        public static int Output_Tax_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Output_Tax_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion


        public static string DirectExpenses_Key
        {
            get
            {
                return "Direct Expenses";
            }

        }
        public static int DirectExpenses_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == DirectExpenses_Key).FirstOrDefault().DataValue;
            }
        }
        public static string IndirectExpense_Key
        {
            get
            {
                return "Indirect Expense";
            }

        }
        public static int IndirectExpense_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == IndirectExpense_Key).FirstOrDefault().DataValue;
            }
        }
        public static string Salary_Key
        {
            get
            {
                return "Salary";
            }

        }
        public static int Salary_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Salary_Key).FirstOrDefault().DataValue;
            }
        }
        public static string PurchaseAccount_Key
        {
            get
            {
                return "Purchase Account";
            }

        }
        public static int PurchaseAccount_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == PurchaseAccount_Key).FirstOrDefault().DataValue;
            }
        }



        #region Purchase_Ac_Ledger
        public static string PurchaseAccount_Ledger_Key
        {
            get
            {
                return "Purchase A/C";
            }

        }
        public static int PurchaseAccount_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == PurchaseAccount_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region Purchase_Return A/C Ledger
        public static string Purchase_Return_Ledger_Key
        {
            get
            {
                return "Purchase Return A/C";
            }

        }
        public static int Purchase_Return_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == Purchase_Return_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region JobOrderIssued_Ledger
        public static string JobOrderIssued_Ledger_Key
        {
            get
            {
                return "Job Order Issued";
            }

        }
        public static int JobOrderIssued_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == JobOrderIssued_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion
        #region JobOrderReceived_Ledger
        public static string JobOrderReceived_Ledger_Key
        {
            get
            {
                return "Job Order Received";
            }

        }
        public static int JobOrderReceived_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == JobOrderReceived_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion

        #region StockInProcess_Ledger
        public static string StockInProcess_Ledger_Key
        {
            get
            {
                return "Stock Process Issue";
            }

        }
        public static int StockInProcess_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == StockInProcess_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion
        #region StockSeperated_Ledger
        public static string StockSeperated_Ledger_Key
        {
            get
            {
                return "Stock Process Received";
            }

        }
        public static int StockSeperated_Ledger_Value
        {
            get
            {
                return toList.Where(x => x.DataKey == StockSeperated_Ledger_Key).FirstOrDefault().DataValue;
            }
        }
        #endregion
        #endregion

        public static List<DataKeyValue> toList
        {
            get
            {
                if (_toList == null)
                {
                    using (var db = new DALNew.nube_new_01Entities())
                    {
                        _toList =  db.DataKeyValues.Where(x=> x.FundMasterId== CompanyId ).ToList().Select(x=> new DataKeyValue() { DataKey= x.DataKey,DataValue = x.DataValue }).ToList();
                    }
                }
                return _toList;
            }
            set
            {
                _toList = value;
            }
        }
        #endregion
        #region Property  Changed Event

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }


        private void NotifyAllPropertyChanged()
        {
            foreach (var p in this.GetType().GetProperties()) NotifyPropertyChanged(p.Name);
        }

        #endregion
        public static void Init()
        {
            
        }

        public static void Write(String Key, int Value)
        {
            using(var db = new DALNew.nube_new_01Entities())
            {
                var dv = new DALNew.DataKeyValue() {
                    DataKey = Key,
                    DataValue = Value,
                    FundMasterId = CompanyId
                };
                db.DataKeyValues.Add(dv);
                db.SaveChanges();
            }
        }

    }
 }
