using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Application.Services.Interface;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bookify.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {

            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            

            return Json(await _dashboardService.GetRegisteredUserChartData());
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            
            return Json(await _dashboardService.GetRevenueChartData());
        }

        public async Task<IActionResult> GetBookigPieChartData()
        {
            

            return Json(await _dashboardService.GetBookigPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            

            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }
    }
}
