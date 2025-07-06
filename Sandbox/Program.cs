using Core;
using Dapper;
using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using static Google.Apis.Requests.BatchRequest;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Sandbox
{
        internal class Program
    {          
        readonly static HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            List<string> OnekVocabs = new List<string>();
            await ReadWordsFromFile(OnekVocabs);
           
            Thread.Sleep(1000);

            int numberTasks = 25;
            List<Task> tasks = new List<Task>();
            foreach (string word in OnekVocabs)
            {
                Task t = GetDefinitionOfWord(word);
                tasks.Add(t);

                if (tasks.Count >= numberTasks)
                {
                    Task finished = await Task.WhenAny(tasks);
                    Thread.Sleep(500);
                    tasks.Remove(finished);
                }
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("Hello World!");
        }
        public static async Task ReadWordsFromFile(List<string> OnekVocabs)
        {
            try
            {
                int max = 150;
                int count = 1;
                OnekVocabs.Clear();
                string pathFather = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
                string file3000 = Path.Combine(pathFather, "3000CommmonWords.txt");
                string? line;
                using (StreamReader streamReader = new StreamReader(file3000))
                {
                    line = await streamReader.ReadLineAsync();
                    while (line!= null && count < max)
                    {
                        OnekVocabs.Add(line);
                        line = await streamReader.ReadLineAsync();
                        if (count == max - 1)
                        {
                            Console.WriteLine($"LASTEST word: {line}");
                        }
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read from file"+ ex.Message);
            }
        }
        public static async Task<bool> GetDefinitionOfWord(string word)
        {
            Console.WriteLine($"Processing {word}");
            List<Vocab> vocabs = new List<Vocab>();
            List<VocabSubMeaning> vocabSubs = new List<VocabSubMeaning>();
            try
            {
                if (word != null)
                {
                    string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                    var response = await httpClient.GetAsync(url);
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
                                                if(processingDef == 0)
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

                                //start to save in db 
                                await SaveToDatabase(vocabs, vocabSubs);
                            }
                        }
                    }
                }
                Console.WriteLine("So tired");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed when try to get definition by calling api: {word.ToUpper()} " + ex.Message);
            }
            return false;
        }
        public static async Task<string> TranslateToVN(string en)
        {
            try
            {
                string sourceLang = "en"; 
                string targetLang = "vi"; 

                var requestBody = new
                {
                    q = en.Trim(),
                    source = sourceLang,
                    target = targetLang
                };
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://deep-translate1.p.rapidapi.com/language/translate/v2"),
                    Headers =
    {
        { "x-rapidapi-key", "5b9d212c25msh6131ec65d45038bp1252f0jsn68d667b446d8" },
        { "x-rapidapi-host", "deep-translate1.p.rapidapi.com" },
    },
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };
                using (var response = await httpClient.SendAsync(request))
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
                        //Console.WriteLine($"Nghĩa cuối cùng: {lastMeaning}");
                        return lastMeaning;
                    }

                    return null;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to translate {en} to VietNamese");
                return "";
            }
        }

        public static async Task<bool> SaveToDatabase (List<Vocab> vocabs, List<VocabSubMeaning> vocabSubs)
        {
            Console.WriteLine($"Start to save {vocabs.Count} vocabs and {vocabSubs.Count} subVocabs of {vocabs[0].vocab.ToUpper()} to database");
            string connectionString = "Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.odpkwfxigvloluqqndik;Password=Nhi10diemNha";
            try
            {
                using( var connecttion = new NpgsqlConnection(connectionString))
                {
                    connecttion.Open();
                    foreach (Vocab vocab in vocabs)
                    {
                        string sql = "INSERT INTO vocab (vocab_id, vocab, primarymeaningvi, primarymeaningen, phonetic, audioUrl, part_of_speech)" +
                            " VALUES (@vocab_id, @vocab, @primarymeaningvi, @primarymeaningen, @phonetic, @audioUrl, @part_of_speech)";
                        var parameters = new
                        {
                            vocab_id = vocab.vocabId,
                            vocab = vocab.vocab,
                            primarymeaningvi = vocab.primaryMeaningVi,
                            primarymeaningen = vocab.primaryMeaningEn,
                            phonetic = vocab.phonetic,
                            audioUrl = vocab.audioUrl,
                            part_of_speech = vocab.partOfSpeech
                        };

                        var result = await connecttion.ExecuteAsync(sql, parameters);
                        if (result < 1) return false;
                    }

                    string sqlSub = "INSERT INTO vocab_sub_meaning (meaning_id  ,vocab_id ,part_of_speech ,meaning_en ,meaning_vi ,example ,audio_url ,image_url)" +
                           " VALUES (@meaning_id ,@vocab_id ,@part_of_speech ,@meaning_en ,@meaning_vi ,@example ,@audio_url ,@image_url)";
                    foreach (VocabSubMeaning sub in vocabSubs)
                    {
                        var parameters = new
                        {
                            meaning_id = sub.meaningId,
                            vocab_id = sub.vocabId,
                            meaning_en = sub.meaningEn,
                            meaning_vi = sub.meaningVi,
                            example = sub.example,
                            audio_url = sub.audioUrl,
                            image_url = sub.imageUrl,
                            part_of_speech = sub.partOfSpeech
                        };

                        var insertedSub = await connecttion.ExecuteAsync(sqlSub, parameters);
                        if (insertedSub < 1) return false;
                        
                    }
                    Console.WriteLine($"---Done to save {vocabs.Count} vocabs and {vocabSubs.Count} subVocabs of {vocabs[0].vocab.ToUpper()} to database");

                    return true;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to save to db {ex.Message}");
                return false;
            }
        }
        public static void InsertData()
        {
            string host = "aws-0-ap-southeast-1.pooler.supabase.com";
            string port = "6543";
            string database = "postgres";
            string username = "postgres.sukitgaakfzpczuodykf";
            string password = "yK56xdqwUBtAAdR4";
            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            ;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM public.essential_3000_vocab";
                //var result = connection.Query<Data>(sql).ToList();
            }
        }
    }
}

