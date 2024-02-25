using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookify.Application.Common.Interfaces;
using Bookify.Application.Services.Interface;
using Bookify.Domain.Entities;

namespace Bookify.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public void CreateVilla(Villa villa)
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
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? villaFromDb = _unitOfWork.Villa.Get(u => u.Id == id);
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

                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    

        public IEnumerable<Villa> GetAllVillas(string? includeProperty = null)
        {
            if(includeProperty is not null)
            {
                return _unitOfWork.Villa.GetAll(includeProperties: includeProperty);
            }

            return _unitOfWork.Villa.GetAll();
        }

        public Villa GetVillaById(int id, string? includeProperty = null)
        {
            if(includeProperty is not null)
            {
                return _unitOfWork.Villa.Get(u => u.Id == id, includeProperties: includeProperty);

            }

            return _unitOfWork.Villa.Get(u => u.Id == id);
        }

        public void UpdateVilla(Villa villa)
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
        }
    }
}
