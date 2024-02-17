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
        private readonly IUnitOfWork _unitOfWork;
        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaList = _unitOfWork.Villa.GetAll();
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
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Villa.Save();
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

            Villa? villa = _unitOfWork.Villa.Get(u => u.Id == villaId);
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
                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Villa.Save();
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

            Villa? villa = _unitOfWork.Villa.Get(u => u.Id == villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaFromDb = _unitOfWork.Villa.Get(u => u.Id == villa.Id);
            if(villaFromDb is not null)
            {
                _unitOfWork.Villa.Remove(villaFromDb);
                _unitOfWork.Villa.Save();
                TempData["success"] = "The villa has been deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be deleted.";
            return View(villa);
        }
    }
}


