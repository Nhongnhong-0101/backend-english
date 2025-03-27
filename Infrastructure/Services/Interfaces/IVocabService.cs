using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IVocabService
    {
        public Task<Vocab> GetFullMeaningsVocabAsync(string vocab);
        public Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync( string vocab);

    }
}
