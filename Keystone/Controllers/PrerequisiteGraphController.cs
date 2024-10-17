using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("PrerequisiteGraph", "")]
    public class PrerequisiteGraphController : BaseController
    {
        private IPrerequisiteProvider _prerequisiteProvider;
        public PrerequisiteGraphController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              IMapper mapper,
                              ISelectListProvider selectListProvider,
                              IPrerequisiteProvider prerequisiteProvider,
                              ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.CurriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var model = _prerequisiteProvider.GetPrerequisiteGraph(criteria.CurriculumVersionId);
            return View(model);
        }

        public void CreateSelectList()
        {
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersions();
        }
    }
}