using Bookify.Application.Common.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;

namespace Bookify.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepository;

        public VillaController(IVillaRepository villaRepository)
        {
            _villaRepository = villaRepository;
        }
        public IActionResult Index()
        {
            var villaList = _villaRepository.GetAll();
            return View(villaList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if(ModelState.IsValid)
            {
                _villaRepository.Add(villa);
                _villaRepository.Save();
                TempData["success"] = "The villa has been created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be created.";
            return View(villa);
        }

        public IActionResult Update(int? villaId)
        {
            if(villaId == 0 || villaId is null)
            {
                return RedirectToAction("Error", "Home");
            } 

            Villa? villa = _villaRepository.Get(u => u.Id == villaId);
            if(villa is null)
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
                _villaRepository.Update(villa);
                _villaRepository.Save();
                TempData["success"] = "The villa has been updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be updated.";
            return View(villa);
        }

        public IActionResult Delete(int? villaId)
        {
            if (villaId == 0 || villaId is null)
            {
                return RedirectToAction("Error", "Home");
            }

            Villa? villa = _villaRepository.Get(u => u.Id == villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaFromDb = _villaRepository.Get(u => u.Id == villa.Id);
            if(villaFromDb is not null)
            {
                _villaRepository.Remove(villaFromDb);
                _villaRepository.Save();
                TempData["success"] = "The villa has been deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be deleted.";
            return View(villa);
        }
    }
}


