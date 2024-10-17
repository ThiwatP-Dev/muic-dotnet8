using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Models.Api;
using System.Globalization;
using KeystoneLibrary.Models.DataModels.Logs;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.Api.ApiResponse;
using Swashbuckle.AspNetCore.Filters;
using Keystone.Extensions;

namespace Keystone.Controllers.MasterTables
{
    [AllowAnonymous]
    public class HolidayAPIController : BaseController
    {
        protected readonly IRoomProvider _roomProvider;
        //protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IReservationProvider _reservationProvider;
        public HolidayAPIController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider,
                                    IRoomProvider roomProvider,
                                    //IRegistrationProvider registrationProvider,
                                    ISectionProvider sectionProvider,
                                    IReservationProvider reservationProvider,
                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider, configurationProvider)
        {
            _roomProvider = roomProvider;
            //_registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
            _reservationProvider = reservationProvider;
        }

        /// <summary>
        /// Get existing Holiday, can leave parameter empty for all
        /// </summary>
        /// <param name="muicId"></param>
        /// <param name="dateTimeStr">Search Date. String Format [dd/MM/yyyy HH:mm ] year is AD (2024)</param>
        /// <param name="isActive">True = for only active, False = for only inactive, null = all</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetHolidayViewModelExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizeAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestWrongDateResponseExample))]
        public async Task<IActionResult> GetHolidayListAsync(long? muicId, string dateTimeStr, bool? isActive)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            DateTime startedAt = DateTime.Now;
            if (!string.IsNullOrEmpty(dateTimeStr))
            {
                CultureInfo enUS = new CultureInfo("en-US");
                if (!DateTime.TryParseExact(dateTimeStr, "dd/MM/yyyy HH:mm", enUS, DateTimeStyles.None, out startedAt))
                {
                    return Error(HolidayAPIException.DateTimeWrongFormat(dateTimeStr));
                }
            }

