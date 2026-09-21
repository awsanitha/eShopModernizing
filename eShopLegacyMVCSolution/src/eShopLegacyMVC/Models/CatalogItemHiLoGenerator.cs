using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace eShopLegacyMVC.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int sequenceId = -1;
        private int remainningLoIds = 0;
        private readonly object sequenceLock = new object();

        public int GetNextSequenceValue(CatalogDBContext db)
        {
            lock (sequenceLock)
            {
                if (remainningLoIds == 0)
                {
                    sequenceId = (int)ExecuteSequenceQuery(db, "catalog_hilo");
                    remainningLoIds = HiLoIncrement - 1;
                    return sequenceId;
                }
                else
                {
                    remainningLoIds--;
                    return ++sequenceId;
                }
            }
        }

        private static long ExecuteSequenceQuery(CatalogDBContext db, string sequenceName)
        {
            var connection = db.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen) connection.Open();
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT NEXT VALUE FOR {sequenceName};";
                return (long)cmd.ExecuteScalar()!;
            }
            finally
            {
                if (!wasOpen) connection.Close();
            }
        }
    }
}
