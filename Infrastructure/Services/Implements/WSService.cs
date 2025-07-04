using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class WSService : IWSService
    {
        private readonly IWSRepository wSRepository;

        public WSService(IWSRepository wSRepository)
        {
            this.wSRepository = wSRepository;
        }

        public async Task<WordSet?> AddNewWordSetAsync(WordSet wordSet)
        {
            if (wordSet == null)
                throw new ArgumentNullException(nameof(wordSet), "WordSet cannot be null");

            if (string.IsNullOrWhiteSpace(wordSet.nameSet))
                throw new ArgumentException("WordSet name cannot be empty", nameof(wordSet.nameSet));

            return await wSRepository.AddNewWordSetAsync(wordSet);
        }

        public async Task<bool> AddVocabsToWSAsync(List<VocabWS> vocabs, Guid wsId)
        {
            if (wsId == Guid.Empty)
            {
                throw new ArgumentException("WordSet ID cannot be empty");
            }
            return await wSRepository.AddVocabsToWSAsync(vocabs, wsId);
        }

        public async Task DeleteWordSetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("WordSet ID cannot be empty");
            }

            await wSRepository.DeleteWordSetByIdAsync(id);
        }

        public async Task<IEnumerable<VocabWS>> GetSavedWordsOfAccount(Guid accountId)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("account ID cannot be empty");
            }

            return await wSRepository.GetSavedWordsWSAsync(accountId);
        }

        public async Task<IEnumerable<VocabWS>> GetVocabsOfWSAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("WordSet ID cannot be empty");
            }

            return await wSRepository.GetVocabsOfWSAsync(id);
        }

        public async Task<WordSet?> GetWordSetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("WordSet ID cannot be empty");
            }

            return await wSRepository.GetWordSetByIdAsync(id);
        }

        public async Task<IEnumerable<WordSet>> GetWordSetsOfAccountAsync(Guid accountId)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("account ID cannot be empty");
            }

            return await wSRepository.GetWordSetsOfAccountAsync(accountId);
        }

        public async Task<bool> SaveVocabsToSavedWSAsync(List<VocabWS> vocabs, Guid accountId)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("account ID cannot be empty");
            }

            return await wSRepository.SaveVocabsToSavedWSAsync(vocabs, accountId);
        }

        public async Task<bool> UpdateVocabsToWSAsync(List<VocabWS> vocabs, Guid wsId)
        {
            if (wsId == Guid.Empty)
            {
                throw new ArgumentException("WordSet ID cannot be empty");
            }
            return await wSRepository.UpdaVocabsToWteSAsync(vocabs, wsId);
        }

        public async Task<WordSet?> UpdateWordSetAsync(WordSet wordSet)
        {
            if (wordSet == null)
                throw new ArgumentNullException(nameof(wordSet), "WordSet cannot be null");

            if (string.IsNullOrWhiteSpace(wordSet.nameSet))
                throw new ArgumentException("WordSet name cannot be empty", nameof(wordSet.nameSet));

            return await wSRepository.UpdateWordSetAsync(wordSet);
        }
    }
}
