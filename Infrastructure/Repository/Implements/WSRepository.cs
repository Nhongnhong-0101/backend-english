using Core.Models;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repository.Implements
{
    public class WSRepository : IWSRepository
    {
        private  string connectionString;

        public WSRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("Supabase");
        }

        public async Task<WordSet?> AddNewWordSetAsync(WordSet wordSet)
        {
            try
            {
                string command = "INSERT INTO word_set (wordset_id, name_set, image_url, updated_at, account_id, is_star)" +
                    " VALUES ( @wordset_id, @name_set, @image_url, @updated_at, @account_id, @is_star)" +
                    "RETURNING *;";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    using (var cmd = new NpgsqlCommand(command, connect))
                    {
                        cmd.Parameters.AddWithValue("@wordset_id", wordSet.wordsetId);
                        cmd.Parameters.AddWithValue("@name_set", wordSet.nameSet);
                        cmd.Parameters.AddWithValue("@image_url", (object?)wordSet.imageUrl?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@account_id", wordSet.accountId);
                        cmd.Parameters.AddWithValue("@is_star", wordSet.isStar);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                WordSet ws = new WordSet();
                                ws.wordsetId = reader.GetGuid(0);
                                ws.nameSet = reader.GetString(1);
                                ws.imageUrl = reader.IsDBNull(2) ? null : reader.GetString(2);
                                ws.updatedAt = reader.GetDateTime(3);
                                ws.accountId = reader.GetGuid(4);
                                ws.isStar = reader.GetBoolean(5);

                                return ws;
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddVocabsToWSAsync(List<Vocab> vocabs, Guid wsId)
        {
                string command = @"
                INSERT INTO ws_m2m_vocab (wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star)
                VALUES (@wordset_id, @vocab, @primarymeaning_vi, @primarymeaning_en, @is_star);";

                using (var connect = new NpgsqlConnection(command))
                {
                    await connect.OpenAsync();

                    using (var transaction = await connect.BeginTransactionAsync()) 
                    {
                    try
                    {
                        foreach (var v in vocabs)
                        {
                            using (var cmd = new NpgsqlCommand(command, connect, transaction))
                            {
                                cmd.Parameters.AddWithValue("@wordset_id", wsId);
                                cmd.Parameters.AddWithValue("@vocab", v.vocab);
                                cmd.Parameters.AddWithValue("@primarymeaning_vi", v.primaryMeaningVi);
                                cmd.Parameters.AddWithValue("@primarymeaning_en", v.primaryMeaningEn);
                                cmd.Parameters.AddWithValue("@is_star", false);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                        return true;

                    }
                    catch (Exception ex) {
                        await transaction.RollbackAsync(); 
                        Console.WriteLine($"Error inserting word sets: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task DeleteWordSetByIdAsync(Guid id)
        {
            string command1 = "DELETE FROM ws_m2m_vocab WHERE wordset_id = @id;";
            string command2 = "DELETE FROM word_set WHERE wordset_id = @id;";
            using (var connect = new NpgsqlConnection(connectionString))
            {
                await connect.OpenAsync();

                using (var transaction = await connect.BeginTransactionAsync())
                {
                    try
                    {
                        using (var cmd1 = new NpgsqlCommand(command1, connect, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            await cmd1.ExecuteNonQueryAsync();
                        }

                        using (var cmd2 = new NpgsqlCommand(command2, connect, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            await cmd2.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error deleting WordSet: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        
            

        public async Task<IEnumerable<Vocab>> GetVocabsOfWSAsync(Guid id)
        {
            try
            {
                string command = "SELECT wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star FROM ws_m2m_vocab" +
                    " WHERE wordset_id = @id;";
                List<Vocab> vocabs = new List<Vocab>();
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    using (var cmd = new NpgsqlCommand(command, connect))
                    {
                        cmd.Parameters.AddWithValue($"@id", id);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Vocab v = new Vocab();
                                v.vocab = reader.GetString(1);
                                v.primaryMeaningVi = reader.GetString(2);
                                v.primaryMeaningEn = reader.GetString(3);

                                vocabs.Add(v);
                            }
                        }
                    }
                }
                return vocabs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<WordSet?> GetWordSetByIdAsync(Guid id)
        {
            try
            {
                string command = "SELECT wordset_id, name_set, image_url, updated_at, account_id, is_star FROM word_set WHERE wordset_id = @id;";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(command, connect))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                WordSet ws = new WordSet();
                                ws.wordsetId = reader.GetGuid(0);
                                ws.nameSet = reader.GetString(1);
                                ws.imageUrl = reader.GetString(2);
                                ws.updatedAt = reader.GetDateTime(3);
                                ws.accountId = reader.GetGuid(4);
                                ws.isStar = reader.GetBoolean(5);

                                return ws;
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<WordSet>> GetWordSetsOfAccountAsync(Guid accountId)
        {
            try
            {
                string query = @"
                SELECT wordset_id, name_set, image_url, updated_at, account_id, is_star 
                FROM word_set 
                WHERE account_id = @accountId;";
                List<WordSet> wordSets = new List<WordSet>();
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                WordSet ws = new WordSet();
                                ws.wordsetId = reader.GetGuid(reader.GetOrdinal("wordset_id"));
                                ws.nameSet = reader.GetString(reader.GetOrdinal("name_set"));
                                ws.imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url"));
                                ws.updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));
                                ws.accountId = reader.GetGuid(reader.GetOrdinal("account_id"));
                                ws.isStar = reader.GetBoolean(reader.GetOrdinal("is_star"));

                                wordSets.Add(ws);
                            }
                        }
                    }
                }
                return wordSets;
            }
            catch (Exception ex)
            {
                throw new Exception(message: "Error fetching word sets " + ex.Message);
            }
        }

        public async Task<WordSet?> UpdateWordSetAsync(WordSet wordSet)
        {
            try
            {
                string query = @"
                    UPDATE word_set 
                    SET name_set = @name_set, 
                        image_url = @image_url, 
                        updated_at = @updated_at, 
                        account_id = @account_id, 
                        is_star = @is_star 
                    WHERE wordset_id = @wordset_id 
                    RETURNING *;";

                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@wordset_id", wordSet.wordsetId);
                        cmd.Parameters.AddWithValue("@name_set", wordSet.nameSet);
                        cmd.Parameters.AddWithValue("@image_url", wordSet.imageUrl ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@updated_at",DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@account_id", wordSet.accountId);
                        cmd.Parameters.AddWithValue("@is_star", wordSet.isStar);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                WordSet ws = new WordSet();
                                ws.wordsetId = reader.GetGuid(reader.GetOrdinal("wordset_id"));
                                ws.nameSet = reader.GetString(reader.GetOrdinal("name_set"));
                                ws.imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url"));
                                ws.updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));
                                ws.accountId = reader.GetGuid(reader.GetOrdinal("account_id"));
                                ws.isStar = reader.GetBoolean(reader.GetOrdinal("is_star"));

                                return ws;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
