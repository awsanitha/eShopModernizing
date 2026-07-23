using Microsoft.EntityFrameworkCore;

namespace eShopLegacyWebForms.Models
{
    public class CatalogItemHiLoGenerator
    {
        private const int HiLoIncrement = 10;
        private int _sequenceId = -1;
        private int _remainingLoIds = 0;
        private readonly object _sequenceLock = new object();

        public int GetNextSequenceValue(CatalogDBContext db)
        {
            lock (_sequenceLock)
            {
                if (_remainingLoIds == 0)
                {
                    var rawResult = db.Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR catalog_hilo;");
                    _sequenceId = (int)rawResult.Single();
                    _remainingLoIds = HiLoIncrement - 1;
                    return _sequenceId;
                }
                else
                {
                    _remainingLoIds--;
                    return ++_sequenceId;
                }
            }
        }
    }
}
