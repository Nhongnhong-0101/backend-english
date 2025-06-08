using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public static class DatabaseConfig
    {
        private static string connectionString = string.Empty;


        public static string getConnectionString()
        {
            return connectionString;
        }
    }
}
