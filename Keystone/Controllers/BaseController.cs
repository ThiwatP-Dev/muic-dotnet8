using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.Api.ApiResponse;
using System.Net;
using KeystoneLibrary.Exceptions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Logs;
using Newtonsoft.Json;

namespace Keystone.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _db;
        protected readonly IFlashMessage _flashMessage;
        protected readonly IMapper _mapper;
        protected readonly ISelectListProvider _selectListProvider;
        protected readonly IMemoryCache _memoryCache;
        protected readonly KeystoneLibrary.Interfaces.IConfigurationProvider _configurationProvider;
        protected readonly IHttpContextAccessor _accessor;        
        
        public BaseController(ApplicationDbContext db)
        {
            _db = db;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage)
        {
            _db = db;
            _flashMessage = flashMessage;
        }

        public BaseController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public BaseController(ApplicationDbContext db, IMemoryCache memoryCache)
        {
            _db = db;
            _memoryCache = memoryCache;
        }
        
        public BaseController(ApplicationDbContext db, ISelectListProvider selectListProvider)
        {
            _db = db;
            _selectListProvider = selectListProvider;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, ISelectListProvider selectListProvider)
        {
            _db = db;
            _flashMessage = flashMessage;
            _selectListProvider = selectListProvider;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, ISelectListProvider selectListProvider, IHttpContextAccessor accessor)
        {
            _db = db;
            _flashMessage = flashMessage;
            _selectListProvider = selectListProvider;
            _accessor = accessor;
        }
        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, ISelectListProvider selectListProvider, KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider)
        {
            _db = db;
            _flashMessage = flashMessage;
            _selectListProvider = selectListProvider;
            _configurationProvider = configurationProvider;
        }

        public BaseController(ApplicationDbContext db, IMapper mapper, ISelectListProvider selectListProvider)
        {
            _db = db;
            _mapper = mapper;
            _selectListProvider = selectListProvider;
        }

        public BaseController(ApplicationDbContext db, IMapper mapper, IMemoryCache memoryCache, ISelectListProvider selectListProvider)
        {
            _db = db;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _selectListProvider = selectListProvider;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, IMapper mapper, IMemoryCache memoryCache, ISelectListProvider selectListProvider)
        {
            _db = db;
            _flashMessage = flashMessage;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _selectListProvider = selectListProvider;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, IMapper mapper, ISelectListProvider selectListProvider)
        {
            _db = db;
            _flashMessage = flashMessage;
            _mapper = mapper;
            _selectListProvider = selectListProvider;
        }
        public BaseController(ApplicationDbContext db, KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider)
        {
            _db = db;
            _configurationProvider = configurationProvider;
        }

        public BaseController(ApplicationDbContext db, IFlashMessage flashMessage, IMapper mapper)
        {
            _db = db;
            _flashMessage = flashMessage;
            _mapper = mapper;
        }

        protected JsonResult Success(dynamic result)
        {
            return Json(APIResponseBuilder.Success(result));
        }

        protected JsonResult Success(ApiCallLog log, dynamic result)
        {
            var jsonReturn = Json(APIResponseBuilder.Success(result));

            log.IsSuccess = true;
            log.ResponseStatusCode = 200;
            log.ResponsePayload = JsonConvert.SerializeObject(jsonReturn);

            return jsonReturn;
        }

        protected JsonResult Error(ResultException error, dynamic result = null)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(APIResponseBuilder.Error(error, result));
        }

        protected JsonResult Error(ApiCallLog log, ResultException error, dynamic result = null)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var jsonReturn = Json(APIResponseBuilder.Error(error, result));

            log.IsSuccess = false;
            log.ResponseStatusCode = Response.StatusCode;
            log.ErrorMessage = error.Message;
            log.ResponsePayload = JsonConvert.SerializeObject(jsonReturn);

            return jsonReturn;
        }

        protected JsonResult Unauthorized(ResultException error, dynamic result = null)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return Json(APIResponseBuilder.Error(error, result));
        }

        protected JsonResult Unauthorized(ApiCallLog log, ResultException error, dynamic result = null)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var jsonReturn = Json(APIResponseBuilder.Error(error, result));

            log.IsSuccess = false;
            log.ResponseStatusCode = Response.StatusCode;
            log.ErrorMessage = error.Message;
            log.ResponsePayload = JsonConvert.SerializeObject(jsonReturn);

            return jsonReturn;
        }

        protected BadRequestObjectResult BadRequest(ApiCallLog log, object error)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            log.IsSuccess = false;
            log.ResponseStatusCode = Response.StatusCode;
            log.ResponsePayload = JsonConvert.SerializeObject(error);

            return BadRequest(error);
        }

        protected bool ValidApiKey()
        {
            if (!HttpContext.Request.Headers.Keys.Contains("x-api-key"))
            {
                return true;
            }
            else 
            {
                var key = _configurationProvider.Get<string>("ApiKey");

                if(HttpContext.Request.Headers["x-api-key"] == key)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        protected ApplicationUser GetUser()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _db.Users.IgnoreQueryFilters().SingleOrDefault(x => x.Id == userId);
        }

        protected long GetInstructorId()
        {
            var user = GetUser();
            var instructorId = user.InstructorId == null ? _db.Instructors.FirstOrDefault(x => x.Code == user.UserName)?.Id ?? 0 : user.InstructorId ?? 0;
            return instructorId;
        }
    }
}
