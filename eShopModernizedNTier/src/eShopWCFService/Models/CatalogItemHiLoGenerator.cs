using System;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace eShopWCFService.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int sequenceId = -1;
        private int remainningLoIds = 0;
        private readonly object sequenceLock = new object();

        public int GetNextSequenceValue(EntityModel db)
        {
            lock (sequenceLock)
            {
                if (remainningLoIds == 0)
                {
                    var connection = db.Database.GetDbConnection();
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT NEXT VALUE FOR catalog_hilo";
                    var value = cmd.ExecuteScalar();
                    sequenceId = (int)(long)value!;
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
