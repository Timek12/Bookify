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

namespace Bookify.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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

            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "AmenityList"),
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
            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);
            
            if(roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";

                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

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

            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentLinkId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.Status == SD.StatusPending)
            {
                // we need to confirm if payment was successful
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId) 
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            if(bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll(u => u.VillaId == bookingFromDb.VillaId && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
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
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == id, includeProperties: "User,Villa");

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

            table.ResetCells(2, 4);

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

            if(downloadType == "word")
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
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking has been updated successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking has been completed successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["success"] = "Booking has been cancelled successfully!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(u => u.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Booking.GetAll(u => u.VillaId == villaId && u.Status == SD.StatusCheckedIn).Select(u => u.VillaNumber);

            foreach(var villaNumber in villaNumbers)
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

            if (User.IsInRole(SD.Role_Admin))
            {
                bookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                bookings = _unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = bookings });
        }

        #endregion

    }
}
