using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        public void Update(Booking entity);
        public void UpdateStatus(int bookingId, string bookingStatus);
        public void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);
    }
}
