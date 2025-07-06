using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
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
                string command = "INSERT INTO word_set (wordset_id, name_set, image_url, updated_at, account_id, is_star, is_default)" +
                    " VALUES ( @wordset_id, @name_set, @image_url, @updated_at, @account_id, @is_star, @is_default)" +
                    "RETURNING *;";
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    using (var cmd = new NpgsqlCommand(command, connect))
                    {
                        cmd.Parameters.AddWithValue("@wordset_id", Guid.NewGuid());
                        cmd.Parameters.AddWithValue("@name_set", wordSet.nameSet);
                        cmd.Parameters.AddWithValue("@image_url", (object?)wordSet.imageUrl?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                        cmd.Parameters.Add("@account_id", NpgsqlTypes.NpgsqlDbType.Uuid)
       .Value = (object?)wordSet.accountId ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@is_star", wordSet.isStar);
                        cmd.Parameters.AddWithValue("@is_default", wordSet.isDefault);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                WordSet ws = new WordSet();
                                ws.wordsetId = reader.GetGuid(0);
                                ws.nameSet = reader.GetString(1);
                                ws.imageUrl = reader.IsDBNull(2) ? null : reader.GetString(2);
                                ws.updatedAt = reader.GetDateTime(3);
                                ws.accountId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4);
                                ws.isStar = reader.GetBoolean(5);
                                ws.isDefault = reader.GetBoolean(6);

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

        public async Task<bool> AddVocabsToWSAsync(List<VocabWS> vocabs, Guid wsId)
        {
                string command = @"
                INSERT INTO ws_m2m_vocab (wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star)
                VALUES (@wordset_id, @vocab, @primarymeaning_vi, @primarymeaning_en, @is_star);";

                using (var connect = new NpgsqlConnection(connectionString))
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

        public async Task<IEnumerable<VocabWS>> GetSavedWordsWSAsync(Guid accountId)
        {
            try
            {

                string query = @"
                    SELECT wordset_id
                    FROM word_set 
                    WHERE account_id = @accountId AND name_set = 'Saved Words'";
                Guid? wsId = null;
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                wsId = reader.GetGuid(reader.GetOrdinal("wordset_id"));
                            }
                        }
                    }
                }
                if (wsId.HasValue)
                {
                    string query2 = "SELECT wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star FROM ws_m2m_vocab" +
                        " WHERE wordset_id = @id;";
                    List<VocabWS> vocabs = new List<VocabWS>();
                    using (var connect = new NpgsqlConnection(connectionString))
                    {
                        await connect.OpenAsync();
                        using (var cmd = new NpgsqlCommand(query2, connect))
                        {
                            cmd.Parameters.AddWithValue($"@id", wsId);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    VocabWS v = new VocabWS();
                                    v.wsId = reader.GetGuid(0);
                                    v.vocab = reader.GetString(1);
                                    v.primaryMeaningVi = reader.GetString(2);
                                    v.primaryMeaningEn = reader.GetString(3);
                                    v.isStar = reader.GetBoolean(4);

                                    vocabs.Add(v);
                                }
                            }
                        }
                    }
                    return vocabs;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<VocabWS>> GetVocabsOfWSAsync(Guid id)
        {
            try
            {
                string command = "SELECT wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star FROM ws_m2m_vocab" +
                    " WHERE wordset_id = @id;";
                List<VocabWS> vocabs = new List<VocabWS>();
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();
                    using (var cmd = new NpgsqlCommand(command, connect))
                    {
                        cmd.Parameters.AddWithValue($"@id", id);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                VocabWS v = new VocabWS();
                                v.wsId = reader.GetGuid(0);
                                v.vocab = reader.GetString(1);
                                v.primaryMeaningVi = reader.GetString(2);
                                v.primaryMeaningEn = reader.GetString(3);
                                v.isStar = reader.GetBoolean(4);

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
                string command = "SELECT wordset_id, name_set, image_url, updated_at, account_id, is_star, is_default" +
                    "FROM word_set" +
                    " WHERE wordset_id = @id;";
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
                                ws.accountId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4);
                                ws.isStar = reader.GetBoolean(5);
                                ws.isDefault = reader.GetBoolean(6);

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
                SELECT wordset_id, name_set, image_url, updated_at, account_id, is_star, is_default
                FROM word_set 
                WHERE (account_id = @accountId AND name_set != 'Saved Words')
                OR (account_id IS NULL AND is_default = TRUE);";
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
                                int accountIdIndex = reader.GetOrdinal("account_id");
                                ws.accountId = reader.IsDBNull(accountIdIndex) ? (Guid?)null : reader.GetGuid(accountIdIndex);
                                ws.isStar = reader.GetBoolean(reader.GetOrdinal("is_star"));
                                ws.isDefault = reader.GetBoolean(reader.GetOrdinal("is_default"));

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

        public async Task<bool> SaveVocabsToSavedWSAsync(List<VocabWS> vocabs, Guid accountId)
        {
            try
            {
                string queryUpdate = @"
                    SELECT wordset_id
                    FROM word_set 
                    WHERE account_id = @accountId AND name_set = 'Saved Words'";

                Guid? wsId = null;
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(queryUpdate, connect))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                wsId = reader.GetGuid(reader.GetOrdinal("wordset_id"));
                            }
                        }
                    }
                }
                if ( !wsId.HasValue)
                {
                    var defaultWordSet = new WordSet
                    {
                        wordsetId = Guid.NewGuid(),
                        nameSet = "Saved Words",
                        accountId = accountId
                    };
                    var wordSetSuccess = await AddNewWordSetAsync(defaultWordSet);

                    if (wordSetSuccess != null && wordSetSuccess.wordsetId != Guid.Empty)
                    {
                        wsId = wordSetSuccess.wordsetId;
                    }
                    else
                    {
                        return false;
                    }
                   
                }

                var success = await AddVocabsToWSAsync(vocabs, (Guid)wsId);
                if (success)
                {
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                throw new Exception(message: "Error fetching word sets " + ex.Message);
            }
        }

        public async Task<bool> UnSaveVocabsToSavedWSAsync(List<VocabWS> vocabs, Guid accountId)
        {
            try
            {
                string queryUpdate = @"
                    SELECT wordset_id
                    FROM word_set 
                    WHERE account_id = @accountId AND name_set = 'Saved Words'";
                string deleteCommand = @"
                    DELETE FROM ws_m2m_vocab
                    WHERE wordset_id = @wordset_id AND vocab = @vocab";

                Guid? wsId = null;
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(queryUpdate, connect))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                wsId = reader.GetGuid(reader.GetOrdinal("wordset_id"));
                            }
                        }
                    }

                    if (wsId == null)
                        throw new Exception("Không tìm thấy wordset 'Saved Words' cho account này");

                    using (var transaction = await connect.BeginTransactionAsync())
                    {
                        try
                        {
                            foreach (var v in vocabs)
                            {
                                using (var cmd = new NpgsqlCommand(deleteCommand, connect, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@wordset_id", wsId);
                                    cmd.Parameters.AddWithValue("@vocab", v.vocab);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"Lỗi khi xóa từ khỏi Saved Words: {ex.Message}");
                            return false;
                        }
                    }

                }

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

        public async Task<bool> UpdaVocabsToWteSAsync(List<VocabWS> vocabs, Guid wsId)
        {
            string updateCommand = @"
                    UPDATE ws_m2m_vocab 
                    SET vocab = @vocab, 
                        primarymeaning_vi = @primarymeaning_vi, 
                        primarymeaning_en = @primarymeaning_en, 
                        is_star = @is_star
                    WHERE wordset_id = @wordset_id AND vocab = @vocab;";

            string insertCommand = @"
                    INSERT INTO ws_m2m_vocab (wordset_id, vocab, primarymeaning_vi, primarymeaning_en, is_star)
                    VALUES (@wordset_id, @vocab, @primarymeaning_vi, @primarymeaning_en, @is_star);";

            using (var connect = new NpgsqlConnection(connectionString))
            {
                await connect.OpenAsync();

                using (var transaction = await connect.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var v in vocabs)
                        {
                            using (var cmd = new NpgsqlCommand(updateCommand, connect, transaction))
                            {
                                cmd.Parameters.AddWithValue("@wordset_id", wsId);
                                cmd.Parameters.AddWithValue("@vocab", v.vocab);
                                cmd.Parameters.AddWithValue("@primarymeaning_vi", v.primaryMeaningVi);
                                cmd.Parameters.AddWithValue("@primarymeaning_en", v.primaryMeaningEn);
                                cmd.Parameters.AddWithValue("@is_star", v.isStar);

                                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                                if (rowsAffected == 0)
                                {
                                    using (var insertCmd = new NpgsqlCommand(insertCommand, connect, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@wordset_id", wsId);
                                        insertCmd.Parameters.AddWithValue("@vocab", v.vocab);
                                        insertCmd.Parameters.AddWithValue("@primarymeaning_vi", v.primaryMeaningVi);
                                        insertCmd.Parameters.AddWithValue("@primarymeaning_en", v.primaryMeaningEn);
                                        insertCmd.Parameters.AddWithValue("@is_star", v.isStar);

                                        await insertCmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return true;

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error updateting vocabs in word set: {ex.Message}");
                        return false;
                    }
                }
            }
        }
    }
}
