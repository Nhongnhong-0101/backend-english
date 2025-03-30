using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Reponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class VocabService : IVocabService
    {
        private readonly IVocabRepository vocabRepository;
        private readonly IVSMeaningRepository vSMeaningRepository; 
        public VocabService(IVocabRepository vocabRepository, IVSMeaningRepository vSMeaningRepository)
        {
            this.vocabRepository = vocabRepository;
            this.vSMeaningRepository = vSMeaningRepository;
        }
        public async Task<VocabResponse> GetFullMeaningsVocabAsync(string vocab)
        {
            try
            {
                vocab = vocab.Trim();
                var result = new List<VocabSubMeaning>();
                VocabResponse response = new VocabResponse();
                response.Vocab = vocab;

                //get meaning from table vocab 
                var resV = await vocabRepository.GetShortMeaningVocabAsync(vocab);
                var resVS = new List<VocabSubMeaning>();
                //get other meanings from table vocab_sub_meaning
                foreach (var item in resV) {
                    var task = await vSMeaningRepository.GetSubMeaningByVocabIdAsync(item.vocabId);
                    resVS = task.ToList();
                    if (resVS != null && resVS.Count > 0)
                    {
                        result.AddRange(resVS);
                    }
                }
                //create a map to save meaning and response to api
                foreach (var item in resV) {
                    var pos = item.partOfSpeech;
                    bool isExisted = false;
                    foreach (var existedPos in response.pos) {
                        isExisted = false;
                        if (existedPos != null && existedPos.Type == pos)
                        {
                            isExisted = true;
                            Meaning newMeaning = new Meaning();
                            newMeaning.MeaningEN = item.primaryMeaningEn;
                            newMeaning.MeaningVI = item.primaryMeaningVi;
                            
                            existedPos.Meanings.Add(newMeaning);
                        }
                    }
                    if (!isExisted)
                    {
                        PartOfSpeech newPos = new PartOfSpeech();
                        newPos.Type = pos;
                        newPos.AudioUrl = item.audioUrl;
                        newPos.Phonetic = item.phonetic;

                        Meaning newMeaning = new Meaning();
                        newMeaning.MeaningEN = item.primaryMeaningEn;
                        newMeaning.MeaningVI = item.primaryMeaningVi;

                        newPos.Meanings.Add(newMeaning);
                        response.pos.Add(newPos);

                    }
                }

                foreach (var item in resVS)
                {
                    var pos = item.partOfSpeech;
                    foreach (var existedPos in response.pos)
                    {
                        if (existedPos != null && existedPos.Type == pos)
                        {
                            Meaning newMeaning = new Meaning();
                            newMeaning.MeaningEN = item.meaningEn;
                            newMeaning.MeaningVI = item.meaningVi;
                            newMeaning.Example = item.example;

                            existedPos.Meanings.Add(newMeaning);
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public Task<VocabResponse> GetMeaningsOfVocabFromApi(string vocab)
        {
            try
            {
                VocabResponse response = new VocabResponse();
                response.Vocab = vocab;


                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync(string vocab)
        {
            throw new NotImplementedException();
        }
    }
}
