using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;

namespace KeystoneLibrary.Providers
{
    public class CalculationProvider : BaseProvider, ICalculationProvider
    {
        protected readonly ICacheProvider _cacheProvider;
        
        public CalculationProvider(ApplicationDbContext db) : base(db) { }

        public decimal GetPercentage(decimal amount, decimal totalAmount)
        {
            var percentage = (amount * 100) / totalAmount;
            return percentage;
        }
    }
}