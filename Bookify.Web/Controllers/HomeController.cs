using Bookify.Application.Common.Interfaces;
using Bookify.Web.Models;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bookify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };

            return View(homeVM);
        }

        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {
            homeVM.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList");
            foreach(var villa in homeVM.VillaList)
            {
                if(villa.Id % 2 == 0)
                {
                    villa.IsAvailable = false;
                }
            }

            return View(homeVM);
        }

        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList").ToList();
            foreach(var villa in villaList) 
            {
                if (villa.Id % 2 == 0)
                {
                    villa.IsAvailable = false;
                }
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
