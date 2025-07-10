using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IVerbService
    {
        public Task<IEnumerable<IrregularVerb>> GetAllAsync();
    }
}
