using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IIdiomService
    {
        public Task<IEnumerable<Idiom>> GetAllIdiom();
        public Task<Idiom> AddNewIdiom(Idiom idiom);
    }
}
