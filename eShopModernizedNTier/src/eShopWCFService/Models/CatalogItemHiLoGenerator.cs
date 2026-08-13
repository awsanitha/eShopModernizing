using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace eShopWCFService.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int sequenceId = -1;
        private int remainningLoIds = 0;
        private object sequenceLock = new object();

        public int GetNextSequenceValue(EntityModel db)
        {
            lock (sequenceLock)
            {
                if (remainningLoIds == 0)
                {
                    // EF Core 7+ scalar SQL query
                    var rawQuery = db.Database.SqlQuery<long>($"SELECT NEXT VALUE FOR catalog_hilo");
                    sequenceId = (int)rawQuery.AsEnumerable().Single();
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
