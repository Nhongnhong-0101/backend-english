using Core.Models;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class LessonRepository : ILessonRepository
    {
        private readonly string connectionString;
        public LessonRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<Lesson?> GetLessonDetail(Guid lessonId)
        {
            try
            {

                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var sql = @"
                SELECT l.lesson_id, l.plan_id, l.title, l.description, l.order_index,
                       l.topics, l.required_questions, l.example
                FROM lesson l
                WHERE l.lesson_id = @lessonId;
            ";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@lessonId", lessonId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Lesson
                    {
                        lessonId = reader.GetGuid(0),
                        planId = reader.GetGuid(1),
                        title = reader.GetString(2),
                        description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        orderIndex = reader.GetInt32(4),
                        topics = reader.IsDBNull(5) ? null : reader.GetString(5),
                        requiredQuestions = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        example = reader.IsDBNull(7) ? null : reader.GetString(7)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed from repository " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<Lesson>> GetLessonsOfAccount(Guid accountId, Guid planId)
        {
            try
            {
                var lessons = new List<Lesson>();

                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var sql = @"
                SELECT l.lesson_id, l.plan_id, l.title, l.description, l.order_index,
                       l.topics, l.required_questions, l.example, 
                       COALESCE(ul.is_passed, false) as is_passed
                FROM lesson l
                LEFT JOIN account_study_lesson ul
                  ON ul.lesson_id = l.lesson_id AND ul.account_id = @accountId
                WHERE l.plan_id = @planId
                ORDER BY l.order_index;
            ";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@planId", planId);
                cmd.Parameters.AddWithValue("@accountId", accountId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lessons.Add(new Lesson
                    {
                        lessonId = reader.GetGuid(0),
                        planId = reader.GetGuid(1),
                        title = reader.GetString(2),
                        description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        orderIndex = reader.GetInt32(4),
                        topics = reader.IsDBNull(5) ? null : reader.GetString(5),
                        requiredQuestions = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        example = reader.IsDBNull(7) ? null : reader.GetString(7),
                        isPassed = reader.GetBoolean(8)
                    });
                }

                return lessons;
            }
            catch (Exception ex) {
                throw new Exception("Failed from repository "+ ex.Message, ex);
            }
        }

        public async Task<bool> UpdateAccountPassLesson(Guid accountId, Guid lessonId)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var checkSql = @"
                SELECT COUNT(*) FROM account_study_lesson
                WHERE lesson_id = @lessonId AND account_id = @accountId;
            ";

                using var checkCmd = new NpgsqlCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@lessonId", lessonId);
                checkCmd.Parameters.AddWithValue("@accountId", accountId);
                var count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    var updateSql = @"
                    UPDATE account_study_lesson
                    SET is_passed = TRUE, completed_at = CURRENT_TIMESTAMP, updated_at = CURRENT_TIMESTAMP
                    WHERE lesson_id = @lessonId AND account_id = @accountId;
                ";

                    using var updateCmd = new NpgsqlCommand(updateSql, connection);
                    updateCmd.Parameters.AddWithValue("@lessonId", lessonId);
                    updateCmd.Parameters.AddWithValue("@accountId", accountId);
                    var excuted = await updateCmd.ExecuteNonQueryAsync();
                    return  excuted > 0;
                }
                else
                {
                    var insertSql = @"
                    INSERT INTO account_study_lesson (
                        progress_id, account_id, lesson_id, start_date, is_passed, completed_at, updated_at
                    ) VALUES (
                        @progressId, @accountId, @lessonId, CURRENT_TIMESTAMP, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                    );
                ";

                    using var insertCmd = new NpgsqlCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@progressId", Guid.NewGuid());
                    insertCmd.Parameters.AddWithValue("@accountId", accountId);
                    insertCmd.Parameters.AddWithValue("@lessonId", lessonId);

                    return await insertCmd.ExecuteNonQueryAsync() > 0;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed from repository " + ex.Message, ex);

            }
        }
    }
}
