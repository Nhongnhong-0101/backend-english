using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IVocabRepository
    {
        public Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync(string vocab);
        public Task<IEnumerable<Vocab>> GetAllVocabAsync();
    }
}

