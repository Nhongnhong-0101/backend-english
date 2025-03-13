using Core;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Sandbox
{
    public class Data
    {
        public Guid id { get; set; }
        public string word { get; set; }
        public string phonetic { get; set; }
        public string description { get; set; }

    }
        internal class Program
    {
        //readonly static HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            //translate3000WordsAndSaveToDb();
            await GetDefinitionOfWord("live");
            Console.WriteLine("Hello World!");
        }
        public static async Task translate3000WordsAndSaveToDb()
        {
            try
            {
                //int count = 1;
                //string file3000 = "D:\\source\\backend-english\\Sandbox\\3000CommmonWords.txt";
                //using (StreamReader streamReader = new StreamReader(file3000))
                //{
                //    List<string> words = new List<string>();
                //    Task<string> taskLine = streamReader.ReadLineAsync();
                //    taskLine.Wait();
                //    while (taskLine.Result != null)
                //    {
                //        Console.WriteLine($"read word {count++}: {taskLine.Result}");
                //        words.Add(taskLine.Result);
                //        //GetDefinitionOfWord(word[i], out Vocab vocab, List<VocabSubMeaning> subs)
                //        //SaveWordToDb(vocab, subs)
                //        taskLine = streamReader.ReadLineAsync();
                //        taskLine.Wait();
                //    }
                //}
                //Vocab vocab;
                //List<VocabSubMeaning> vocabSubs = new List<VocabSubMeaning>();
   
                await GetDefinitionOfWord("live");
                Console.WriteLine("Done the task");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task<bool> GetDefinitionOfWord(string word)
        {
            Vocab dbvocab = new Vocab();
            List<VocabSubMeaning> vocabSubs = new List<VocabSubMeaning>();
            try
            {
                if (word != null)
                {
                    string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                    using (HttpClient httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                    {
                        var response = await httpclient.GetAsync(url);
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
                                    //get the main meaning to save as Vocab 
                                    phonetic = res.RootElement[0].GetProperty("phonetic").ToString();
                                    JsonElement phonetics = res.RootElement[0].GetProperty("phonetics");
                                    foreach (var p in phonetics.EnumerateArray())
                                    {
                                        if (string.IsNullOrEmpty(phonetic))
                                        {
                                            if (p.TryGetProperty("text", out JsonElement curPhonetic))
                                            {
                                                phonetic = !string.IsNullOrEmpty(curPhonetic.ToString()) ? curPhonetic.ToString() : "";
                                            }
                                        }
                                        if (p.TryGetProperty("audio", out JsonElement audioUrl))
                                        {
                                            audio = string.IsNullOrEmpty(audioUrl.GetString()) ? audioUrl.ToString() : "";
                                        }
                                        if (!string.IsNullOrEmpty(phonetic) && !string.IsNullOrEmpty(audio)) { break; }
                                    }
                                    JsonElement meanings = res.RootElement[0].GetProperty("meanings");
                                    pos = meanings[0].GetProperty("partOfSpeech").ToString();
                                    string enDefinition = meanings[0].GetProperty("definitions")[0].GetProperty("definition").ToString();
                                    dbvocab.vocabId = new Guid();
                                    dbvocab.vocab = word;
                                    dbvocab.audioUrl = audio;
                                    dbvocab.phonetic = phonetic;
                                    dbvocab.primaryMeaningEn = enDefinition;
                                    //we need to translate primaryMeaningEn before saving to db

                                    //get other meanings to save as SubVocabMeaning
                                    int lengthOfRespones = res.RootElement.GetArrayLength();
                                    int i = 0;
                                    while (i < lengthOfRespones)
                                    {
                                        meanings = res.RootElement[i].GetProperty("meanings");
                                        int processingMean = 0;
                                        while (processingMean < meanings.GetArrayLength() && processingMean < 3)
                                        {
                                            processingMean++;
                                            pos = meanings[processingMean].GetProperty("partOfSpeech").ToString();
                                            JsonElement definitions = meanings[processingMean].GetProperty("definitions");
                                            enDefinition = "";
                                            string example = "";
                                            int processingDef = 1;// the first (=0) definition is saved in Vocab
                                            while (processingDef < definitions.GetArrayLength() && processingDef < 4)
                                            {
                                                if (definitions[processingDef].TryGetProperty("definition", out JsonElement def))
                                                {
                                                    enDefinition = !string.IsNullOrEmpty(def.ToString()) ? def.ToString() : "";
                                                }
                                                if (definitions[processingDef].TryGetProperty("example", out JsonElement exampleElement))
                                                {
                                                    example = !string.IsNullOrEmpty(exampleElement.ToString()) ? exampleElement.ToString() : "";
                                                }
                                                if (!string.IsNullOrEmpty(enDefinition.ToString()) && !string.IsNullOrEmpty(example.ToString()))
                                                {
                                                    VocabSubMeaning dbSubVocab = new VocabSubMeaning(new Guid(), dbvocab.vocabId, pos, enDefinition, "", example, audio, "");
                                                    //we need to translate primaryMeaningEn before saving to db
                                                    vocabSubs.Add(dbSubVocab);
                                                    processingDef++;
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                       
                    }
                    Console.WriteLine("So tired");
                    return true;
                }
            }
            catch (Exception ex
            {
                Console.WriteLine("Failed when try to get definition by calling api: " + ex.Message);
            }
            return false;
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
                var result = connection.Query<Data>(sql).ToList();


            }
        }
    }
}

