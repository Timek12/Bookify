using Bookify.Application.Common.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }

        public IActionResult Index()
        {
            IEnumerable<Amenity> amenityList = _unitOfWork.Amenity.GetAll();
            return View(amenityList);
        }
    }
}
