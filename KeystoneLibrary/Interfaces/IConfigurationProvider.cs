namespace KeystoneLibrary.Interfaces
{
    public interface IConfigurationProvider
    {
        T Get<T>(string key);
        bool Update(string value, string key);
    }
}