using Core.Models;
using Dapper;
using Infrastructure.Repository.Interfaces;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class USResultRepository : IUSResultRepository
    {
        private readonly string connectionString;

        public USResultRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }
        public async Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserAsync(Guid userId)
        {
            try
            {
                using( var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = "select * from user_speaking_result where account_id = @userId";
                    var result = await connect.QueryAsync<UserSpeakingResult>(query, new { userId });
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByTopicAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserByTopicAsync(Guid userId, string topic)
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    string query = @"
                        SELECT usr.* FROM user_speaking_result usr
                        JOIN speaking_question sq ON usr.question_id = sq.question_id
                        WHERE usr.account_id = @userId AND sq.topic = @topic";
                    var result = await connect.QueryAsync<UserSpeakingResult>(query, new { userId, topic });
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByTopicAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, (int total, int practiced)>> GetUserResultEachTopicAsync(Guid accountId)
        {
            try
            {
                string commnad = "SELECT sq.topic, " +
                    "COUNT(sq.question_id) AS total_questions, COUNT(DISTINCT usr.question_id) AS practiced_questions" +
                    "FROM speaking_question sq" +
                    "LEFT JOIN user_speaking_result usr " +
                    "ON sq.question_id = usr.question_id " +
                    "AND usr.account_id = @AccountId" +
                    "GROUP BY sq.topic" +
                    "ORDER BY sq.topic";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var resultSql = await connect.QueryAsync(commnad, new { AccountId = accountId });

                    var result = new Dictionary<string, (int, int)>();
                    foreach (var row in resultSql)
                    {
                        result[row.topic] = ((int)row.total_questions, (int)row.practiced_questions);
                    }
                    return result;

                }
            } catch (Exception ex) {
                Console.WriteLine($"Error in GetUserTopicProgressAsync: {ex.Message}");
                throw;
            }

        }

        public async Task<IEnumerable<UserSpeakingResult>> SaveResultOfUserAsync(UserSpeakingResult result)
        {
            try
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    //query to check exxisting row
                    string checkQuery = @"SELECT * FROM user_speaking_result 
                                          WHERE account_id = @AccountId AND question_id = @QuestionId";
                    var existing = await connect.QueryFirstOrDefaultAsync<UserSpeakingResult>(checkQuery, result);
                    if (existing != null)
                    {
                        string updateQuery = @"UPDATE user_speaking_result 
                                               SET score = @Score, created_at = now() 
                                               WHERE result_id = @ResultId";
                        await connect.ExecuteAsync(updateQuery, new { Score = result.score, ResultId = existing.resultId });
                    }
                    else
                    {
                        result.resultId = Guid.NewGuid();

                        string command = @"INSERT INTO user_speaking_result(result_id, account_id, question_id, created_at, topic, score) 
                                               VALUES(@ResultId, @AccountId, @QuestionId, @Created_at, @Topic, @Score)";
                        using (var cmd = new NpgsqlCommand(command, connect))
                        {
                            cmd.Parameters.AddWithValue("@ResultId", result.resultId);
                            cmd.Parameters.AddWithValue("@AccountId", result.accountId);
                            cmd.Parameters.AddWithValue("@QuestionId", result.questionId);
                            cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);
                            cmd.Parameters.AddWithValue("@Topic", result.topic);
                            cmd.Parameters.AddWithValue("@Score", result.score);
                        }                                
                    }

                    return await GetResultsOfUserAsync(result.accountId);
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
