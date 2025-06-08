using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Reponses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class VocabService : IVocabService
    {
        private readonly IVocabRepository vocabRepository;
        private readonly IVSMeaningRepository vSMeaningRepository;
        private readonly IConfiguration configuration;
        public VocabService(IVocabRepository vocabRepository, IVSMeaningRepository vSMeaningRepository, IConfiguration configuration)
        {
            this.vocabRepository = vocabRepository;
            this.vSMeaningRepository = vSMeaningRepository;
            this.configuration = configuration;
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
                
                if(resV != null && resV.Count() >0) {
                var resVS = new List<VocabSubMeaning>();
                    //get other meanings from table vocab_sub_meaning
                    foreach (var item in resV)
                    {
                        var task = await vSMeaningRepository.GetSubMeaningByVocabIdAsync(item.vocabId);
                        resVS = task.ToList();
                        if (resVS != null && resVS.Count > 0)
                        {
                            result.AddRange(resVS);
                        }
                    }
                response = await ProcessToResponse(resV.ToList(), result);
                }
                else
                {
                    response = await GetMeaningsOfVocabFromApi(vocab);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<VocabResponse> GetMeaningsOfVocabFromApi(string word)
        {
            List<Vocab> vocabs = new List<Vocab>();
            List<VocabSubMeaning> vocabSubs = new List<VocabSubMeaning>();
            VocabResponse vocabResponse = new VocabResponse();
            vocabResponse.Vocab = word;
            try
            {
                if (word != null)
                {
                    string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync(url);
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var json = await response.Content.ReadAsStringAsync();
                                var res = JsonDocument.Parse(json);
                                if (res != null)
                                {
                                    string pos, phonetic, audio;
                                    audio = "";
                                    //get other meanings to save as SubVocabMeaning
                                    var listOfMeans = new List<Dictionary<string, object>>();
                                    //process all meanings
                                    int numberOfWords = res.RootElement.GetArrayLength();
                                    JsonElement meanings = new JsonElement();
                                    string enDefinition;
                                    int i = 0;
                                    Dictionary<string, object> posNDef = new Dictionary<string, object>();
                                    while (i < numberOfWords)
                                    {
                                        //get audio 
                                        phonetic = res.RootElement[i].GetProperty("phonetic").ToString();
                                        JsonElement phonetics = res.RootElement[i].GetProperty("phonetics");
                                        foreach (var p in phonetics.EnumerateArray())
                                        {
                                            if (string.IsNullOrEmpty(phonetic) && p.TryGetProperty("text", out JsonElement curPhonetic))
                                            {
                                                phonetic = !string.IsNullOrEmpty(curPhonetic.ToString()) ? curPhonetic.ToString() : "";
                                            }
                                            if (p.TryGetProperty("audio", out JsonElement audioUrl))
                                            {
                                                audio = !string.IsNullOrEmpty(audioUrl.GetString()) ? audioUrl.ToString() : "";
                                            }
                                            if (!string.IsNullOrEmpty(phonetic) && !string.IsNullOrEmpty(audio)) { break; }
                                        }

                                        meanings = res.RootElement[i].GetProperty("meanings");
                                        int processingMean = 0;
                                        while (processingMean < meanings.GetArrayLength())
                                        {
                                            //get definitions
                                            pos = meanings[processingMean].GetProperty("partOfSpeech").ToString();//key
                                            List<Dictionary<string, string>> listDefsOfPos = new List<Dictionary<string, string>>();  //value     

                                            JsonElement definitions = meanings[processingMean].GetProperty("definitions");
                                            enDefinition = "";
                                            string example = "";
                                            int processingDef = 0;
                                            while (processingDef < definitions.GetArrayLength() && processingDef < 2)
                                            {
                                                if (definitions[processingDef].TryGetProperty("definition", out JsonElement def))
                                                {
                                                    enDefinition = !string.IsNullOrEmpty(def.ToString()) ? def.ToString() : "";
                                                }
                                                if (definitions[processingDef].TryGetProperty("example", out JsonElement exampleElement))
                                                {
                                                    example = !string.IsNullOrEmpty(exampleElement.ToString()) ? exampleElement.ToString() : "";
                                                }
                                                if (!string.IsNullOrEmpty(enDefinition.ToString()))
                                                {
                                                    string viDefinition = await TranslateToVN(enDefinition);
                                                    Dictionary<string, string> resultDef = new Dictionary<string, string>();
                                                    resultDef["enDefinition"] = enDefinition;
                                                    resultDef["viDefinition"] = viDefinition;
                                                    resultDef["example"] = example;

                                                    listDefsOfPos.Add(resultDef);
                                                    bool isVocab = false;
                                                    if (processingDef == 0)
                                                    {
                                                        isVocab = vocabs.Find(e => e.partOfSpeech == pos) != null ? false : true;
                                                        if (isVocab)
                                                        {
                                                            Vocab v = new Vocab();
                                                            v.vocabId = Guid.NewGuid();
                                                            v.vocab = word;
                                                            v.partOfSpeech = pos;
                                                            v.phonetic = phonetic;
                                                            v.audioUrl = audio;
                                                            v.primaryMeaningEn = enDefinition;
                                                            v.primaryMeaningVi = viDefinition;
                                                            vocabs.Add(v);
                                                        }
                                                    }

                                                    if (processingDef > 0 || !isVocab)
                                                    {
                                                        VocabSubMeaning vocabSub = new VocabSubMeaning();
                                                        vocabSub.meaningId = Guid.NewGuid();
                                                        vocabSub.partOfSpeech = pos;
                                                        vocabSub.audioUrl = audio;
                                                        vocabSub.meaningEn = enDefinition;
                                                        vocabSub.meaningVi = viDefinition;
                                                        vocabSub.example = example;
                                                        Vocab father = vocabs.Find(v => v.partOfSpeech == vocabSub.partOfSpeech);
                                                        if (father != null)
                                                        {
                                                            vocabSub.vocabId = father.vocabId;
                                                            vocabSubs.Add(vocabSub);
                                                        }
                                                    }
                                                    processingDef++;
                                                }
                                            }

                                            //add to posNDef if pos isn't in this dictionary 
                                            if (!posNDef.ContainsKey(pos))
                                            {
                                                posNDef[pos] = new Dictionary<string, object>
                                                {
                                                    {"phonetic", phonetic},
                                                    {"audio", audio },
                                                    {"definitions", listDefsOfPos}
                                                };
                                            }
                                            processingMean++;
                                        }
                                        i++;
                                    }
                                    listOfMeans.Add(posNDef);
                                    Dictionary<string, object> result = new Dictionary<string, object>();
                                    result["word"] = res.RootElement[0].GetProperty("word").ToString();
                                    result["meanings"] = listOfMeans;
                                }
                            }
                        }
                    }
                }
                vocabResponse = await ProcessToResponse(vocabs, vocabSubs);
                return vocabResponse;
            }
            catch (Exception e)
            {
                throw new Exception (message: "Fails from service while trying to get meaning: "+ e.Message);
            }
        }
        public Task<VocabResponse> ProcessToResponse(List<Vocab> vocabs, List<VocabSubMeaning> vocabSubs)
        {
            VocabResponse response = new VocabResponse();
            response.Vocab = vocabs.First().vocab;
            foreach (var item in vocabs)
            {
                var pos = item.partOfSpeech;
                bool isExisted = false;
                foreach (var existedPos in response.pos)
                {
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

            foreach (var item in vocabSubs)
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

            return Task.FromResult(response);
        }
        public async Task<string> TranslateToVN(string en)
        {
            try
            {
                string sourceLang = "en";
                string targetLang = "vi";
                HttpClient client = new HttpClient();
                string baseUrl = configuration["TranslateApi:BaseUrl"];
                string apiKey = configuration["TranslateApi:ApiKey"];
                string apiHost = configuration["TranslateApi:ApiHost"];
                var requestBody = new
                {
                    q = en.Trim(),
                    source = sourceLang,
                    target = targetLang
                };
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(baseUrl),
                    Headers =
    {
        { "x-rapidapi-key", apiKey },
        { "x-rapidapi-host", apiHost},
    },
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(body);

                    JsonElement root = doc.RootElement;

                    // Truy cập "translatedText"
                    if (root.TryGetProperty("data", out JsonElement data) &&
                        data.TryGetProperty("translations", out JsonElement translations) &&
                        translations.TryGetProperty("translatedText", out JsonElement translatedTextArray) &&
                        translatedTextArray.ValueKind == JsonValueKind.Array)
                    {
                        string lastMeaning = translatedTextArray[translatedTextArray.GetArrayLength() - 1].GetString();
                        return lastMeaning;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to translate {en} to VietNamese");
                return "";
            }
        }



        public Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync(string vocab)
        {
            throw new NotImplementedException();
        }
    }
}
