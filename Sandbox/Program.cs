using Core;
using Dapper;
using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
        readonly static HttpClient httpClient = new HttpClient();

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
                                string test = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                            }
                        }
                    }
                }
                Console.WriteLine("So tired");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed when try to get definition by calling api: " + ex.Message);
            }
            return false;
        }
        public static async Task<string> TranslateToVN(string en)
        {
            try
            {
                string url = $"https://api.mymemory.translated.net/get?q={en}&langpair=en|vi";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var translation = JsonDocument.Parse(result);
                    var data = translation.RootElement.GetProperty("responseData");
                    var res = data.GetProperty("translatedText");
                    Console.WriteLine($"Done to translate {en} to VietNamese");
                    return res.ToString();
                }
                return "";
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to translate {en} to VietNamese");
                return "";
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
                var result = connection.Query<Data>(sql).ToList();
            }
        }
    }
}

