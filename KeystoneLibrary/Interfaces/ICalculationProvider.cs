namespace KeystoneLibrary.Interfaces
{
    public interface ICalculationProvider
    {
        decimal GetPercentage(decimal amount, decimal totalAmount);
    }
}