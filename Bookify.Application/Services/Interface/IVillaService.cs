using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Services.Interface
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas(string? includeProperty = null);
        Villa GetVillaById(int id, string? includeProperty = null);
        void CreateVilla(Villa villa);
        void UpdateVilla(Villa villa);
        bool DeleteVilla(int id);

        IEnumerable<Villa> GetAvailableVillasByDate(int nights, DateOnly checkInDate);
        bool IsVillaAvailableByDate(int villaId, int nights,  DateOnly checkInDate);
    }
}
