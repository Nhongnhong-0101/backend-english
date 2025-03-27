using Infrastructure.Repository.Interfaces;
using System;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Core.Models;

namespace Infrastructure.Repository.Implements
{
    public class VocabRepository : IVocabRepository
    {
        private readonly string connectionString;

        public VocabRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public Task<IEnumerable<Vocab>> GetAllVocabAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Vocab>> GetShortMeaningVocabAsync(string vocab)
        {
            try
            {
                List<Vocab> result = new List<Vocab>();
                vocab = vocab.Trim().ToLower();
                string command = "select * from vocab where vocab =@Vocab";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var excute = await connect.QueryAsync<Vocab>(command, new { Vocab = vocab });
                    result = excute.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
