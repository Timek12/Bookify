using Bookify.Application.Common.Utility;
using Bookify.Application.Services.Interface;
using Bookify.Domain.Entities;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;
        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            IEnumerable<Amenity> amenityList = _amenityService.GetAllAmenities(includeProperty: "Villa");
            return View(amenityList);
        }

        public IActionResult Create()
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _amenityService.CreateAmenity(amenityVM.Amenity);

                TempData["success"] = "The amenity has been created successfully!";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            TempData["error"] = "The amenity could not be created.";

            return View(amenityVM);
        }

        public IActionResult Update(int id)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _amenityService.GetAmenityById(id)
            };


            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Update(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _amenityService.UpdateAmenity(amenityVM.Amenity);
                TempData["success"] = "The amenity has been updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            TempData["error"] = "The amenity could not be updated.";

            return View(amenityVM);
        }

        public IActionResult Delete(int id)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity =_amenityService.GetAmenityById(id)
            };


            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            Amenity? amenityFromDb = _amenityService.GetAmenityById(amenityVM.Amenity.Id);
            if(amenityFromDb is not null)
            {
                _amenityService.DeleteAmenity(amenityFromDb.Id);
                TempData["success"] = "The amenity has been deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The amenity could not be deleted successfully!";
            return View(amenityVM);
        }
    }
}
