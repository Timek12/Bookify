using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Services.Interface
{
    public interface IVillaNumberService
    {
        IEnumerable<VillaNumber> GetAllVillaNumbers(string? includeProperty = null);
        VillaNumber GetVillaNumberById(int id, string? includeProperty = null);
        void CreateVillaNumber(VillaNumber villaNumber);
        void UpdateVillaNumber(VillaNumber villaNumber);
        bool DeleteVillaNumber(int id);

        bool CheckVilaNumberExists(int villa_Number);
    }
}
