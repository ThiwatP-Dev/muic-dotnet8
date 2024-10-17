using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.DataModels.Prerequisites;

namespace KeystoneLibrary.Providers
{
    public class CurriculumProvider : BaseProvider, ICurriculumProvider
    {
        protected IPrerequisiteProvider _prerequisiteProvider;

        public CurriculumProvider(ApplicationDbContext db,
                                  IMapper mapper,
                                  IPrerequisiteProvider prerequisiteProvider) : base(db, mapper) 
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public Curriculum GetCurriculum(long id)
        {
            var curriculum = _db.Curriculums.Include(x => x.AcademicLevel)
                                            .Include(x => x.Faculty)
                                            .Include(x => x.Department)
                                            .Include(x => x.CurriculumVersions)
                                                .ThenInclude(x => x.OpenedTerm)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);

            curriculum.CurriculumVersions = curriculum.CurriculumVersions.OrderBy(x => x.NameEn)
                                                                         .ToList();

            return curriculum;
        }

        public string GetCurriculumVersionName(long versionId)
        {
            return _db.CurriculumVersions.SingleOrDefault(x => x.Id == versionId).NameEn;
        }

        public CurriculumVersion GetCurriculumVersion(long id)
        {
            var versions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                     .ThenInclude(x => x.Faculty)
                                                 .Include(x => x.Curriculum)
                                                     .ThenInclude(x => x.Department)
                                                 .Include(x => x.Curriculum)
                                                     .ThenInclude(x => x.AcademicLevel)
                                                 .Include(x => x.AcademicProgram)
                                                 .Include(x => x.ClosedTerm)
                                                 .Include(x => x.OpenedTerm)
                                                 .Include(x => x.Term)
                                                 .Include(x => x.CurriculumInstructors)
                                                     .ThenInclude(x => x.Instructor)
                                                     .ThenInclude(x => x.InstructorWorkStatus)
                                                     .ThenInclude(x => x.AcademicLevel)
                                                 .Include(x => x.CurriculumBlacklistCourses)
                                                     .ThenInclude(x => x.Course)
                                                 .IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);

