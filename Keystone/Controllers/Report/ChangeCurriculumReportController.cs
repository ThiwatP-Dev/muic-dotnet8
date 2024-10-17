using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ChangeCurriculumReport", "")]
    public class ChangeCurriculumReportController : BaseController
    {
        public ChangeCurriculumReportController(ApplicationDbContext db,
                                                IFlashMessage flashMessage,
                                                IMapper mapper,
                                                ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var logs = _db.StudentTransferLogs.Include(x => x.Term)
                                              .Include(x => x.Student)
                                                  .ThenInclude(x => x.AcademicInformation)
                                                      .ThenInclude(x => x.Faculty)
                                           .Where(x => x.Source.StartsWith("Change Curriculum")
                                                       && x.TermId == criteria.TermId)
                                           .Select(x => new
                                                        {
                                                            x.Remark,
                                                            FacultyName = x.Student.AcademicInformation.Faculty.NameEn,
                                                            StudentCode = x.Student.Code,
                                                            StudentFullName = x.Student.FullNameEn,
                                                            TermText = x.Term.TermText,
                                                            x.CreatedAt
                                                        })
                                           .ToList();

            List<ChangeCurriculumReportViewModel> results = new List<ChangeCurriculumReportViewModel>();
            foreach (var item in logs)
            {
                if (string.IsNullOrEmpty(item.Remark))
                {
                    continue;
                }
                // Find the index of the first occurrence of each ID
                int oldIdStart = item.Remark.IndexOf("Old Curriculum Version Id':") + "Old Curriculum Version Id':".Length;
                int newIdStart = item.Remark.IndexOf("New Curriculum Version Id':") + "New Curriculum Version Id':".Length;

                // Extract the IDs as substrings
                string oldIdString = item.Remark.Substring(oldIdStart, item.Remark.IndexOf(",", oldIdStart) - oldIdStart);
                string newIdString = item.Remark.Substring(newIdStart, item.Remark.IndexOf(")", newIdStart) - newIdStart);

                // Parse the IDs to integers
                long oldVersionId = long.Parse(oldIdString);
                long newVersionId = long.Parse(newIdString);

                string oldCurriculum = string.Empty;
                try
                {
                    var oldId = oldVersionId;
                    var oldVersion = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                           .SingleOrDefault(x => x.Id == oldId);
                    oldVersionId = oldVersion?.CurriculumId ?? 0;
                    oldCurriculum = oldVersion?.CodeAndName;
                }
                catch { }

                string newCurriculum = string.Empty;
                try
                {
                    var newId = newVersionId;
                    var newVersion = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                           .SingleOrDefault(x => x.Id == newId);
                    newVersionId = newVersion?.CurriculumId ?? 0;
                    newCurriculum = newVersion?.CodeAndName;
                }
                catch { }

                results.Add(new ChangeCurriculumReportViewModel
                            {
                                OldCurriculumId = oldVersionId,
                                OldCurriculumName = oldCurriculum,
                                NewCurriculumId = newVersionId,
                                NewCurriculumName = newCurriculum,
                                Faculty = item.FacultyName,
                                StudentCode = item.StudentCode,
                                StudentFullName = item.StudentFullName,
                                RequestedTerm = item.TermText,
                                ApprovedTerm = item.TermText,
                                Remark = string.Empty,
                                ApprovedDate =  item.CreatedAt.ToString(StringFormat.ShortDate)
                            });
            }

            var model = results.Where(x => (criteria.OldCurriculumId == 0 || x.OldCurriculumId == criteria.OldCurriculumId)
                                           && (criteria.NewCurriculumId == 0 || x.NewCurriculumId == criteria.NewCurriculumId))
                               .AsQueryable()
                               .GetPaged(criteria, page, true);
                               
            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculum();
        }
    }
}