using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);
        Booking GetBookingById(int id);
        IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilterList = "");

        public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        public void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);
    }
}
