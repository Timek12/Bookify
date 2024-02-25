using Bookify.Application.Common.Interfaces;
using Bookify.Application.Common.Utility;
using Bookify.Web.Models;
using Bookify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Syncfusion.Presentation;
using System.Diagnostics;

namespace Bookify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };

            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList").ToList();
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();
            
            foreach(var villa in villaList) 
            {
                int roomsAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomsAvailable > 0;
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList", homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.Villa.GetAll(includeProperties: "AmenityList").FirstOrDefault(u => u.Id == id);
            if(villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/Exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if(shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy: {0} adults", villa.Occupancy);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.Sqft);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Price: {0}/night", villa.Price.ToString("c"));
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                List<string> listItems = villa.AmenityList.Select(x => x.Name).ToList();

                shape.TextBody.Text = "";

                foreach(var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 12;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "applicaiton/pptx", "VillaDetails.pptx");

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
