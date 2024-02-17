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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
            if (ModelState.IsValid)
            {
                if (villa.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be created.";
            return View(villa);
        }

        public IActionResult Update(int? villaId)
        {
            if (villaId == 0 || villaId is null)
            {
                return RedirectToAction("Error", "Home");
            }

            Villa? villa = _unitOfWork.Villa.Get(u => u.Id == villaId);
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
                if (villa.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");

                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }


                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();
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
            if (villaFromDb is not null)
            {
                if (!string.IsNullOrEmpty(villaFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
             
                _unitOfWork.Villa.Remove(villaFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "The villa could not be deleted.";
            return View(villa);
        }
    }
}


