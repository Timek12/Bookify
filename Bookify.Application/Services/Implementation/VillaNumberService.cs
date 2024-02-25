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
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckVilaNumberExists(int villa_Number)
        {
            return _unitOfWork.VillaNumber.Any(u => u.Villa_Number == villa_Number);
        }

        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Add(villaNumber);
            _unitOfWork.Save();
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                VillaNumber? villaNumberFromDb = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id);
                if (villaNumberFromDb is not null)
                {
                    _unitOfWork.VillaNumber.Remove(villaNumberFromDb);
                    _unitOfWork.Save();
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbers(string? includeProperty = null)
        {
            if(includeProperty is not null)
            {
                return _unitOfWork.VillaNumber.GetAll(includeProperties: includeProperty);
            }
            
            return _unitOfWork.VillaNumber.GetAll();    
        }

        public VillaNumber GetVillaNumberById(int id, string? includeProperty = null)
        {
            if(includeProperty is not null)
            {
                return _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id, includeProperties: "Villa");
            }

            return _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id);
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Update(villaNumber);
            _unitOfWork.Save();
        }
    }
}
