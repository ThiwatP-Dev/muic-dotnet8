using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradeTemplate", "")]
    public class GradeTemplateController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IGradeProvider _gradeProvider;

        public GradeTemplateController(ApplicationDbContext db,
                                       IGradeProvider gradeProvider,
                                       IMapper mapper,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _gradeProvider = gradeProvider;
        }

        public IActionResult Index()
        {
            var templates = _gradeProvider.GetTemplates();
            var models = _mapper.Map<List<GradeTemplateViewModel>>(templates);
            return View(models);
        }

        [PermissionAuthorize("GradeTemplate", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            CreateSelectList();
            return View(new GradeTemplateViewModel());
        }

        [PermissionAuthorize("GradeTemplate", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GradeTemplateViewModel model)
        {
            var gradeTemple = _mapper.Map<GradeTemplateViewModel, GradeTemplate>(model);
            if (!ModelState.IsValid || gradeTemple == null)
            {
                CreateSelectList();
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            try
            {
                _gradeProvider.AddGradeTemplate(gradeTemple);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public IActionResult Edit(long id)
        {
            CreateSelectList();
            var template = _gradeProvider.FindGradeTemplate(id);
            var model = _mapper.Map<GradeTemplate, GradeTemplateViewModel>(template);
            return View(model);
        }

        [PermissionAuthorize("GradeTemplate", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GradeTemplateViewModel model)
        {
            var template = _gradeProvider.FindGradeTemplate(model.Id);
            if (!ModelState.IsValid || template == null || !await TryUpdateModelAsync(template))
            {
                CreateSelectList();
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            else
            {
                // Need to update GradeIds manually (model which is sent back from view doesn't include in key-value pair of Request.Form)
                template.GradeIds = model.GradeIds;
            }

            try
            {
                _gradeProvider.UpdateGradeTemplate(template);
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("GradeTemplate", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            try
            {
                _gradeProvider.DeleteGradeTemplate(id);
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult GetGrade(long id)
        {
            var grade = _gradeProvider.GetGradeById(id);
            return Ok(grade);
        }

        private IActionResult GetGrades(List<long> gradeIds)
        {
            var grades = _gradeProvider.GetGrades()
                                       .Join(gradeIds, x => x.Id, y => y, (x, y) => x)
                                       .ToList();
            return Ok(grades);
        }

        private void CreateSelectList()
        {
            ViewBag.Grades = _selectListProvider.GetGrades();
        }
    }
}