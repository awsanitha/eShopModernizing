using System;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace eShopLegacyMVC.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int sequenceId = -1;
        private int remainningLoIds = 0;
        private object sequenceLock = new object();

        public int GetNextSequenceValue(CatalogDBContext db)
        {
            lock (sequenceLock)
            {
                if (remainningLoIds == 0)
                {
                    var connection = db.Database.GetDbConnection();
                    bool wasOpen = connection.State == ConnectionState.Open;
                    if (!wasOpen)
                        connection.Open();
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT NEXT VALUE FOR catalog_hilo;";
                            var result = command.ExecuteScalar();
                            sequenceId = Convert.ToInt32(result);
                        }
                    }
                    finally
                    {
                        if (!wasOpen)
                            connection.Close();
                    }

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
    }
}
