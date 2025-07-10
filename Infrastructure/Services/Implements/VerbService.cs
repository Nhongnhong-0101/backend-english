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
    public class VerbService : IVerbService
    {
        private readonly string connectionString;
        public VerbService(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<IEnumerable<IrregularVerb>> GetAllAsync()
        {
            try
            {
                var verbs = new List<IrregularVerb>();

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "SELECT infinitive, pastsimple, pastparticiple, meaning FROM irregular_verb";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string infinitive = reader.GetString(0);
                            string pastSimple = reader.GetString(1);
                            string pastParticiple = reader.GetString(2);
                            string meaning = reader.GetString(3);

                            var verb = new IrregularVerb(infinitive, pastSimple, pastParticiple, meaning);
                            verbs.Add(verb);
                        }
                    }
                }

                verbs = verbs.OrderBy(v => v.infinitive).ToList();
                return verbs;
            }
            catch
            {
                throw;
            }
        }
    }
}
