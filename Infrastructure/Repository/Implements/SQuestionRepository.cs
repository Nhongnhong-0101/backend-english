using Core.Models;
using Dapper;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class SQuestionRepository : ISQuestionRepository
    {
        private readonly string connectionString;
        public SQuestionRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<List<SpeakingQuestion>> GetAllAsync()
        {
            try
            {
                string command = "select * from speaking_question";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var result = await connect.QueryAsync<SpeakingQuestion>(command);
                    return result.ToList();
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetAllTopicsAsync()
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT DISTINCT topic FROM speaking_question WHERE topic IS NOT NULL AND topic <> ''";
                    var result = await connect.QueryAsync<string>(query);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTopicsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<SpeakingQuestion?> GetByIdAsync(Guid questionId)
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT * FROM speaking_question WHERE question_id = @QuestionId";
                    var result = await connect.QueryFirstOrDefaultAsync<SpeakingQuestion>(query, new { QuestionId = questionId });
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SpeakingQuestion>> GetByTopicAsync(string topic)
        {
            try
            {
                List<SpeakingQuestion> questions = new List<SpeakingQuestion>();
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT question_id, sentence, level, topic, content_type FROM speaking_question WHERE topic = @Topic";
                    using var cmd = new NpgsqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@topic", topic);

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var question = new SpeakingQuestion
                        {
                            questionId = reader.GetGuid(0),
                            sentence = reader.GetString(1),
                            level = reader.IsDBNull(2) ? null : reader.GetString(2),
                            topic = reader.IsDBNull(3) ? null : reader.GetString(3),
                            contentType = reader.IsDBNull(4) ? null : reader.GetString(4),
                        };

                        questions.Add(question);
                    }
                    return questions;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByTopicAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<SpeakingQuestion?> GetFirstQuestionInTopic(string topic)
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT * FROM speaking_question WHERE topic = @Topic AND content_type =@ContenType ORDER BY question_id LIMIT 1";
                    var result = await connect.QueryFirstOrDefaultAsync<SpeakingQuestion>(query, new { Topic = topic, ContenType = "prompt" });
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFirstByTopicAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<SpeakingQuestion?> GetNextQuestionByEmbeddingAsync(float[] userAnswerEmbedding, string topic)
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var embeddingString = $"ARRAY[{string.Join(",", userAnswerEmbedding)}]::vector";

                    string query = $@"
                    SELECT *, embedding <=> {embeddingString} AS similarity
                    FROM speaking_question
                    WHERE topic = @Topic
                    AND content_type =@ContentType
                    ORDER BY similarity
                    LIMIT 1;
                    ";
                    var param = new { UserEmbedding = embeddingString, Topic = topic, ContentType = "prompt" };
                    var question = await connect.QueryFirstOrDefaultAsync<SpeakingQuestion>(query, param);
                    return question;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextQuestionByEmbeddingAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SpeakingQuestion>> GetQuestionsAsync(string topic, string contentType, int num = 3)
        {
            try
            {
                List<SpeakingQuestion> sp = new List<SpeakingQuestion>();
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT question_id, sentence, level, topic, content_type FROM speaking_question " +
                        "WHERE topic = @Topic AND content_type =@ContenType ORDER BY question_id " +
                        "LIMIT @Num";
                    var result = await connect.QueryAsync<SpeakingQuestion>(query, new { Topic = topic, ContenType = contentType, Num = num });
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFirstByTopicAsync: {ex.Message}");
                throw;
            }
        }
    }
}
