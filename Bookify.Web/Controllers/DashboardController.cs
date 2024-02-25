using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bookify.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending && u.Status != SD.StatusPending);

            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate && u.BookingDate <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();

            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate && u.CreatedAt <= currentMonthStartDate);



            return Json(GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending && u.Status != SD.StatusPending);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));

            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);

            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate && u.BookingDate <= currentMonthStartDate).Sum(u => u.TotalCost);

            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBookigPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
                (u.Status != SD.StatusPending && u.Status != SD.StatusPending));

            var customerWithOneBooking = totalBookings.GroupBy(u => u.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto PieChartDto = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return Json(PieChartDto);
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
                u.BookingDate <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });

            var customerData = _unitOfWork.User.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
                u.CreatedAt <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });

            var leftJoin = bookingData.GroupJoin(customerData,
                booking => booking.DateTime, customer => customer.DateTime,
                (booking, customer) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
                });

            var rightJoin = customerData.GroupJoin(bookingData,
                customer => customer.DateTime, booking => booking.DateTime, 
                (customer, booking) => new 
                {
                    customer.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount
                });

            var mergedData = leftJoin.Union(rightJoin)
                .OrderBy(x => x.DateTime)
                .ToList();

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("dd/MM/yyyy")).ToArray();

            List<ChartData> chartData = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Customers",
                    Data = newCustomerData
                }

            };

            LineChartDto LineChartDto = new()
            {
                Categories = categories,
                Series = chartData
            };

            return Json(LineChartDto);
        }

        private static RadialBarChartDto GetRadialChartDataModel(int totalCount, double currentMonthCount, double previousMonthCount)
        {
            int increaseDecreaseRatio = 100;

            if (previousMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - previousMonthCount) / previousMonthCount * 100);
            }

            RadialBarChartDto RadialBarChartDto = new()
            {
                TotalCount = totalCount,
                CountInCurrentMonth = Convert.ToInt32(currentMonthCount),
                HasRatioIncreased = currentMonthCount > previousMonthCount,
                Series = new int[] { increaseDecreaseRatio }
            };

            return RadialBarChartDto;
        }
    }
}
