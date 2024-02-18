using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Common.Interfaces
{
    public interface IAmenityRepository
    {
        void Update(Amenity entity);
    }
}
