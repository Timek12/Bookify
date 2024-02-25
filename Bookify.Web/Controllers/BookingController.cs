using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System;
using Syncfusion.Drawing;
using System.Security.Claims;
using Syncfusion.Pdf;
using Bookify.Application.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Bookify.Infrastructure.Repository;

namespace Bookify.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public BookingController(IBookingService bookingService, IVillaService villaService,
                                IVillaNumberService villaNumberService, IWebHostEnvironment webHostEnvironment,
                                UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            DateTime checkInDateTime = new DateTime(checkInDate.Year, checkInDate.Month, checkInDate.Day);

            DateTime switchedDateTime = new DateTime(checkInDateTime.Year, checkInDateTime.Day, checkInDateTime.Month);

            DateOnly switchedCheckInDate = DateOnly.FromDateTime(switchedDateTime);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _villaService.GetVillaById(villaId, includeProperty: "AmenityList"),
                CheckInDate = switchedCheckInDate,
                Nights = nights,
                CheckOutDate = switchedCheckInDate.AddDays(nights),
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            };

            booking.TotalCost = booking.Villa.Price * booking.Nights;

            return View(booking);
        }

        [Authorize, HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            if (!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
            {
                TempData["error"] = "Room has been sold out!";

                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _bookingService.CreateBooking(booking);

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        // Images = new List<string> { domain + villa.ImageUrl },
                    }
                },
                Quantity = 1,
            });


            var service = new SessionService();
            Session session = service.Create(options);

            _bookingService.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentLinkId);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);
            if (bookingFromDb.Status == SD.StatusPending)
            {
                // we need to confirm if payment was successful
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _bookingService.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                }
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == bookingFromDb.VillaId && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [Authorize]
        [HttpPost]
        public IActionResult GenerateInvoice(string downloadType, int id)
        {
            string basePath = _webHostEnvironment.WebRootPath;

            WordDocument document = new WordDocument();

            // Load the template
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            // Update Tempalte
            Booking bookingFromDb = _bookingService.GetBookingById(id);

            TextSelection textSelection = document.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;
            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PhoneNumber;

            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();

            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c");

            textSelection = document.Find("xx_booking_number", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID - " + bookingFromDb.Id.ToString();

            textSelection = document.Find("xx_booking_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFromDb.PaymentDate.ToShortDateString();

            WTable table = new(document);

            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingFromDb.VillaNumber > 0 ? 3 : 2;

            table.ResetCells(rows, 4);

            WTableRow row0 = table.Rows[0];

            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;

            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;

            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");

            row0.Cells[3].AddParagraph().AppendText("TOTAL");
            row0.Cells[3].Width = 80;

            WTableRow row1 = table.Rows[1];

            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;

            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;

            row1.Cells[2].AddParagraph().AppendText((bookingFromDb.TotalCost / bookingFromDb.Nights).ToString("c"));

            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;

            if (bookingFromDb.VillaNumber > 0)
            {
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;

            table.ApplyStyle("CustomStyle");

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);

            using DocIORenderer renderer = new();

            if (downloadType == "word")
            {
                MemoryStream stream = new();
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = renderer.ConvertToPDF(document);
                MemoryStream stream = new();
                pdfDocument.Save(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", "BookingDetails.pdf");
            }

        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckIn(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            TempData["success"] = "Booking has been updated successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            TempData["success"] = "Booking has been completed successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CancelBooking(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            TempData["success"] = "Booking has been cancelled successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == villaId);

            var checkedInVilla = _bookingService.GetCheckedInVillaNumbers(villaId);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;
        }

        #region API Calls

        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;

            string userId = "";

            if(string.IsNullOrEmpty(status))
            {
                status = "";
            }

            if (!User.IsInRole(SD.Role_Admin))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            bookings = _bookingService.GetAllBookings(userId, status);

            return Json(new { data = bookings });
        }

        #endregion

    }
}
