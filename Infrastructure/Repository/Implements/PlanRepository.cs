using Core.Models;
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
    public class PlanRepository : IPlanRepository
    {
        private readonly string connectionString;
        public PlanRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }
        public async Task<IEnumerable<Plan>> GetPlansOfAccount(Guid accountId, string skill)
        {
            var plans = new List<Plan>();

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                SELECT 
                    sp.plan_id,
                    sp.description,
                    sp.purpose,
                    sp.order_index,
                    sp.required_questions,
                    sp.skill,
                    sp.total_lessons,
                    sp.created_at,
                    usp.is_passed IS NOT NULL AS is_done
                FROM study_plan sp
                LEFT JOIN account_study_plan usp 
                    ON sp.plan_id = usp.plan_id AND usp.account_id = @accountId
                WHERE (@skill IS NULL OR sp.skill = @skill)
                ORDER BY sp.order_index;
            ";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);
                        cmd.Parameters.AddWithValue("@skill", (object?)skill ?? DBNull.Value);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var plan = new Plan
                                {
                                    planId = reader.GetGuid(0),
                                    description = reader.GetString(1),
                                    purpose = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    orderIndex = reader.GetInt32(3),
                                    requiredQuestions = reader.GetInt32(4),
                                    skill = reader.GetString(5),
                                    totalLessons = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                    createdAt = reader.GetDateTime(7),
                                    isDone = reader.GetBoolean(8) 
                                };

                                plans.Add(plan);
                            }
                        }
                    }
                }

                return plans;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> UpdateAccountPassPlan(Guid accountId, Guid planId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var checkSql = @"
                SELECT COUNT(*) FROM account_study_plan
                WHERE account_id = @accountId AND plan_id = @planId;
            ";

                    using (var checkCmd = new NpgsqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@accountId", accountId);
                        checkCmd.Parameters.AddWithValue("@planId", planId);

                        var count = (long)await checkCmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            var updateSql = @"
                        UPDATE account_study_plan
                        SET is_passed = TRUE, updated_at = CURRENT_TIMESTAMP
                        WHERE account_id = @accountId AND plan_id = @planId;
                    ";

                            using (var updateCmd = new NpgsqlCommand(updateSql, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@accountId", accountId);
                                updateCmd.Parameters.AddWithValue("@planId", planId);
                                var rows = await updateCmd.ExecuteNonQueryAsync();
                                return rows > 0;
                            }
                        }
                        else
                        {
                            var insertSql = @"
                        INSERT INTO account_study_plan (
                            progress_id, account_id, plan_id, start_date, is_passed, created_at, updated_at
                        )
                        VALUES (
                            @progressId, @accountId, @planId, CURRENT_TIMESTAMP, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                        );
                    ";

                            using (var insertCmd = new NpgsqlCommand(insertSql, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@progressId", Guid.NewGuid());
                                insertCmd.Parameters.AddWithValue("@accountId", accountId);
                                insertCmd.Parameters.AddWithValue("@planId", planId);

                                var rows = await insertCmd.ExecuteNonQueryAsync();
                                return rows > 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                throw;
            }

        }
    }
}
