// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using SafehavenPMS.Data;
// using SafehavenPMS.Models;

// namespace SafehavenPMS.Helpers
// {
//     public static class BillingHelper
//     {
//         // program: 35,000 per month for 3 months
//         public const decimal MonthlyFee = 35000m;
//         public const int ProgramMonths = 3;
//         public static decimal ProgramTotal => MonthlyFee * ProgramMonths;

//         public class PatientBalance
//         {
//             public int PatientId { get; set; }
//             public string PatientName { get; set; }
//             public decimal ProgramCharge { get; set; }
//             public decimal Billables { get; set; }
//             public decimal Medications { get; set; }
//             public decimal Invoices { get; set; }
//             public decimal Payments { get; set; }
//             public decimal Balance => ProgramCharge + Billables + Medications + Invoices - Payments;
//         }

//         // Calculates balances for all patients. Assumes entity sets: Patient, Billable, Medication, Invoice, Payment.
//         public static async Task<List<PatientBalance>> CalculateAllPatientBalancesAsync(SafehavenPMSContext ctx)
//         {
//             var patients = await ctx.Set<Patient>()
//                                     .AsNoTracking()
//                                     .Select(p => new { p.PatientId, Name = (p.Firstname + " " + p.Lastname).Trim() })
//                                     .ToListAsync();

//             var ids = patients.Select(p => p.PatientId).ToList();

//             var billables = await ctx.Set<Billable>()
//                                      .Where(b => b.PatientId.HasValue && ids.Contains(b.PatientId.Value))
//                                      .GroupBy(b => b.PatientId)
//                                      .Select(g => new { Id = g.Key, Total = g.Sum(x => (decimal?)x.Total) ?? 0m })
//                                      .ToListAsync();

//             var meds = await ctx.Set<MedicationOrder>()
//                                 .Where(m => m.PatientId.HasValue && ids.Contains(m.PatientId.Value))
//                                 .GroupBy(m => m.PatientId)
//                                 .Select(g => new { Id = g.Key, Total = g.Sum(x => (decimal?)x.Medicine.Price) ?? 0m })
//                                 .ToListAsync();

//             var invoices = await ctx.Set<Invoice>()
//                                     .Where(i => i.PatientId.HasValue && ids.Contains(i.PatientId.Value))
//                                     .GroupBy(i => i.PatientId)
//                                     .Select(g => new { Id = g.Key, Total = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m })
//                                     .ToListAsync();

//             var payments = await ctx.Set<Payment>()
//                                     .Where(p => p.PatientId.HasValue && ids.Contains(p.PatientId.Value))
//                                     .GroupBy(p => p.PatientId)
//                                     .Select(g => new { Id = g.Key, Total = g.Sum(x => (decimal?)x.Amount) ?? 0m })
//                                     .ToListAsync();

//             var result = new List<PatientBalance>(patients.Count);
//             foreach (var p in patients)
//             {
//                 var bill = billables.FirstOrDefault(x => x.Id == p.Id)?.Total ?? 0m;
//                 var med = meds.FirstOrDefault(x => x.Id == p.Id)?.Total ?? 0m;
//                 var inv = invoices.FirstOrDefault(x => x.Id == p.Id)?.Total ?? 0m;
//                 var pay = payments.FirstOrDefault(x => x.Id == p.Id)?.Total ?? 0m;

//                 result.Add(new PatientBalance
//                 {
//                     PatientId = p.Id,
//                     PatientName = p.Name,
//                     ProgramCharge = ProgramTotal,
//                     Billables = bill,
//                     Medications = med,
//                     Invoices = inv,
//                     Payments = pay
//                 });
//             }

//             return result;
//         }

//         // Single patient balance
//         public static async Task<PatientBalance> CalculatePatientBalanceAsync(SafehavenPMSContext ctx, int patientId)
//         {
//             var p = await ctx.Set<Patient>()
//                              .AsNoTracking()
//                              .Where(x => x.Id == patientId)
//                              .Select(x => new { x.Id, Name = (x.FirstName + " " + x.LastName).Trim() })
//                              .FirstOrDefaultAsync();
//             if (p == null) return null;

//             var bill = await ctx.Set<Billable>().Where(b => b.PatientId == patientId).SumAsync(b => (decimal?)b.Amount) ?? 0m;
//             var med = await ctx.Set<MedicationOrder>().Where(m => m.PatientId == patientId).SumAsync(m => (decimal?)m.Amount) ?? 0m;
//             var inv = await ctx.Set<Invoice>().Where(i => i.PatientId == patientId).SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
//             var pay = await ctx.Set<Payment>().Where(x => x.PatientId == patientId).SumAsync(x => (decimal?)x.Amount) ?? 0m;

//             return new PatientBalance
//             {
//                 PatientId = p.Id,
//                 PatientName = p.Name,
//                 ProgramCharge = ProgramTotal,
//                 Billables = bill,
//                 Medications = med,
//                 Invoices = inv,
//                 Payments = pay
//             };
//         }
//     }
// }