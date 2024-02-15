﻿using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;

namespace Bookify.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villaList = _db.Villas.ToList();
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
                _db.Villas.Add(villa);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(villa);
        }

        public IActionResult Update(int? villaId)
        {
            if(villaId == 0 || villaId == null)
            {
                return NotFound();
            } 

            Villa? villa = _db.Villas.FirstOrDefault(u => u.Id == villaId);
            if(villa == null)
            {
                return NotFound();
            }

            return View(villa);
        }
    }
}


