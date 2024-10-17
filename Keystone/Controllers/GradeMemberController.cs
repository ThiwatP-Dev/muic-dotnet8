using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("GradeMember", "")]
    public class GradeMemberController : BaseController
    {
        public GradeMemberController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria, int page = 1)
        {
            var members = _db.GradeMembers.Where(x => string.IsNullOrEmpty(criteria.CodeAndName) 
                                                      || x.User.FirstnameEN.Contains(criteria.CodeAndName)
                                                      || x.User.UserName.Contains(criteria.CodeAndName))
                                          .Include(x => x.User)
                                          .Select(x => new GradeMemberDetailViewModel
                                                       {
                                                           Id = x.Id,
                                                           UserId = x.UserId,
                                                           UserName = x.User.UserName,
                                                           FirstName = x.User.FirstnameEN,
                                                           LastName = x.User.LastnameEN
                                                       })
                                          .OrderBy(x => x.UserName)
                                          .GetPaged(criteria ,page);

            if(members.Results.Count > 0)
            {
                var userIds = members.Results.Select(x => x.UserId).ToList();
                var userNames = members.Results.Select(x => x.UserName).ToList();
                var instructors = _db.Instructors.Where(x => userNames.Contains(x.Code)
                                                             && x.Title.NameEn != "Not Specified")
                                                 .Select(x => new 
                                                              {
                                                                  UserName = x.Code,
                                                                  FullNameEn = x.Title.NameEn + " " + x.FirstNameEn + " " + x.LastNameEn
                                                              })
                                                 .ToList();
                var gradeMember = _db.GradeMembers.Where(x => userIds.Any(y => y == x.UserId)).ToList();
                foreach (var item in members.Results)
                {
                    item.FullNameEn = instructors.Any(x => x.UserName == item.UserName) ? instructors.Where(x => x.UserName == item.UserName)
                                                                                                    .FirstOrDefault().FullNameEn
                                                                                        : string.IsNullOrEmpty(item.FirstName) ? item.UserName
                                                                                                                               : item.FirstName + " " + item.LastName;
                }
            }
            return View(members);
        }

        [PermissionAuthorize("GradeMember", PolicyGenerator.Write)]
        public ActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _db.Users.Where(x => string.IsNullOrEmpty(criteria.CodeAndName) 
                                             || x.FirstnameEN.Contains(criteria.CodeAndName)
                                             || x.UserName.Contains(criteria.CodeAndName))
                                 .Select(x => new GradeMemberDetailViewModel
                                              {
                                                  UserId = x.Id,
                                                  UserName = x.UserName,
                                                  FirstName = x.FirstnameEN,
                                                  LastName = x.LastnameEN
                                              })
                                 .OrderBy(x => x.UserName)
                                 .ToList();
            var userIds = model.Select(x => x.UserId).ToList();
            var userNames = model.Select(x => x.UserName).ToList();
            var instructors = _db.Instructors.Where(x => userNames.Contains(x.Code)
                                                         && x.Title.NameEn != "Not Specified")
                                             .Select(x => new 
                                                          {
                                                             UserName = x.Code,
                                                             FullNameEn = x.Title.NameEn + " " + x.FirstNameEn + " " + x.LastNameEn
                                                          })
                                             .ToList();
            var gradeMember = _db.GradeMembers.Where(x => userIds.Any(y => y == x.UserId)).ToList();
            foreach (var item in model)
            {
                item.IsChecked = gradeMember.Any(x => x.UserId == item.UserId) ? "on" : null;
                item.FullNameEn = instructors.Any(x => x.UserName == item.UserName) ? instructors.Where(x => x.UserName == item.UserName)
                                                                                                 .FirstOrDefault().FullNameEn
                                                                                    : string.IsNullOrEmpty(item.FirstName) ? item.UserName
                                                                                                                           : item.FirstName + " " + item.LastName;
            }
            var result = new GradeMemberViewModel
                         {
                             Criteria = criteria,
                             GradeMembers = model
                         };

            return View(result);
        }

        [PermissionAuthorize("GradeMember", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult SaveCreate(GradeMemberViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            try
            {
                var duplicateIds = _db.GradeMembers.Where(x => model.GradeMembers.Any(y => y.UserId == x.UserId))
                                                   .Select(x => x.UserId)
                                                   .ToList();

                var removeUnCheckedIds = model.GradeMembers.Where(x => x.IsChecked == null)
                                                           .Select(x => x.UserId)
                                                           .ToList();

                var removeGradeMembers = _db.GradeMembers.Where(x => removeUnCheckedIds.Contains(x.UserId))
                                                         .ToList();

                model.GradeMembers = model.GradeMembers.Where(x => x.IsChecked == "on"
                                                                   && !duplicateIds.Contains(x.UserId))
                                                       .ToList();

                _db.GradeMembers.RemoveRange(removeGradeMembers);
                var gradeMembers = new List<GradeMember>();
                foreach (var item in model.GradeMembers)
                {
                    gradeMembers.Add(new GradeMember
                                     {
                                         UserId = item.UserId
                                     });
                }

                _db.GradeMembers.AddRange(gradeMembers);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Create), model.Criteria);
            }
        }

        [PermissionAuthorize("GradeMember", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = Find(id);
            try
            {
                model.IsActive = false;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private GradeMember Find(long? id)
        {
            var model = _db.GradeMembers.SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}