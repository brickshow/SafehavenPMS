// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SafehavenPMS.Data;
// using SafehavenPMS.ViewModel;
// using System;
// using System.Globalization;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace SafehavenPMS.Controllers
// {
//     public class ReportsController : Controller
//     {
//         private readonly SafehavenPMSContext _context;
//         public ReportsController(SafehavenPMSContext context)
//         {
//             _context = context;
//         }

//         // Index: show UI and server-rendered dataset
//         public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "Month")
//         {
//             // Default range: last 6 months
//             var now = DateTime.UtcNow.Date;
//             endDate ??= now;
//             startDate ??= now.AddMonths(-5).AddDays(1 - now.Day); // start of 6-month window

//             // NOTE: Adjust the DbSet name and property names below to match your model.
//             // This scaffold expects a DbSet named "Billings" or change to _context.Payments / _context.Billable etc.
//             // Expected columns: Date (DateTime), Amount (decimal), IsExpense (bool) OR Type(string with "Expense"/"Income")
//             var query = _context.Set<dynamic>().AsQueryable();

//             // Try to use Billing if available (preferred)
//             if (_context.Model.FindEntityType("SafehavenPMS.Models.Billing") != null)
//                 query = _context.Set<Billing>().AsQueryable();
//             else if (_context.Model.FindEntityType("SafehavenPMS.Models.Billable") != null)
//                 query = _context.Set<SafehavenPMS.Models.Billable>().AsQueryable();
//             else if (_context.Model.FindEntityType("SafehavenPMS.Models.Payment") != null)
//                 query = _context.Set<SafehavenPMS.Models.Payment>().AsQueryable();
//             else
//             {
//                 // Fallback: try "Billings" property on context
//                 try
//                 {
//                     query = _context.Billings.AsQueryable(); // TODO: remove/adjust if property name differs
//                 }
//                 catch
//                 {
//                     // If nothing matches, return helpful view telling dev to adjust model name
//                     ViewBag.Error = "No recognized financial DbSet found. Update ReportsController to point to your Billing/Payment DbSet.";
//                     return View(new FinancialReportViewModel { StartDate = startDate, EndDate = endDate, GroupBy = groupBy });
//                 }
//             }

//             // Materialize into anonymous accessible list — adjust field access to your entity properties
//             // The example below assumes your entity has: Date (DateTime) and Amount (decimal) and either IsExpense(bool) or Type(string)
//             var list = await query
//                 .Where(e => EF.Property<DateTime>(e, "Date") >= startDate.Value && EF.Property<DateTime>(e, "Date") <= endDate.Value)
//                 .Select(e => new
//                 {
//                     Date = EF.Property<DateTime>(e, "Date"),
//                     Amount = EF.Property<decimal>(e, "Amount"),
//                     // Try IsExpense boolean property first; if you have a Type string, change logic accordingly
//                     IsExpense = _context.Model.FindEntityType(query.ElementType.FullName)?.GetProperties().Any(p => p.Name == "IsExpense") == true
//                         ? EF.Property<bool>(e, "IsExpense")
//                         : (EF.Property<string>(e, "Type") ?? "").ToLower() == "expense"
//                 })
//                 .ToListAsync();

//             // Grouping by Month (default)
//             var vm = new FinancialReportViewModel
//             {
//                 StartDate = startDate,
//                 EndDate = endDate,
//                 GroupBy = groupBy
//             };

//             var groups = list
//                 .GroupBy(x =>
//                 {
//                     if (groupBy?.Equals("Year", StringComparison.OrdinalIgnoreCase) == true)
//                         return x.Date.Year.ToString();
//                     if (groupBy?.Equals("Quarter", StringComparison.OrdinalIgnoreCase) == true)
//                     {
//                         var q = ((x.Date.Month - 1) / 3) + 1;
//                         return $"{x.Date.Year}-Q{q}";
//                     }
//                     // Month default
//                     return x.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
//                 })
//                 .OrderBy(g => g.Key)
//                 .ToList();

//             foreach (var g in groups)
//             {
//                 var income = g.Where(r => !r.IsExpense).Sum(r => r.Amount);
//                 var expense = g.Where(r => r.IsExpense).Sum(r => r.Amount);
//                 vm.Labels.Add(g.Key);
//                 vm.IncomeValues.Add(income);
//                 vm.ExpenseValues.Add(expense);
//                 vm.ProfitValues.Add(income - expense);
//                 vm.Points.Add(new FinancialPoint { Label = g.Key, Income = income, Expense = expense });
//                 vm.TotalIncome += income;
//                 vm.TotalExpense += expense;
//             }
//             vm.TotalProfit = vm.TotalIncome - vm.TotalExpense;

//             return View(vm);
//         }

//         // JSON endpoint for async refresh (optional)
//         [HttpGet]
//         public async Task<IActionResult> Data(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "Month")
//         {
//             var result = await Index(startDate, endDate, groupBy) as ViewResult;
//             if (result?.Model is FinancialReportViewModel vm) return Json(vm);
//             return BadRequest();
//         }

//         // CSV Export
//         public async Task<IActionResult> ExportCsv(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "Month")
//         {
//             var result = await Index(startDate, endDate, groupBy) as ViewResult;
//             if (result?.Model is FinancialReportViewModel vm)
//             {
//                 var sb = new StringBuilder();
//                 sb.AppendLine("Period,Income,Expense,Profit");
//                 foreach (var p in vm.Points)
//                 {
//                     sb.AppendLine($"{p.Label},{p.Income},{p.Expense},{p.Profit}");
//                 }
//                 var bytes = Encoding.UTF8.GetBytes(sb.ToString());
//                 return File(bytes, "text/csv", $"financial-report-{DateTime.UtcNow:yyyyMMdd}.csv");
//             }
//             return BadRequest();
//         }
//     }
// }