            var list = await _db.MuicHolidays.AsNoTracking()
                                             .IgnoreQueryFilters()
                                             .Where(x => (isActive == null || x.IsActive == isActive)
                                                              && (muicId == null || muicId <= 0 || x.MuicId == muicId)
                                                              && (string.IsNullOrEmpty(dateTimeStr)
                                                                  || (x.StartedAt <= startedAt
                                                                      && x.EndedAt >= startedAt)
                                                                 )
                                                    )
                                             .Select(x => new HolidayViewModel
                                             {
                                                 EndedAt = x.EndedAt,
                                                 Id = x.Id,
                                                 IsAllowMakeup = x.IsMakeUpAble,
                                                 MuicId = x.MuicId,
                                                 Name = x.Name,
                                                 Remark = x.Remark,
                                                 StartedAt = x.StartedAt,
                                                 IsActive = x.IsActive,
                                                 CreatedAt = x.CreatedAt,
                                                 CreatedBy = x.CreatedBy
                                             })
                                             .ToListAsync();
            return Success(list);
        }

        /// <summary>
        /// Create holiday date and cancel all specified section slot. This will be use to prevent future slot creation on the specified date.
        /// </summary>
        /// <param name="model">CreateHolidayViewModel</param>
        /// <response code="200">Holiday is successfully created and all section slot is canceled.</response>
        /// <response code="401">Specify api-key</response>
        /// <response code="400">Wrong input format or error see message detail</response>
        [HttpPost]
        [Route("[controller]/Add")]
        [SwaggerRequestExample(typeof(CreateHolidayViewModel), typeof(CreateHolidayViewModelDefault))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SuccessAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizeAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestWrongDateResponseExample))]
        public async Task<IActionResult> CreateHolidayAsync([FromBody] CreateHolidayViewModel model)
        {
            DateTime now = DateTime.UtcNow;
            ApiCallLog log = new ApiCallLog
            {
                Endpoint = HttpContext.Request.Path,
                HttpMethod = HttpContext.Request.Method,
                ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                RequestPayload = JsonConvert.SerializeObject(model),
                Timestamp = now,
            };

            try
            {
                if (ValidApiKey())
                {
                    return Unauthorized(log, ApiException.InvalidKey());
                }

                if (model == null)
                {
                    return BadRequest(log, ApiException.InvalidParameter());
                }

                if (model.MuicId < 1)
                {
                    return BadRequest(log, ApiException.InvalidParameter("'MuicId' is required and must be more than 0"));
                }

                if (string.IsNullOrEmpty(model.CreatedBy))
                {
                    return BadRequest(log, ApiException.InvalidParameter("'CreatedBy' is required"));
                }

                if (string.IsNullOrEmpty(model.Name) || model.Name.Length > 500)
                {
                    return BadRequest(log, ApiException.InvalidParameter("'Name' is required and max length = 500"));
                }

                if (!string.IsNullOrEmpty(model.Remark) && model.Remark.Length > 1000)
                {
                    return BadRequest(log, ApiException.InvalidParameter("'Remark' max length = 1000"));
                }

                if (string.IsNullOrEmpty(model.StartedAt))
                {
                    return BadRequest(log, ApiException.InvalidParameter("'StartedAt' is required"));
                }
                if (string.IsNullOrEmpty(model.EndedAt))
                {
                    return BadRequest(log, ApiException.InvalidParameter("'EndedAt' is required"));
                }
                DateTime startedAt;
                DateTime endedAt;
                CultureInfo enUS = new CultureInfo("en-US");
                if (!DateTime.TryParseExact(model.StartedAt, "dd/MM/yyyy HH:mm", enUS, DateTimeStyles.None, out startedAt))
                {
                    return Error(log, HolidayAPIException.DateTimeWrongFormat(model.StartedAt));
                }
                if (!DateTime.TryParseExact(model.EndedAt, "dd/MM/yyyy HH:mm", enUS, DateTimeStyles.None, out endedAt))
                {
                    return Error(log, HolidayAPIException.DateTimeWrongFormat(model.EndedAt));
                }
                if (startedAt > endedAt)
                {
                    return Error(log, HolidayAPIException.WrongDateRange(model.StartedAt, model.EndedAt));
                }

                model.CancelSectionSlotIds = model.CancelSectionSlotIds?.Distinct().ToList() ?? new List<long>();
                var sectionSlots = _db.SectionSlots.Where(x => model.CancelSectionSlotIds.Contains(x.Id)
                                                                    && x.Date >= startedAt
                                                                    && x.Date <= endedAt
                    ).ToList();
                var foundSectionSlotIds = sectionSlots.Select(x => x.Id).ToList();
                if (model.CancelSectionSlotIds.Count != foundSectionSlotIds.Count)
                {
                    var notFound = model.CancelSectionSlotIds.Except(foundSectionSlotIds).ToList();
                    return Error(log, HolidayAPIException.WrongSectionIds(notFound));
                }
                var roomSlots = _db.RoomSlots.Where(x => foundSectionSlotIds.Contains(x.SectionSlotId.Value)).ToList();

                MuicHoliday muicHoliday = new MuicHoliday
                {
                    CreatedAt = now,
                    CreatedBy = model.CreatedBy,
                    EndedAt = endedAt,
                    IsMakeUpAble = model.IsAllowMakeup,
                    MuicId = model.MuicId,
                    Name = model.Name,
                    Remark = model.Remark,
                    StartedAt = startedAt,
                    UpdatedAt = now,
                    UpdatedBy = model.CreatedBy,
                };

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        roomSlots.Select(x => { x.IsCancel = true; return x; }).ToList();
                        sectionSlots.Select(x => { x.Status = "c"; return x; }).ToList();
                        _db.SaveChanges();

                        _db.MuicHolidays.Add(muicHoliday);
                        _db.SaveChangesNoAutoUserIdAndTimestamps();

                        log.ReferTable = "MuicHolidays";
                        log.ReferId = muicHoliday.Id + "";

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return Error(log, HolidayAPIException.UpdateError(e.Message));
                    }
                }

                return Success(log, 1);
            }
            finally
            {
                try
                {
                    await _db.ApiCallLogs.AddAsync(log);
                    await _db.SaveChangesAsync();
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Function to inactive all holiday with the same `muicId`. This will allow section creation. [This will not revert already cancelled slot] 
        /// </summary>
        /// <param name="muicId"></param>
        /// <param name="updatedBy"></param>
        /// <response code="200">Holiday is successfully created and all section slot is canceled.</response>
        /// <response code="401">Specify api-key</response>
        /// <response code="400">Wrong input format or error see message detail</response>
        [HttpDelete]
        [Route("[controller]/{muicId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SuccessAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizeAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestWrongDateResponseExample))]
        public async Task<IActionResult> DeleteHolidayAsync(long muicId, [Required] string updatedBy)
        {
            DateTime now = DateTime.UtcNow;
            ApiCallLog log = new ApiCallLog
            {
                Endpoint = HttpContext.Request.Path,
                HttpMethod = HttpContext.Request.Method,
                ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                RequestPayload = $"muicId={muicId},updatedBy={updatedBy}",
                Timestamp = now,
            };

            try
            {
                if (ValidApiKey())
                {
                    return Unauthorized(log, ApiException.InvalidKey());
                }

                if (muicId < 1)
                {
                    return BadRequest(log, ApiException.InvalidParameter("'muicId' is required and must be more than 0"));
                }
                if (string.IsNullOrEmpty(updatedBy))
                {
                    return BadRequest(log, ApiException.InvalidParameter("'updatedBy' is required"));
                }


                var muicHolidays = _db.MuicHolidays.Where(x => x.MuicId == muicId).ToList();

                muicHolidays = muicHolidays.Select(x =>
                {
                    x.IsActive = false;
                    x.UpdatedBy = updatedBy;
                    x.UpdatedAt = DateTime.UtcNow;
                    return x;
                }).ToList();

                _db.SaveChangesNoAutoUserIdAndTimestamps();

                log.ReferTable = "MuicHolidays";

                return Success(log, 1);
            }
            finally
            {
                try
                {
                    await _db.ApiCallLogs.AddAsync(log);
                    await _db.SaveChangesAsync();
                }
                catch (Exception)
                {
                }
            }
        }

    }

    public class CreateHolidayViewModelDefault : IExamplesProvider<CreateHolidayViewModel>
    {
        public CreateHolidayViewModel GetExamples()
        {
            return new CreateHolidayViewModel
            {
                MuicId = 1001,
                Name = "Mother Day",
                Remark = "",
                StartedAt = "12/08/2024 00:00",
                EndedAt = "12/08/2024 00:00",
                CancelSectionSlotIds = new List<long>() { 6521236, 652199 },
                IsAllowMakeup = false,
                CreatedBy = "MUIC Staff"
            };
        }
    }

    public class GetHolidayViewModelExample : IExamplesProvider<APIResponse>
    {
        public APIResponse GetExamples()
        {
            return new APIResponse
            {
                Code = "200",
                Message = "Success",
                Data = new List<HolidayViewModel>
                        {
                            new HolidayViewModel
                            {
                                Id = 1,
                                MuicId = 1001,
                                Name = "Mother Day",
                                Remark = "",
                                StartedAt = new DateTime (2024,08,12, 0,0 ,0 ),
                                EndedAt =  new DateTime (2024,08,12, 0,0 ,0 ),
                                IsAllowMakeup = false,
                                IsActive = true,
                                CreatedBy = "MUIC Staff",
                                CreatedAt = new DateTime(2024,08,25,20,08,00),
                            },
                            new HolidayViewModel
                            {
                                Id = 2,
                                MuicId = 1001,
                                Name = "Mother Day",
                                Remark = "",
                                StartedAt = new DateTime (2024,08,12, 0,0 ,0 ),
                                EndedAt =  new DateTime (2024,08,12, 0,0 ,0 ),
                                IsAllowMakeup = true,
                                IsActive = false,
                                CreatedBy = "MUIC Staff",
                                CreatedAt = new DateTime(2024,08,25,23,08,00),
                            },
                        }
            };
        }
    }
}