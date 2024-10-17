using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class UserProvider : BaseProvider, IUserProvider
    {
        public UserProvider(ApplicationDbContext db) : base(db) { }

        public UserTimeStamp FillUserTimeStampFullName(UserTimeStamp model)
        {
            var createBy = (from user in _db.Users
                            join instructor in _db.Instructors on user.UserName equals instructor.Code into instructors
                            from instructor in instructors.DefaultIfEmpty()
                            join title in _db.Titles on instructor.TitleId equals title.Id into titles
                            from title in titles.DefaultIfEmpty()
                            where user.Id == model.CreatedBy
                            select new
                            {
                                user,
                                instructor,
                                title
                            }).FirstOrDefault();
            if (createBy != null) {
                model.CreatedByFullNameEn = createBy.instructor != null ? createBy.instructor.FullNameEn 
                                                                        : string.IsNullOrEmpty(createBy.user.FirstnameEN) 
                                                                        ? createBy.user.UserName
                                                                        : $"{ createBy.user.FirstnameEN } { createBy.user.LastnameEN }";
            }
            var updateBy = (from user in _db.Users
                            join instructor in _db.Instructors on user.UserName equals instructor.Code into instructors
                            from instructor in instructors.DefaultIfEmpty()
                            join title in _db.Titles on instructor.TitleId equals title.Id into titles
                            from title in titles.DefaultIfEmpty()
                            where user.Id == model.UpdatedBy
                            select new
                            {
                                user,
                                instructor,
                                title
                            }).FirstOrDefault();
            if (updateBy != null)
            {
                model.UpdatedByFullName = updateBy.instructor != null ? updateBy.instructor.FullNameEn 
                                                                      : string.IsNullOrEmpty(updateBy.user.FirstnameEN) 
                                                                      ? updateBy.user.UserName
                                                                      : $"{ updateBy.user.FirstnameEN } { updateBy.user.LastnameEN }";
            }
            return model;
        }

        public UserTimeStamp FillUserTimeStampFullNameWithOptionalStudent(UserTimeStamp model)
        {
            FillUserTimeStampFullName(model);
            if (string.IsNullOrEmpty(model.CreatedByFullNameEn) && !string.IsNullOrEmpty(model.CreatedBy))
            {
                // Try find student id from createdby
                if (Guid.TryParse(model.CreatedBy, out Guid guid))
                {
                    var student = _db.Students.AsNoTracking()
                                              .FirstOrDefault(x => x.Id == guid);
                    if (student != null)
                    {
                        model.CreatedByFullNameEn = student.CodeAndName;
                    }
                }
            }
            if (string.IsNullOrEmpty(model.UpdatedByFullName) && !string.IsNullOrEmpty(model.UpdatedBy))
            {
                if (Guid.TryParse(model.UpdatedBy, out Guid guid))
                {
                    var student = _db.Students.AsNoTracking()
                                              .FirstOrDefault(x => x.Id == guid);
                    if (student != null)
                    {
                        model.CreatedByFullNameEn = student.CodeAndName;
                    }
                }
            }
            return model;
        }

        public List<UserTimeStamp> FillUserTimeStampFullName(List<UserTimeStamp> models)
        {
            foreach (var model in models)
            {
                FillUserTimeStampFullName(model);
            }
            return models;
        }

        public List<UserTimeStamp> FillUserTimeStampFullNameWithOptionalStudent(List<UserTimeStamp> models)
        {
            foreach (var model in models)
            {
                FillUserTimeStampFullNameWithOptionalStudent(model);
            }
            return models;
        }

        public ApplicationUser GetUser(string id)
        {
            return _db.Users.IgnoreQueryFilters().SingleOrDefault(x => x.Id == id);
        }

        public List<UserTimeStamp> GetCreatedFullNameByIds(List<string> Ids)
        {
            var createBys = (from user in _db.Users
                            join instructor in _db.Instructors on user.UserName equals instructor.Code into instructors
                            from instructor in instructors.DefaultIfEmpty()
                            join title in _db.Titles on instructor.TitleId equals title.Id into titles
                            from title in titles.DefaultIfEmpty()
                            where Ids.Contains(user.Id)
                            select new
                            {
                                user,
                                instructor,
                                title
                            }).ToList();

            var result = createBys.GroupBy(x => x.user)
                                  .Select(x => new UserTimeStamp
                                               {
                                                   CreatedBy = x.FirstOrDefault().user != null ? x.FirstOrDefault().user.Id : "",
                                                   CreatedByFullNameEn = x.FirstOrDefault().instructor != null ? x.FirstOrDefault().instructor.FullNameEn 
                                                                         : string.IsNullOrEmpty(x.FirstOrDefault().user.FirstnameEN) 
                                                                         ? x.FirstOrDefault().user.UserName
                                                                         : $"{ x.FirstOrDefault().user.FirstnameEN } { x.FirstOrDefault().user.LastnameEN }"
                                               })
                                  .ToList();
            return result;
        }

        public string GetUserFullNameById(string id)
        {
            var createBy = (from user in _db.Users
                            join instructor in _db.Instructors on user.UserName equals instructor.Code into instructors
                            from instructor in instructors.DefaultIfEmpty()
                            join title in _db.Titles on instructor.TitleId equals title.Id into titles
                            from title in titles.DefaultIfEmpty()
                            where user.Id == id
                            select new
                            {
                                user,
                                instructor,
                                title
                            }).FirstOrDefault();
            if (createBy != null) {
                return createBy.instructor != null ? createBy.instructor.FullNameEn 
                                                   : string.IsNullOrEmpty(createBy.user.FirstnameEN) 
                                                   ? createBy.user.UserName
                                                   : $"{ createBy.user.FirstnameEN } { createBy.user.LastnameEN }";
            }
            
            return "";
        }
    }
}