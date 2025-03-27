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
    public class VSMeaningRepository : IVSMeaningRepository
    {
        private readonly string connectionString;

        public VSMeaningRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<VocabSubMeaning?> GetSubMeaningByIdAsync(Guid id)
        {
            try
            {
                string command = "select * from vocab_sub_meaning where meaning_id =@Id";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var excute = await connect.QueryAsync<VocabSubMeaning>(command, new { Id = id });
                    var result = excute.FirstOrDefault();
                    return result;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<IEnumerable<VocabSubMeaning>> GetSubMeaningByVocabIdAsync(Guid vocabId)
        {
            try
            {
                string command = "select * from vocab_sub_meaning where vocab_id =@VocabId";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    var excute = await connect.QueryAsync<VocabSubMeaning>(command, new { VocabId = vocabId });
                    var result = excute.ToList();
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
