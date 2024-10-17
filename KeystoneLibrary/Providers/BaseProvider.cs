using AutoMapper;
using KeystoneLibrary.Data;
using Microsoft.Extensions.Configuration;

namespace KeystoneLibrary.Providers
{
    public class BaseProvider
    {
        protected readonly string _USparkAPIURL = "";//"https://muicservice.azurewebsites.net/api/keystone/v1";
        protected readonly string _USparkAPIKey = ""; //"HqFFZBLwy2CbHqdkergnwJ68";
        protected readonly ApplicationDbContext _db;
        protected readonly IMapper _mapper;
        protected readonly IConfiguration _config;


        public BaseProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public BaseProvider(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public BaseProvider(IConfiguration config, ApplicationDbContext db, IMapper mapper)
        {
            _config = config;
            _db = db;
            _mapper = mapper;

            _USparkAPIURL = _config["USparkUrl"] ?? _USparkAPIURL;
            _USparkAPIKey = _config["USparkServiceAPIKey"] ?? _USparkAPIURL;
        }
    }
}