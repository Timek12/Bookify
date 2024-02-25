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
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {
                Amenity? amenityFromDb = _unitOfWork.Amenity.Get(u => u.Id == id);
                if (amenityFromDb is not null)
                {
                    _unitOfWork.Amenity.Remove(amenityFromDb);
                    _unitOfWork.Save();
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<Amenity> GetAllAmenities(string? includeProperty = null)
        {
            if (includeProperty is not null)
            {
                return _unitOfWork.Amenity.GetAll(includeProperties: includeProperty);
            }

            return _unitOfWork.Amenity.GetAll();
        }

        public Amenity GetAmenityById(int id, string? includeProperty = null)
        {
            if(includeProperty is not null)
            {
                return _unitOfWork.Amenity.Get(u => u.Id == id, includeProperties: "Villa");
            }

            return _unitOfWork.Amenity.Get(u => u.Id == id);
        }

        public void UpdateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Save();
        }
    }
}
