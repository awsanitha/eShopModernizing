using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                    // EF Core 7+ SqlQueryRaw for scalar queries
                    var result = db.Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR catalog_hilo;").ToList();
                    sequenceId = (int)result[0];
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
