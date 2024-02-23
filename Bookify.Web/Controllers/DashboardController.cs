using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

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

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double currentMonthCount, double previousMonthCount)
        {
            int increaseDecreaseRatio = 100;

            if (previousMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - previousMonthCount) / previousMonthCount * 100);
            }

            RadialBarChartVM radialBarChartVM = new()
            {
                TotalCount = totalCount,
                CountInCurrentMonth = Convert.ToInt32(currentMonthCount),
                HasRatioIncreased = currentMonthCount > previousMonthCount,
                Series = new int[] { increaseDecreaseRatio }
            };

            return radialBarChartVM;
        }
    }
}
