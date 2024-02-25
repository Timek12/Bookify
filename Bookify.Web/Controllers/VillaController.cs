using Bookify.Application.Common.Interfaces;
using Bookify.Application.Services.Interface;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villaList = _villaService.GetAllVillas();
            return View(villaList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (ModelState.IsValid)
            {
                _villaService.CreateVilla(villa);

                TempData["success"] = "The villa has been created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be created.";
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            if (villaId == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            Villa? villa = _villaService.GetVillaById(villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid)
            {
                _villaService.UpdateVilla(villa);

                TempData["success"] = "The villa has been updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be updated.";
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            if (villaId == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            Villa? villa = _villaService.GetVillaById(villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            bool isVillaDeleted = _villaService.DeleteVilla(villa.Id);
            if(isVillaDeleted)
            {
                TempData["success"] = "The villa has been deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
               
            TempData["error"] = "The villa could not be deleted.";
            return View(villa);
        }
    }
}


