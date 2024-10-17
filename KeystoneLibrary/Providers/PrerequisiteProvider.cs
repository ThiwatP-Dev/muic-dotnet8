using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class PrerequisiteProvider : BaseProvider, IPrerequisiteProvider
    {        
        private const string _graphIdentifierFormat = "00000000";

        public PrerequisiteProvider(ApplicationDbContext db, 
                                    IMapper mapper) : base(db, mapper) 
        {
        }

        public StudentInfoForPrerequisiteViewModel GetStudentInfoForPrerequisite(Guid studentId)
        {
            var studentInfo = new StudentInfoForPrerequisiteViewModel();
            var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
            if (student == null)
            {
                return null;
            }
            else
            {
                studentInfo.Student = student;
                studentInfo.RegistrationCourses = _db.RegistrationCourses.Include(x => x.Course)
                                                                         .Include(x => x.Grade)
                                                                         .Where(x => x.StudentId == studentId && x.IsPaid && x.Status != "d")
                                                                         .ToList();
                studentInfo.AcademicInformation = _db.AcademicInformations.Include(x => x.CurriculumVersion).SingleOrDefault(x => x.StudentId == studentId);
                return studentInfo;
            }
        }

        public StudentInfoForPrerequisiteViewModel GetStudentInfoForPrerequisite(string studentCode)
        {
            var student = _db.Students.Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.Department)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.SpecializationGroupInformations)
                                          .ThenInclude(x => x.SpecializationGroup)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .SingleOrDefault(x => x.Code == studentCode);
            if (student == null)
            {
                return null;
            }
            else
            {
                return GetStudentInfoForPrerequisite(student.Id);
            }
        }

        public List<CoursePrerequisiteViewModel> GetPrerequisiteCurriculumVersion(long curriculumVersionId)
        {
            var result = new List<CoursePrerequisiteViewModel>();
            var courses = (from courseGroup in _db.CourseGroups
                           join curriculumCourse in _db.CurriculumCourses on courseGroup.Id equals curriculumCourse.CourseGroupId
                           join course in _db.Courses on curriculumCourse.CourseId equals course.Id
                           where courseGroup.CurriculumVersionId == curriculumVersionId
                           select course).ToList();

            foreach (var course in courses)
            {
                var coursePrerequisite = new CoursePrerequisiteViewModel();
                coursePrerequisite.Course = course;
                coursePrerequisite.PrerequisiteDetail = GetPrerequisiteByCourseId(course.Id);
                result.Add(coursePrerequisite);
            }

            return result;
        }

        public List<string> GetPrerequisitesByCourseId(long courseId, long curriculumVersionId)
        {
            List<string> prerequisitesText = new List<string>();
            var prerequisites = _db.Prerequisites.Where(x => x.CourseId == courseId
                                                             && x.CurriculumVersionId == curriculumVersionId
                                                             && (x.ExpiredAt == null
                                                                 || x.ExpiredAt > DateTime.Now)).ToList();
            foreach (var prerequisite in prerequisites)
            {
                prerequisitesText.Add(GetConditionDescription(prerequisite.ConditionType, prerequisite.ConditionId, true));
            }
            return prerequisitesText;
        }

        public List<Prerequisite> GetPrerequisiteByCurriculumVersionId(long curriculumVersionId)
        {
            var prerequisites = _db.Prerequisites.Where(x => x.CurriculumVersionId == curriculumVersionId)
                                                 .ToList();
            return prerequisites;
        }

        public string GetPrerequisiteByCourseId(long courseId)
        {
            var prerequisite = _db.Prerequisites.FirstOrDefault(x => x.CourseId == courseId
                                                                     && (x.ExpiredAt == null
                                                                         || x.ExpiredAt > DateTime.Now));
            return prerequisite == null ? "" : GetConditionDescription(prerequisite.ConditionType, prerequisite.ConditionId, true);
        }

        public string GetConditionDescription(string conditionType, long conditionId, bool isGetDeeperLevel) 
        {
            switch (conditionType.Trim())
            {
                case "and":
                    var andCondition = _db.AndConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (andCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetAndConditionDescription(andCondition, isGetDeeperLevel);
                    }

                case "or":
                    var orCondition = _db.OrConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (orCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetOrConditionDescription(orCondition, isGetDeeperLevel);
                    }

                case "grade":
                    var gradeCondition = _db.GradeConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (gradeCondition == null) 
                    {
                        return "";
                    }
                    else
                    {
                        return GetGradeConditionDescription(gradeCondition, isGetDeeperLevel);
                    }

                case "credit":
                    var creditCondition = _db.CreditConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (creditCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetCreditConditionDescription(creditCondition);
                    }

                case "term":
                    var termCondition = _db.TermConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (termCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetTermConditionDescription(termCondition);
                    }

                case "gpa":
                    var gpaCondition = _db.GPAConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (gpaCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetGPAConditionDescription(gpaCondition);
                    }

                case "id":
                    return "";

                case "corequisites":
                case "Core::Dependency::CourseCorequisite":
                    var corequisiteCondition = _db.Corequisites.SingleOrDefault(x => x.Id == conditionId);
                    if (corequisiteCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetCorequisiteDescription(corequisiteCondition);
                    }

                case "coursegroup":
                    var courseGroupCondition = _db.CourseGroupConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (courseGroupCondition == null) 
                    {
                        return "";
                    }
                    else 
                    {
                        return GetCourseGroupConditionDescription(courseGroupCondition);
                    }

                case "predefinedcourse":
                    var predefinedCourseCondition = _db.PredefinedCourses.SingleOrDefault(x => x.Id == conditionId);
                    if (predefinedCourseCondition == null) 
                    {
                        return "";
                    }
                    else 
                    {
                        return GetPredefinedCourseDescription(predefinedCourseCondition);
                    }

                case "totalcoursegroup":
                    var totalCourseGroupCondition = _db.TotalCourseGroupConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (totalCourseGroupCondition == null) 
                    {
                        return "";
                    }
                    else 
                    {
                        return GetTotalCourseGroupConditionDescription(totalCourseGroupCondition);
                    }

                case "ability":
                case "Core::Condition::Ability":
                    var abilityCondition = _db.AbilityConditions.Include(x => x.Ability)
                                                                .SingleOrDefault(x => x.Id == conditionId);
                    if (abilityCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetAbilityConditionDescription(abilityCondition);
                    }

                case "batch":
                    var batchCondition = _db.BatchConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (batchCondition == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetBatchConditionDescription(batchCondition);
                    }

                default:
                    break;
            }
            return "";
        }

        public string GetUpdateAndConditionDescription(AndCondition condition, bool isGetDeeperLevel)
        {
            return String.Format("({0}) and ({1})"
                                 , GetConditionDescription(condition.FirstConditionType, condition.FirstConditionId, isGetDeeperLevel)
                                 , GetConditionDescription(condition.SecondConditionType, condition.SecondConditionId, isGetDeeperLevel));
        }

        public string GetAndConditionDescription(AndCondition condition, bool isGetDeeperLevel)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return GetUpdateAndConditionDescription(condition, isGetDeeperLevel);
            }
            else 
            {
                return condition.Description;
            }
        }

        public string GetUpdateOrConditionDescription(OrCondition condition, bool isGetDeeperLevel)
        {
            return String.Format("({0}) or ({1})"
                                 , GetConditionDescription(condition.FirstConditionType, condition.FirstConditionId, isGetDeeperLevel)
                                 , GetConditionDescription(condition.SecondConditionType, condition.SecondConditionId, isGetDeeperLevel));
        }
        
        public string GetOrConditionDescription(OrCondition condition, bool isGetDeeperLevel)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return GetUpdateOrConditionDescription(condition, isGetDeeperLevel);
            }
            else 
            {
                return condition.Description;
            }
        }

        public string GetCourseGroupConditionDescription(CourseGroupCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.Id == condition.CourseGroupId);
                if (courseGroup != null)
                {
                    return String.Format("(CREDITS >= {0} for {1})", condition.Credit, courseGroup.NameEn);
                }
                return "";
            }
            else 
            {
                return condition.Description;
            }
        }

        public string GetCorequisiteDescription(Corequisite condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                var firstCourse = _db.Courses.SingleOrDefault(x => x.Id == condition.FirstCourseId);
                var secondCourse = _db.Courses.SingleOrDefault(x => x.Id == condition.SecondCourseId);
                if (firstCourse != null && secondCourse != null) 
                {
                    return String.Format("(REGISTER {0} AND {1})", firstCourse.Code, secondCourse.Code);
                }
                return "";
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetCreditConditionDescription(CreditCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                var curriculumVersion = _db.CurriculumVersions.SingleOrDefault(x => x.Id == condition.CurriculumVersionId);
                if (curriculumVersion != null)
                {
                    return String.Format("(CREDITS >= {0} for {1})", condition.Credit, curriculumVersion.NameEn);
                }
                else 
                {
                    return String.Format("(CREDITS >= {0})", condition.Credit);
                }
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetGPAConditionDescription(GPACondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return String.Format("(GPA >= {0:0.00})", condition.GPA);
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetGradeConditionDescription(GradeCondition condition, bool isGetDeeperLevel)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                var course = _db.Courses.SingleOrDefault(x => x.Id == condition.CourseId);
                var grade = _db.Grades.SingleOrDefault(x => x.Id == condition.GradeId);
                string creditText = $"[{course.CreditText}]";
                if (isGetDeeperLevel)
                {
                    var morePrerequisite = _db.Prerequisites.Where(x => x.CourseId == condition.CourseId).ToList();
                    if (morePrerequisite != null) 
                    {
                        if (morePrerequisite.Count == 1)
                        {
                            return String.Format("({0} GRADE MORE THAN {1}) and ({2})", course.Code + creditText, grade.Name, GetPrerequisiteByCourseId(condition.CourseId));
                        }
                        else
                        {
                            // not sure why it would happen but there might be a chance
							return String.Format("({0} GRADE MORE THAN {1}) and *({2})", course.Code + creditText, grade.Name, GetPrerequisiteByCourseId(condition.CourseId));
						}
                    }
                    else 
                    {
                        return String.Format("({0} GRADE MORE THAN {1})", course.Code + creditText, grade.Name);
                    }
                }
                else 
                {
                    return String.Format("({0} GRADE MORE THAN {1})", course.Code + creditText, grade.Name);
                }
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetPredefinedCourseDescription(PredefinedCourse condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return "";
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetTermConditionDescription(TermCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return String.Format("(TERM >= {0})", condition.Term);
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetTotalCourseGroupConditionDescription(TotalCourseGroupCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return String.Format("(TOTAL GROUPS >= {0})", condition.CourseGroupAmount);
            }
            else 
            {
                return condition.Description;
            }
        }
        
        public string GetAbilityConditionDescription(AbilityCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return condition.Ability.NameEn;
                //String.Format("(ABILITY {0})", condition.Ability.ShortNameEn);
            }
            else 
            {
                return condition.Description;
            }
        }

        public string GetBatchConditionDescription(BatchCondition condition)
        {
            if (String.IsNullOrEmpty(condition.Description))
            {
                return String.Format("(BATCH {0})", condition.Batch);
            }
            else 
            {
                return condition.Description;
            }
        }
        

        // public bool CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId) 
        // {
        //     var prerequisite = _db.Prerequisites.FirstOrDefault(x => x.CourseId == courseId
        //                                                              && (x.ExpiredAt == null
        //                                                                  || x.ExpiredAt > DateTime.Now));
        //     return prerequisite == null ? true : CheckCondition(studentInfo, prerequisite.ConditionType, prerequisite.ConditionId);
        // }

        public bool CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId, long curriculumVersionId, out string message) 
        {
            var result = true;

            var errorMessages = new List<string>();
            message = string.Empty;

            // Predefine course check -> IF PREDEFINED ALLOW ALWAYS
            var predefinedCourses = _db.StudentPredefinedCourses.AsNoTracking()
                                                                .Any(x => x.StudentId == studentInfo.Student.Id
                                                                          && x.CourseId == courseId
                                                                          && string.Equals(x.Type,"w",StringComparison.InvariantCultureIgnoreCase));

            if (predefinedCourses)
            {
                return true;
            }
            
            // Prereqisite Check
            var prerequisites = _db.Prerequisites.AsNoTracking()
                                                 .Where(x => x.CourseId == courseId
                                                             && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.Now))
                                                 .ToList();

            if (!prerequisites.Any())
            {
                return true;
            }

            var courseGroups = _db.CourseGroups.AsNoTracking()
                                               .Include(x => x.CurriculumCourses)
                                               .Where(x => x.CurriculumVersionId == curriculumVersionId)
                                               .ToList();

            var curriculumCourses = courseGroups.SelectMany(x => x.CurriculumCourses)
                                                .ToList();

            var isInCurriculumCourses = curriculumCourses.Any(x => x.CourseId == courseId);

            if (isInCurriculumCourses)
            {
                var checkedPrerequisite = prerequisites.Where(x => x.CurriculumVersionId.Value == curriculumVersionId)
                                                       .ToList();

                foreach (var curriculumPrerequisite in checkedPrerequisite)
                {
                    var isPrerequisiteConditionMatch = CheckCondition(studentInfo, curriculumPrerequisite.ConditionType, curriculumPrerequisite.ConditionId, curriculumVersionId, out string conditionMessage);

                    if (!isPrerequisiteConditionMatch)
                    {
                        result = false;
                        errorMessages.Add(conditionMessage);
                    }
                }
            }
            else
            {
                var groupRequisiteByCurriculum = (from requisite in prerequisites
                                                  group requisite by requisite.CurriculumVersionId into curriculumRequisites
                                                  select new
                                                  {
                                                      CurriculumVersionId = curriculumRequisites.Key,
                                                      Requisites = curriculumRequisites.ToList()
                                                  }).ToList();

                foreach(var curriculum in groupRequisiteByCurriculum)
                {
                    var isConditionMatching = true;
                    var versionId = curriculum.CurriculumVersionId.HasValue ? curriculum.CurriculumVersionId.Value
                                                                            : 0;


                    foreach (var requisite in curriculum.Requisites)
                    {
                        var isPrerequisiteConditionMatch = CheckCondition(studentInfo, requisite.ConditionType, requisite.ConditionId, versionId, out string conditionMessage);

                        if (!isPrerequisiteConditionMatch)
                        {
                            isConditionMatching = false;
                            break;
                        }
                    }

                    if (isConditionMatching)
                    {
                        return true;
                    }
                }

                result = false;
                errorMessages.Add("NOT MATCH ANY PREQUISITE CONDITIONS IN ALL CURRICULUM");
            }

            if (!result)
            {
                message = string.Join(Environment.NewLine, errorMessages);
            }

            return result;
        }

        public List<PrerequisiteCheckDetailViewModel> CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId, long curriculumVersionId)
        {
            var result = new List<PrerequisiteCheckDetailViewModel>();

            var prerequisiteList = _db.Prerequisites.AsNoTracking()
                                                    .Where(x => x.CourseId == courseId
                                                                && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.UtcNow))
                                                    .ToList();

            if (!prerequisiteList.Any())
            {
                return result;
            }

            var courseGroups = _db.CourseGroups.AsNoTracking()
                                               .Include(x => x.CurriculumCourses)
                                               .Where(x => x.CurriculumVersionId == curriculumVersionId)
                                               .ToList();

            var curriculumCourses = courseGroups.SelectMany(x => x.CurriculumCourses)
                                                .ToList();

            var isInCurriculumCourses = curriculumCourses.Any(x => x.CourseId == courseId);

            if (isInCurriculumCourses)
            {
                prerequisiteList = prerequisiteList.Where(x => x.CurriculumVersionId == curriculumVersionId).ToList();
                foreach (var prerequisite in prerequisiteList)
                {
                    result.AddRange(CheckCondition(studentInfo, prerequisite.ConditionType, prerequisite.ConditionId, studentInfo.AcademicInformation.CurriculumVersion, out bool temp));
                }
            }
            else
            {
                var groupRequisiteByCurriculum = (from requisite in prerequisiteList
                                                  group requisite by requisite.CurriculumVersionId into curriculumRequisites
                                                  select new
                                                  {
                                                      CurriculumVersionId = curriculumRequisites.Key,
                                                      Requisites = curriculumRequisites.ToList()
                                                  }).ToList();

                var curriculumVersionIds = groupRequisiteByCurriculum.Select(x => x.CurriculumVersionId)
                                                                     .Distinct()
                                                                     .ToList();

                var curriculumVersions = _db.CurriculumVersions.AsNoTracking()
                                                               .Where(x => curriculumVersionIds.Contains(x.Id))
                                                               .ToList();

                foreach (var curriculum in groupRequisiteByCurriculum)
                {
                    var curriculumVersion = curriculumVersions.SingleOrDefault(x => x.Id == curriculum.CurriculumVersionId);

                    if(curriculumVersion == null)
                    {
                        continue;
                    }

                    foreach (var prerequisite in curriculum.Requisites)
                    {
                        result.AddRange(CheckCondition(studentInfo, prerequisite.ConditionType, prerequisite.ConditionId, curriculumVersion, out bool temp));
                    }
                }
            }
            result = result.OrderBy(x => x.Curriculum).ToList();
            return result;
        }

        public Course CheckCourseCorequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId)
        {
            var corequisite = _db.Corequisites.FirstOrDefault(x => (x.FirstCourseId == courseId
                                                                    || x.SecondCourseId == courseId)
                                                                   && (x.ExpiredAt == null
                                                                       || x.ExpiredAt > DateTime.Now));
            if (corequisite == null)
            {
                return null;
            }
            else
            {
                var cocourse = _db.Courses.SingleOrDefault(x => x.Id == (corequisite.FirstCourseId == courseId
                                                                         ? corequisite.SecondCourseId : corequisite.FirstCourseId));
                return cocourse;
            }
        }

        // private bool CheckCondition(StudentInfoForPrerequisiteViewModel studentInfo, string conditionType, long conditionId)
        // {
        //     switch (conditionType.Trim())
        //     {
        //         case "Core::Condition::And":
        //             var andCondition = _db.AndConditions.SingleOrDefault(x => x.Id == conditionId);
        //             if (andCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 return CheckCondition(studentInfo, andCondition.FirstConditionType, andCondition.FirstConditionId)
        //                        && CheckCondition(studentInfo, andCondition.SecondConditionType, andCondition.SecondConditionId);
        //             }

        //         case "Core::Condition::Or":
        //             var orCondition = _db.OrConditions.SingleOrDefault(x => x.Id == conditionId);
        //             if (orCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 return CheckCondition(studentInfo, orCondition.FirstConditionType, orCondition.FirstConditionId)
        //                     || CheckCondition(studentInfo, orCondition.SecondConditionType, orCondition.SecondConditionId);
        //             }

        //         case "Core::Condition::Grade":
        //             var gradeCondition = _db.GradeConditions.SingleOrDefault(x => x.Id == conditionId);
        //             if (gradeCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 var bestGradeFromCourse = (from course in studentInfo.RegistrationCourses
        //                                            join grade in _db.Grades on course.GradeId equals grade.Id
        //                                            where course.CourseId == gradeCondition.CourseId
        //                                            && course.IsGradePublished
        //                                            orderby grade.Weight descending
        //                                            select grade).FirstOrDefault();

        //                 var morePrerequisite = _db.Prerequisites.SingleOrDefault(x => x.CourseId == gradeCondition.CourseId);
        //                 if (bestGradeFromCourse == null)
        //                 {
        //                     return false;
        //                 }
        //                 else
        //                 {
        //                     var grade = _db.Grades.SingleOrDefault(x => x.Id == gradeCondition.GradeId);
        //                     return bestGradeFromCourse.Weight >= grade.Weight &&
        //                            (morePrerequisite == null ? true : CheckPrerequisite(studentInfo, gradeCondition.CourseId));
        //                 }

        //             }

        //         case "Core::Condition::Credit":
        //             var creditCondition = _db.CreditConditions.SingleOrDefault(x => x.Id == conditionId);
        //             if (creditCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 return studentInfo.AcademicInformation.CreditComp >= creditCondition.Credit;
        //             }

        //         case "Core::Condition::Ability":
        //             return false;

        //         case "Core::Condition::Term":
        //             return false;

        //         case "Core::Condition::Gpa":
        //             var gpaCondition = _db.GPAConditions.SingleOrDefault(x => x.Id == conditionId);
        //             if (gpaCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 return studentInfo.AcademicInformation.GPA >= gpaCondition.GPA;
        //             }

        //         case "Core::Condition::Id":
        //             return false;

        //         case "Core::Dependency::CourseCorequisite":
        //             var corequisiteCondition = _db.Corequisites.SingleOrDefault(x => x.Id == conditionId);
        //             if (corequisiteCondition == null)
        //             {
        //                 return false;
        //             }
        //             else
        //             {
        //                 var firstCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.FirstCourseId);
        //                 var secondCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.SecondCourseId);
        //                 if (firstCourseRegistration == null || secondCourseRegistration == null)
        //                 {
        //                     return false;
        //                 }
        //                 else
        //                 {
        //                     return firstCourseRegistration.TermId == secondCourseRegistration.TermId;
        //                 }
        //             }

        //         default:
        //             break;
        //     }

        //     return false;
        // }

        private List<PrerequisiteCheckDetailViewModel> CheckCondition(
            StudentInfoForPrerequisiteViewModel studentInfo,
            string conditionType,
            long conditionId,
            CurriculumVersion curriculumVersion, out bool isPassCondition) 
        {
            var result = new List<PrerequisiteCheckDetailViewModel>();

            isPassCondition = false;

            switch (conditionType.Trim())
            {
                case "and":
                    var andCondition = _db.AndConditions.AsNoTracking()
                                                        .SingleOrDefault(x => x.Id == conditionId);
                    if (andCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "And condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }
                    
                    var firstAndConditions = CheckCondition(studentInfo, andCondition.FirstConditionType, andCondition.FirstConditionId, curriculumVersion, out bool isFirstConditionPass);
                    var secondAndConditions = CheckCondition(studentInfo, andCondition.SecondConditionType, andCondition.SecondConditionId, curriculumVersion, out bool isSecondConditionPass);
                    result.AddRange(firstAndConditions);
                    result.AddRange(secondAndConditions);

                    isPassCondition = isFirstConditionPass && isSecondConditionPass;

                    result.Add(new PrerequisiteCheckDetailViewModel()
                    {
                        Curriculum = curriculumVersion.NameEn,
                        Description = andCondition.Description,
                        IsPass = isFirstConditionPass && isSecondConditionPass
                    });

                    return result;

                case "or":
                    var orCondition = _db.OrConditions.AsNoTracking()
                                                      .SingleOrDefault(x => x.Id == conditionId);
                    if (orCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Or condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }
                    
                    var firstOrConditions = CheckCondition(studentInfo, orCondition.FirstConditionType, orCondition.FirstConditionId, curriculumVersion, out bool isFirsOrConditionPass);
                    var secondOrConditions = CheckCondition(studentInfo, orCondition.SecondConditionType, orCondition.SecondConditionId, curriculumVersion, out bool isSecondOrConditionPass);
                    result.AddRange(firstOrConditions);
                    result.AddRange(secondOrConditions);

                    isPassCondition = isFirsOrConditionPass || isSecondOrConditionPass;

                    result.Add(new PrerequisiteCheckDetailViewModel()
                    {
                        Curriculum = curriculumVersion.NameEn,
                        Description = orCondition.Description,
                        IsPass = isFirsOrConditionPass || isSecondOrConditionPass
                    });

                    return result;
                    
                case "grade":
                    var gradeCondition = _db.GradeConditions.AsNoTracking()
                                                            .SingleOrDefault(x => x.Id == conditionId);

                    if (gradeCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Grade condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    var grades = _db.Grades.AsNoTracking()
                                           .ToList();
                    
                    var lastGradeFromCourse = (from course in studentInfo.RegistrationCourses
                                               join grade in grades on course.GradeId equals grade.Id
                                               where course.CourseId == gradeCondition.CourseId
                                                     && course.IsGradePublished
                                               orderby course.TermId descending
                                               select grade)
                                               .FirstOrDefault();

                    if (lastGradeFromCourse == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel()
                        {
                            Curriculum = curriculumVersion.NameEn,
                            Description = gradeCondition.Description,
                            IsPass = false,
                            IsNotFound = true
                        });
                        return result;
                    }

                    var requiredGrade = grades.FirstOrDefault(x => x.Id == gradeCondition.GradeId);

                    isPassCondition = lastGradeFromCourse.Weight >= requiredGrade.Weight;

                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                {
                                    Curriculum = curriculumVersion.NameEn,
                                    Description = gradeCondition.Description,
                                    IsPass = lastGradeFromCourse.Weight >= requiredGrade.Weight
                                });

                    return result;
                    
                case "credit":
                    var creditCondition = _db.CreditConditions.AsNoTracking()
                                                              .SingleOrDefault(x => x.Id == conditionId);
                    if (creditCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Credit condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    isPassCondition = true;
                    //isPassCondition = studentInfo.AcademicInformation.CreditComp >= creditCondition.Credit;
                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                    {
                                        Curriculum = curriculumVersion.NameEn,
                                        Description = $"{creditCondition.Description} [ByPass]",
                                        IsPass = isPassCondition
                                    });

                    return result;
                    

                case "ability":
                    var ability = _db.AbilityConditions.AsNoTracking()
                                                       .SingleOrDefault(x => x.Id == conditionId);

                    if (ability == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Ability condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }
                    
                    var specializationGroup = _db.SpecializationGroups.AsNoTracking()
                                                                      .SingleOrDefault(x => x.Id == ability.AbilityId);

                    if(specializationGroup == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Ability condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    isPassCondition = _db.SpecializationGroupInformations.AsNoTracking()
                                                                         .Include(x => x.CurriculumInformation)
                                                                         .Include(x => x.SpecializationGroup)
                                                                         .Any(x => x.CurriculumInformation.StudentId == studentInfo.Student.Id
                                                                                                && x.SpecializationGroup.Id == specializationGroup.Id
                                                                                                && x.IsActive);
                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                    {
                                        Curriculum = curriculumVersion.NameEn,
                                        Description = ability.Description,
                                        IsPass = isPassCondition
                                     });

                    return result;
                    
                    
                case "term":
                    var termCondition = _db.TermConditions.AsNoTracking()
                                                          .SingleOrDefault(x => x.Id == conditionId);

                    if (termCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Term condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    var termIds = studentInfo.RegistrationCourses.Select(x => x.TermId)
                                                                 .Distinct()
                                                                 .ToList();
                    var terms = _db.Terms.AsNoTracking()
                                        .Include(x => x.TermType)
                                        .Where(x => termIds.Contains(x.Id))
                                        .ToList();

                    var validTerms = (from term in terms
                                      where !string.IsNullOrEmpty(term.TermType.Code)
                                            && string.Equals(term.TermType.Code, "n", StringComparison.InvariantCultureIgnoreCase)
                                      select term.Id)
                                      .Distinct()
                                      .Count();

                    isPassCondition = validTerms + 1 >= termCondition.Term;

                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                    {
                                        Curriculum = curriculumVersion.NameEn,
                                        Description = termCondition.Description,
                                        IsPass = isPassCondition
                                    });
                    return result;
                    

                case "gpa":
                    var gpaCondition = _db.GPAConditions.AsNoTracking()
                                                        .SingleOrDefault(x => x.Id == conditionId);

                    if (gpaCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "GPA condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }
                    
                    isPassCondition = studentInfo.AcademicInformation.GPA >= gpaCondition.GPA;

                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                    {
                                        Curriculum = curriculumVersion.NameEn,
                                        Description = gpaCondition.Description,
                                        IsPass = isPassCondition
                                    });
                    return result;
                    
                case "corequisites":
                    var corequisiteCondition = _db.Corequisites.AsNoTracking()
                                                               .SingleOrDefault(x => x.Id == conditionId);

                    if (corequisiteCondition == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Course corequisite condition not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    var firstCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.FirstCourseId);
                    var secondCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.SecondCourseId);

                    if (firstCourseRegistration == null || secondCourseRegistration == null)
                    {
                        result.Add(new PrerequisiteCheckDetailViewModel() 
                                        {
                                            Curriculum = curriculumVersion.NameEn,
                                            Description = "Course registration not found.",
                                            IsPass = false,
                                            IsNotFound = true
                                        });
                        return result;
                    }

                    isPassCondition = firstCourseRegistration.TermId == secondCourseRegistration.TermId;
                    result.Add(new PrerequisiteCheckDetailViewModel() 
                                    {
                                        Curriculum = curriculumVersion.NameEn,
                                        Description = corequisiteCondition.Description,
                                        IsPass = isPassCondition
                    });

                    return result;

                default:
                    break;
            }

            result.Add(new PrerequisiteCheckDetailViewModel() 
                            {
                                Curriculum = curriculumVersion.NameEn,
                                Description = "Condition type not found.",
                                IsPass = false,
                                IsNotFound = true
                            });
            return result;
        }

        private bool CheckCondition(StudentInfoForPrerequisiteViewModel studentInfo, string conditionType, long conditionId, long curriculumVersionId, out string message) 
        {
            switch (conditionType.Trim())
            {
                case "and":
                    var andCondition = _db.AndConditions.AsNoTracking()
                                                        .SingleOrDefault(x => x.Id == conditionId);
                    if (andCondition == null)
                    {
                        message = "And condition not found.";
                        return false;
                    }

                    string andConditionA = string.Empty;
                    string andConditionB = string.Empty;

                    if (CheckCondition(studentInfo, andCondition.FirstConditionType, andCondition.FirstConditionId, curriculumVersionId, out andConditionA))
                    {
                        if (CheckCondition(studentInfo, andCondition.SecondConditionType, andCondition.SecondConditionId, curriculumVersionId, out andConditionB))
                        {
                            message = string.Empty;
                            return true;
                        }
                        else
                        {
                            message = andConditionB;
                            return false;
                        }
                    }
                    else
                    {
                        message = andConditionA;
                        return false;
                    }
                    
                case "or":
                    var orCondition = _db.OrConditions.AsNoTracking()
                                                      .SingleOrDefault(x => x.Id == conditionId);
                    if (orCondition == null)
                    {
                        message = "Or condition not found.";
                        return false;
                    }

                    string orConditionA = string.Empty;
                    string orConditionB = string.Empty;
                    if (CheckCondition(studentInfo, orCondition.FirstConditionType, orCondition.FirstConditionId, curriculumVersionId, out orConditionA))
                    {
                        message = string.Empty;
                        return true;
                    }
                    else
                    {
                        if (CheckCondition(studentInfo, orCondition.SecondConditionType, orCondition.SecondConditionId, curriculumVersionId, out orConditionB))
                        {
                            message = string.Empty;
                            return true;
                        }
                        else
                        {
                            message = $"{ orConditionA } or { orConditionB }";
                            return false;
                        }
                    }
                    

                case "grade":
                    var gradeCondition = _db.GradeConditions.AsNoTracking()
                                                            .SingleOrDefault(x => x.Id == conditionId);
                    if (gradeCondition == null)
                    {
                        message = "Grade condition not found.";
                        return false;
                    }

                    var grades = _db.Grades.AsNoTracking()
                                           .ToList();

                    var bestGradeFromCourse = (from course in studentInfo.RegistrationCourses
                                            join grade in grades on course.GradeId equals grade.Id
                                            where course.CourseId == gradeCondition.CourseId
                                                  && course.IsGradePublished
                                            orderby grade.Weight descending
                                            select grade)
                                            .FirstOrDefault();

                    var requiredGrade = _db.Grades.AsNoTracking()
                                                  .FirstOrDefault(x => x.Id == gradeCondition.GradeId);

                    if (bestGradeFromCourse == null)
                    {
                        message = $"{gradeCondition.Description}";
                        return false;
                    }
                    else
                    {
                        if (bestGradeFromCourse.Weight >= requiredGrade.Weight)
                        {
                            message = string.Empty;
                            return true;
                        }
                        else
                        {
                            message = $"{gradeCondition.Description}";
                            return false;
                        }
                    }                     
                    
                case "credit":
                    var creditCondition = _db.CreditConditions.AsNoTracking()
                                                              .SingleOrDefault(x => x.Id == conditionId);

                    if (creditCondition == null)
                    {
                        message = "Credit condition not found.";
                        return false;
                    }
                    else
                    {
                        message = $"Credit must be greater than {creditCondition.Credit}";
                        //return studentInfo.AcademicInformation.CreditComp >= creditCondition.Credit;
                        return true;
                    }

                case "ability":
                    var ability = _db.AbilityConditions.AsNoTracking()
                                                       .SingleOrDefault(x => x.Id == conditionId);

                    if(ability == null)
                    {
                        message = "Ability condition not found.";
                        return false;
                    }
                    else 
                    {
                        var specializationGroup = _db.SpecializationGroups.AsNoTracking()
                                                                          .SingleOrDefault(x => x.Id == ability.AbilityId);

                        if(specializationGroup == null)
                        {
                            message = "Ability condition not found.";
                            return false;
                        }
                        else 
                        {
                            message = $"Student must have ability {specializationGroup.NameTh}";
                            return _db.SpecializationGroupInformations.AsNoTracking()
                                                                      .Include(x => x.CurriculumInformation)
                                                                      .Include(x => x.SpecializationGroup)
                                                                      .Any(x => x.CurriculumInformation.StudentId == studentInfo.Student.Id
                                                                                && x.SpecializationGroup.Id == specializationGroup.Id
                                                                                && x.IsActive);
                        }
                    }
                    

                case "term":
                    var termCondition = _db.TermConditions.AsNoTracking()
                                                          .SingleOrDefault(x => x.Id == conditionId);

                    if (termCondition == null)
                    {
                        message = "Term condition not found.";
                        return false;
                    }

                    message = $"Terms must be more than {termCondition.Term}";

                    var termIds = studentInfo.RegistrationCourses.Select(x => x.TermId)
                                                                 .Distinct()
                                                                 .ToList();
                    var terms = _db.Terms.AsNoTracking()
                                        .Include(x => x.TermType)
                                        .Where(x => termIds.Contains(x.Id))
                                        .ToList();

                    var validTerms = (from term in terms
                                      where !string.IsNullOrEmpty(term.TermType.Code)
                                            && string.Equals(term.TermType.Code, "n", StringComparison.InvariantCultureIgnoreCase)
                                      select term.Id)
                                      .Distinct()
                                      .Count();

                    return validTerms + 1 >= termCondition.Term;
                   

                case "gpa":
                    var gpaCondition = _db.GPAConditions.AsNoTracking()
                                                        .SingleOrDefault(x => x.Id == conditionId);
                    if (gpaCondition == null)
                    {
                        message = "GPA condition not found.";
                        return false;
                    }
                    else
                    {
                        message = $"GPA must be greater than {gpaCondition.GPA}";
                        return studentInfo.AcademicInformation.GPA >= gpaCondition.GPA;
                    }

                case "id":
                    message = "Condition condition not found.";
                    return false;

                case "corequisites":
                    var corequisiteCondition = _db.Corequisites.AsNoTracking()
                                                               .SingleOrDefault(x => x.Id == conditionId);
                    if (corequisiteCondition == null)
                    {
                        message = "Course corequisite condition not found.";
                        return false;
                    }
                    else
                    {
                        var firstCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.FirstCourseId);
                        var secondCourseRegistration = studentInfo.RegistrationCourses.FirstOrDefault(x => x.CourseId == corequisiteCondition.SecondCourseId);
                        if (firstCourseRegistration == null || secondCourseRegistration == null)
                        {
                            message = "Course registration not found.";
                            return false;
                        }
                        else
                        {
                            if (firstCourseRegistration.TermId == secondCourseRegistration.TermId)
                            {
                                message = string.Empty;
                                return true;
                            }
                            else
                            {
                                message = "CourseCorequisite";
                                return false;
                            }
                        }
                    }

                default:
                    break;
            }

            message = "Condition type not found.";
            return false;
        }

        public GradeCondition GetGradeCondition(long id)
        {
            var model = _db.GradeConditions.Include(x => x.Grade)
                                           .Include(x => x.Course)
                                           .IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public CreditCondition GetCreditCondition(long id)
        {
            var model = _db.CreditConditions.Include(x => x.CurriculumVersion)
                                                .ThenInclude(x => x.Curriculum)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model?.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0;
            model.CurriculumId = model?.CurriculumVersion?.CurriculumId ?? 0;

            return model;
        }

        public GPACondition GetGPACondition(long id)
        {
            var model = _db.GPAConditions.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public CourseGroupCondition GetCourseGroupCondition(long id)
        {
            var model = _db.CourseGroupConditions.Include(x => x.CourseGroup)
                                                     .ThenInclude(x => x.CurriculumVersion)
                                                     .ThenInclude(x => x.Curriculum)
                                                 .IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model?.CourseGroup?.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0;
            model.CurriculumId = model?.CourseGroup?.CurriculumVersion?.CurriculumId ?? 0;
            model.CurriculumVersionId = model?.CourseGroup?.CurriculumVersionId ?? 0;
            return model;
        }

        public Corequisite GetCorequisite(long id)
        {
            var corequisite = _db.Corequisites.IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return corequisite;
        }

        public TotalCourseGroupCondition GetTotalCourseGroupCondition(long id)
        {
            var model = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
                                                          .ThenInclude(x => x.Curriculum)
                                                      .IgnoreQueryFilters()
                                                      .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model?.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0;
            model.CurriculumId = model?.CurriculumVersion?.CurriculumId ?? 0;
            return model;
        }

        public TermCondition GetTermCondition(long id)
        {
            var model = _db.TermConditions.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public PredefinedCourse GetPredefinedCourse(long id)
        {
            var model = _db.PredefinedCourses.Include(x => x.CurriculumVersion)
                                                 .ThenInclude(x => x.Curriculum)
                                             .Include(x => x.FirstCourse)
                                             .Include(x => x.SecondCourse)
                                             .IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model.CurriculumVersion.Curriculum.AcademicLevelId;
            model.CurriculumId = model.CurriculumVersion.CurriculumId;
            return model;
        }

        public bool IsExistGradeCondition(GradeCondition grade)
        {
            var isExist = _db.GradeConditions.Any(x => x.GradeId == grade.GradeId
                                                       && x.CourseId == grade.CourseId
                                                       && x.Id != grade.Id);
            return isExist;
        }

        public bool IsExistBatchCondition(BatchCondition batch)
        {
            var isExist = _db.BatchConditions.Any(x => x.Batch == batch.Batch
                                                       && x.Id != batch.Id);
            return isExist;
        }

        public bool IsExistGPACondition(GPACondition gpa)
        {
            var isExist = _db.GPAConditions.Any(x => x.GPA == gpa.GPA
                                                     && x.Id != gpa.Id);
            return isExist;
        }

        public bool IsExistCourseGroupCondition(CourseGroupCondition courseGroup)
        {
            var isExist = _db.CourseGroupConditions.Any(x => x.CourseGroupId == courseGroup.CourseGroupId
                                                             && x.Credit == courseGroup.Credit
                                                             && x.Id != courseGroup.Id);
            return isExist;
        }

        public List<AndCondition> GetAndConditions()
        {
            var conditions = _db.AndConditions.Where(x => x.ExpiredAt == null
                                                          || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                              .ToList();
            return conditions;
        }

        public List<OrCondition> GetOrConditions()
        {
            var conditions = _db.OrConditions.Where(x => x.ExpiredAt == null
                                                         || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                             .ToList();
            return conditions;
        }

        public List<CourseGroupCondition> GetCourseGroupConditions()
        {
            var conditions = _db.CourseGroupConditions.Include(x => x.CourseGroup)
                                                      .Where(x => x.ExpiredAt == null
                                                                  || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                      .ToList();
            return conditions;
        }

        public List<CreditCondition> GetCreditConditions()
        {
            var conditions = _db.CreditConditions.Include(x => x.CurriculumVersion)
                                                 .Where(x => x.ExpiredAt == null
                                                             || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                 .ToList();
            return conditions;
        }

        public List<GPACondition> GetGPAConditions()
        {
            var conditions = _db.GPAConditions.Where(x => x.ExpiredAt == null
                                                          || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                              .ToList();
            return conditions;
        }

        public List<GradeCondition> GetGradeConditions()
        {
            var conditions = _db.GradeConditions.Include(x => x.Grade)
                                                .Include(x => x.Course)
                                                .Where(x => x.ExpiredAt == null
                                                            || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                .ToList();
            return conditions;
        }

        public List<TermCondition> GetTermConditions()
        {
            var conditions = _db.TermConditions.Where(x => x.ExpiredAt == null
                                                           || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                               .ToList();
            return conditions;
        }

        public List<TotalCourseGroupCondition> GetTotalCourseGroupConditions()
        {
            var conditions = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
                                                           .Where(x => x.ExpiredAt == null
                                                                       || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                           .ToList();
            return conditions;
        }
        
        public List<BatchCondition> GetBatchConditions()
        {
            var conditions = _db.BatchConditions.Where(x => x.ExpiredAt == null
                                                            || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                .ToList();
            return conditions;
        }

        public List<AbilityCondition> GetAbilityConditions()
        {
            var conditions = _db.AbilityConditions.Include(x => x.Ability)
                                                  .Where(x => x.ExpiredAt == null
                                                              || x.ExpiredAt.Value.Date >= DateTime.Today.Date)
                                                  .ToList();
            return conditions;
        }

        public AndCondition GetAndConditionById(long id)
        {
            var condition = _db.AndConditions.IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);
            return condition;
        }

        public OrCondition GetOrConditionById(long id)
        {
            var condition = _db.OrConditions.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return condition;
        }

        public BatchCondition GetBatchConditionById(long id)
        {
            var condition = _db.BatchConditions.IgnoreQueryFilters()
                                               .SingleOrDefault(x => x.Id == id);
            return condition;
        }

        public AbilityCondition GetAbilityConditionById(long id)
        {
            var condition = _db.AbilityConditions.IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);
            return condition;
        }

        public bool IsExistsPrerequisite(long courseId)
        {
            bool isExist = _db.Prerequisites.Any(x => x.CourseId == courseId);
            return isExist;
        }

        public bool IsExistCourseCorequisite(Corequisite corequisite)
        {
            var courseIds = new List<long> { corequisite.FirstCourseId, corequisite.SecondCourseId };
            var isExist = _db.Corequisites.Any(x => courseIds.Contains(x.FirstCourseId)
                                                    && courseIds.Contains(x.SecondCourseId)
                                                    && x.Id != corequisite.Id);
            return isExist;
        }

        public bool IsExistCreditCondition(CreditCondition creditCondition)
        {
            var isExist = _db.CreditConditions.Any(x => x.Credit == creditCondition.Credit
                                                        && x.TeachingTypeId == creditCondition.TeachingTypeId
                                                        && x.CurriculumVersionId == creditCondition.CurriculumVersionId
                                                        && x.CourseGroupId == creditCondition.CourseGroupId
                                                        && x.CreditType == creditCondition.CreditType
                                                        && x.Id != creditCondition.Id);
            return isExist;
        }

        public bool IsExistTotalCourseGroupCondition(TotalCourseGroupCondition totalCourseGroupCondition)
        {
            var isExist = _db.TotalCourseGroupConditions.Any(x => x.CurriculumVersionId == totalCourseGroupCondition.CurriculumVersionId
                                                                  && x.CourseGroupAmount == totalCourseGroupCondition.CourseGroupAmount
                                                                  && x.Id != totalCourseGroupCondition.Id);
            return isExist;
        }

        public bool IsExistTermCondition(TermCondition termCondition)
        {
            var isExist = _db.TermConditions.Any(x => x.Term == termCondition.Term
                                                      && x.Id != termCondition.Id);
            return isExist;
        }

        public bool IsExistAndCondition(AndCondition andCondition)
        {
            // paramenter type and paramenter condition == database type and database condition 
            var isExist = _db.AndConditions.Any(x => x.Id != andCondition.Id
                                                     && ((x.FirstConditionType == andCondition.FirstConditionType
                                                          && x.FirstConditionId == andCondition.FirstConditionId
                                                          && x.SecondConditionType == andCondition.SecondConditionType
                                                          && x.SecondConditionId == andCondition.SecondConditionId)
                                                        || (x.FirstConditionType == andCondition.SecondConditionType
                                                            && x.FirstConditionId == andCondition.SecondConditionId
                                                            && x.SecondConditionType == andCondition.FirstConditionType
                                                            && x.SecondConditionId == andCondition.FirstConditionId))
                                                     && (x.ExpiredAt == null
                                                         || x.ExpiredAt.Value.Date >= DateTime.Today.Date));
            return isExist;
        }

        public bool IsExistOrCondition(OrCondition orCondition)
        {
            var isExist = _db.AndConditions.Any(x => x.Id != orCondition.Id
                                                     && ((x.FirstConditionType == orCondition.FirstConditionType
                                                          && x.FirstConditionId == orCondition.FirstConditionId
                                                          && x.SecondConditionType == orCondition.SecondConditionType
                                                          && x.SecondConditionId == orCondition.SecondConditionId)
                                                        || (x.FirstConditionType == orCondition.SecondConditionType
                                                            && x.FirstConditionId == orCondition.SecondConditionId
                                                            && x.SecondConditionType == orCondition.FirstConditionType
                                                            && x.SecondConditionId == orCondition.FirstConditionId))
                                                     && (x.ExpiredAt == null
                                                         || x.ExpiredAt.Value.Date >= DateTime.Today.Date));
            return isExist;
        }

        public bool IsExistedPredefinedCourse(PredefinedCourse course)
        {
            var isExist = _db.PredefinedCourses.Any(x => x.Id != course.Id
                                                         && x.CurriculumVersionId == course.CurriculumVersionId
                                                         && ((x.FirstCourseId == course.FirstCourseId
                                                              && x.SecondCourseId == course.SecondCourseId)
                                                            || (x.FirstCourseId == course.SecondCourseId
                                                                && x.SecondCourseId == course.FirstCourseId)));

            return isExist;
        }

        public bool IsExistAbilityCondition(AbilityCondition ability)
        {
            var isExist = _db.AbilityConditions.Any(x => x.AbilityId == ability.AbilityId
                                                         && x.Id != ability.Id);
            return isExist;
        }

        public List<AndCondition> GetAndConditionNames(List<AndCondition> conditions)
        {
            var ands = _db.AndConditions.IgnoreQueryFilters();
            var courseGroups = _db.CourseGroupConditions.Include(x => x.CourseGroup)
                                                        .IgnoreQueryFilters();
            var credits = _db.CreditConditions.Include(x => x.CurriculumVersion)
                                              .IgnoreQueryFilters();
            var gpas = _db.GPAConditions.IgnoreQueryFilters();
            var grades = _db.GradeConditions.Include(x => x.Grade)
                                            .Include(x => x.Course)
                                            .IgnoreQueryFilters();
            var ors = _db.OrConditions.IgnoreQueryFilters();
            var terms = _db.TermConditions.IgnoreQueryFilters();
            var totalCourses = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
                                                             .IgnoreQueryFilters();
            var batches = _db.BatchConditions.IgnoreQueryFilters();
            var abilities = _db.AbilityConditions.Include(x => x.Ability)
                                                 .IgnoreQueryFilters();

            foreach (var item in conditions)
            {
                if (item.FirstConditionType == "and")
                {
                    item.FirstConditionName = ands.SingleOrDefault(x => x.Id == item.FirstConditionId)?.AndConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "or")
                {
                    item.FirstConditionName = ors.SingleOrDefault(x => x.Id == item.FirstConditionId)?.OrConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "coursegroup")
                {
                    item.FirstConditionName = courseGroups.SingleOrDefault(x => x.Id == item.FirstConditionId)?.CourseGroupConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "credit")
                {
                    item.FirstConditionName = credits.SingleOrDefault(x => x.Id == item.FirstConditionId)?.CreditConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "gpa")
                {
                    item.FirstConditionName = gpas.SingleOrDefault(x => x.Id == item.FirstConditionId)?.GPAConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "grade")
                {
                    item.FirstConditionName = grades.SingleOrDefault(x => x.Id == item.FirstConditionId)?.GradeConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "term")
                {
                    item.FirstConditionName = terms.SingleOrDefault(x => x.Id == item.FirstConditionId)?.TermConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "totalcoursegroup")
                {
                    item.FirstConditionName = totalCourses.SingleOrDefault(x => x.Id == item.FirstConditionId)?.TotalCourseGroupConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "batch")
                {
                    item.FirstConditionName = batches.SingleOrDefault(x => x.Id == item.FirstConditionId)?.Description ?? "N/A";
                }
                else if (item.FirstConditionType == "ability")
                {
                    item.FirstConditionName = abilities.SingleOrDefault(x => x.Id == item.FirstConditionId)?.Ability?.NameEn ?? "N/A";
                }

                if (item.SecondConditionType == "and")
                {
                    item.SecondConditionName = ands.SingleOrDefault(x => x.Id == item.SecondConditionId)?.AndConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "or")
                {
                    item.SecondConditionName = ors.SingleOrDefault(x => x.Id == item.SecondConditionId)?.OrConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "coursegroup")
                {
                    item.SecondConditionName = courseGroups.SingleOrDefault(x => x.Id == item.SecondConditionId)?.CourseGroupConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "credit")
                {
                    item.SecondConditionName = credits.SingleOrDefault(x => x.Id == item.SecondConditionId)?.CreditConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "gpa")
                {
                    item.SecondConditionName = gpas.SingleOrDefault(x => x.Id == item.SecondConditionId)?.GPAConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "grade")
                {
                    item.SecondConditionName = grades.SingleOrDefault(x => x.Id == item.SecondConditionId)?.GradeConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "term")
                {
                    item.SecondConditionName = terms.SingleOrDefault(x => x.Id == item.SecondConditionId)?.TermConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "totalcoursegroup")
                {
                    item.SecondConditionName = totalCourses.SingleOrDefault(x => x.Id == item.SecondConditionId)?.TotalCourseGroupConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "batch")
                {
                    item.SecondConditionName = batches.SingleOrDefault(x => x.Id == item.SecondConditionId)?.Description ?? "N/A";
                }
                else if (item.SecondConditionType == "ability")
                {
                    item.SecondConditionName = abilities.SingleOrDefault(x => x.Id == item.SecondConditionId)?.Ability?.NameEn ?? "N/A";
                }
            }

            return conditions;
        }

        public List<OrCondition> GetOrConditionNames(List<OrCondition> conditions)
        {
            var ands = _db.AndConditions.IgnoreQueryFilters();
            var courseGroups = _db.CourseGroupConditions.Include(x => x.CourseGroup)
                                                        .IgnoreQueryFilters();
            var credits = _db.CreditConditions.Include(x => x.CurriculumVersion)
                                              .IgnoreQueryFilters();
            var gpas = _db.GPAConditions.IgnoreQueryFilters();
            var grades = _db.GradeConditions.Include(x => x.Grade)
                                            .Include(x => x.Course)
                                            .IgnoreQueryFilters();
            var ors = _db.OrConditions.IgnoreQueryFilters();
            var terms = _db.TermConditions.IgnoreQueryFilters();
            var totalCourses = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
                                                             .IgnoreQueryFilters();
            var batches = _db.BatchConditions.IgnoreQueryFilters();
            var abilities = _db.AbilityConditions.Include(x => x.Ability)
                                                 .IgnoreQueryFilters();

            foreach (var item in conditions)
            {
                if (item.FirstConditionType == "and")
                {
                    item.FirstConditionName = ands.SingleOrDefault(x => x.Id == item.FirstConditionId)?.AndConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "or")
                {
                    item.FirstConditionName = ors.SingleOrDefault(x => x.Id == item.FirstConditionId)?.OrConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "coursegroup")
                {
                    item.FirstConditionName = courseGroups.SingleOrDefault(x => x.Id == item.FirstConditionId)?.CourseGroupConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "credit")
                {
                    item.FirstConditionName = credits.SingleOrDefault(x => x.Id == item.FirstConditionId)?.CreditConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "gpa")
                {
                    item.FirstConditionName = gpas.SingleOrDefault(x => x.Id == item.FirstConditionId)?.GPAConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "grade")
                {
                    item.FirstConditionName = grades.SingleOrDefault(x => x.Id == item.FirstConditionId)?.GradeConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "term")
                {
                    item.FirstConditionName = terms.SingleOrDefault(x => x.Id == item.FirstConditionId)?.TermConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "totalcoursegroup")
                {
                    item.FirstConditionName = totalCourses.SingleOrDefault(x => x.Id == item.FirstConditionId)?.TotalCourseGroupConditionName ?? "N/A";
                }
                else if (item.FirstConditionType == "batch")
                {
                    item.FirstConditionName = batches.SingleOrDefault(x => x.Id == item.FirstConditionId)?.Description ?? "N/A";
                }
                else if (item.FirstConditionType == "ability")
                {
                    item.FirstConditionName = abilities.SingleOrDefault(x => x.Id == item.FirstConditionId)?.Ability?.NameEn ?? "N/A";
                }

                if (item.SecondConditionType == "and")
                {
                    item.SecondConditionName = ands.SingleOrDefault(x => x.Id == item.SecondConditionId)?.AndConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "or")
                {
                    item.SecondConditionName = ors.SingleOrDefault(x => x.Id == item.SecondConditionId)?.OrConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "coursegroup")
                {
                    item.SecondConditionName = courseGroups.SingleOrDefault(x => x.Id == item.SecondConditionId)?.CourseGroupConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "credit")
                {
                    item.SecondConditionName = credits.SingleOrDefault(x => x.Id == item.SecondConditionId)?.CreditConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "gpa")
                {
                    item.SecondConditionName = gpas.SingleOrDefault(x => x.Id == item.SecondConditionId)?.GPAConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "grade")
                {
                    item.SecondConditionName = grades.SingleOrDefault(x => x.Id == item.SecondConditionId)?.GradeConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "term")
                {
                    item.SecondConditionName = terms.SingleOrDefault(x => x.Id == item.SecondConditionId)?.TermConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "totalcoursegroup")
                {
                    item.SecondConditionName = totalCourses.SingleOrDefault(x => x.Id == item.SecondConditionId)?.TotalCourseGroupConditionName ?? "N/A";
                }
                else if (item.SecondConditionType == "batch")
                {
                    item.SecondConditionName = batches.SingleOrDefault(x => x.Id == item.SecondConditionId)?.Description ?? "N/A";
                }
                else if (item.SecondConditionType == "ability")
                {
                    item.SecondConditionName = abilities.SingleOrDefault(x => x.Id == item.SecondConditionId)?.Ability?.NameEn ?? "N/A";
                }
            }

            return conditions;
        }

        public void GetPrerequisiteNames(ref List<Prerequisite> conditions)
        {
            var conditionTypes = conditions.Select(x => x.ConditionType).Distinct();
            foreach (var conditionType in conditionTypes)
            {
                var conditionByTypes = conditions.Where(x => x.ConditionType == conditionType);
                var conditionIds = conditionByTypes.Select(x => x.ConditionId);
                switch (conditionType)
                {
                    case "ability":
                        var abilityConditions = _db.AbilityConditions.Where(x => conditionIds.Contains(x.Id))
                                                                     .IgnoreQueryFilters()
                                                                     .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = abilityConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;
                    case "and":
                        var andConditions = _db.AndConditions.Where(x => conditionIds.Contains(x.Id))
                                                             .IgnoreQueryFilters()
                                                             .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = andConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;
                    case "or":
                        var orConditions = _db.OrConditions.Where(x => conditionIds.Contains(x.Id))
                                                           .IgnoreQueryFilters()
                                                           .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = orConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;
                    case "batch":
                        var batchConditions = _db.BatchConditions.Where(x => conditionIds.Contains(x.Id))
                                                                 .IgnoreQueryFilters()
                                                                 .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = batchConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;  
                    case "credit":
                        var creditConditions = _db.CreditConditions.Where(x => conditionIds.Contains(x.Id))
                                                                   .IgnoreQueryFilters()
                                                                   .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = creditConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;  
                    case "gpa":
                        var gpaConditions = _db.GPAConditions.Where(x => conditionIds.Contains(x.Id))
                                                             .IgnoreQueryFilters()
                                                             .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = gpaConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;  
                    case "grade":
                        var gradeConditions = _db.GradeConditions.Where(x => conditionIds.Contains(x.Id))
                                                                 .IgnoreQueryFilters()
                                                                 .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = gradeConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;   
                    case "coursegroup":
                        var coursegroupConditions = _db.CourseGroupConditions.Where(x => conditionIds.Contains(x.Id))
                                                                             .IgnoreQueryFilters()
                                                                             .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = coursegroupConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;    
                    case "term":
                        var termConditions = _db.TermConditions.Where(x => conditionIds.Contains(x.Id))
                                                               .IgnoreQueryFilters()
                                                               .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = termConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break;      
                    case "totalcoursegroup":
                        var totalCourseGroupConditions = _db.TotalCourseGroupConditions.Where(x => conditionIds.Contains(x.Id))
                                                                                       .IgnoreQueryFilters()
                                                                                       .AsNoTracking();
                        foreach (var conditionByType in conditionByTypes)
                        {      
                            conditionByType.PrerequisiteName = totalCourseGroupConditions.SingleOrDefault(x => x.Id == conditionByType.ConditionId)?.Description ?? "N/A";
                        }
                        break; 
                    default:
                        break;
                }
            }
            
           
            // var totalCourses = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
            //                                                  .IgnoreQueryFilters();
            // var batches = _db.BatchConditions.IgnoreQueryFilters();
            // var abilities = _db.AbilityConditions.Include(x => x.Ability)
            //                                      .IgnoreQueryFilters();

            // foreach (var item in conditions)
            // {
            //     if (item.ConditionType == "and")
            //     {
            //         item.PrerequisiteName = ands.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "or")
            //     {
            //         item.PrerequisiteName = ors.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "coursegroup")
            //     {
            //         item.PrerequisiteName = courseGroups.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "credit")
            //     {
            //         item.PrerequisiteName = credits.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "gpa")
            //     {
            //         item.PrerequisiteName = gpas.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "grade")
            //     {
            //         item.PrerequisiteName = grades.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "term")
            //     {
            //         item.PrerequisiteName = terms.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "totalcoursegroup")
            //     {
            //         item.PrerequisiteName = totalCourses.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "batch")
            //     {
            //         item.PrerequisiteName = batches.SingleOrDefault(x => x.Id == item.ConditionId)?.Description ?? "N/A";
            //     }
            //     else if (item.ConditionType == "ability")
            //     {
            //         item.PrerequisiteName = abilities.SingleOrDefault(x => x.Id == item.ConditionId)?.Ability?.NameEn ?? "N/A";
            //     }
            // }
        }

        public PrerequisiteFormViewModel GetPrerequisiteFormViewModel(long id, long? curriculumVersionId)
        {
            var prerequisite = _db.Prerequisites.Include(x => x.Course)
                                                .Include(x => x.CurriculumVersion)
                                                .Where(x => x.CourseId == id
                                                            && x.CurriculumVersionId == curriculumVersionId)
                                                .GroupBy(x => x.CourseId)
                                                .Select(x => new PrerequisiteFormViewModel
                                                {
                                                    CourseId = x.Key,
                                                    CourseName = x.FirstOrDefault().Course.CourseAndCredit,
                                                    CurriculumVersionId = x.FirstOrDefault().CurriculumVersionId ?? 0,
                                                    CurriculumVersionName = x.FirstOrDefault().CurriculumVersion.NameEn,
                                                    AndConditionIds = x.Where(y => y.ConditionType == "and")
                                                                                  .Select(y => y.ConditionId).ToList(),
                                                    OrConditionIds = x.Where(y => y.ConditionType == "or")
                                                                                 .Select(y => y.ConditionId).ToList(),
                                                    GradeConditionIds = x.Where(y => y.ConditionType == "grade")
                                                                                      .Select(y => y.ConditionId).ToList(),
                                                    CourseGroupConditionIds = x.Where(y => y.ConditionType == "coursegroup")
                                                                                            .Select(y => y.ConditionId).ToList(),
                                                    TermConditionIds = x.Where(y => y.ConditionType == "term")
                                                                                     .Select(y => y.ConditionId).ToList(),
                                                    GPAConditionIds = x.Where(y => y.ConditionType == "gpa")
                                                                                    .Select(y => y.ConditionId).ToList(),
                                                    CreditConditionIds = x.Where(y => y.ConditionType == "credit")
                                                                                       .Select(y => y.ConditionId).ToList(),
                                                    TotalCourseGroupConditionIds = x.Where(y => y.ConditionType == "totalcoursegroup")
                                                                                                 .Select(y => y.ConditionId).ToList(),
                                                    BatchConditionIds = x.Where(y => y.ConditionType == "batch")
                                                                         .Select(y => y.ConditionId).ToList(),
                                                    AbilityConditionIds = x.Where(y => y.ConditionType == "ability")
                                                                           .Select(y => y.ConditionId).ToList(),
                                                })
                                                .SingleOrDefault();

            return prerequisite;
        }

        public Prerequisite GetPrerequisite(long id)
        {
            var model = _db.Prerequisites.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
        public PrerequisiteGraphViewModel GetPrerequisiteGraph(long curriculumVersionId)
        {
            var graphSources = new List<string>();
            var result = new PrerequisiteGraphViewModel();
            result.GraphSource = @"digraph structs{
                bgcolor=""#6BBCFF""
                rankdir=LR";
            var courses = _db.CurriculumCourses.Include(x => x.CourseGroup)
                                               .Include(x => x.Course)
                                               .Where(x => x.CourseGroup.CurriculumVersionId == curriculumVersionId)
                                               .Select(x => x.Course)
                                               .Distinct()
                                               .OrderBy(x => x.Code)
                                               .ToList();
            var curriculumVersionPrerequisites = _db.Prerequisites.Where(x => x.CurriculumVersionId == curriculumVersionId && courses.Select(y => y.Id).Contains(x.CourseId)).ToList();
            foreach (var course in courses)
            {
                var prerequisites = curriculumVersionPrerequisites.Where(x => x.CourseId == course.Id).ToList();
                if (prerequisites.Any())
                {
                    var graphIdentifier = $"CRS{ course.Id.ToString(_graphIdentifierFormat) }";
                    graphSources.Add(CreateCourseGraphNode(graphIdentifier, course.Code, course.CreditText, course.NameEn));
                    
                    foreach (var prerequisite in prerequisites)
                    {
                        GetConditionStringForGraph(ref graphSources, prerequisite.ConditionType, prerequisite.ConditionId, graphIdentifier);
                    }
                }
            }

            foreach(var source in graphSources)
            {
                result.GraphSource += $@"
                    { source }";
            }
            result.GraphSource += @"
                }";
            return result;
        }

        public CheckPrerequisiteConditionViewModel IsConditionAlreadyExits(long id, string type)
        {
            var result = new CheckPrerequisiteConditionViewModel()
                         {
                            Type = type
                         };

            if(_db.Prerequisites.Any(x => x.ConditionId == id && type == x.ConditionType))
            {
                result.IsAlreadyExist = true;
                result.Messages.Add("Prerequisites");
            }

            if(_db.AndConditions.Any(x => x.FirstConditionId == id && type == x.FirstConditionType))
            {
                var andConditions = _db.AndConditions.Where(x => x.FirstConditionId == id && type == x.FirstConditionType)
                                                    .Select(x => x.Id.ToString()).ToList();

                var andConditionString = string.Join(",", andConditions);
                result.IsAlreadyExist = true;
                result.Messages.Add("AndCondition(First)" + "[" + andConditionString + "]");
            }

            if(_db.AndConditions.Any(x => x.SecondConditionId == id && type == x.SecondConditionType))
            {
                var andConditions = _db.AndConditions.Where(x => x.SecondConditionId == id && type == x.SecondConditionType)
                                                     .Select(x => x.Id.ToString())
                                                     .ToList();

                var andConditionString = string.Join(",", andConditions);
                result.IsAlreadyExist = true;
                result.Messages.Add("AndCondition(Second)" + "[" + andConditionString + "]");
            }

            if(_db.OrConditions.Any(x => x.FirstConditionId == id && type == x.FirstConditionType))
            {
                var orConditions = _db.OrConditions.Where(x => x.FirstConditionId == id && type == x.FirstConditionType)
                                                     .Select(x => x.Id.ToString())
                                                     .ToList();

                var orConditionsString = string.Join(",", orConditions);
                result.IsAlreadyExist = true;
                result.Messages.Add("OrCondition(First)" + "[" + orConditionsString + "]");
            }

            if(_db.OrConditions.Any(x => x.SecondConditionId == id && type == x.SecondConditionType))
            {
                var orConditions = _db.OrConditions.Where(x => x.SecondConditionId == id && type == x.SecondConditionType)
                                                   .Select(x => x.Id.ToString())
                                                   .ToList();

                var orConditionsString = string.Join(",", orConditions);
                result.IsAlreadyExist = true;
                result.Messages.Add("OrCondition(Second)" + "[" + orConditionsString + "]");
            }

            return result;
        }

        private string CreateCourseGraphNode(string identifier, string code, string credit, string name)
        {
            return $@"{ identifier } [label=""{ code } { credit }\n{ name }"",shape=box3d,style=filled,color=lightgreen]";
        }

        private void AddUniquelyToSource(ref List<string> graphSources, string source)
        {
            if (!graphSources.Contains(source))
            {
                graphSources.Add(source);
            }
        }

        private void GetConditionStringForGraph(ref List<string> graphSources, string conditionType, long conditionId, string currentCondition)
        {
            switch (conditionType.Trim())
            {
                case "and":
                    var andCondition = _db.AndConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (andCondition != null)
                    {
                        var graphIdentifier = $"AND{ andCondition.Id.ToString(_graphIdentifierFormat) }";

                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""&"",shape=circle,style=filled,color=""#FF9D4D""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                        GetConditionStringForGraph(ref graphSources, andCondition.FirstConditionType, andCondition.FirstConditionId, graphIdentifier);
                        GetConditionStringForGraph(ref graphSources, andCondition.SecondConditionType, andCondition.SecondConditionId, graphIdentifier);
                    }
                    return;

                case "or":
                    var orCondition = _db.OrConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (orCondition != null)
                    {
                        var graphIdentifier = $"ORR{ orCondition.Id.ToString(_graphIdentifierFormat) }";

                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""or"",shape=circle,style=filled,color=""#FFD3C4""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                        GetConditionStringForGraph(ref graphSources, orCondition.FirstConditionType, orCondition.FirstConditionId, graphIdentifier);
                        GetConditionStringForGraph(ref graphSources, orCondition.SecondConditionType, orCondition.SecondConditionId, graphIdentifier);
                    }
                    return;

                case "grade":
                    var gradeCondition = _db.GradeConditions.Include(x => x.Course)
                                                            .Include(x => x.Grade)
                                                            .SingleOrDefault(x => x.Id == conditionId);
                    if (gradeCondition != null)
                    {
                        var graphIdentifier = $"GRA{ gradeCondition.Id.ToString(_graphIdentifierFormat) }";

                        AddUniquelyToSource(ref graphSources, $@"{ CreateCourseGraphNode($"CRS{ gradeCondition.Course.Id.ToString(_graphIdentifierFormat) }", gradeCondition.Course.Code, gradeCondition.Course.CreditText, gradeCondition.Course.NameEn) }");
                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""{ gradeCondition.Grade.Name }"",shape=circle,style=filled,color=""#FFF380""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                        AddUniquelyToSource(ref graphSources, $@"CRS{ gradeCondition.Course.Id.ToString(_graphIdentifierFormat) } -> { graphIdentifier } [color=white]");
                    }
                    return;

                case "credit":
                    var creditCondition = _db.CreditConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (creditCondition != null)
                    {
                        var graphIdentifier = $"CRE{ creditCondition.Id.ToString(_graphIdentifierFormat) }";

                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""credit_{ creditCondition.CreditType } ({ creditCondition.Credit })"",style=filled,color=""#FFF380""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                    }
                    return;

                case "term":
                    var termCondition = _db.TermConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (termCondition != null)
                    {
                        var graphIdentifier = $"TER{ termCondition.Id.ToString(_graphIdentifierFormat) }";

                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""Term >= { termCondition.Term }"",style=filled,color=""#FFF380""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                    }
                    return;

                case "gpa":
                    var gpaCondition = _db.GPAConditions.SingleOrDefault(x => x.Id == conditionId);
                    if (gpaCondition != null)
                    {
                        var graphIdentifier = $"GPA{ gpaCondition.Id.ToString(_graphIdentifierFormat) }";
                        
                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""GPA >= { gpaCondition.GPA }"",style=filled,color=""#FFF380""]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                    }
                    return;

                case "ability":
                    var abilityCondition = _db.AbilityConditions.Include(x => x.Ability)
                                                                .SingleOrDefault(x => x.Id == conditionId);
                    if (abilityCondition != null)
                    {
                        var graphIdentifier = $"ABI{ abilityCondition.Id.ToString(_graphIdentifierFormat) }";
                        
                        AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } [label=""({ abilityCondition.Ability.NameEn })"",shape=component,style=filled,color=white]");
                        if(currentCondition.Contains("CRS"))
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=""#FF6B6B""]");
                        } else 
                        {
                            AddUniquelyToSource(ref graphSources, $@"{ graphIdentifier } -> { currentCondition } [color=white]");
                        }
                    }
                    return;

                default:
                    break;
            }
            return;
        }
    }
}