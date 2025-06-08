using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IVSMeaningRepository
    {
        public Task<IEnumerable<VocabSubMeaning>> GetSubMeaningByVocabIdAsync(Guid vocabId);
        public Task<VocabSubMeaning?> GetSubMeaningByIdAsync(Guid id);

    }
}
