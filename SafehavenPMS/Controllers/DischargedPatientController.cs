using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Linq;
using System.Threading.Tasks;


namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class DischargedPatientController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public DischargedPatientController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
                   int? page = 1,
                   int? pageSize = 10,
                   string searchQuery = null,
                   string status = null,
                   string sortOrder = null,
                  string sortBy = null)
        {
            ViewBag.SortBy = sortBy ?? "";
            ViewBag.PageSize = pageSize ?? 10;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchQuery = searchQuery;

            // Do not filter by status here — return all records from DischargedPatients
            var query = _context.DischargedPatients
                .Include(a => a.Patient)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var q = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Patient.Firstname.ToLower().Contains(q) ||
                    a.Patient.Lastname.ToLower().Contains(q) ||
                    a.PatientId.ToString().Contains(q));
            }

            // Apply sorting: support Name, DateAdded (patient.CreatedAt) and default DischargeDate
            var asc = string.Equals(sortOrder, "ascending", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
            {
                query = asc
                    ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                    : query.OrderByDescending(a => a.Patient.Firstname).ThenByDescending(a => a.Patient.Lastname);
            }
            else if (string.Equals(sortBy, "DateAdded", StringComparison.OrdinalIgnoreCase))
            {
                // assumes Patient.CreatedAt exists; fallback to DischargeDate if null
                query = asc
                    ? query.OrderBy(a => a.Patient.CreatedAt)
                    : query.OrderByDescending(a => a.Patient.CreatedAt);
            }
            else
            {
                query = asc
                    ? query.OrderBy(a => a.DischargeDate)
                    : query.OrderByDescending(a => a.DischargeDate);
            }

            // Pagination and projection
            int totalItems = await query.CountAsync();
            ViewBag.TotalPatientCount = totalItems;
            int totalPages = (pageSize > 0 && pageSize.Value > 0) ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            List<SafehavenPMS.Models.DischargedPatient> dischargedPatients;
            if (pageSize == 0)
            {
                dischargedPatients = await query.ToListAsync();
            }
            else
            {
                dischargedPatients = await query
                    .Skip((currentPage - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }

            var viewModel = dischargedPatients.Select(a => new DischargedViewModel
            {
                DischargeId = a.DischargeId,
                PatientId = a.PatientId,
                Photo = a.Patient?.PhotoUrl,
                PatientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "Unknown",
                Reason = a.Reason,
                DischargedBy = a.CreatedBy,
                DischargeDate = a.DischargeDate,
                Status = a.Status,
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            // Redirect to this controller's Index so filtering/searching happens on discharged list
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize = 10,
                sortOrder = "descending"
            });
        }
        
        // POST: Patient/ReopenPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReopenPatient(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return NotFound();

            // Set status to NewIntake when reopening a discharged patient
            patient.PatientStatus = PatientStatusEnum.NewIntake.ToString();
            _context.Patients.Update(patient);

            //Remove the Patients From PatientDischarged Table
            var dischargedRecord = await _context.DischargedPatients
                .FirstOrDefaultAsync(d => d.PatientId == patientId);

            if (dischargedRecord != null)
            {
                _context.DischargedPatients.Remove(dischargedRecord);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient reopened to New Intake.";
            return RedirectToAction("Index", "DischargedPatient");
        }

        [HttpGet]
        public async Task<IActionResult> Discharge()
        {
            var patients = await _context.Patients
                .Where(p => p.PatientStatus == PatientStatusEnum.Admitted.ToString()
                         || p.PatientStatus == PatientStatusEnum.InTreatment.ToString()
                         || p.PatientStatus == PatientStatusEnum.NewIntake.ToString())
                .OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = $"{p.PatientId} - {p.Firstname} {p.Lastname}"
                })
                .ToListAsync();

            // AdmissionDate left at default until a patient is chosen
            var vm = new DischargePatientViewModel
            {
                DischargeDate = DateTime.Today,
                PatientOptions = patients
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(DischargePatientViewModel model)
        {
            if (model.PatientId == 0)
            {
                ModelState.AddModelError("PatientId", "Please select a patient.");
                return await RebuildAndReturn(model);
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Discharge));
            }

            // Auto-populate admission date (replace CreatedAt with actual AdmissionDate field if present)
            var admissionDate = patient.CreatedAt != null ? patient.CreatedAt.Date : DateTime.Today;
            model.AdmissionDate = admissionDate;

            if (!ModelState.IsValid)
                return await RebuildAndReturn(model, patient);

            bool hasUnpaid = false;
            if (_context.Invoices != null)
                hasUnpaid = await _context.Invoices.AnyAsync(i => i.PatientId == model.PatientId && i.Status == "Unpaid");

            if (hasUnpaid && !model.ProceedAnyway)
            {
                ModelState.AddModelError(string.Empty, "Patient has unpaid invoices. Confirm to proceed.");
                model.HasUnpaidInvoices = true;
                return await RebuildAndReturn(model, patient);
            }

            patient.PatientStatus = PatientStatusEnum.Discharged.ToString();

            // Mark related clinical forms as archived (Intake, Initial Assessment, Psychiatric Assessment)
            try
            {
                // Intake Forms
                if (_context.IntakeForms != null)
                {
                    var intakeForms = await _context.IntakeForms
                        .Where(f => f.PatientId == patient.PatientId)
                        .ToListAsync();

                    foreach (var f in intakeForms)
                    {
                        // Adjust property names if different in your models
                        var statusProp = f.GetType().GetProperty("Status");
                        var isArchivedProp = f.GetType().GetProperty("IsArchived");
                        if (isArchivedProp != null)
                            isArchivedProp.SetValue(f, true);
                        if (statusProp != null)
                            statusProp.SetValue(f, "Archived");
                    }
                }

                // Initial Assessments
                if (_context.InitialAssessmentForms != null)
                {
                    var initialAssessments = await _context.InitialAssessmentForms
                        .Where(f => f.PatientId == patient.PatientId)
                        .ToListAsync();

                    foreach (var f in initialAssessments)
                    {
                        var statusProp = f.GetType().GetProperty("Status");
                        var isArchivedProp = f.GetType().GetProperty("IsArchived");
                        if (isArchivedProp != null)
                            isArchivedProp.SetValue(f, true);
                        if (statusProp != null)
                            statusProp.SetValue(f, "Archived");
                    }
                }

                // Psychiatric Assessments
                if (_context.PsychiatricAssessments != null)
                {
                    var psych = await _context.PsychiatricAssessments
                        .Where(f => f.PatientId == patient.PatientId)
                        .ToListAsync();

                    foreach (var f in psych)
                    {
                        var statusProp = f.GetType().GetProperty("Status");
                        var isArchivedProp = f.GetType().GetProperty("IsArchived");
                        if (isArchivedProp != null)
                            isArchivedProp.SetValue(f, true);
                        if (statusProp != null)
                            statusProp.SetValue(f, "Archived");
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional: log ex
            }

            var record = new DischargedPatient
            {
                PatientId = patient.PatientId,
                AdmissionDate = admissionDate,
                DischargeDate = model.DischargeDate,
                Reason = model.Reason,
                Notes = model.Notes,
                Status = "Discharged",
                CreatedBy = User.Identity?.Name
            };

            _context.DischargedPatients.Add(record);
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient discharged.";
            return RedirectToAction(nameof(Index));

            async Task<IActionResult> RebuildAndReturn(DischargePatientViewModel vm, Patient patientEntity = null)
            {
                vm.PatientOptions = await _context.Patients
                    .Where(p => p.PatientStatus == PatientStatusEnum.Admitted.ToString()
                             || p.PatientStatus == PatientStatusEnum.InTreatment.ToString()
                             || p.PatientStatus == PatientStatusEnum.NewIntake.ToString())
                    .OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.PatientId} - {p.Firstname} {p.Lastname}"
                    })
                    .ToListAsync();

                if (patientEntity == null && vm.PatientId > 0)
                    patientEntity = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == vm.PatientId);

                if (patientEntity != null)
                {
                    vm.PatientName = $"{patientEntity.Firstname} {patientEntity.Lastname}";
                    vm.PatientNumber = patientEntity.PatientId.ToString();
                    vm.PhotoUrl = patientEntity.PhotoUrl;
                    vm.Sex = patientEntity.Sex;
                    vm.Address = patientEntity.Address;
                    vm.AdmissionDate = patientEntity.CreatedAt.Date;
                }
                return View("Discharge", vm);
            }
        }
    }
}