            if(versions?.CurriculumBlacklistCourses?.Any() ?? false)
            {
                versions.CurriculumBlacklistCourses = versions.CurriculumBlacklistCourses.OrderBy(x => x.Course.Code).ToList();
            }
            return versions;
        }

        public Curriculum GetCurriculumByVersionId(long curriculumVersionId)
        {
            var curriculumVersion = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                          .IgnoreQueryFilters()
                                                          .SingleOrDefault(x => x.Id == curriculumVersionId);

            return curriculumVersion == null ? new Curriculum() : curriculumVersion.Curriculum;
        }

        public CurriculumInformation GetCurriculumInformation(long id) 
        {
            var curriculumInformation = _db.CurriculumInformations.Include(x => x.CurriculumVersion)
                                                                  .Include(x => x.Faculty)
                                                                  .Include(x => x.Department)
                                                                  .Include(x => x.StudyPlan)
                                                                  .Include(x => x.Student)
                                                                      .ThenInclude(x => x.AcademicInformation)
                                                                  .Include(x => x.SpecializationGroupInformations)
                                                                  .IgnoreQueryFilters()
                                                                  .SingleOrDefault(x => x.Id == id);
            return curriculumInformation;
        }

        public List<CourseGroup> GetParentCourseGroupsByVersionId(long curriculumVersionId)
        {
            var courseGroups = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                               .Include(x => x.CurriculumCourses).ThenInclude(x => x.Course)
                                               .Where(x => x.CurriculumVersionId == curriculumVersionId
                                                           && x.CourseGroupId == null)
                                               .ToList();

            var grades = _db.Grades.IgnoreQueryFilters()
                                   .ToList();

            courseGroups.Select(x =>
            {
                x.RequiredGradeText = grades.SingleOrDefault(y => y.Id == x.RequiredGradeId)?.Name ?? "";
                return x;
            })
                        .ToList();

            return courseGroups;
        }


        public List<CourseGroup> GetCourseGroupsByVersionId(long curriculumVersionId)
        {
            var courseGroups = GetParentCourseGroupsByVersionId(curriculumVersionId);
            foreach (var item in courseGroups)
            {
                if (item.CourseGroupId.HasValue)
                {
                    item.NameEn = String.Format("{0} - {1}", GetCourseGroupName(item.CourseGroupId.Value), item.NameEn);
                }
            }

            return courseGroups;
        }

        public List<CourseGroup> GetCourseGroupsWithCourses(long versionId, long minorId, long concentrationId)
        {
            var courseGroups = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                               .Include(x => x.CurriculumCourses)
                                                  .ThenInclude(x => x.Course)
                                               .Where(x => x.CurriculumVersionId == versionId
                                                           && (minorId == 0
                                                               || x.SpecializationGroupId == minorId)
                                                           && (concentrationId == 0
                                                               || x.SpecializationGroupId == concentrationId))
                                               .OrderBy(x => x.Sequence)
                                               .ToList();

            foreach (var item in courseGroups)
            {
                if (item.CourseGroupId.HasValue)
                {
                    item.NameEn = String.Format("{0} - {1}", GetCourseGroupName(item.CourseGroupId.Value), item.NameEn);
                }
            }

            return courseGroups;
        }

        public List<CourseGroup> GetCourseGroupRecursiveByVersionId(long versionId)
        {
            var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null)
                                          .ToList();
                                          
            foreach (var parent in parents)
            {
                parent.ChildCourseGroups = RecursiveCourseGroupByCourseGroup(parent);
            }

            return parents;
        }

        private List<CourseGroup> RecursiveCourseGroupByCourseGroup(CourseGroup group)
        {
            var CourseGroups = new List<CourseGroup>();
            var children = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                           .Where(x => x.CourseGroupId == group.Id)
                                           .ToList();
            if (children.Any())
            {
                group.ChildCourseGroups = children;
                foreach (var child in children)
                {
                    CourseGroups.AddRange(RecursiveCourseGroupByCourseGroup(child));
                }
                return CourseGroups;
            }
            else 
            {
                var curriculumCourses = _db.CurriculumCourses.Include(x => x.Course)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.CourseGroupId == group.Id)
                                                             .OrderBy(x => x.Course.Code)
                                                             .ToList();

                if (curriculumCourses.Any())
                {
                    group.CurriculumCourses = curriculumCourses;
                    CourseGroups.Add(group);
                }
                
                return CourseGroups;
            }
        }
        public List<CourseGroup> GetCourseGroupsForChangeCurriculum(long versionId, long minorId, long concentrationId)
        {
            var courseGroups = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.CurriculumVersionId == versionId
                                                      && (minorId == 0
                                                          || x.SpecializationGroupId == minorId)
                                                      && (concentrationId == 0
                                                          || x.SpecializationGroupId == concentrationId))
                                          .ToList();
            foreach (var item in courseGroups)
            {
                if (item.CourseGroupId.HasValue)
                {
                    item.NameEn = String.Format("{0} - {1}", GetCourseGroupName(item.CourseGroupId.Value), item.NameEn);
                }
            }
            return courseGroups;
        }

        private string GetCourseGroupName(long courseGroupId)
        {
            var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.Id == courseGroupId);
            if (courseGroup.CourseGroupId.HasValue)
            {
                return String.Format("{0} - {1}", GetCourseGroupName(courseGroup.CourseGroupId.Value), courseGroup.NameEn);
            }
            else
            {
                return courseGroup.NameEn;
            }
        }

        public List<CourseGroup> GetCourseGroups(long versionId, long minorId, long concentrationId)
        {
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null
                                                      && (minorId == 0
                                                          || x.SpecializationGroupId == minorId)
                                                      && (concentrationId == 0
                                                          || x.SpecializationGroupId == concentrationId))
                                          .OrderBy(x => x.Sequence)
                                          .ToList();

            foreach (var group in parents)
            {
                RecursiveCourseGroup(group);
            }

            return parents;
        }

        public List<CourseGroup> GetCourseGroupBySpecialGroup(long specialGroupId)
        {
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.SpecializationGroupId == specialGroupId)
                                          .ToList();

            foreach (var group in parents)
            {
                RecursiveCourseGroup(group);
            }

            return parents;
        }

        public List<CourseGroup> GetCourseGroupsAndChildren(long versionId, long minorId, long concentrationId)
        {
            var courseGroups = new List<CourseGroup>();
            var parents = _db.CourseGroups.AsNoTracking()
                                          .Include(x => x.SpecializationGroup)
                                          .Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null
                                                      && (minorId == 0
                                                          || x.SpecializationGroupId == minorId)
                                                      && (concentrationId == 0
                                                          || x.SpecializationGroupId == concentrationId))
                                          .ToList();

            foreach (var group in parents)
            {
                group.FullPathName = group.NameEn;
                courseGroups.Add(group);
                courseGroups.AddRange(RecursiveCourseGroupAndChildren(group));
            }

            return courseGroups;
        }

        public List<CourseGroup> GetRegistrationCourseGroups(long versionId, List<CourseGroupingDetail> courses)
        {
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null)
                                          .ToList();

            foreach (var group in parents)
            {
                RecursiveRegistrationCourseGroup(group, courses);
            }

            return parents;
        }
        
        public List<CourseGroup> GetCourseGroupsBySpecializationGroupId(long specializationGroupId)
        {
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.SpecializationGroupId == specializationGroupId
                                                      && x.CourseGroupId == null
                                                      && x.CurriculumVersionId == null)
                                          .ToList();
                                          
            foreach (var group in parents)
            {
                RecursiveCourseGroup(group);
            }

            return parents;
        }

        private void RecursiveCourseGroup(CourseGroup courseGroup)
        {
            var groups = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                         .Where(x => x.CourseGroupId == courseGroup.Id)
                                         .OrderBy(x => x.Sequence)
                                         .ToList();

            courseGroup.CurriculumCourses = _db.CurriculumCourses.Include(x => x.Course)
                                                                     .Where(x => x.CourseGroupId == courseGroup.Id)
                                                                     .OrderBy(x => x.Course.Code)
                                                                     .ToList();
            courseGroup.CurriculumCourses.Select(x => {
                                                          x.RequiredGradeText = _db.Grades.SingleOrDefault(y => x.RequiredGradeId == y.Id)?.Name ?? "";
                                                          return x;
                                                      }).ToList();
                                                      
            if (groups.Any())
            {
                courseGroup.ChildCourseGroups = groups;
                foreach (var group in groups)
                {
                    RecursiveCourseGroup(group);
                }
            }
        }

        private List<CourseGroup> RecursiveCourseGroupAndChildren(CourseGroup courseGroup)
        {
            var allCourseGroups = new List<CourseGroup>();
            var childCourseGroups = _db.CourseGroups.Where(x => x.CourseGroupId == courseGroup.Id).ToList();
            if (childCourseGroups.Any())
            {
                foreach (var childCourseGroup in childCourseGroups)
                {
                    childCourseGroup.FullPathName = courseGroup.FullPathName + " -> " + childCourseGroup.NameEn;
                }
                allCourseGroups.AddRange(childCourseGroups);
                foreach (var group in childCourseGroups)
                {
                    RecursiveCourseGroupAndChildren(group);
                }
            }

            return allCourseGroups;
        }

        private void RecursiveRegistrationCourseGroup(CourseGroup courseGroup, List<CourseGroupingDetail> courses)
        {
            var groups = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                         .Where(x => x.CourseGroupId == courseGroup.Id).ToList();
            if (groups.Any())
            {
                courseGroup.ChildCourseGroups = groups;
                foreach (var group in groups)
                {
                    RecursiveRegistrationCourseGroup(group, courses);
                }
            }
            else
            {
                courseGroup.CurriculumCourses = (from curriculum in _db.CurriculumCourses.Include(x => x.Course)
                                                                                         .Include(x => x.Grade)
                                                 join course in courses on curriculum.CourseId equals course.CourseId
                                                 where curriculum.CourseGroupId == courseGroup.Id
                                                 select new CurriculumCourse
                                                 {
                                                     Course = curriculum.Course,
                                                     RequiredGradeText = curriculum.Grade == null ? string.Empty : curriculum.Grade.Name,
                                                     RegistrationGradeText = course.RegistrationCourse.GradeName
                                                 }).ToList();
            }
        }

        public bool IsExistCurriculumCode(string code)
        {
            var isExist = _db.CurriculumVersions.Any(x => x.Code == code);
            return isExist;
        }

        public List<CurriculumInstructor> SetCurriculumInstructor(List<long> instructorIds, string type, long versionId)
        {
            List<CurriculumInstructor> instructors = new List<CurriculumInstructor>();
            if (instructorIds != null)
            {
                instructors = instructorIds.Select(x => new CurriculumInstructor
                {
                    CurriculumVersionId = versionId,
                    InstructorId = x,
                    Type = type
                })
                                           .ToList();
            }

            return instructors;
        }

        public List<CurriculumStudyPlanViewModel> GetStudyPlansByCurriculumVersion(long versionId)
        {
            var plans = _db.StudyPlans.Include(x => x.StudyCourses)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.AcademicProgram)
                                      .Include(x => x.SpecializationGroup)
                                      .Where(x => x.CurriculumVersionId == versionId)
                                      .GroupBy(x => x.Year)
                                      .ToList()
                                      .Select(x => new CurriculumStudyPlanViewModel
                                      {
                                          Year = x.Key,
                                          TotalCredit = x.Sum(y => y.TotalCredit),
                                          CurriculumVersionId = x.First().CurriculumVersionId,
                                          StudyPlans = x.OrderBy(y => y.Term)
                                                                     .ToList(),
                                          CurriculumVersion = x.First().CurriculumVersion.NameEn,
                                          AcademicProgram = x.First().AcademicProgram?.NameEn ?? "N/A",
                                          SpecializationGroup = x.First().SpecializationGroup?.NameEn ?? "N/A",
                                      })
                                       .OrderBy(x => x.Year)
                                       .ToList();

            return plans;
        }

        public List<CorequisiteDetail> GetCurriculumCorequisites(long versionId)
        {
            var corequisites = (from curriculumDependency in _db.CurriculumDependencies
                                join curriculumVersion in _db.CurriculumVersions on curriculumDependency.CurriculumVersionId equals curriculumVersion.Id
                                join corequisite in _db.Corequisites on curriculumDependency.DependencyId equals corequisite.Id
                                join firstCourse in _db.Courses on corequisite.FirstCourseId equals firstCourse.Id
                                join secondCourse in _db.Courses on corequisite.SecondCourseId equals secondCourse.Id
                                where curriculumVersion.Id == versionId
                                      && curriculumDependency.DependencyType == "Corequisite"
                                select new CorequisiteDetail
                                       {
                                           CorequisiteId = corequisite.Id,
                                           CurriculumVersionId = curriculumVersion.Id,
                                           FirstCourse = firstCourse.CourseAndCredit,
                                           SecondCourse = secondCourse.CourseAndCredit,
                                           ExpiredDate = corequisite.ExpiredAtText,
                                           Description = corequisite.Description
                                       }).ToList();

            return corequisites;
        }

        public List<CourseEquivalentDetail> GetCurriculumCourseEquivalents(long versionId)
        {
            var courseEquivalents = (from curriculumDependency in _db.CurriculumDependencies
                                     join curriculumVersion in _db.CurriculumVersions on curriculumDependency.CurriculumVersionId equals curriculumVersion.Id
                                     join courseEquivalent in _db.CourseEquivalents on curriculumDependency.DependencyId equals courseEquivalent.Id
                                     join course in _db.Courses on courseEquivalent.CourseId equals course.Id
                                     join equivalentCourse in _db.Courses on courseEquivalent.EquilaventCourseId equals equivalentCourse.Id
                                     where curriculumVersion.Id == versionId
                                           && curriculumDependency.DependencyType == "Equivalence"
                                     select new CourseEquivalentDetail
                                            {
                                                CourseId = courseEquivalent.CourseId,
                                                CourseEquivalentId = courseEquivalent.Id,
                                                CurriculumVersionId = curriculumVersion.Id,
                                                Course = course.CourseAndCredit,
                                                EquivalentCourse = equivalentCourse.CourseAndCredit,
                                                EffectiveDate = courseEquivalent.EffectivedAtText,
                                                EndDate = courseEquivalent.EndedAtText,
                                                Remark = courseEquivalent.Remark
                                            }).ToList();

            return courseEquivalents;
        }

        public List<SpecializationGroup> GetSpecializationInformations()
        {       
            var specializationGroup = _db.SpecializationGroups.ToList();           
            return specializationGroup;
        }

        public List<SpecializationGroup> GetSpecializationInformationByCurriculumVersionId(long curriculumVersionId)
        {
            List<SpecializationGroup> selectedGroups = new List<SpecializationGroup>();     
            var specializationGroups = _db.SpecializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_ABILITY_CODE
                                                                           || x.Type == SpecializationGroup.TYPE_MINOR_CODE
                                                                           || x.Type == SpecializationGroup.TYPE_MODULE_CODE)
                                                               .AsNoTracking()
                                                               .ToList();   
            // Ability
            selectedGroups.AddRange(specializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_ABILITY_CODE));

            // Minor     
            selectedGroups.AddRange(specializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_MINOR_CODE));

            // Concentration
            var concentrationGroups = _db.CurriculumSpecializationGroups.Include(x => x.SpecializationGroup)
                                                                        .Where(x => x.SpecializationGroup.Type == SpecializationGroup.TYPE_CONCENTRATION_CODE
                                                                                    && x.CurriculumVersionId == curriculumVersionId)
                                                                        .AsNoTracking()
                                                                        .Select( x => x.SpecializationGroup) 
                                                                        .ToList(); 
            selectedGroups.AddRange(concentrationGroups);

            // Module     
            selectedGroups.AddRange(specializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_MODULE_CODE));

            return selectedGroups;
        }

        public StudyPlan GetStudyPlanById(long id)
        {
            var plan = _db.StudyPlans.Include(x => x.StudyCourses)
                                         .ThenInclude(x => x.Course)
                                     .SingleOrDefault(x => x.Id == id);
            return plan;
        }

        public List<StudyCourse> GetStudyCoursesByPlanId(long planId)
        {
            var courses = _db.StudyCourses.Where(x => x.StudyPlanId == planId)
                                          .ToList();
            return courses;
        }

        public List<Course> GetCurriculumCourse(long versionId)
        {
            var courses = _db.CurriculumCourses.Include(x => x.CourseGroup)
                                               .Include(x => x.Course)
                                               .Where(x => x.CourseGroup.CurriculumVersionId == versionId)
                                               .Select(x => x.Course)
                                               .Distinct()
                                               .OrderBy(x => x.Code)
                                               .ToList();

            return courses;
        }

        public List<CurriculumCourse> GetCurriculumCourses(List<long> courseGroupIds)
        {
            var curriculumCourses = _db.CurriculumCourses.Include(x => x.Course)
                                                         .Where(x => courseGroupIds.Contains(x.CourseGroupId))
                                                         .ToList();
            return curriculumCourses;
        }

        public void CopyCurriculumVersion(long curriculumVersionId, 
            long oldCurriculumVersionId, 
            List<CourseGroup> courseGroups, 
            bool isCopyPrerequisite,
            bool isCopySpecializeGroup,
            bool isCopyBlacklistCourses, 
            bool isCopyCoRequisiteAndCourseEquivalent)
        {
            if (isCopyPrerequisite)
            {
                var prerequisites = _prerequisiteProvider.GetPrerequisiteByCurriculumVersionId(oldCurriculumVersionId);
                if (prerequisites != null)
                {
                    foreach (var item in prerequisites)
                    {
                        _db.Prerequisites.Add(new Prerequisite
                                            {
                                                ConditionType = item.ConditionType,
                                                CourseId = item.CourseId,
                                                ConditionId = item.ConditionId,
                                                ExpiredAt = item.ExpiredAt,
                                                CreatedAt = item.CreatedAt,
                                                CreatedBy = item.CreatedBy,
                                                UpdatedAt = item.UpdatedAt,
                                                UpdatedBy = item.UpdatedBy,
                                                IsActive = item.IsActive,
                                                Description = item.Description,
                                                CurriculumVersionId = curriculumVersionId,
                                                MUICId = item.MUICId
                                            });
                    }

                    _db.SaveChanges();
                }
            }

            if (isCopySpecializeGroup)
            {
                var copiedSpecializeGroup = _db.CurriculumSpecializationGroups.AsNoTracking()
                                                                              .Where(x => x.CurriculumVersionId == oldCurriculumVersionId)
                                                                              .ToList()
                                                                              .Select(x => new CurriculumSpecializationGroup
                                                                              {
                                                                                  CurriculumVersionId = curriculumVersionId,
                                                                                  SpecializationGroupId = x.SpecializationGroupId,
                                                                                  IsActive = x.IsActive
                                                                              })
                                                                              .ToList();
                if (copiedSpecializeGroup != null && copiedSpecializeGroup.Any())
                {
                    _db.CurriculumSpecializationGroups.AddRange(copiedSpecializeGroup);
                    _db.SaveChanges();
                }
            }

            if (isCopyBlacklistCourses)
            {
                var copiedBlacklistCourses = _db.CurriculumBlacklistCourses.AsNoTracking()
                                                                           .Where(x => x.CurriculumVersionId == oldCurriculumVersionId)
                                                                           .ToList()
                                                                           .Select(x => new CurriculumBlacklistCourse
                                                                           {
                                                                               CurriculumVersionId = curriculumVersionId,
                                                                               CourseId = x.CourseId,
                                                                               IsActive = x.IsActive
                                                                           })
                                                                           .ToList();
                if (copiedBlacklistCourses != null && copiedBlacklistCourses.Any())
                {
                    _db.CurriculumBlacklistCourses.AddRange(copiedBlacklistCourses);
                    _db.SaveChanges();
                }
            }

            if (isCopyCoRequisiteAndCourseEquivalent)
            {
                var copiedCurriculumDependencies = _db.CurriculumDependencies.AsNoTracking()
                                                                             .Where(x => x.CurriculumVersionId == oldCurriculumVersionId)
                                                                             .ToList()
                                                                             .Select(x => new CurriculumDependency
                                                                             {
                                                                                 CurriculumVersionId = curriculumVersionId,
                                                                                 DependencyId = x.DependencyId,
                                                                                 DependencyType = x.DependencyType,
                                                                                 IsActive = x.IsActive
                                                                             })
                                                                             .ToList();
                if (copiedCurriculumDependencies != null && copiedCurriculumDependencies.Any())
                {
                    _db.CurriculumDependencies.AddRange(copiedCurriculumDependencies);
                    _db.SaveChanges();
                }
            }
                
            var selectedCourseGroups = new List<CourseGroup>();
            var selectedCourseGroupParent = courseGroups.Where(x => x.IsSelected)
                                                        .ToList();
                                                        
            selectedCourseGroups.AddRange(selectedCourseGroupParent);
            var selectedCourseGroupAndVersionIds = selectedCourseGroupParent.Select(x => new
            {
                CourseGroupId = x.Id,
                VersionId = x.CurriculumVersionId
            });
            foreach (var id in selectedCourseGroupAndVersionIds)
            {
                selectedCourseGroups.AddRange(GetChildCourseGroups(id.VersionId ?? 0, id.CourseGroupId));
            }

            var parentCourseGroups = selectedCourseGroups.Where(x => x.CourseGroupId == null);
            var curriculumCourses = GetCurriculumCourses(selectedCourseGroups.Select(x => x.Id).ToList());
            foreach (var parentCourseGroup in parentCourseGroups)
            {
                var newParentCourseGroup = CopyCourseGroupModel(parentCourseGroup, curriculumVersionId);
                _db.CourseGroups.Add(newParentCourseGroup);
                _db.SaveChanges();

                var parentCurriculumCourses = curriculumCourses.Where(x => x.CourseGroupId == parentCourseGroup.Id);
                CopyCurriculumCourse(newParentCourseGroup.Id, parentCurriculumCourses);

                var childCourseGroups = selectedCourseGroups.Where(x => x.CourseGroupId == parentCourseGroup.Id);
                foreach (var childCourseGroup in childCourseGroups)
                {
                    var newChildCourseGroup = CopyCourseGroupModel(childCourseGroup, curriculumVersionId, newParentCourseGroup.Id);
                    _db.CourseGroups.Add(newChildCourseGroup);
                    _db.SaveChanges();

                    var childCurriculumCourses = curriculumCourses.Where(x => x.CourseGroupId == childCourseGroup.Id);
                    CopyCurriculumCourse(newChildCourseGroup.Id, childCurriculumCourses);

                    if (childCourseGroup.ChildCourseGroups != null && childCourseGroup.ChildCourseGroups.Any())
                    {
                        NestedCopyCourseGroup(childCourseGroup.ChildCourseGroups, curriculumVersionId, newChildCourseGroup.Id);
                    }
                }
            }
        }

        private void NestedCopyCourseGroup(ICollection<CourseGroup> childCourseGroups, long curriculumVersionId, long newParentCourseGroupId)
        {
            var curriculumCourses = GetCurriculumCourses(childCourseGroups.Select(x => x.Id).ToList());
            foreach (var childCourseGroup in childCourseGroups)
            {
                var newChildCourseGroup = CopyCourseGroupModel(childCourseGroup, curriculumVersionId, newParentCourseGroupId);
                _db.CourseGroups.Add(newChildCourseGroup);
                _db.SaveChanges();

                var childCurriculumCourses = curriculumCourses.Where(x => x.CourseGroupId == childCourseGroup.Id);
                CopyCurriculumCourse(newChildCourseGroup.Id, childCurriculumCourses);

                if (childCourseGroup.ChildCourseGroups != null && childCourseGroup.ChildCourseGroups.Any())
                {
                    NestedCopyCourseGroup(childCourseGroup.ChildCourseGroups, curriculumVersionId, newChildCourseGroup.Id);
                }
            }
        }

        private CourseGroup CopyCourseGroupModel(CourseGroup courseGroup, long curriculumVersionId, long? courseGroupId = null)
        {
            var newCourseGroup = new CourseGroup
            {
                CourseGroupId = courseGroupId,
                CurriculumVersionId = curriculumVersionId,
                RequiredGradeId = courseGroup.RequiredGradeId,
                NameEn = courseGroup.NameEn,
                NameTh = courseGroup.NameTh,
                DescriptionEn = courseGroup.DescriptionEn,
                DescriptionTh = courseGroup.DescriptionTh,
                Credit = courseGroup.Credit,
                SpecializationGroupId = courseGroup.SpecializationGroupId,
                Type = courseGroup.Type ?? "",
                Sequence = courseGroup.Sequence,
                Remark = courseGroup.Remark
            };
            return newCourseGroup;
        }

        private void CopyCurriculumCourse(long courseGroupId, IEnumerable<CurriculumCourse> curriculumCourses)
        {
            foreach (var childCurriculumCourse in curriculumCourses)
            {
                var newParentCurriculumCourse = new CurriculumCourse
                {
                    CourseId = childCurriculumCourse.CourseId,
                    CourseGroupId = courseGroupId,
                    IsRequired = childCurriculumCourse.IsRequired,
                    RequiredGradeId = childCurriculumCourse.RequiredGradeId,
                    GradeTemplateId = childCurriculumCourse.GradeTemplateId
                };
                _db.CurriculumCourses.Add(newParentCurriculumCourse);
                _db.SaveChanges();
            }
        }

        public List<Curriculum> GetCurriculumByAcademicLevelId(long id)
        {
            List<Curriculum> curriculums = _db.Curriculums.Where(x => x.AcademicLevelId == id)
                                                          .ToList();
            return curriculums;
        }

        public string GetCurriculumNameByIds(List<long> ids)
        {
            var curriculums = _db.Curriculums.Where(x => ids.Contains(x.Id))
                                             .Select(x => x.NameEn)
                                             .ToList();

            return String.Join(",", curriculums);
        }

        public CurriculumVersion GetVersionIdByCourseGroup(long id)
        {
            var version = _db.CourseGroups.Include(x => x.CurriculumVersion)
                                              .ThenInclude(x => x.Curriculum)
                                          .SingleOrDefault(x => x.Id == id)
                                          .CurriculumVersion;
            return version;
        }

        public List<CourseGroup> GetCourseGroupChilds(long parentId)
        {
            var courseGroups = new List<CourseGroup>();
            GetCourseGroupChildsRecursive(parentId, courseGroups);

            return courseGroups;
        }

        private void GetCourseGroupChildsRecursive(long parentId, List<CourseGroup> courseGroups)
        {
            var children = _db.CourseGroups.Where(x => x.CourseGroupId == parentId);
            if (children != null)
            {
                courseGroups.AddRange(children);
                foreach (var item in children)
                {
                    GetCourseGroupChildsRecursive(item.Id, courseGroups);
                }
            }
        }

        public List<CurriculumCourse> FindCurriculumCourses(long courseGroupId, long? curriculumVersionId)
        {
            var model = _db.CurriculumCourses.Where(x => x.CourseGroupId == courseGroupId)
                                             .ToList();
            model.Select(x =>
            {
                x.CurriculumVersionId = curriculumVersionId;
                return x;
            }).ToList();

            return model;
        }

        public CourseGroup FindCourseGroup(long? id)
        {
            var model = _db.CourseGroups.SingleOrDefault(x => x.Id == id);

            return model;
        }

        public List<CurriculumInstructor> GetCurriculumInstructors(long versionId)
        {
            var instructors = _db.CurriculumInstructor.Where(x => x.CurriculumVersionId == versionId)
                                                      .ToList();
            return instructors;
        }

        public List<CurriculumCourseGroup> GetCurriculumCourseGroups()
        {
            var groups = _db.CurriculumCourseGroups.ToList();
            return groups;
        }

        public List<CurriculumVersion> GetCurriculumVersionsByCurriculumIdAndStudentId(Guid studentId, long curriculumId)
        {
            var admissionTerm = _db.AdmissionInformations.Include(x => x.AdmissionTerm)
                                                         .SingleOrDefault(x => x.StudentId == studentId)
                                                         .AdmissionTerm;

            var curriculumVersions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                           .Include(x => x.Term)
                                                           .Include(x => x.OpenedTerm)
                                                           .Include(x => x.ClosedTerm)
                                                           .Where(x => x.CurriculumId == curriculumId
                                                                       && (x.Term.AcademicYear <= admissionTerm.AcademicYear
                                                                           && x.Term.AcademicTerm <= admissionTerm.AcademicTerm)
                                                                           && ((x.OpenedTerm == null
                                                                               || (admissionTerm.AcademicYear >= x.OpenedTerm.AcademicYear
                                                                                   && admissionTerm.AcademicTerm >= x.OpenedTerm.AcademicTerm))
                                                                               && (x.ClosedTerm == null
                                                                                   || (admissionTerm.AcademicYear <= x.ClosedTerm.AcademicYear
                                                                                   && admissionTerm.AcademicTerm <= x.ClosedTerm.AcademicTerm))))
                                                           .ToList();

            return curriculumVersions;
        }

        public List<CurriculumVersion> GetImplementedCurriculumVersionsByStudentId(Guid studentId, long facultyId = 0, long departmentId = 0)
        {
            var admissionTerm = _db.AdmissionInformations.Include(x => x.AdmissionTerm)
                                                         .SingleOrDefault(x => x.StudentId == studentId)
                                                         .AdmissionTerm;

            var curriculumVersions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                           .Include(x => x.Term)
                                                           .Include(x => x.ClosedTerm)
                                                           .Where(x => (facultyId == 0
                                                                        || x.Curriculum.FacultyId == facultyId)
                                                                       && (departmentId == 0
                                                                           || x.Curriculum.DepartmentId == departmentId)
                                                                       && (x.Term.AcademicYear <= admissionTerm.AcademicYear
                                                                               && x.Term.AcademicTerm <= admissionTerm.AcademicTerm)
                                                                        && (x.ClosedTerm == null
                                                                            || (admissionTerm.AcademicYear <= x.ClosedTerm.AcademicYear
                                                                                && admissionTerm.AcademicTerm <= x.ClosedTerm.AcademicTerm)))
                                                           .ToList();

            return curriculumVersions;
        }
        public bool IsExistCourseExclusion(CourseExclusion model)
        {
            var isExist = _db.CourseExclusions.Any(x => x.Id != model.Id
                                                        && x.CourseId == model.CourseId
                                                        && x.ExcludingCourseId == model.ExcludingCourseId
                                                        && x.CurriculumVersionId == model.CurriculumVersionId
                                                        && x.EffectivedAt.Date >= model.EffectivedAt.Date
                                                        && (x.EndedAt == null
                                                            || model.EndedAt == null
                                                            || x.EndedAt.Value.Date <= model.EndedAt.Value.Date));
            return isExist;
        }

        public bool IsExistCurriculumInformation(CurriculumInformation model)
        {
            var isExist = _db.CurriculumInformations.IgnoreQueryFilters()
                                                    .Any(x => x.Id != model.Id
                                                              && x.StudentId == model.StudentId
                                                              && x.CurriculumVersionId == model.CurriculumVersionId);
            return isExist;
        }

        public bool IsActiveCurriculumInformation(Guid studentId, long curriculumInformationId)
        {
            var isExist = _db.CurriculumInformations.Any(x => x.StudentId == studentId
                                                              && x.IsActive
                                                              && x.Id != curriculumInformationId);

            return isExist;
        }
        
        public string ExportCurriculum(long facultyId, long departmentId)
        {
            var curriculums = _db.CurriculumVersions.Where(x => x.Curriculum.FacultyId == facultyId
                                                                && x.Curriculum.DepartmentId == departmentId)
                                                    .ToList();
            foreach (var curriculum in curriculums)
            {
                curriculum.CourseGroups = new List<CourseGroup>();
                curriculum.CourseGroups = GetCourseGroups(curriculum.Id);
            }
            var json = JsonConvert.SerializeObject(curriculums, new JsonSerializerSettings { ContractResolver = new IgnoreJsonAttributesResolver(), ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return json;
        }
        public CurriculumInformationViewModel GetMinorAndConcentration(Guid studentId, string language)
        {
            var data = _db.CurriculumInformations.Include(x => x.CurriculumVersion)
                                                 .Include(x => x.SpecializationGroupInformations)
                                                     .ThenInclude(x => x.SpecializationGroup)
                                                 .Where(x => x.StudentId == studentId)
                                                 .FirstOrDefault();

            string minor = "", concentration = "";
            var model = new CurriculumInformationViewModel();

            if (data == null)
                return model;

            var minorList = new List<string>();
            var concentrationList = new List<string>();
            foreach (var item in data.SpecializationGroupInformations)
            {
                if (language == "en")
                {
                    model.CurriculumVersion = data.CurriculumVersion.NameEn;
                    if (item.SpecializationGroup.Type == SpecializationGroup.TYPE_MINOR_CODE)
                    {
                        minor = item.SpecializationGroup.NameEn;
                        minorList.Add(minor);
                    }
                    if (item.SpecializationGroup.Type == SpecializationGroup.TYPE_CONCENTRATION_CODE)
                    {
                        concentration = item.SpecializationGroup.NameEn;
                        concentrationList.Add(concentration);
                    }
                }

                if (language == "th")
                {
                    model.CurriculumVersion = data.CurriculumVersion.NameTh;
                    if (item.SpecializationGroup.Type == SpecializationGroup.TYPE_MINOR_CODE)
                    {
                        minor = item.SpecializationGroup.NameTh;
                        minorList.Add(minor);
                    }
                    if (item.SpecializationGroup.Type == SpecializationGroup.TYPE_CONCENTRATION_CODE)
                    {
                        concentration = item.SpecializationGroup.NameTh;
                        concentrationList.Add(concentration);
                    }
                }
            }

            minor = $"{ string.Join(", ", minorList) }";
            concentration = $"{ string.Join(", ", concentrationList) }";

            model.Minor = minor;
            model.Concentration = concentration;

            return model;
        }

        public List<CourseGroupViewModel> GetCourseGroupWithRegistrationCourses(Guid studentId, long versionId, out int totalCourseGroup)
        {
            // GET REGISTRATION COURSES
            var regisCourses =  _db.RegistrationCourses.Include(x => x.Course)
                                                       .Include(x => x.Term)
                                                       .Include(x => x.Grade)
                                                       .Where(x => x.StudentId == studentId 
                                                                   && (x.Status != "d" || x.Grade.Name.ToUpper() == "W"))
                                                       .OrderByDescending(x => x.Term.AcademicYear)
                                                            .ThenByDescending(x => x.Term.AcademicTerm)
                                                       .AsNoTracking()
                                                       .ToList();

            // GET CURRICULUM COURSE GROUPS
            int sequence = 2;
            var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null
                                                      && x.SpecializationGroup.Type != SpecializationGroup.TYPE_MINOR_CODE
                                                      && !x.IsAutoAssignGraduationCourse)
                                          .OrderBy(x => x.Sequence)
                                          .Select(x => new CourseGroupViewModel
                                                       {
                                                           CourseGroupId = x.Id,
                                                           FullPathName = x.NameEn,
                                                           NameEn = x.NameEn,
                                                           SpecializationType = x.SpecializationGroup.Type,
                                                           DescriptionEn = x.DescriptionEn,
                                                           RequiredCreditCompleted = x.Credit
                                                       })
                                          .AsNoTracking()
                                          .ToList();

            // GET COURSE EQUIVALENCE
            var dependencyIds = _db.CurriculumDependencies.Where(x => x.DependencyType == "Equivalence" 
                                                                      && x.CurriculumVersionId == versionId)
                                                          .AsNoTracking()
                                                          .Select(x => x.DependencyId);

            var courseEquivalences = _db.CourseEquivalents.Include(x => x.EquilaventCourse)
                                                          .Where(x => dependencyIds.Contains(x.Id))
                                                          .AsNoTracking()
                                                          .ToList();
            

            // GET BLACKLIST COURSE
            var blacklistCourseIds = _db.CurriculumBlacklistCourses.Where(x => x.CurriculumVersionId == versionId)
                                                                   .AsNoTracking()
                                                                   .Select(x => x.CourseId)
                                                                   .ToList();
                           
            
            // EXCLUDED STAR AND BLACKLIST COURSES
            var selectedRegisCourses = regisCourses.Where(x => !x.IsStarCourse
                                                               && !blacklistCourseIds.Contains(x.CourseId))
                                                   .ToList();
            List<CourseGroupCourseViewModel> curriculumCourses = new List<CourseGroupCourseViewModel>();
            foreach (var parent in parents)
            {
                parent.Sequence = sequence++;
                curriculumCourses.AddRange(RecursiveCourseGroupWithRegistrationCourses(parent
                                                                                       , studentId
                                                                                       , curriculumCourses
                                                                                       , courseEquivalences
                                                                                       , ref selectedRegisCourses
                                                                                       , ref sequence));
            }
            totalCourseGroup = sequence;

            // GET MINOR COURSE GROUPS
            var minorIds = _db.SpecializationGroupInformations.AsNoTracking()
                                                              .Where(x => x.CurriculumInformation.StudentId == studentId
                                                                          && x.SpecializationGroup.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                              .Select(x => x.SpecializationGroupId)
                                                              .ToList();

            var minorCourseGroups = _db.CourseGroups.Where(x => x.SpecializationGroupId.HasValue
                                                                && minorIds.Contains(x.SpecializationGroupId.Value)
                                                                && (x.CourseGroupId == null || x.CourseGroupId == 0))
                                         .Select(x => new CourseGroupViewModel
                                                      {
                                                          CourseGroupId = x.Id,
                                                          FullPathName = x.NameEn,
                                                          NameEn = x.NameEn,
                                                          SpecializationName = x.SpecializationGroup.NameEn,
                                                          SpecializationType = x.SpecializationGroup.Type,
                                                          DescriptionEn = x.DescriptionEn,
                                                          RequiredCreditCompleted = x.Credit,
                                                          IsMinor = true
                                                      })
                                         .AsNoTracking()
                                         .ToList();

            foreach (var minorCourseGroup in minorCourseGroups)
            {
                minorCourseGroup.Sequence = sequence++;
                curriculumCourses.AddRange(RecursiveCourseGroupWithRegistrationCourses(minorCourseGroup
                                                                                       , studentId
                                                                                       , curriculumCourses
                                                                                       , courseEquivalences
                                                                                       , ref selectedRegisCourses
                                                                                       , ref sequence));
            }
            parents.AddRange(minorCourseGroups);
            totalCourseGroup = sequence;

            // OTHER COURSE GROUPS - FREE ELECTIVE
            var freeElectiveGroups = _db.CourseGroups.AsNoTracking()
                                                     .Where(x => x.CurriculumVersionId == versionId
                                                                 && x.IsAutoAssignGraduationCourse)
                                                     .Select(x => new CourseGroupViewModel
                                                                  {
                                                                      CourseGroupId = x.Id,
                                                                      NameEn = x.NameEn,
                                                                      DescriptionEn = string.Empty,
                                                                      RequiredCreditCompleted = x.Credit
                                                                  })
                                                     .ToList();
            if (freeElectiveGroups != null && freeElectiveGroups.Any())
            {    
                foreach (var freeElectiveGroup in freeElectiveGroups)
                {
                    freeElectiveGroup.Sequence = sequence++;
                    freeElectiveGroup.Courses = new List<CourseGroupCourseViewModel>();
                    var regisFreeElecCourses = selectedRegisCourses.Where(x => !x.IsAssignToCourseGroup).ToList();
                    var distinctFreeElecCourseIds = regisFreeElecCourses.Select(x => x.CourseId).Distinct();
                    var freeElecCourses = _db.Courses.Where(x => distinctFreeElecCourseIds.Contains(x.Id))
                                                     .AsNoTracking()
                                                     .ToList();
                    
                    var totalCredit = 0;
                    foreach (var freeElecCourse in freeElecCourses)
                    {
                        //if (freeElectiveGroup.RequiredCreditCompleted > totalCredit)
                        //{
                        var courseGroupCourse =  new CourseGroupCourseViewModel()
                                                {
                                                    CourseId = freeElecCourse.Id,
                                                    CourseCode = freeElecCourse.Code,
                                                    CourseNameEn = freeElecCourse.NameEn,
                                                    Credit = freeElecCourse.Credit,
                                                    CreditText = freeElecCourse.CreditText,
                                                };

                        courseGroupCourse.Grades = new List<CourseGroupCourseGradeViewModel>();
                        var selectedRegisFreeElecCourses = regisFreeElecCourses.Where(x => x.CourseId == freeElecCourse.Id);
                        foreach (var selectedRegisFreeElecCourse in selectedRegisFreeElecCourses)
                        {
                            selectedRegisFreeElecCourse.IsAssignToCourseGroup = true;
                            courseGroupCourse.Grades.Add(new CourseGroupCourseGradeViewModel()
                                                        {
                                                                TermText = selectedRegisFreeElecCourse?.Term.TermPeriodText,
                                                                IsTransferCourse = selectedRegisFreeElecCourse?.IsTransferCourse ?? false,
                                                                IsStarCourse = selectedRegisFreeElecCourse?.IsStarCourse ?? false,
                                                                RegisteredGradeName = selectedRegisFreeElecCourse?.GradeName,
                                                                Credit = selectedRegisFreeElecCourse.Course.Credit,
                                                                IsGradePublished = (selectedRegisFreeElecCourse.GradeId != null && selectedRegisFreeElecCourse.GradeName != "x")
                                                        });
                        }
                            
                        freeElectiveGroup.Courses.Add(courseGroupCourse);
                        totalCredit += freeElecCourse.Credit;
                        //}
                        //else 
                        //{
                        //    break;
                        //}
                    }

                    freeElectiveGroup.TotalCreditsCompleted = freeElectiveGroup.Courses.Sum(x => x.Credit);
                    parents.Add(freeElectiveGroup);
                    totalCourseGroup++;
                }
            }

            // OTHER COURSE GROUPS - STAR 
            var starGroup = new CourseGroupViewModel()
                            {
                                Sequence = sequence,
                                CourseGroupId = -2,
                                NameEn = "STAR",
                                DescriptionEn = string.Empty,
                                RequiredCreditCompleted = 0
                            };
            
            sequence++;
            starGroup.Courses = new List<CourseGroupCourseViewModel>();
            var regisStarCourses = regisCourses.Where(x => x.IsStarCourse).ToList();
            var distinctStarCourseIds = regisStarCourses.Select(x => x.CourseId).Distinct();
            var starCourses = _db.Courses.Where(x => distinctStarCourseIds.Contains(x.Id))
                                         .AsNoTracking()
                                         .ToList();
            foreach (var starCourse in starCourses)
            {
                var courseGroupCourse =  new CourseGroupCourseViewModel()
                                         {
                                            CourseId = starCourse.Id,
                                            CourseCode = starCourse.Code,
                                            CourseNameEn = starCourse.NameEn,
                                            Credit = starCourse.Credit,
                                            CreditText = starCourse.CreditText,
                                         };

                courseGroupCourse.Grades = new List<CourseGroupCourseGradeViewModel>();
                var selectedRegisStarCourses = regisStarCourses.Where(x => x.CourseId == starCourse.Id);
                foreach (var selectedRegisStarCourse in selectedRegisStarCourses)
                {
                     courseGroupCourse.Grades.Add(new CourseGroupCourseGradeViewModel()
                                                  {
                                                        TermText = selectedRegisStarCourse?.Term.TermPeriodText,
                                                        IsTransferCourse = selectedRegisStarCourse?.IsTransferCourse ?? false,
                                                        IsStarCourse = selectedRegisStarCourse?.IsStarCourse ?? false,
                                                        RegisteredGradeName = selectedRegisStarCourse?.GradeName,
                                                        Credit = selectedRegisStarCourse.Course.Credit,
                                                        IsGradePublished = (selectedRegisStarCourse.GradeId != null && selectedRegisStarCourse.GradeName != "x")
                                                  });
                }
                starGroup.Courses.Add(courseGroupCourse);
            }
           
            starGroup.IsCompleted = true;
            starGroup.TotalCreditsCompleted = starGroup.Courses.Sum(x => x.Credit);
            parents.Add(starGroup);
            totalCourseGroup++;

            // OTHER COURSE GROUPS - NOT COUNT 
            var notCountGroup = new CourseGroupViewModel()
                                {
                                    Sequence = sequence,
                                    CourseGroupId = -3,
                                    NameEn = "NOT COUNT",
                                    DescriptionEn = string.Empty,
                                    RequiredCreditCompleted = 0
                                };

            sequence++;
            notCountGroup.Courses = new List<CourseGroupCourseViewModel>();
            
            // OTHER COURSES
            var regisOtherCourses = selectedRegisCourses.Where(x => !x.IsAssignToCourseGroup).ToList();
            var distinctOtherCourseIds = regisOtherCourses.Select(x => x.CourseId).Distinct();
            var otherCourses = _db.Courses.Where(x => distinctOtherCourseIds.Contains(x.Id))
                                             .AsNoTracking()
                                             .ToList();
            
            foreach (var otherCourse in otherCourses)
            {
                var courseGroupCourse =  new CourseGroupCourseViewModel()
                                        {
                                            CourseId = otherCourse.Id,
                                            CourseCode = otherCourse.Code,
                                            CourseNameEn = otherCourse.NameEn,
                                            Credit = otherCourse.Credit,
                                            CreditText = otherCourse.CreditText,
                                        };

                courseGroupCourse.Grades = new List<CourseGroupCourseGradeViewModel>();
                var selectedRegisOtherCourses = regisOtherCourses.Where(x => x.CourseId == otherCourse.Id);
                foreach (var selectedRegisOtherCourse in selectedRegisOtherCourses)
                {
                    selectedRegisOtherCourse.IsAssignToCourseGroup = true;
                    courseGroupCourse.Grades.Add(new CourseGroupCourseGradeViewModel()
                                                {
                                                        TermText = selectedRegisOtherCourse?.Term.TermPeriodText,
                                                        IsTransferCourse = selectedRegisOtherCourse?.IsTransferCourse ?? false,
                                                        IsStarCourse = selectedRegisOtherCourse?.IsStarCourse ?? false,
                                                        RegisteredGradeName = selectedRegisOtherCourse?.GradeName,
                                                        Credit = selectedRegisOtherCourse.Course.Credit,
                                                        IsGradePublished = (selectedRegisOtherCourse.GradeId != null && selectedRegisOtherCourse.GradeName != "x")
                                                });
                }
                
                notCountGroup.Courses.Add(courseGroupCourse);
            }

            // BLACKLIST COURSES
            var regisNotCountCourses = regisCourses.Where(x => blacklistCourseIds.Contains(x.CourseId)).ToList();
            var distinctNotCountCourseIds = regisNotCountCourses.Select(x => x.CourseId).Distinct();
            var notCountCourses = _db.Courses.Where(x => distinctNotCountCourseIds.Contains(x.Id))
                                             .AsNoTracking()
                                             .ToList();
            foreach (var notCountCourse in notCountCourses)
            {
                var courseGroupCourse =  new CourseGroupCourseViewModel()
                                         {
                                            CourseId = notCountCourse.Id,
                                            CourseCode = notCountCourse.Code,
                                            CourseNameEn = notCountCourse.NameEn,
                                            Credit = notCountCourse.Credit,
                                            CreditText = notCountCourse.CreditText,
                                         };

                courseGroupCourse.Grades = new List<CourseGroupCourseGradeViewModel>();
                var selectedRegisNotCountCourses = regisNotCountCourses.Where(x => x.CourseId == notCountCourse.Id);
                foreach (var selectedRegisNotCountCourse in selectedRegisNotCountCourses)
                {
                     courseGroupCourse.Grades.Add(new CourseGroupCourseGradeViewModel()
                                                  {
                                                        TermText = selectedRegisNotCountCourse?.Term.TermPeriodText,
                                                        IsTransferCourse = selectedRegisNotCountCourse?.IsTransferCourse ?? false,
                                                        IsStarCourse = selectedRegisNotCountCourse?.IsStarCourse ?? false,
                                                        RegisteredGradeName = selectedRegisNotCountCourse?.GradeName,
                                                        Credit = selectedRegisNotCountCourse.Course.Credit,
                                                        IsGradePublished = selectedRegisNotCountCourse.IsGradePublished
                                                  });
                }
                notCountGroup.Courses.Add(courseGroupCourse);
            }
           
            notCountGroup.IsCompleted = true;
            notCountGroup.TotalCreditsCompleted = notCountGroup.Courses.Sum(x => x.Credit);
            parents.Add(notCountGroup);
            return parents;
        }

        public List<CourseGroupViewModel> GetCourseGroups(Guid studentId, long versionId, out int totalCourseGroup)
        {
            // GET CURRICULUM COURSE GROUPS
            int sequence = 1;
            var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null)
                                          .Select(x => new CourseGroupViewModel
                                                       {
                                                           CourseGroupId = x.Id,
                                                           FullPathName = x.NameEn,
                                                           NameEn = x.NameEn,
                                                           DescriptionEn = x.DescriptionEn,
                                                           RequiredCreditCompleted = x.Credit
                                                       })
                                          .AsNoTracking()
                                          .ToList();          
                                          
            foreach (var parent in parents)
            {
                parent.Sequence = sequence++;
                RecursiveCourseGroups(parent, studentId, ref sequence);
            }

            totalCourseGroup = sequence;
            return parents;
        }

        public List<CourseGroupViewModel> GetCurriculumCourseGroups(long versionId, Guid studentId, ref List<RegistrationCourse> regisCourses, out List<CourseGroupCourseViewModel> curriculumCourses, out int totalCourseGroup)
        {
            curriculumCourses = new List<CourseGroupCourseViewModel>();
            int sequence = 1;
            var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null)
                                          .Select(x => new CourseGroupViewModel
                                                       {
                                                           CourseGroupId = x.Id,
                                                           FullPathName = x.NameEn,
                                                           NameEn = x.NameEn,
                                                           DescriptionEn = x.DescriptionEn,
                                                           RequiredCreditCompleted = x.Credit
                                                       })
                                          .ToList();

            // GET COURSE EQUIVALENCE
            var dependencyIds = _db.CurriculumDependencies.Where(x => x.DependencyType == "Equivalence" 
                                                                      && x.CurriculumVersionId == versionId)
                                                          .AsNoTracking()
                                                          .Select(x => x.DependencyId);

            var courseEquivalences = _db.CourseEquivalents.Where(x => dependencyIds.Contains(x.Id))
                                                          .AsNoTracking()
                                                          .ToList();

            foreach (var parent in parents)
            {
                parent.Sequence = sequence++;
                curriculumCourses.AddRange(RecursiveCourseGroupWithRegistrationCourses(parent, studentId, curriculumCourses, courseEquivalences, ref regisCourses, ref sequence));
            }
            
            totalCourseGroup = sequence;
            return parents;
        }

        private void RecursiveCourseGroups(CourseGroupViewModel group, Guid studentId, ref int sequence)
        {
            group.Courses = new List<CourseGroupCourseViewModel>();
            var childCourseGroups = _db.CourseGroups.Where(x => x.CourseGroupId == group.CourseGroupId)
                                                    .Select(x => new CourseGroupViewModel
                                                                 {
                                                                    CourseGroupId = x.Id,
                                                                    FullPathName = x.NameEn,
                                                                    NameEn = x.NameEn,
                                                                    DescriptionEn = x.DescriptionEn,
                                                                    RequiredCreditCompleted = x.Credit,
                                                                 })
                                                    .AsNoTracking()
                                                    .ToList();
            if (childCourseGroups.Any())
            {
                group.Children = childCourseGroups;
    
                foreach (var child in group.Children)
                {
                    child.FullPathName = group.FullPathName + " -> " + child.NameEn;
                    child.Sequence = sequence++;
                    RecursiveCourseGroups(child, studentId, ref sequence);
                    group.TotalCreditsCompleted += child.TotalCreditsCompleted;
                }
            }

            var courses = _db.CurriculumCourses.Include(x => x.Course)
                                                .Include(x => x.Grade)
                                                .Where(x => x.CourseGroupId == group.CourseGroupId)
                                                .OrderBy(x => x.Course.Code)
                                                .Select(x => new CourseGroupCourseViewModel
                                                            {
                                                                CourseId = x.CourseId,
                                                                CourseCode = x.Course.Code,
                                                                CourseNameEn = x.Course.NameEn,
                                                                Credit = x.Course.Credit,
                                                                CreditText = x.Course.CreditText,
                                                                RequiredGradeWeight = x.Grade.Weight,
                                                                RequiredGradeName = x.Grade.Name
                                                            })
                                                .AsNoTracking()
                                                .ToList();

            group.Courses.AddRange(courses);
        }

        private List<CourseGroupCourseViewModel> RecursiveCourseGroupWithRegistrationCourses(CourseGroupViewModel group, Guid studentId, List<CourseGroupCourseViewModel> curriculumCourses, List<CourseEquivalent> courseEquivalents, ref List<RegistrationCourse> regisCourses, ref int sequence)
        {
            curriculumCourses = new List<CourseGroupCourseViewModel>();
            group.Courses = new List<CourseGroupCourseViewModel>();
            var childCourseGroups = _db.CourseGroups.Where(x => x.CourseGroupId == group.CourseGroupId)
                                                    .OrderBy(x => x.Sequence)
                                                    .Select(x => new CourseGroupViewModel
                                                                 {
                                                                    CourseGroupId = x.Id,
                                                                    FullPathName = x.NameEn,
                                                                    NameEn = x.NameEn,
                                                                    DescriptionEn = x.DescriptionEn,
                                                                    RequiredCreditCompleted = x.Credit
                                                                 })
                                                    .AsNoTracking()
                                                    .ToList();
            if (childCourseGroups.Any())
            {
                group.Children = childCourseGroups;
    
                foreach (var child in group.Children)
                {
                    // child.FullPathName = group.FullPathName + " -> " + child.NameEn;
                    child.FullPathName = child.NameEn;
                    child.Sequence = sequence++;
                    curriculumCourses.AddRange(RecursiveCourseGroupWithRegistrationCourses(child, studentId, curriculumCourses, courseEquivalents, ref regisCourses, ref sequence));
                    group.TotalCreditsCompleted += child.TotalCreditsCompleted;
                }
            }
            
            // GET MODIFICATION COURSES
            var modificationCourses = _db.CourseGroupModifications.Include(x => x.Course)
                                                                  .Include(x => x.RequiredGrade)
                                                                  .Where(x => x.StudentId == studentId
                                                                              && (x.CourseGroupId == group.CourseGroupId 
                                                                                  || x.MoveCourseGroupId == group.CourseGroupId))
                                                                  .AsNoTracking()
                                                                  .ToList();
           
            var movedOutCoureIds = modificationCourses.Where(x => x.CourseGroupId == group.CourseGroupId 
                                                                  && x.MoveCourseGroupId != group.CourseGroupId
                                                                  && x.MoveCourseGroupId != null)
                                                      .Select(x => x.CurriculumCourseId);

            var disabledCourseIds = modificationCourses.Where(x => x.IsDisabled)
                                                       .Select(x => x.CurriculumCourseId);

            var courses = _db.CurriculumCourses.Include(x => x.Course)
                                                .Include(x => x.Grade)
                                                .Where(x => x.CourseGroupId == group.CourseGroupId
                                                            && (!movedOutCoureIds.Contains(x.Id))
                                                            && !disabledCourseIds.Contains(x.Id))
                                                .OrderBy(x => x.Course.Code)
                                                .Select(x => new CourseGroupCourseViewModel
                                                             {
                                                                Id = x.Id,
                                                                CourseId = x.CourseId,
                                                                CourseCode = x.Course.Code,
                                                                CourseNameEn = x.Course.NameEn,
                                                                Credit = x.Course.Credit,
                                                                CreditText = x.Course.CreditText,
                                                                RequiredGradeWeight = x.Grade.Weight,
                                                                RequiredGradeName = x.Grade.Name,
                                                                IsRequired = x.IsRequired
                                                             })
                                                .AsNoTracking()
                                                .ToList();

            // GET MOVED IN COURES
            var movedInCoureIds = modificationCourses.Where(x => x.CourseGroupId != group.CourseGroupId  
                                                               && x.MoveCourseGroupId == group.CourseGroupId)
                                                   .Select(x => x.CurriculumCourseId);

            var movedInCourses = _db.CurriculumCourses.Include(x => x.Course)
                                                      .Include(x => x.Grade)
                                                      .Where(x => movedInCoureIds.Contains(x.Id))
                                                      .OrderBy(x => x.Course.Code)
                                                      .Select(x => new CourseGroupCourseViewModel
                                                                   {
                                                                        Id = x.Id,
                                                                        CourseId = x.CourseId,
                                                                        CourseCode = x.Course.Code,
                                                                        CourseNameEn = x.Course.NameEn,
                                                                        Credit = x.Course.Credit,
                                                                        CreditText = x.Course.CreditText,
                                                                        RequiredGradeWeight = x.Grade.Weight,
                                                                        RequiredGradeName = x.Grade.Name,
                                                                        IsMoved = true,
                                                                        IsRequired = x.IsRequired
                                                                   })
                                                      .AsNoTracking()
                                                      .ToList();
            courses.AddRange(movedInCourses);

            // NEW COURSES FROM USER
            var addCoures = modificationCourses.Where(x => x.IsAddManually)
                                               .OrderBy(x => x.Course.Code)
                                               .Select(x => new CourseGroupCourseViewModel
                                                            {
                                                                Id = x.Id,
                                                                CourseId = x.CourseId,
                                                                CourseCode = x.Course.Code,
                                                                CourseNameEn = x.Course.NameEn,
                                                                Credit = x.Course.Credit,
                                                                CreditText = x.Course.CreditText,
                                                                RequiredGradeWeight = x.RequiredGrade?.Weight,
                                                                RequiredGradeName = x.RequiredGrade?.Name,
                                                                Remark = x.Remark
                                                            })
                                               .ToList();
            courses.AddRange(addCoures);

            int totalCreditCompleted = 0;

            var coursePredictions = _db.CoursePredictions.Include(x => x.GraduatingRequest)
                                                         .Where(x => x.GraduatingRequest.StudentId == studentId)
                                                         .AsNoTracking()
                                                         .Select(x => x.CourseId)
                                                         .ToList();
            foreach (var course in courses)
            {
                course.Grades = new List<CourseGroupCourseGradeViewModel>();
                course.Remark = modificationCourses.Where(x => x.CourseId == course.CourseId)
                                                   .FirstOrDefault()?.Remark ?? "";
                // COURSE EXIST
                if (regisCourses.Any(x => x.CourseId == course.CourseId))
                {
                    var selectedCourses = regisCourses.Where(x => x.CourseId == course.CourseId).ToList();
                    foreach (var selectedCourse in selectedCourses)
                    {
                        selectedCourse.IsAssignToCourseGroup = true;
                        course.Grades.Add(new CourseGroupCourseGradeViewModel
                                                {
                                                    TermText = selectedCourse?.Term.TermPeriodText,
                                                    IsTransferCourse = selectedCourse?.IsTransferCourse ?? false,
                                                    IsStarCourse = selectedCourse?.IsStarCourse ?? false,
                                                    RegisteredGradeWeight = selectedCourse.Grade?.Weight,
                                                    RegisteredGradeName = selectedCourse?.GradeName,
                                                    Credit = selectedCourse.Course.Credit,
                                                    IsGradePublished = (selectedCourse.GradeId != null && selectedCourse.GradeName != "x"),
                                                    IsPassed = selectedCourse.Grade?.Weight != null && (selectedCourse.Grade?.Weight ?? 0) >= (course.RequiredGradeWeight ?? 0)
                                                });
                                        
                    }
                    if (course.Grades.Any( x => x.IsPassed))
                    {
                        totalCreditCompleted += course.Credit;  
                    }
                }
                else if (coursePredictions.Any(x => x == course.CourseId))
                {
                    course.IsPlanned = true;
                }
                else // TODO : COURSE EQUIVALENCE
                {
                    if (courseEquivalents.Any(x => x.CourseId == course.CourseId 
                                                   || x.EquilaventCourseId == course.CourseId))
                    {
                        var selectCourseEquivalents = courseEquivalents.Where(x => x.CourseId == course.CourseId
                                                                                   || x.EquilaventCourseId == course.CourseId)
                                                                       .ToList();
                        if (selectCourseEquivalents != null && selectCourseEquivalents.Any())
                        {
                            var compareCourseIds = selectCourseEquivalents.Select(x => x.CourseId).ToList();
                            compareCourseIds.AddRange(selectCourseEquivalents.Select(x => x.EquilaventCourseId));
                            compareCourseIds.RemoveAll(x => x == course.CourseId);

                            if (regisCourses.Any(x => compareCourseIds.Contains(x.CourseId)))
                            {
                                var selectedCourses = regisCourses.Where(x => compareCourseIds.Contains(x.CourseId)).ToList();
                                foreach (var selectedCourse in selectedCourses)
                                {
                                    selectedCourse.IsAssignToCourseGroup = true;
                                    course.Grades.Add(new CourseGroupCourseGradeViewModel
                                                      {
                                                        TermText = selectedCourse?.Term.TermPeriodText,
                                                        IsTransferCourse = selectedCourse?.IsTransferCourse ?? false,
                                                        IsStarCourse = selectedCourse?.IsStarCourse ?? false,
                                                        RegisteredGradeWeight = selectedCourse.Grade?.Weight,
                                                        RegisteredGradeName = selectedCourse?.GradeName,
                                                        Credit = selectedCourse.Course.Credit,
                                                        IsGradePublished = (selectedCourse.GradeId != null && selectedCourse.GradeName != "x"),
                                                        IsPassed = selectedCourse.Grade?.Weight != null && (selectedCourse.Grade?.Weight ?? 0) >= (course.RequiredGradeWeight ?? 0)
                                                      });            
                                }
                                course.IsCourseEquivalent = true;
                                var courseEquivalenceNames =  selectedCourses.Select(x => x.Course.NameEnAndCredit).ToList();
                                course.CourseEquivalentName = courseEquivalenceNames.Aggregate("", (current, next) => current + ", " + next);
                            }
                            // var  = regisCourses.Any(x => x.CourseId == selectCourseEquivalents.Select(x))
                            // foreach (var selectCourseEquivalent in selectCourseEquivalents)
                            // {
                            //     course.CourseEquivalentName = selectCourseEquivalent?.EquilaventCourse?.NameEn;
                            // }
                        }
                    }
                }
            }
            
            curriculumCourses.AddRange(courses);
            group.Courses.AddRange(courses);
            group.TotalCreditsCompleted += totalCreditCompleted;
            group.IsCompleted = group.TotalCreditsCompleted >= group.RequiredCreditCompleted;
            group.IsNone = group.Courses.Count == 0 && group.RequiredCreditCompleted == 0;

            return curriculumCourses;
        }

        public List<CourseGroupViewModel> GetCourseGroupModifications(long versionId, Guid studentId, long graduatingRequestId, out int totalCourseGroup)
        {
            // GET CURRICULUM COURSE GROUPS
            int sequence = 1;
            var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                      && x.CourseGroupId == null
                                                      && x.SpecializationGroup.Type != SpecializationGroup.TYPE_MINOR_CODE)
                                          .Select(x => new CourseGroupViewModel
                                                       {
                                                           CourseGroupId = x.Id,
                                                           FullPathName = x.NameEn,
                                                           NameEn = x.NameEn,
                                                           SpecializationType = x.SpecializationGroup.Type,
                                                           DescriptionEn = x.DescriptionEn,
                                                           RequiredCreditCompleted = x.Credit,
                                                           CurriculumVersionId = versionId,
                                                           GraduatingRequestId = graduatingRequestId
                                                       })
                                          .AsNoTracking()
                                          .ToList();

            foreach (var parent in parents)
            {
                parent.Sequence = sequence++;
                RecursiveGraduationCourseGroups(parent, studentId, graduatingRequestId, ref sequence);
            }

            // GET MINOR COURSE GROUPS
            var minorIds = _db.SpecializationGroupInformations.AsNoTracking()
                                                              .Where(x => x.CurriculumInformation.StudentId == studentId
                                                                          && x.SpecializationGroup.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                              .Select(x => x.SpecializationGroupId)
                                                              .ToList();
            
            var minorCourseGroups = _db.CourseGroups.Where(x => x.SpecializationGroupId.HasValue
                                                                && minorIds.Contains(x.SpecializationGroupId.Value))
                                                    .Select(x => new CourseGroupViewModel
                                                                {
                                                                    CourseGroupId = x.Id,
                                                                    FullPathName = x.NameEn,
                                                                    NameEn = x.NameEn,
                                                                    SpecializationType = x.SpecializationGroup.Type,
                                                                    DescriptionEn = x.DescriptionEn,
                                                                    RequiredCreditCompleted = x.Credit,
                                                                    CurriculumVersionId = versionId,
                                                                    GraduatingRequestId = graduatingRequestId
                                                                })
                                                    .AsNoTracking()
                                                    .ToList();

            foreach (var minorCourseGroup in minorCourseGroups)
            {
                minorCourseGroup.Sequence = sequence++;
                RecursiveGraduationCourseGroups(minorCourseGroup, studentId, graduatingRequestId, ref sequence);
            }
            
            parents.AddRange(minorCourseGroups);
            totalCourseGroup = sequence;
            return parents;
        }

        private void RecursiveGraduationCourseGroups(CourseGroupViewModel group, Guid studentId, long graduatingRequestId, ref int sequence)
        {
            group.Courses = new List<CourseGroupCourseViewModel>();
            var childCourseGroups = _db.CourseGroups.Where(x => x.CourseGroupId == group.CourseGroupId)
                                                    .Select(x => new CourseGroupViewModel
                                                                 {
                                                                    CourseGroupId = x.Id,
                                                                    FullPathName = x.NameEn,
                                                                    NameEn = x.NameEn,
                                                                    DescriptionEn = x.DescriptionEn,
                                                                    RequiredCreditCompleted = x.Credit,
                                                                    GraduatingRequestId = graduatingRequestId
                                                                 })
                                                    .AsNoTracking()
                                                    .ToList();

            if (childCourseGroups.Any())
            {
                group.Children = childCourseGroups;
    
                foreach (var child in group.Children)
                {
                    child.FullPathName = group.FullPathName + " -> " + child.NameEn;
                    child.Sequence = sequence++;
                    child.CurriculumVersionId = group.CurriculumVersionId;
                    child.GraduatingRequestId = group.GraduatingRequestId;
                    RecursiveGraduationCourseGroups(child, studentId, graduatingRequestId, ref sequence);
                    group.TotalCreditsCompleted += child.TotalCreditsCompleted;
                }
            }

            var courses = _db.CurriculumCourses.Include(x => x.Course)
                                                .Include(x => x.Grade)
                                                .Where(x => x.CourseGroupId == group.CourseGroupId)
                                                .OrderBy(x => x.Course.Code)
                                                .Select(x => new CourseGroupCourseViewModel
                                                             {
                                                                CourseId = x.CourseId,
                                                                CourseCode = x.Course.Code,
                                                                CourseNameEn = x.Course.NameEn,
                                                                Credit = x.Course.Credit,
                                                                CreditText = x.Course.CreditText,
                                                                RequiredGradeWeight = x.Grade.Weight,
                                                                RequiredGradeName = x.Grade.Name
                                                             })
                                                .AsNoTracking()
                                                .ToList();
            
            var courseIds = courses.Select(x => x.CourseId).ToList();
            var courseEquivalences = GetCurriculumCourseEquivalents(group.CurriculumVersionId);
            var courseGroupModifications = _db.CourseGroupModifications.Include(x => x.Course)
                                                                       .Include(x => x.CourseGroup)
                                                                       .Where(x => x.StudentId == studentId
                                                                                   && x.CourseGroupId == group.CourseGroupId)
                                                                       .AsNoTracking()
                                                                       .Select(x => new CourseGroupCourseViewModel
                                                                                    {
                                                                                        CourseId = x.CourseId,
                                                                                        CourseCode = x.Course.Code,
                                                                                        CourseNameEn = x.Course.NameEn,
                                                                                        Credit = x.Course.Credit,
                                                                                        CreditText = x.Course.CreditText,
                                                                                        MoveCourseGroupId = x.MoveCourseGroupId ?? 0,
                                                                                        MoveCourseGroup = x.MoveCourseGroup.NameEn,
                                                                                        IsAddManually = x.IsAddManually,
                                                                                        RequiredGradeWeight = x.RequiredGrade.Weight,
                                                                                        RequiredGradeName = x.RequiredGrade.Name,
                                                                                        CourseModificationId = x.Id,
                                                                                        IsDisabled = x.IsDisabled,
                                                                                        Remark = x.Remark
                                                                                    })
                                                                       .ToList();

            foreach (var courseGroupModification in courseGroupModifications)
            {
                var course = courses.FirstOrDefault(x => x.CourseId == courseGroupModification.CourseId);
                if (course != null)
                {
                    course.CourseModificationId = courseGroupModification.CourseModificationId;
                    course.IsAddManually = courseGroupModification.IsAddManually;
                    course.MoveCourseGroupId = courseGroupModification.MoveCourseGroupId;
                    course.MoveCourseGroup = courseGroupModification.MoveCourseGroup;
                    course.IsDisabled = courseGroupModification.IsDisabled;
                    course.Remark = courseGroupModification.Remark;
                }
            }

            foreach (var course in courses)
            {
                var courseEquivalents = courseEquivalences.Where(x => x.CourseId == course.CourseId)
                                                          .ToList();
                course.CourseEquivalentName = courseEquivalents.Aggregate("", (current, next) => current + ", " + next);
            }

            courses.AddRange(courseGroupModifications.Where(x => !courseIds.Contains(x.CourseId)).ToList());

            // var groupingCourseIds = courseGroupingDetails.Select(x => x.CourseId).ToList();
            // var moveCourseGroupIds = courseGroupingDetails.Select(x => x.MovedCourseGroupId).ToList();
            // var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.Id == group.CourseGroupId);
            // var curriculumCourseInGroup = _db.CurriculumCourses.Include(x => x.Grade)
            //                                                    .Where(x => x.CourseGroupId == group.CourseGroupId)
            //                                                    .ToList();

            // var curriculumCourseInGroupIds = curriculumCourseInGroup.Select(x => x.CourseId);
            // var courses = _db.CourseGroupingDetails.Include(x => x.Course)
            //                                        .Where(x => x.CourseGroupId == group.CourseGroupId
            //                                                    && groupingCourseIds.Contains(x.CourseId))
            //                                        .OrderBy(x => x.Course.Code)
            //                                        .Select(x => new CourseGroupCourseViewModel
            //                                                     {
            //                                                         CourseId = x.CourseId,
            //                                                         CourseCode = x.Course.Code,
            //                                                         CourseNameEn = x.Course.NameEn,
            //                                                         Credit = x.Course.Credit,
            //                                                         CreditText = x.Course.CreditText
            //                                                     })
            //                                        .ToList();

            // var moveCourseGroups = _db.CourseGroups.Where(x => moveCourseGroupIds.Contains(x.Id)).ToList();
            // foreach (var course in courses)
            // {
            //     if (!curriculumCourseInGroupIds.Contains(course.CourseId))
            //     {
            //         course.IsCustomAdd = true;
            //     }
            //     else
            //     {
            //         course.RequiredGradeWeight = curriculumCourseInGroup.SingleOrDefault(x => x.CourseId == course.CourseId)?.Grade?.Weight;
            //         course.RequiredGradeName = curriculumCourseInGroup.SingleOrDefault(x => x.CourseId == course.CourseId)?.Grade?.Name;
            //     }

            //     course.Grades = new List<CourseGroupCourseGradeViewModel>();
            //     if (moveCourseGroups.Any())
            //     {
            //         foreach (var groupingDetail in courseGroupingDetails)
            //         {
            //             if (groupingDetail.MovedCourseGroupId != null
            //                 && groupingDetail.CourseId == course.CourseId)
            //             {
            //                 var moveCourseGroupName = moveCourseGroups.SingleOrDefault(x => x.Id == groupingDetail.MovedCourseGroupId).NameEn;
            //                 course.MoveCourseGroup = moveCourseGroupName;
            //             }
            //         }
            //     }
            // }
            
            group.Courses.AddRange(courses);
        }

        private void RecursiveCourseGroupRegistrationLogs(CourseGroupViewModel group, Guid studentId, List<CourseGroupingDetail> logDetails)
        {
            var children = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                           .Where(x => x.CourseGroupId == group.CourseGroupId)
                                           .Select(x => new CourseGroupViewModel
                                                        {
                                                            CourseGroupId = x.Id,
                                                            RequiredCreditCompleted = x.Credit,
                                                            NameEn = x.NameEn,
                                                            DescriptionEn = x.DescriptionEn
                                                        })
                                           .ToList();
            if (children.Any())
            {
                group.Children = children;
                foreach (var child in children)
                {
                    RecursiveCourseGroupRegistrationLogs(child, studentId, logDetails);
                }
            }
            else 
            {
                var courses = _db.CurriculumCourses.Include(x => x.Course)
                                                   .Include(x => x.Grade)
                                                   .Where(x => x.CourseGroupId == group.CourseGroupId)
                                                   .OrderBy(x => x.Course.Code)
                                                   .Select(x => new 
                                                                {
                                                                    x.CourseId,
                                                                    CourseCode = x.Course.Code,
                                                                    CourseNameEn = x.Course.NameEn,
                                                                    x.Course.Credit,
                                                                    x.Course.CreditText,
                                                                    RequiredGradeWeight = x.Grade.Weight,
                                                                    RequiredGradeName = x.Grade.Name,
                                                                })
                                                   .ToList();

                group.Courses = new List<CourseGroupCourseViewModel>();
                foreach (var course in courses)
                {
                    var courseViewModel = new CourseGroupCourseViewModel()
                                          {
                                                CourseId = course.CourseId,
                                                CourseCode = course.CourseCode,
                                                CourseNameEn = course.CourseNameEn,
                                                Credit = course.Credit,
                                                CreditText = course.CreditText,
                                                RequiredGradeWeight = course.RequiredGradeWeight,
                                                RequiredGradeName = course.RequiredGradeName,
                                          };

                    courseViewModel.Grades = new List<CourseGroupCourseGradeViewModel>();
                    if (logDetails.Any(x => x.CourseId == course.CourseId))
                    {
                        if (logDetails.Any(x => x.CourseId == course.CourseId
                                                && x.CourseGroupId == group.CourseGroupId))
                        {
                            courseViewModel.Grades.AddRange(logDetails.Where(x => x.CourseId == course.CourseId
                                                                                  && x.CourseGroupId == group.CourseGroupId)
                                                                      .Select(x => 
                                                                                   {
                                                                                       var weight = x?.RegistrationCourse?.Grade?.Weight;
                                                                                       return new CourseGroupCourseGradeViewModel
                                                                                              {
                                                                                                  TermText = x?.RegistrationCourse?.Term?.TermText,
                                                                                                  IsTransferCourse = x?.RegistrationCourse?.IsTransferCourse ?? false,
                                                                                                  IsStarCourse = x?.RegistrationCourse?.IsStarCourse ?? false,
                                                                                                  RegisteredGradeWeight = weight,
                                                                                                  RegisteredGradeName = x?.RegistrationCourse?.GradeName,
                                                                                                  IsPassed = weight != null && (weight ?? 0) >= (course.RequiredGradeWeight ?? 0)
                                                                                              };
                                                                                   }));
                            group.Courses.Add(courseViewModel);
                        }
                    }
                    else
                    {
                        group.Courses.Add(courseViewModel);
                    }
                }

                if (logDetails.Any(x => x.CourseGroupId == group.CourseGroupId
                                        && !courses.Any(y => y.CourseId == x.CourseId)))
                {
                    var newCourses = logDetails.Where(x => x.CourseGroupId == group.CourseGroupId
                                                          && !courses.Any(y => y.CourseId == x.CourseId))
                                               .OrderBy(x => x.Course.Code)
                                               .ToList();
                    foreach (var item in newCourses.GroupBy(x => x.CourseId))
                    {
                        var course = item.FirstOrDefault().Course;
                        var courseViewModel = new CourseGroupCourseViewModel()
                                              {
                                                  CourseId = item.Key,
                                                  CourseCode = course.Code,
                                                  CourseNameEn = course.NameEn,
                                                  Credit = course.Credit,
                                                  CreditText = course.CreditText,
                                                  Grades = new List<CourseGroupCourseGradeViewModel>()
                                              };
                        var registrations = item.OrderBy(x => x.RegistrationCourse.Term.TermText)
                                                .ToList();
                        foreach (var grade in registrations)
                        {
                            courseViewModel.Grades.Add(new CourseGroupCourseGradeViewModel
                                                       {
                                                           TermText = grade?.RegistrationCourse?.Term?.TermText,
                                                           IsTransferCourse = grade?.RegistrationCourse?.IsTransferCourse ?? false,
                                                           IsStarCourse = grade?.RegistrationCourse?.IsStarCourse ?? false,
                                                           RegisteredGradeWeight = grade?.RegistrationCourse?.Grade?.Weight,
                                                           RegisteredGradeName = grade?.RegistrationCourse?.GradeName,
                                                           IsPassed = true
                                                       });   
                        }

                        group.Courses.Add(courseViewModel);
                    }
                }
            }
        }

        public CourseGroupViewModel GetOtherCourseGroupRegistrations(Guid studentId, List<CourseGroupCourseViewModel> curriculumCourses)
        {
            CourseGroupViewModel otherGroup = new CourseGroupViewModel
                                              {
                                                  NameEn = "List of Other Course(s)",
                                                  Courses = new List<CourseGroupCourseViewModel>()
                                              };
                                              
            otherGroup.Courses = _db.RegistrationCourses.Include(x => x.Term)
                                                        .Include(x => x.Grade)
                                                        .Include(x => x.Course)
                                                        .Where(x => x.StudentId == studentId
                                                                    && x.Status != "d"
                                                                    && !curriculumCourses.Any(y => y.CourseId == x.CourseId))
                                                        .OrderBy(x => x.Course.Code)
                                                        .Select(x => new CourseGroupCourseViewModel
                                                                     {
                                                                         TermText = x.Term.TermText,
                                                                         CourseId = x.CourseId,
                                                                         CourseCode = x.Course.Code,
                                                                         CourseNameEn = x.Course.NameEn,
                                                                         IsTransferCourse = x.IsTransferCourse,
                                                                         IsStarCourse = x.IsStarCourse,
                                                                         Credit = x.Course.Credit,
                                                                         CreditText = x.Course.CreditText,
                                                                         RegisteredGradeWeight = x.Grade.Weight,
                                                                         RegisteredGradeName = x.GradeName,
                                                                     })
                                                        .ToList();
                                                        
            otherGroup.Courses = otherGroup.Courses.GroupBy(x => x.CourseId)
                                                   .Select(x => x.OrderByDescending(y => y.RegisteredGradeWeight)
                                                                 .FirstOrDefault())
                                                   .ToList();

            return otherGroup;                          
        }

        public List<CourseGroupViewModel> GetCourseSpecializationGroupRegistrations(Guid studentId, string specializationGroupType, long? specializationGroupId)
        {
            var curriculumCourses = new List<CourseGroupCourseViewModel>();
            int sequence = 1;
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.CurriculumVersionId == null
                                                      && x.CourseGroupId == null
                                                      && x.SpecializationGroupId != null
                                                      && x.SpecializationGroup.Type == specializationGroupType
                                                      && ((specializationGroupId ?? 0) == 0 
                                                          || x.SpecializationGroupId == specializationGroupId))
                                          .Select(x => new CourseGroupViewModel
                                                       {
                                                           Sequence = sequence,
                                                           CourseGroupId = x.Id,
                                                           RequiredCreditCompleted = x.Credit,
                                                           NameEn = x.NameEn
                                                       })
                                          .ToList();
            
            sequence++;
            // foreach (var parent in parents)
            // {
            //     curriculumCourses.AddRange(RecursiveCourseGroups(parent, studentId, curriculumCourses, ref sequence));
            //     sequence++;
            // }

            return parents;
        }

        public List<CourseGroupViewModel> GetCourseGroupRegistrationLogs(long versionId, Guid studentId)
        {
            var logId = _db.CourseGroupingLogs.Include(x => x.GraduatingRequest)
                                              .FirstOrDefault(x => x.GraduatingRequest.StudentId == studentId
                                                                   && x.IsApproved)?.Id;
            if (logId > 0)
            {
                var logDetails = _db.CourseGroupingDetails.Include(x => x.Course)
                                                          .Include(x => x.RegistrationCourse)
                                                              .ThenInclude(x => x.Term)
                                                          .Include(x => x.RegistrationCourse)
                                                              .ThenInclude(x => x.Grade)
                                                          .Where(x => x.CourseGroupingLogId == logId)
                                                          .ToList();
                                                            
                var parents = _db.CourseGroups.Where(x => x.CurriculumVersionId == versionId
                                                          && x.CourseGroupId == null)
                                              .Select(x => new CourseGroupViewModel
                                                           {
                                                               CourseGroupId = x.Id,
                                                               RequiredCreditCompleted = x.Credit,
                                                               NameEn = x.NameEn,
                                                               DescriptionEn = x.DescriptionEn
                                                           })
                                              .ToList();
                                            
                foreach (var parent in parents)
                {
                    RecursiveCourseGroupRegistrationLogs(parent, studentId, logDetails);
                }

                var nonCourseGroups = logDetails.Where(x => x.CourseGroupId <= 0)
                                                .ToList();
                foreach (var courseGroup in nonCourseGroups.GroupBy(x => x.CourseGroupId))
                {
                    var group = new CourseGroupViewModel
                                {
                                    NameEn = courseGroup.FirstOrDefault(x => !string.IsNullOrEmpty(x.CourseGroupName))?.CourseGroupName,
                                    RequiredCreditCompleted = 0,
                                    Courses = new List<CourseGroupCourseViewModel>()
                                };

                    var courses = courseGroup.OrderBy(x => x.Course.Code)
                                             .GroupBy(x => x.CourseId);
                    foreach (var course in courses)
                    {
                        var courseViewModel = new CourseGroupCourseViewModel()
                                              {
                                                      CourseId = course.Key,
                                                      CourseCode = course.FirstOrDefault().Course.Code,
                                                      CourseNameEn = course.FirstOrDefault().Course.NameEn,
                                                      Credit = course.FirstOrDefault().Course.Credit,
                                                      CreditText = course.FirstOrDefault().Course.CreditText
                                              };

                        courseViewModel.Grades = new List<CourseGroupCourseGradeViewModel>();
                        var registrations = course.OrderBy(x => x.RegistrationCourse.Term.TermCompare)
                                                  .ToList();
                        foreach (var grade in registrations)
                        {
                            courseViewModel.Grades.Add(new CourseGroupCourseGradeViewModel
                                                       {
                                                               CourseId = course.Key,
                                                               TermText = grade.RegistrationCourse?.Term?.TermText,
                                                               IsTransferCourse = grade.RegistrationCourse?.IsTransferCourse ?? false,
                                                               IsStarCourse= grade.RegistrationCourse?.IsStarCourse ?? false,
                                                               RegisteredGradeWeight = grade.RegistrationCourse?.Grade.Weight,
                                                               RegisteredGradeName = grade.RegistrationCourse?.GradeName,
                                                               IsPassed = true
                                                       });                                        
                        }

                        group.Courses.Add(courseViewModel);
                    }

                    parents.Add(group);
                }

                return parents;
            }

            return null;
        }
        
        private class IgnoreJsonAttributesResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                foreach (var prop in props)
                {
                    prop.Ignored = false;   // Ignore [JsonIgnore]
                    prop.Converter = null;  // Ignore [JsonConverter]
                    prop.PropertyName = prop.UnderlyingName;  // restore original property name
                }
                return props;
            }
        }
        private List<CourseGroup> GetCourseGroups(long curriculumVersionId)
        {
            var courseGroupsResult = new List<CourseGroup>();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId
                                                           && x.CourseGroupId == null)
                                               .ToList();
            foreach (var courseGroup in courseGroups)
            {
                var courseGroupResult = courseGroup;
                courseGroupResult.CurriculumCourses = GetCurriculumCourses(courseGroup.Id);
                courseGroupResult.ChildCourseGroups = new List<CourseGroup>();
                courseGroupResult.ChildCourseGroups = GetChildCourseGroups(curriculumVersionId, courseGroup.Id);
                courseGroupsResult.Add(courseGroupResult);
            }

            return courseGroupsResult;
            // var json = JsonConvert.SerializeObject(courseGroupsResult);
            // return json;
        }
        private List<CourseGroup> GetChildCourseGroups(long curriculumVersionId, long? parentCourseGroupId)
        {
            var courseGroupsResult = new List<CourseGroup>();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId
                                                           && x.CourseGroupId == parentCourseGroupId)
                                               .ToList();
            foreach (var courseGroup in courseGroups)
            {
                if (!_db.CourseGroups.Any(x => x.CurriculumVersionId == curriculumVersionId
                                               && x.CourseGroupId == courseGroup.Id))
                {
                    courseGroup.CurriculumCourses = GetCurriculumCourses(courseGroup.Id);
                    courseGroupsResult.Add(courseGroup);
                }
                else
                {
                    courseGroup.CurriculumCourses = GetCurriculumCourses(courseGroup.Id);
                    courseGroup.ChildCourseGroups = GetChildCourseGroups(curriculumVersionId, courseGroup.Id);
                    courseGroupsResult.Add(courseGroup);
                }
            }
            return courseGroupsResult;
        }
        private ICollection<CurriculumCourse> GetCurriculumCourses(long courseGroupId)
        {
            return _db.CurriculumCourses.Include(x => x.Course)
                                        .Where(x => x.CourseGroupId == courseGroupId)
                                        .ToList();
        }

        // Ability
        public List<CurriculumCourse> GetAbilityCourses(long courseGroupId)
        {
            return _db.CurriculumCourses.Include(x => x.Course)
                                        .Include(x => x.CourseGroup)
                                        .Where(x => x.CourseGroupId == courseGroupId)
                                        .OrderBy(x => x.Sequence)
                                        .ToList();
        }
        
        public List<SpecializationGroupBlackList> GetAbilityBlacklistDepartments(long specializationGroupId)
        {
            var blacklists = _db.SpecializationGroupBlackLists.Include(x => x.Department)
                                                              .Where(x => x.SpecializationGroupId == specializationGroupId).ToList();
            return blacklists;
        }
        public void SaveCurriculumVersionExpectCredit(List<CurriculumVersion> model)
        {
            model = model.Where(x => x.ExpectCredit != 0 && x.ExpectCredit != null)
                         .ToList();

            foreach (var item in model)
            {
                var curriculumVersion = _db.CurriculumVersions.SingleOrDefault(x => x.Id == item.Id);
                curriculumVersion.ExpectCredit = item.ExpectCredit;
            }

            _db.SaveChanges();
        }

        // Curriculum Structure Report
        public CurriculumVersionStructureViewModel GetCurriculumVersionStructureByStudent(string studentId)
        {
            var academicInfo = _db.AcademicInformations.FirstOrDefault(x => x.StudentId.ToString() == studentId);
            if (academicInfo != null)
            {
                var curriculumVersion = GetCurriculumVersionStructureViewModel(academicInfo.CurriculumVersionId ?? 0);
                return curriculumVersion;
            }
            else
            {
                return null;
            }
        }
        public CurriculumVersionStructureViewModel GetCurriculumVersionStructure(long curriculumVersionId)
        {
            var curriculumVersion = GetCurriculumVersionStructureViewModel(curriculumVersionId);
            return curriculumVersion;
        }
        public string GetCurriculumVersionStructureJson(long curriculumVersionId)
        {
            var curriculumVersion = GetCurriculumVersionStructureViewModel(curriculumVersionId);
            var json = JsonConvert.SerializeObject(curriculumVersion, new JsonSerializerSettings { ContractResolver = new IgnoreJsonAttributesResolver(), ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return json;
        }

        public string GetCurriculumVersionCourseStructuresJson(long curriculumVersionId)
        {
            var curriculumVersion = _db.CurriculumVersions.Find(curriculumVersionId);
            List<CourseStructureViewModel> coursesViewModel = new List<CourseStructureViewModel>();
            var courseGroups = GetCourseGroupStructures(curriculumVersion.Id);

            foreach (var courseGroup in courseGroups)
            {
                GetCourseStructuresJson(courseGroup, ref coursesViewModel, curriculumVersionId);
            }
            var json = JsonConvert.SerializeObject(coursesViewModel, new JsonSerializerSettings { ContractResolver = new IgnoreJsonAttributesResolver(), ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return json;
        }

        private CurriculumVersionStructureViewModel GetCurriculumVersionStructureViewModel(long curriculumVersionId)
        {
            CurriculumVersionStructureViewModel curriculumVersionViewModel = new CurriculumVersionStructureViewModel();
            var curriculumInfo = (from version in _db.CurriculumVersions
                                  join curriculum in _db.Curriculums on version.CurriculumId equals curriculum.Id
                                  join level in _db.AcademicLevels on curriculum.AcademicLevelId equals level.Id
                                  join faculty in _db.Faculties on curriculum.FacultyId equals faculty.Id
                                  join department in _db.Departments on curriculum.DepartmentId equals department.Id
                                  join program in _db.AcademicPrograms on version.AcademicProgramId equals program.Id into programTmp
                                  from program in programTmp.DefaultIfEmpty()
                                  join implementedTerm in _db.Terms on version.ImplementedTermId equals implementedTerm.Id into implementedTermTmp
                                  from implementedTerm in implementedTermTmp.DefaultIfEmpty()
                                  join openedTerm in _db.Terms on version.OpenedTermId equals openedTerm.Id into openedTermTmp
                                  from openedTerm in openedTermTmp.DefaultIfEmpty()
                                  join closedTerm in _db.Terms on version.ClosedTermId equals closedTerm.Id into closedTermTmp
                                  from closedTerm in closedTermTmp.DefaultIfEmpty()
                                  where version.Id == curriculumVersionId
                                  select new
                                  {
                                      CurriculumVersion = version,
                                      Curriculum = curriculum,
                                      AcademicLevel = level,
                                      Faculty = faculty,
                                      Department = department,
                                      ImplementedTerm = implementedTerm,
                                      OpenedTerm = openedTerm,
                                      ClosedTerm = closedTerm,
                                      AcademicProgram = program
                                  }).FirstOrDefault();

            if (curriculumInfo != null)
            {

                curriculumVersionViewModel.AcademicLevel = curriculumInfo.AcademicLevel.NameEn;
                curriculumVersionViewModel.Faculty = curriculumInfo.Faculty.NameEn;
                curriculumVersionViewModel.Department = curriculumInfo.Department.NameEn;
                curriculumVersionViewModel.Curriculum = curriculumInfo.Curriculum.NameEn;
                curriculumVersionViewModel.CurriculumVersion = curriculumInfo.CurriculumVersion.Code + " - " + curriculumInfo.CurriculumVersion.NameEn;
                curriculumVersionViewModel.ImplementedTerm = curriculumInfo.ImplementedTerm?.TermText ?? "N/A";
                curriculumVersionViewModel.OpenedTerm = curriculumInfo.OpenedTerm?.TermText ?? "N/A";
                curriculumVersionViewModel.ClosedTerm = curriculumInfo.ClosedTerm?.TermText ?? "N/A";
                curriculumVersionViewModel.MinimumTerm = curriculumInfo.CurriculumVersion.MinimumTerm;
                curriculumVersionViewModel.MaximumTerm = curriculumInfo.CurriculumVersion.MaximumTerm;
                curriculumVersionViewModel.TotalCredit = curriculumInfo.CurriculumVersion.TotalCredit;
            }

            curriculumVersionViewModel.CourseGroups = GetCourseGroupStructures(curriculumVersionId);
            return curriculumVersionViewModel;
        }

        private void GetCourseStructuresJson(CourseGroupStructureViewModel courseGroup, ref List<CourseStructureViewModel> courses, long curriculumVersionId)
        {
            courses.AddRange(courseGroup.Courses);
            foreach (var course in courseGroup.Courses)
            {
                course.Prerequisites = new List<string>();
                course.Prerequisites.AddRange(_prerequisiteProvider.GetPrerequisitesByCourseId(course.CourseId, curriculumVersionId));
            }
            if (courseGroup.CourseGroups != null && courseGroup.CourseGroups.Any())
            {
                foreach (var childCourseGroup in courseGroup.CourseGroups)
                {
                    GetCourseStructuresJson(childCourseGroup, ref courses, curriculumVersionId);
                }
            }
            else
            {
                return;
            }
        }
        private List<CourseGroupStructureViewModel> GetCourseGroupStructures(long curriculumVersionId)
        {
            var courseGroupsViewModel = new List<CourseGroupStructureViewModel>();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId
                                                           && x.CourseGroupId == null)
                                               .ToList();
            foreach (var courseGroup in courseGroups)
            {
                CourseGroupStructureViewModel courseGroupViewModel = new CourseGroupStructureViewModel()
                {
                    CourseGroupId = courseGroup.Id,
                    Name = courseGroup.NameEn,
                    Description = courseGroup.DescriptionEn != null ? courseGroup.DescriptionEn.Trim() : "",
                    TotalCredit = courseGroup.Credit,
                    Courses = GetCourseStructures(courseGroup.Id, curriculumVersionId),
                    CourseGroups = GetChildCourseGroupStructures(curriculumVersionId, courseGroup.Id)
                };
                courseGroupsViewModel.Add(courseGroupViewModel);
            }
            return courseGroupsViewModel;
        }
        private List<CourseGroupStructureViewModel> GetChildCourseGroupStructures(long curriculumVersionId, long? parentCourseGroupId)
        {
            var courseGroupsViewModel = new List<CourseGroupStructureViewModel>();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId
                                                           && x.CourseGroupId == parentCourseGroupId)
                                               .ToList();

            foreach (var courseGroup in courseGroups)
            {
                CourseGroupStructureViewModel courseGroupViewModel = new CourseGroupStructureViewModel();
                courseGroupViewModel.CourseGroupId = courseGroup.Id;
                courseGroupViewModel.Name = courseGroup.NameEn;
                courseGroupViewModel.Description = courseGroup.DescriptionEn != null ? courseGroup.DescriptionEn.Trim() : "";
                courseGroupViewModel.TotalCredit = courseGroup.Credit;
                courseGroupViewModel.Courses = GetCourseStructures(courseGroup.Id, curriculumVersionId);
                // If has child course group
                if (_db.CourseGroups.Any(x => x.CurriculumVersionId == curriculumVersionId
                                               && x.CourseGroupId == courseGroup.Id))
                {
                    courseGroupViewModel.CourseGroups = GetChildCourseGroupStructures(curriculumVersionId, courseGroup.Id);
                }
                courseGroupsViewModel.Add(courseGroupViewModel);
            }
            return courseGroupsViewModel;
        }
        private List<CourseStructureViewModel> GetCourseStructures(long courseGroupId, long curriculumVersionId)
        {
            var courses = _db.CurriculumCourses.Include(x => x.Course)
                                               .Include(x => x.Grade)
                                               .Where(x => x.CourseGroupId == courseGroupId)
                                               .Select(x => new CourseStructureViewModel
                                               {
                                                   CourseId = x.Course.Id,
                                                   Code = x.Course.Code,
                                                   Credit = $"{ x.Course.Credit.ToString(StringFormat.GeneralDecimal) }({ x.Course.Lecture.ToString(StringFormat.GeneralDecimal) }-{ x.Course.Lab.ToString(StringFormat.GeneralDecimal) }-{ x.Course.Other.ToString(StringFormat.GeneralDecimal) })",
                                                   Name = x.Course.NameEn,
                                                   IsRequiredGrade = x.IsRequired,
                                                   Grade = x.Grade.Name
                                               })
                                               .ToList();
            if (courses != null && courses.Any())
            {
                foreach (var course in courses)
                {
                    course.Prerequisites = new List<string>();
                    course.Prerequisites.AddRange(_prerequisiteProvider.GetPrerequisitesByCourseId(course.CourseId, curriculumVersionId));
                }
            }
            return courses;
        }
        public void CopyCourseGroupFromSpecilizationGroup(CourseGroup parentGroup)
        {
            var parents = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                          .Where(x => x.SpecializationGroupId == parentGroup.SpecializationGroupId
                                                      && x.CourseGroupId == null
                                                      && x.CurriculumVersionId == null)
                                          .ToList();
                                          
            foreach (var group in parents)
            {
                var courseGroup = _mapper.Map<CourseGroup, CourseGroup>(group);
                courseGroup.CurriculumVersionId = parentGroup.CurriculumVersionId;
                courseGroup.CourseGroupId = parentGroup.Id;
                _db.CourseGroups.Add(courseGroup);
                _db.SaveChanges();
                
                CopyCourseGroup(group, courseGroup.Id, parentGroup.CurriculumVersionId.Value);
            }
        }

        private void CopyCourseGroup(CourseGroup originalGroup, long newCourseGroupId, long curriculumVersionId)
        {
            var groups = _db.CourseGroups.Where(x => x.CourseGroupId == originalGroup.Id).ToList();
            if (groups.Any())
            {
                foreach (var group in groups)
                {
                    var courseGroup = _mapper.Map<CourseGroup, CourseGroup>(group);
                    courseGroup.CurriculumVersionId = curriculumVersionId;
                    courseGroup.CourseGroupId = newCourseGroupId;
                    _db.CourseGroups.Add(courseGroup);
                    _db.SaveChanges();

                    CopyCourseGroup(group, courseGroup.Id, curriculumVersionId);
                }
            }
            else 
            {
                var curriculumCourses = _db.CurriculumCourses.Where(x => x.CourseGroupId == originalGroup.Id)
                                                                .Select(x => _mapper.Map<CurriculumCourse, CurriculumCourse>(x))
                                                                .ToList();
                curriculumCourses.ForEach(x => x.CourseGroupId = newCourseGroupId);
                _db.CurriculumCourses.AddRange(curriculumCourses);
                _db.SaveChanges();
            }
        }

        public List<CurriculumSpecializationGroup> GetSpecializationGroupsByCurriculumVersion(long curriculumVersionId)
        {
            var result = _db.CurriculumSpecializationGroups.Include(x => x.SpecializationGroup)
                                                           .Where(x => x.CurriculumVersionId == curriculumVersionId)
                                                           .OrderBy(x => x.SpecializationGroup.Type)
                                                              .ThenBy(x => x.SpecializationGroup.Code)
                                                           .ToList();
            return result;
        }
    }
}