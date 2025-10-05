using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Helpers
{
    public static class BillingHelper
    {
        /// <summary>
        /// Generate monthly invoices for all patients.
        /// Includes all Billables dated within the month and a single Standard Monthly Fee line.
        /// If persist = true the invoices and lines are saved to the database.
        /// Returns list of created Invoice entities (detached if not persisted).
        /// </summary>
        public static async Task<List<Invoice>> GenerateMonthlyInvoicesAsync(
            SafehavenPMSContext db,
            int month,
            int year,
            decimal standardMonthlyFee,
            DateTime dueDate,
            string createdBy,
            bool persist = true)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            var periodStart = new DateTime(year, month, 1);
            var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

            // fetch patients (change filter if you only want admitted/active patients)
            var patients = await db.Patients.AsNoTracking().ToListAsync();

            var createdInvoices = new List<Invoice>();
            var now = DateTime.UtcNow;

            foreach (var p in patients)
            {
                // fetch billables for this patient within period
                var billables = await db.Billables
                    .Where(b => b.PatientId == p.PatientId
                                && b.DateAdded >= periodStart
                                && b.DateAdded <= periodEnd)
                    .OrderBy(b => b.Category)
                    .ThenBy(b => b.DateAdded)
                    .ToListAsync();

                // skip creating invoice if no billables and standard fee is zero
                if (!billables.Any() && standardMonthlyFee <= 0m)
                    continue;

                var invoice = new Invoice
                {
                    PatientId = p.PatientId,
                    Month = month,
                    Year = year,
                    DueDate = dueDate.Date,
                    CreatedAt = now,
                    CreatedBy = createdBy ?? "system",
                    InvoiceNumber = GenerateInvoiceNumber(p.PatientId, year, month)
                };

                // add billable items as invoice lines
                foreach (var b in billables)
                {
                    var line = new InvoiceLine
                    {
                        Category = b.Category,
                        Description = b.Description,
                        Quantity = b.Quantity,
                        UnitPrice = b.UnitPrice,
                        Amount = b.Amount,
                        ReferenceBillableId = b.BillableId,
                        DateAdded = b.DateAdded
                    };
                    invoice.Lines.Add(line);
                }

                // add standard monthly fee line (always one line)
                if (standardMonthlyFee > 0m)
                {
                    var feeLine = new InvoiceLine
                    {
                        Category = "StandardMonthlyFee",
                        Description = "Standard Monthly Program Fee",
                        Quantity = 1m,
                        UnitPrice = standardMonthlyFee,
                        Amount = standardMonthlyFee,
                        ReferenceBillableId = null,
                        DateAdded = now
                    };
                    invoice.Lines.Add(feeLine);
                }

                invoice.TotalAmount = invoice.Lines.Sum(l => l.Amount);

                if (persist)
                {
                    // detach navigation to avoid EF tracking problems when adding many
                    db.Invoices.Add(invoice);
                }

                createdInvoices.Add(invoice);
            }

            if (persist && createdInvoices.Any())
            {
                await db.SaveChangesAsync();

                // assign a stable sequential reference using the persisted InvoiceId
                // format: INV-000000{n}  => InvoiceId 1 => INV-0000001
                foreach (var inv in createdInvoices)
                {
                    inv.InvoiceNumber = $"INV-{inv.InvoiceId:D7}";
                    // mark modified so EF will persist the new invoice number
                    db.Entry(inv).Property(i => i.InvoiceNumber).IsModified = true;
                }

                await db.SaveChangesAsync();
            }

            return createdInvoices;
        }

        private static string GenerateInvoiceNumber(int patientId, int year, int month)
        {
            // simple deterministic invoice number used before persistence (keeps readability)
            return $"INV-{year}{month:00}-{patientId:D4}";
        }
    }
}
