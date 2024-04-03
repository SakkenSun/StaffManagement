using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Models;

namespace StaffManagement.Controllers
{
    public class StaffsController : Controller
    {
        private readonly StaffManagementContext _context;

        public StaffsController(StaffManagementContext context)
        {
            _context = context;
        }

        // GET: Staffs
        public async Task<IActionResult> Index(string searchBy, string search)
        {
            if (searchBy == "Id")
            {
                return View(await _context.Staff.Where(x => x.Id == search || search == null).ToListAsync());
            }
            else if (searchBy == "Fullname")
            {
                return View(await _context.Staff.Where(x => x.Fullname.StartsWith(search) || search == null).ToListAsync());
            }
            else if (searchBy == "Gender")
            {
                return View(await _context.Staff.Where(x => x.Gender == search || search == null).ToListAsync());
            }
            return View(await _context.Staff.ToListAsync());
        }

        // GET: Staffs/Details
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Staffs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Staffs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fullname,BirthDate,Gender")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staffs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        // POST: Staffs/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Fullname,BirthDate,Gender")] Staff staff)
        {
            if (id != staff.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staffs/Delete
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // POST: Staffs/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                _context.Staff.Remove(staff);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(string id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }


        [HttpGet]
        public IActionResult Export(string format)
        {
            if (format == "excel")
            {
                // Use ClosedXML to generate Excel file
                return File(GenerateExcel(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "staff_data.xlsx");
            }
            else if (format == "pdf")
            {
                // Use iTextSharp to generate PDF file
                return File(GeneratePdf(), "application/pdf", "staff_data.pdf");
            }
            else
            {
                return BadRequest("Invalid export format");
            }
        }

        private byte[] GenerateExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Staff Data");

                // Add headers to the first row
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Fullname";
                worksheet.Cell(1, 3).Value = "BirthDate";
                worksheet.Cell(1, 4).Value = "Gender";

                int row = 2;
                foreach (var staff in _context.Staff)
                {
                    worksheet.Cell(row, 1).Value = staff.Id;
                    worksheet.Cell(row, 2).Value = staff.Fullname;
                    worksheet.Cell(row, 3).Value = staff.BirthDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = staff.Gender;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }

            }
        }

        private byte[] GeneratePdf()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var document = new Document(PageSize.A4))
                {
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Add a title to the PDF
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var titleParagraph = new Paragraph("Staff Data", titleFont);
                    titleParagraph.Alignment = Element.ALIGN_CENTER;
                    document.Add(titleParagraph);

                    // Create a PDF table
                    var table = new PdfPTable(4);
                    table.WidthPercentage = 80;

                    // Add headers to the table
                    table.AddCell(new Phrase("Id", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    table.AddCell(new Phrase("Fullname", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    table.AddCell(new Phrase("BirthDate", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    table.AddCell(new Phrase("Gender", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));

                    // Write staff data to table rows
                    foreach (var staff in _context.Staff)
                    {
                        table.AddCell(new Phrase(staff.Id.ToString()));
                        table.AddCell(new Phrase(staff.Fullname));
                        table.AddCell(new Phrase(staff.BirthDate.ToString("yyyy-MM-dd")));
                        table.AddCell(new Phrase(staff.Gender));
                    }

                    document.Add(table);
                    document.Close();
                }

                return memoryStream.ToArray();
            }
        }
    }
}
