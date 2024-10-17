using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using System.Globalization;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        protected readonly ApplicationDbContext _db;

        public ConfigurationProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public T Get<T>(string key)
        {
            var data = _db.Configurations.SingleOrDefault(x => x.Key == key);
            if (data != null)
            {
                return (T)Convert.ChangeType(data.Value, typeof(T), CultureInfo.InvariantCulture);
            }

            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture); ;
        }

        public bool Update(string value, string key)
        {
            var data = _db.Configurations.SingleOrDefault(x => x.Key == key);
            if (data != null)
            {
                data.Value = value;
            }
            else
            {
                Configuration configuration = new Configuration();
                configuration.Key = key;
                configuration.Value = value;
                _db.Configurations.Add(configuration);
            }

            try
            {
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
            
        }
    }
}