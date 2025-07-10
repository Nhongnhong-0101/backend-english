using Core.Models;
using Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class IdiomServicce : IIdiomService
    {
        private readonly string connectionString;
        public IdiomServicce(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<Idiom> AddNewIdiom(Idiom idiom)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "INSERT INTO idiom (idiom, meaning) VALUES (@Idiom, @Meaning) RETURNING idiom, meaning";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Idiom", idiom.idiom);
                        cmd.Parameters.AddWithValue("@Meaning", idiom.meaning);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string idiomText = reader.GetString(0);
                                string meaningText = reader.GetString(1);
                                return new Idiom(idiomText, meaningText);
                            }
                        }
                    }
                }
                return new Idiom(null, null);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Idiom>> GetAllIdiom()
        {
            try
            {
                var idioms = new List<Idiom>();

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "SELECT idiom, meaning FROM idiom";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string idiomText = reader.GetString(0);
                            string meaning = reader.GetString(1);

                            Idiom idiom = new Idiom(idiomText, meaning);
                            idioms.Add(idiom);
                        }
                    }
                }
                idioms = idioms.OrderBy(i => i.idiom).ToList();
                return idioms;
            }
            catch
            {
                throw;
            }
        }
    }
}
