﻿using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking has been updated successfully!";
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
