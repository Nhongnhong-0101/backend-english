using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IWSService
    {
        Task<WordSet?> GetWordSetByIdAsync(Guid id);
        Task<WordSet?> AddNewWordSetAsync(WordSet wordSet);
        Task<WordSet?> UpdateWordSetAsync(WordSet wordSet);
        Task<IEnumerable<Vocab>> GetVocabsOfWSAsync(Guid id);
        Task<bool> AddVocabsToWSAsync(List<Vocab> vocabs, Guid wsId);
        Task<IEnumerable<WordSet>> GetWordSetsOfAccountAsync(Guid accountId);
        Task DeleteWordSetByIdAsync(Guid id);
    }
}
