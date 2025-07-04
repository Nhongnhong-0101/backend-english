using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IWSRepository
    {
        Task<WordSet?> GetWordSetByIdAsync( Guid id);
        Task<WordSet?> AddNewWordSetAsync( WordSet wordSet );
        Task<WordSet?> UpdateWordSetAsync( WordSet wordSet );
        Task<IEnumerable<VocabWS>> GetVocabsOfWSAsync( Guid id );
        Task<bool> AddVocabsToWSAsync(List<VocabWS> vocabs, Guid wsId );
        Task<bool> UpdaVocabsToWteSAsync(List<VocabWS> vocabs, Guid wsId );
        Task<IEnumerable<WordSet>> GetWordSetsOfAccountAsync(Guid accountId);
        Task DeleteWordSetByIdAsync( Guid id );
        Task<bool> SaveVocabsToSavedWSAsync (List<VocabWS> vocabs, Guid accountId);
        Task <IEnumerable<VocabWS>> GetSavedWordsWSAsync(Guid accountId);
    }
}
