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
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "SELECT * FROM speaking_question WHERE topic = @Topic";
                    var result = await connect.QueryAsync<SpeakingQuestion>(query, new { Topic = topic });
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByTopicAsync: {ex.Message}");
                throw;
            }
        }
    }
}
