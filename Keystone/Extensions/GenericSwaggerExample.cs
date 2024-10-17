using Keystone.Controllers.MasterTables;
using KeystoneLibrary.Models.Api.ApiResponse;
using Swashbuckle.AspNetCore.Filters;
using static Keystone.Controllers.MasterTables.SectionAPIController;

namespace Keystone.Extensions
{
    public class GenericSwaggerExample
    {
        public static void AddSwaggerExamplesFromAssemblies(IServiceCollection services)
        {
            services.AddSwaggerExamplesFromAssemblyOf<CreateHolidayViewModelDefault>();
            services.AddSwaggerExamplesFromAssemblyOf<GetSectionSlotsDefault>();
            services.AddSwaggerExamplesFromAssemblyOf<SuccessAPIResponseExample>();
            services.AddSwaggerExamplesFromAssemblyOf<UnAuthorizeAPIResponseExample>();
            services.AddSwaggerExamplesFromAssemblyOf<BadRequestWrongDateResponseExample>();
        }
    }

    public class SuccessAPIResponseExample : IExamplesProvider<APIResponse>
    {
        public APIResponse GetExamples()
        {
            return new APIResponse
            {
                Code = "200",
                Message = "Success",
                Data = 1,
            };
        }

    }

    public class UnAuthorizeAPIResponseExample : IExamplesProvider<APIResponse>
    {
        public APIResponse GetExamples()
        {
            return new APIResponse
            {
                Code = "401API004",
                Message = "Invalid Key",
                Data = null,
            };
        }

    }

    public class BadRequestWrongDateResponseExample : IExamplesProvider<APIResponse>
    {
        public APIResponse GetExamples()
        {
            return new APIResponse
            {
                Code = "400API029",
                Message = "Given Date (13/07/2024d) is invalid. Format = (dd/mm/yyyy)",
                Data = null,
            };
        }

    }
}
