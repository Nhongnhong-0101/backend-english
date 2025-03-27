using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class VocabService : IVocabService
    {
        private readonly IVocabRepository _vocabRepository;
        public VocabService(IVocabRepository vocabRepository)
        {
            _vocabRepository = vocabRepository;
        }
        public Task<Vocab> GetFullMeaningsVocabAsync(string vocab)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync(string vocab)
        {
            throw new NotImplementedException();
        }
    }
}
