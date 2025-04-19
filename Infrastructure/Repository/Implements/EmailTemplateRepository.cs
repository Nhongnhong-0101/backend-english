using Core.Models;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly string connectionString;
        public EmailTemplateRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<(string Subject, string BodyHtml)> GetEmailTemplate(string type)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select subject, body from email_template where type = @Type ";
                    string subject = string.Empty;
                    string body = string.Empty;
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Type", type);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                 subject = reader.GetString(reader.GetOrdinal("subject"));
                                 body = reader.GetString(reader.GetOrdinal("body"));
                            }
                        }

                    }
                   return (subject, body); 
                }
            }

            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
