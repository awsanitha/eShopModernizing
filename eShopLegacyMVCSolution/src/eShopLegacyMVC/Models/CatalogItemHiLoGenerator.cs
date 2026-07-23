using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace eShopLegacyMVC.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int sequenceId = -1;
        private int remainingLoIds = 0;
        private readonly object sequenceLock = new object();

        public int GetNextSequenceValue(CatalogDBContext db)
        {
            lock (sequenceLock)
            {
                if (remainingLoIds == 0)
                {
                    // EF Core: use raw SQL to get the next sequence value
                    var rawSequenceId = db.Database
                        .SqlQueryRaw<long>("SELECT NEXT VALUE FOR catalog_hilo")
                        .Single();
                    sequenceId = (int)rawSequenceId;
                    remainingLoIds = HiLoIncrement - 1;
                    return sequenceId;
                }
                else
                {
                    remainingLoIds--;
                    return ++sequenceId;
                }
            }
        }
    }
}
