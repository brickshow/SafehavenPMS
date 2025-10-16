using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel
{
    public class FinancialPoint
    {
        public string Label { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Profit => Income - Expense;
    }

    public class FinancialReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; } = "Month"; // Month / Quarter /Year
        public List<string> Labels { get; set; } = new();
        public List<decimal> IncomeValues { get; set; } = new();
        public List<decimal> ExpenseValues { get; set; } = new();
        public List<decimal> ProfitValues { get; set; } = new();
        public List<FinancialPoint> Points { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalProfit { get; set; }
    }
}