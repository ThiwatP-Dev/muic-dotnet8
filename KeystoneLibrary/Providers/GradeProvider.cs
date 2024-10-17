using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class GradeProvider : BaseProvider, IGradeProvider
    {
        public const string GradeAll = "All";
        public const string GradeNotReleased = "Not Released";
        protected IRegistrationProvider _registrationProvider { get; }
        protected ICalculationProvider _calculationProvider { get; }
        protected IAcademicProvider _academicProvider { get; }
        protected IInstructorProvider _instructorProvider { get; }
        protected IStudentProvider _studentProvider { get; }
        protected IDateTimeProvider _dateTimeProvider { get; }

        public GradeProvider(ApplicationDbContext db,
                             IMapper mapper,
                             IRegistrationProvider registrationProvider,
                             ICalculationProvider calculationProvider,
                             IAcademicProvider academicProvider,
                             IInstructorProvider instructorProvider,
                             IDateTimeProvider dateTimeProvider,
                             IStudentProvider studentProvider) : base(db, mapper)
        {
            _registrationProvider = registrationProvider;
            _calculationProvider = calculationProvider;
            _academicProvider = academicProvider;
            _instructorProvider = instructorProvider;
            _studentProvider = studentProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public List<ExaminationType> GetExaminationTypes()
        {
            var types = _db.ExaminationTypes.OrderBy(x => x.NameEn)
                                            .ToList();
            return types;

        }

        public List<string> GetGradeOptions()
        {
            List<string> gradeOptions = new List<string>();
            var grades = _db.Grades.IgnoreQueryFilters()
                                   .Select(x => x.Name)
                                   .ToList();

            gradeOptions.Add(GradeNotReleased);
            gradeOptions.Add(GradeAll);
            gradeOptions.AddRange(grades);
            return gradeOptions;
        }

        public List<Grade> GetGrades()
        {
            var grades = _db.Grades.OrderBy(x => x.Name)
                                   .ToList();
            return grades;
        }

        public List<GradingLog> GetGradingLogsByRegistrationCourseId(long Id)
        {
            var gradingLogs = _db.GradingLogs.Include(x => x.RegistrationCourse)
                                                 .ThenInclude(x => x.Student)
                                             .Include(x => x.RegistrationCourse)
                                                 .ThenInclude(x => x.Course)
                                             .Include(x => x.RegistrationCourse)
                                                 .ThenInclude(x => x.Section)
                                             .Where(x => x.RegistrationCourse.Id == Id)
                                             .OrderBy(x => x.UpdatedAt)
                                             .ToList();
            
            return gradingLogs;
        }

        public List<Grade> GetWeightGrades()
        {
            var grades = _db.Grades.Where(x => x.Weight != null)
                                   .OrderBy(x => x.Name)
                                   .ToList();
            return grades;
        }

        public List<Grade> GetNonWeightGrades()
        {
            var grades = _db.Grades.Where(x => x.Weight == null)
                                   .OrderBy(x => x.Name)
                                   .ToList();
            return grades;
        }

        public Grade GetGradeById(long Id)
        {
            var grade = _db.Grades.SingleOrDefault(x => x.Id == Id);

            return grade;
        }

        public List<long> GetCoordinatorCourse(long instructorId, long termId)
        {
            var coordinatorCourses = _db.Sections.Where(x => x.MainInstructorId == instructorId
                                                             && x.TermId == termId)
                                                 .Select(x => x.CourseId)
                                                 .ToList();

            return coordinatorCourses;
        }

        public List<Section> GetSectionByInstructorId(long instructorId, long courseId, long termId)
        {
            var masterSections = _db.Sections.Include(x => x.Course)
                                             .Include(x => x.RegistrationCourses)
                                                .ThenInclude(x => x.Withdrawals)
                                             .Where(x => x.MainInstructorId == instructorId
                                                         && x.CourseId == courseId
                                                         && x.TermId == termId
                                                         && x.ParentSectionId == null)
                                             .OrderBy(x => x.Course.Code)
                                                .ThenBy(x => x.Number)
                                             .ToList();

            foreach (var section in masterSections)
            {
                section.ChildrenSections = _db.Sections.Include(x => x.Course)
                                                       .Include(x => x.RegistrationCourses)
                                                            .ThenInclude(x => x.Withdrawals)
                                                       .Where(x => x.ParentSectionId.Value == section.Id)
                                                       .OrderBy(x => x.Course.Code)
                                                            .ThenBy(x => x.Number)
                                                       .ToList();
            }
            return masterSections;
        }

        public bool CheckCourseHaveCoordinator(long termId, long courseId)
        {
            var checkCourseHaveCoordinator = _db.Coordinators.Any(x => x.TermId == termId
                                                                       && x.CourseId == courseId);
            return checkCourseHaveCoordinator;
        }

        public Grade GetGradeByName(string name)
        {
            name = string.IsNullOrEmpty(name) ? name : name.ToUpper();
            var grade = _db.Grades.SingleOrDefault(x => x.Name.ToUpper() == name);

            return grade;
        }

        public List<GradeSection> GetGradeSectionsByCourseIds(List<long> courseIds)
        {
            var sections = _db.Sections.Include(x => x.Course)
                                       .Include(x => x.RegistrationCourses)
                                           .ThenInclude(x => x.Withdrawals)
                                       .Where(x => courseIds.Contains(x.CourseId))
                                       .Select(x => _mapper.Map<Section, GradeSection>(x))
                                       .OrderBy(x => x.CourseCode)
                                           .ThenBy(x => x.SectionNumber)
                                       .ToList();
            return sections;
        }

        public List<Coordinator> GetCoordinators(long courseId, long termId)
        {
            var coordinators = _db.Coordinators.Where(x => x.TermId == termId
                                                           && x.CourseId == courseId)
                                               .ToList();

            return coordinators;
        }

        public List<StudentGradeRecord> GetStudentScoresByBarcodeId(long barcodeId)
        {
            var rawScores = _db.StudentRawScores.AsNoTracking()
                                                .Include(x => x.Section)
                                                .Where(x => x.BarcodeId == barcodeId 
                                                            && !x.IsSkipGrading
                                                            && x.RegistrationCourse.IsPaid)
                                                .Select(x => new StudentGradeRecord
                                                             {
                                                                 StudentCode = x.Student.Code,
                                                                 StudentName = x.Student.FullNameEn,
                                                                 SectionNumber = x.Section.Number,
                                                                 CourseCode = x.Course.Code,
                                                                 Grade = x.Grade.Name,
                                                                 TotalScore = x.TotalScore,
                                                                 // SectionType = x.Section.SectionTypes,
                                                                 IsPublished = x.RegistrationCourse.IsGradePublished
                                                             })
                                                .OrderBy(x => x.CourseCode)
                                                .ThenBy(x => x.StudentCode)
                                                .ToList();
            return rawScores;
        }

        public GradingLog GetLatestGradingLog(long studentRawScoreId)
        {
            return _db.GradingLogs.Where(x => x.StudentRawScoreId == studentRawScoreId)
                                  .OrderByDescending(x => x.UpdatedAt)
                                  .FirstOrDefault();
        }

        public List<GradingCurve> GetGradingCurveByCourseIdAndTermId(long courseId, long termId)
        {
            var gradeCurves = new List<GradingCurve>();
            if(!_db.GradingCurves.Any(x => x.TermId == termId
                                          && x.CourseId == courseId))
            {
                gradeCurves = _db.StandardGradingScores.Include(x => x.StandardGradingGroup)
                                                       .Include(x => x.Grade)
                                                       .AsNoTracking()
                                                       .Select(x => new GradingCurve
                                                                    {
                                                                        CourseId = courseId,
                                                                        TermId = termId,
                                                                        GradeTemplateId = x.StandardGradingGroup.GradeTemplateId,
                                                                        GradeId = x.GradeId,
                                                                        Minimum = x.Minimum,
                                                                        Maximum = x.Maximum
                                                                    })
                                                       .ToList();
                _db.GradingCurves.AddRange(gradeCurves);
                _db.SaveChanges();
            }

            gradeCurves = _db.GradingCurves.Include(x => x.Grade)
                                           .Where(x => x.TermId == termId 
                                                       && x.CourseId == courseId)
                                           .AsNoTracking()
                                           .ToList();
            return gradeCurves;
        }

        public List<GradingRange> GetSummaryGradingCurves(long courseId, long termId, List<StudentScoreAllocation> students)
        {
            // item.Frequency = studentScoreAllocations.Count(y => y.Grade == item.Grade);
            // item.TotalPercentage = (item.Frequency * 100) / totalStudent;
            // currentTotal = item.TotalPercentage;
            // if (previousTotal == 100)
            // {
            //     previousTotal = 0;
            // }
                
            // item.CumulativePercentage = previousTotal + currentTotal
            // previousTotal = item.CumulativePercentage;
            // currentTotal = 0
            // item.GradePercentage = totalStudent - totalNotPassStudent == 0
            //                        ? 0 : ((item.Frequency * index) / (totalStudent - totalNotPassStudent)) / 100;
            var gradingRanges = new List<GradingRange>();
            var gradingScore = GetGradingCurveByCourseIdAndTermId(courseId, termId);
            if (gradingScore != null)
            {
                var grade = _db.Grades.Where(x => students.Any(y => y.GradeId == x.Id) && !gradingScore.Any(y => y.GradeId == x.Id))
                                      .Select(x => new GradingRange
                                                  {
                                                      IsCalculated = x.IsCalculateGPA,
                                                      MaxRange = 0,
                                                      MinRange = null,
                                                      Grade = x.Name
                                                  }).ToList();
                gradingRanges = gradingScore.Select(x => new GradingRange
                                                         {
                                                             IsCalculated = x.Grade.IsCalculateGPA,
                                                             MaxRange = x.Maximum,
                                                             MinRange = x.Minimum,
                                                             Grade = x.Grade.Name,
                                                             GradeTemplateId = x.GradeTemplateId
                                                         })
                                            .ToList();
                gradingRanges.AddRange(grade);
                gradingRanges = gradingRanges.OrderByDescending(x => x.IsCalculated).ThenByDescending(x => x.MaxRange).ToList();
                var totalStudent = students.Count();
                decimal previousTotal = 0;
                gradingRanges.Select(x =>
                {
                    x.Frequency = students.Count(y => y.Grade == x.Grade);
                    x.TotalPercentage = totalStudent == 0 ? 0 : (x.Frequency * 100) / totalStudent;
                    var currentTotal = x.TotalPercentage;
                    previousTotal = previousTotal + currentTotal;
                    x.CumulativePercentage = previousTotal;
                    return x;
                }).ToList();
            }

            return gradingRanges;
        }

        public GradingLog SetGradingLog(long? studentRawScoreId, long registrationCourseId, string previousGrade, 
                                        string currentGrade, string userId, string remark, string type)
        {
            var grade = new GradingLog 
                        {
                            StudentRawScoreId = studentRawScoreId == 0 ? null : studentRawScoreId,
                            RegistrationCourseId = registrationCourseId,
                            PreviousGrade = previousGrade,
                            CurrentGrade = currentGrade ?? "",
                            UpdatedBy = userId ?? "N/A",
                            UpdatedAt = DateTime.UtcNow,
                            ApprovedBy = userId ?? "N/A",
                            ApprovedAt = DateTime.UtcNow,
                            Remark = remark,
                            Type = type
                        };
                        
            return grade;
        }

        public List<Grade> GetUncalculatedGrade()
        {
            var grades = _db.Grades.Where(x => !x.IsCalculateGPA)
                                   .ToList();
            return grades;
        }

        public List<Grade> GetShowTranscriptGrade()
        {
            var grades = _db.Grades.Where(x => x.IsShowInTranscript)
                                   .OrderByDescending(x => x.Weight)
                                       .ThenBy(x => x.Name)
                                   .ToList();
            return grades;
        }

        public List<StandardGradingGroup> GetStandardGradingGroups(List<long> ids)
        {
            return _db.StandardGradingGroups.Include(x => x.StandardGradingScores)
                                                .ThenInclude(x => x.Grade)
                                            .Where(x => ids.Contains(x.Id))
                                            .ToList();
        }
        
        #region Grade Template
        public GradeTemplate AddGradeTemplate(GradeTemplate template)
        {
            _db.GradeTemplates.Add(template);
            _db.SaveChanges();

            return template;
        }

        public GradeTemplate DeleteGradeTemplate(long id)
        {
            var template = FindGradeTemplate(id);
            _db.GradeTemplates.Remove(template);
            _db.SaveChanges();

            return template;
        }

        public GradeTemplate FindGradeTemplate(long id)
        {
            var template = _db.GradeTemplates.IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);
            MapGradeIds(template);
            return template;
        }

        public IEnumerable<GradeTemplate> GetTemplates()
        {
            var templates = _db.GradeTemplates.IgnoreQueryFilters()
                                              .ToList();

            // Map GradeIds in json string to List<Grade>
            MapGradeIds(templates.ToArray());

            return templates;
        }

        public GradeTemplate UpdateGradeTemplate(GradeTemplate template)
        {
            _db.Entry(template).State = EntityState.Modified;
            _db.SaveChanges();
            return template;
        }

        private IEnumerable<GradeTemplate> MapGradeIds(params GradeTemplate[] templates)
        {
            var grades = GetGrades();
            templates.ToList().ForEach(x =>
            {
                if (x == null)
                {
                    return;
                }

                var gradesIds = JsonConvert.DeserializeObject<List<long>>(x.GradeIds);
                x.Grades = gradesIds.Join(grades, y => y, z => z.Id, (y, z) => z)
                                    .ToList();
            });

            return templates;
        }

        public GradeTemplate GetGradeTemplateByCourses(List<long> courseIds)
        {
            var gradeTemplate = _db.Courses.Include(x => x.GradeTemplate)
                                           .Where(x => courseIds.Contains(x.Id))
                                           .Select(x => x.GradeTemplate)
                                           .Distinct()
                                           .SingleOrDefault();

            if (gradeTemplate != null)
            {
                return gradeTemplate;
            }

            return null;
        }

        public List<Grade> GetGradesByGradeTemplate(GradeTemplate gradeTemplate)
        {
            var gradeIds = JsonConvert.DeserializeObject<List<long>>(gradeTemplate.GradeIds);
            var grades = _db.Grades.Where(x => gradeIds.Contains(x.Id))
                                   .OrderByDescending(x => x.Weight)
                                   .ToList();
            return grades;
        }

        public List<GradingFrequency> GetGradingFrequencies(List<StudentScoreAllocation> studentScoreAllocations)
        {
            var gradeFrequencies = new List<GradingFrequency>();
            for (var i = 0; i <= 100; i++)
            {
                gradeFrequencies.Add(new GradingFrequency
                                     {
                                         Score = i
                                     });
            }

            foreach(var item in studentScoreAllocations)
            {
                foreach(var gradeFrequency in gradeFrequencies)
                {
                    if (Math.Floor(item.Round) == gradeFrequency.Score)
                    {
                        gradeFrequency.Frequency = gradeFrequency.Frequency + 1;
                    }
                }
            }

            gradeFrequencies.Select(x => {
                                             if(studentScoreAllocations.Count() == 0)
                                             {
                                                x.TotalPercentage = 0;
                                             }
                                             else
                                             {
                                                x.TotalPercentage = (x.Frequency * 100) / studentScoreAllocations.Count();
                                             }
                                             return x;
                                         })
                            .ToList();

            return gradeFrequencies;
        }
        public ClassStatistics GetClassStatisticsGradeScoreSummary(List<StudentScoreAllocation> studentScoreAllocations)
        {
            var classRoundedScores = studentScoreAllocations.Select(x => x.TotalScore ?? 0)
                                                            .OrderBy(x => x)
                                                            .ToList();

            decimal creditSum = 0;
            studentScoreAllocations.Select(x => {
                                          creditSum += x.GradeWeight * x.CourseCredit;
                                          return x;
                                      })
                         .ToList();

            var sumScore = studentScoreAllocations.Sum(x => x.CourseCredit);
            var gpa = sumScore == 0 ? 0 : creditSum / sumScore;
            double mean = 0.0;
            double sd = 0.0;
            int index = 0;

            foreach (double value in classRoundedScores) 
            {
                index++;
                double meanTemp = mean;
                mean += (value - meanTemp) / index;
                sd += (value - meanTemp) * (value - mean);
            }
            var classStatistics = new ClassStatistics();
            if(index > 0)
            {
                var median = index % 2 != 0 ? classRoundedScores[index/2] : (classRoundedScores[(index/2) - 1] + classRoundedScores[index/2]) / 2;

                classStatistics.GPA = gpa.ToString(StringFormat.TwoDecimal);
                classStatistics.Median = median;
                if(index == 1)
                {
                    classStatistics.SD = 0;
                }
                else
                {
                    var deviation = (decimal)Math.Sqrt(sd / (index - 1));
                    classStatistics.SD = deviation;
                }
            }
            if (classRoundedScores.Any())
            {
                classStatistics.Min = classRoundedScores.Min();
                classStatistics.Mean = classRoundedScores.Count() == 0 ? 0 : classRoundedScores.Sum() / classRoundedScores.Count();
                classStatistics.Max = classRoundedScores.Max();
            }

            return classStatistics;
        }

        public List<GradeNormalCurve> GetGradeNormalCurves(ClassStatistics classStatistics)
        {
            var normalCurves = new List<GradeNormalCurve>();
            normalCurves.Add(new GradeNormalCurve
                             {
                                 Grade = "F",
                                 CurvePercentage = classStatistics.Mean - (2 * classStatistics.SD)
                             });

            normalCurves.Add(new GradeNormalCurve
                             {
                                 Grade = "D",
                                 CurvePercentage = classStatistics.Mean - classStatistics.SD
                             });

            normalCurves.Add(new GradeNormalCurve
                             {
                                 Grade = "C",
                                 CurvePercentage = classStatistics.Mean + classStatistics.SD
                             });
            
            normalCurves.Add(new GradeNormalCurve
                             {
                                 Grade = "B",
                                 CurvePercentage = classStatistics.Mean + (2 * classStatistics.SD)
                             });

            normalCurves.Add(new GradeNormalCurve
                             {
                                 Grade = "A",
                                 CurvePercentage = 100
                             });

            return normalCurves;
        }

        public GradingReportViewModel GetGradeScoreSummaryViewModelByCourseId(long courseId, long termId, long instructorId, bool isGradeMember = false)
        {

            var allocation = _db.MarkAllocations.Where(x => x.CourseId == courseId && x.TermId == termId)
                                                .OrderBy(x => x.Sequence).ToList();

            var studentRawScoreDetails = allocation.Select(x => new StudentRawScoreDetail
                                                                {  
                                                                    MarkAllocationId = x.Id,
                                                                    MarkAllocation = x
                                                                })
                                                   .ToList(); 
            var course = _registrationProvider.GetCourse(courseId);

            var sections = _db.Sections.Where(x => x.CourseId == courseId
                                                   && (x.ParentSectionId == null 
                                                       || x.ParentSectionId == 0)
                                                   && termId == x.TermId 
                                                   && (x.MainInstructorId == instructorId || isGradeMember))
                                       .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        SeatUsed = x.SeatUsed,
                                                        SectionNumber = x.Number,
                                                        CourseName = course.NameEn,
                                                        CourseCode = course.CodeAndSpecialChar,
                                                        MainInstructorId = x.MainInstructorId
                                                    })
                                       .OrderBy(x => x.SectionNumber)
                                       .ToList();
            var mainInstructorIds = sections.Select(x => x.MainInstructorId).ToList();
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Where(x => mainInstructorIds.Contains(x.Id))
                                             .AsNoTracking()
                                             .ToList();

            var sectionIdsNullable = sections.Select(x => (long?)x.SectionId).ToList();

            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new 
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            sectionIdsNullable.AddRange(jointSections.Select(x => (long?)x.Id).ToList());

            // var gradeSkip = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
            //                                       || x.Name.ToUpper() == "AU" )
            //                           .Select(x => (long?)x.Id)
            //                           .ToList();
            // var studentResultRawScore = GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails).Where(x => x.Id != 0 && !(x.IsSkipGrading || x.IsWithdrawal ||x.IsGradePublish || x.TotalScore == null || gradeSkip.Contains(x.GradeId))).ToList();
            var studentResultRawScore = GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails).Where(x => x.Id != 0 && !(x.IsWithdrawal)).ToList();
            
            var studentScoreAllocations = studentResultRawScore.Select(x => new StudentScoreAllocation
                                                                            {
                                                                              StudentId = x.StudentId,
                                                                              Section = x.SectionNumber,
                                                                              SectionId = x.SectionId,
                                                                              CourseId = x.CourseId,
                                                                              CourseCode = x.CourseCode,
                                                                              StudentCode = x.StudentCode,
                                                                              StudentTitle = x.StudentTitle,
                                                                              StudentFirstName = x.StudentFirstNameEn,
                                                                              StudentMidName = x.StudentMidNameEn,
                                                                              StudentLastName = x.StudentLastNameEn,
                                                                              Grade = x.GradeName ?? "",
                                                                              Round = x.Percentage ?? 0,
                                                                              CourseCredit = x.Credit,
                                                                              GradeWeight = x.GradeWeight,
                                                                              GradeId = x.GradeId,
                                                                              IsCalcGrade = x.IsCalcGrade,
                                                                              IsGradePublish = x.IsGradePublish,
                                                                              GradeTemplateId = x.GradeTemplateId,
                                                                              AllocationTotal = x.TotalScore.ToString() ?? "",
                                                                              TotalScore = x.TotalScore,
                                                                              Allocations = x.StudentRawScoreDetails.Select(y => new Allocation
                                                                                                                                 {
                                                                                                                                     Type = y.MarkAllocation?.Name,
                                                                                                                                     Abbreviation = y.MarkAllocation?.Abbreviation,
                                                                                                                                     FullScore = y.Score
                                                                                                                                 }).ToList(),
                                                                              Major = x.DepartmentCode
                                                                            })
                                                               .ToList();

            var sectionIds = studentScoreAllocations.Select(x => x.SectionId).Distinct().ToList();
            var studentSectionAllocations = _db.Sections.Include(x => x.Course).Where(x => sectionIds.Contains(x.Id))
                                                        .Select(x => new StudentSectionAllocation
                                                                     {
                                                                         CourseId = x.CourseId,
                                                                         SectionId = x.Id,
                                                                         Section = x.Number,
                                                                         Course = x.Course.CodeAndSpecialChar,
                                                                         Credit = x.Course.CreditText,
                                                                         SectionType = x.ParentSectionId == null ? "Master" : "Joint"
                                                                     })
                                                        .OrderBy(x => x.Section)
                                                            .ThenByDescending(x => x.SectionType)
                                                            .ThenBy(x => x.Course)
                                                        .ToList();

            var term = _academicProvider.GetTerm(termId);

            var sectionMasterIds = sections.Select(x => x.SectionId).ToList();
            var classStatistics = GetClassStatisticsGradeScoreSummary(studentScoreAllocations);
            var barcode = _db.Barcodes.Where(x => sectionMasterIds.Contains(x.SectionId)).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            var sectionMarkAllocations = new List<SectionMarkAllocationsViewModel>();

            // GRADING CURVE
            var gradeCurves = GetGradingCurveByCourseIdAndTermId(courseId, termId);
            
            var markAllocations = allocation.Select(x => new Allocation
                                                         {
                                                             Id = x.Id,
                                                             Type = x.Name,
                                                             Abbreviation = x.Abbreviation,
                                                             FullScore = x.Score
                                                         })
                                            .ToList();
            foreach(var item in sections)
            {
                var jointSectionIds = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                   .Select(x => x.Id)
                                                   .ToList(); 
                var students = studentScoreAllocations.Where(x => x.SectionId == item.SectionId)
                                                      .ToList();

                var studentJointSections = studentScoreAllocations.Where(x => jointSectionIds.Contains(x.SectionId))
                                                                  .OrderBy(x => x.CourseCode)
                                                                  .ToList();
 
                students.AddRange(studentJointSections);

                var instructorsName = _db.SectionSlots.Where(x => x.SectionId == item.SectionId)
                                                 .Select(x => x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + " " + x.Instructor.LastNameEn)
                                                 .GroupBy(x => x)
                                                 .Select(x => x.First())
                                                 .ToList();
                if(students.Any())
                {
                    sectionMarkAllocations.Add(new SectionMarkAllocationsViewModel
                                            {
                                                StudentScoreAllocations = students,
                                                MarkAllocaiton = markAllocations,
                                                InstructorFullNameEns = instructorsName.Any() ? string.Join(", ",instructorsName) : " - ",
                                                SectionAllocations = studentSectionAllocations.Where(x => x.SectionId == item.SectionId || jointSectionIds.Contains(x.SectionId))
                                                                                                    .OrderByDescending(x => x.SectionType)
                                                                                                        .ThenBy(x => x.Course)
                                                                                                .ToList()
                                            });
                }
            }

            var reportModel = new GradingReportViewModel
                              {
                                  PrintedAt = _dateTimeProvider.ConvertFromUtcToSE(DateTime.UtcNow)?.ToString(StringFormat.ShortDate),
                                  BarcodeNumber = barcode == null ? "N/A" : barcode.BarcodeNumber,
                                  Faculty = course.Faculty == null ? "-" : course.Faculty.NameEn,
                                  Department = course.Department == null ? "-" : course.Department.NameEn,
                                  Semester = term.TermText,
                                  AcademicYear = term.AcademicYear.ToString(),
                                  AcademicTerm = term.AcademicTerm.ToString(),
                                  AcademicLevelId = term.AcademicLevelId,
                                  TermId = term.Id,
                                  CourseId = course.Id,
                                  Course = course.CodeAndName,
                                  CourseCreitText = course.CreditText,
                                  Sections = sections.Select(x => x.SectionNumber).ToList(),
                                  Instructors = instructors,
                                  ClassStatistics = classStatistics,
                                  GradingCurves = gradeCurves.Where(x => x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                  GradingCurvesNotCalc = gradeCurves.Where(x => !x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                  IsCalGradeExist = studentScoreAllocations.Any(x => x.IsCalcGrade),
                                  IsNotCalGradeExist = studentScoreAllocations.Any(x => !x.IsCalcGrade),
                                  GradeNormalCurves = GetGradeNormalCurves(classStatistics),
                                  GradingRanges = GetSummaryGradingCurves(courseId, termId, studentScoreAllocations).ToList(),
                                  GradingFrequencies = GetGradingFrequencies(studentScoreAllocations),
                                  MarkAllocaiton = markAllocations,
                                  StudentScoreAllocations = studentScoreAllocations,
                                  SectionMarkAllocations = sectionMarkAllocations,
                                  StudentRawScores = studentResultRawScore
                              };

            return reportModel;
        }

        public GradingReportViewModel GetGradeScoreSummaryViewModelByCourseIdForReport(long courseId, long termId, long instructorId, bool isGradeMember = false)
        {
            var allocation = _db.MarkAllocations.Where(x => x.CourseId == courseId && x.TermId == termId)
                                                .OrderBy(x => x.Sequence).ToList();

            var studentRawScoreDetails = allocation.Select(x => new StudentRawScoreDetail
                                                                {  
                                                                    MarkAllocationId = x.Id,
                                                                    MarkAllocation = x
                                                                })
                                                   .ToList(); 
            var course = _registrationProvider.GetCourse(courseId);

            var sections = _db.Sections.Where(x => x.CourseId == courseId
                                                   && (x.ParentSectionId == null 
                                                       || x.ParentSectionId == 0)
                                                   && termId == x.TermId 
                                                   && (x.MainInstructorId == instructorId || isGradeMember))
                                       .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        SeatUsed = x.SeatUsed,
                                                        SectionNumber = x.Number,
                                                        CourseName = course.NameEn,
                                                        CourseCode = course.CodeAndSpecialChar,
                                                        MainInstructorId = x.MainInstructorId
                                                    })
                                       .OrderBy(x => x.SectionNumber)
                                       .ToList();

            var sectionIdsNullable = sections.Select(x => (long?)x.SectionId).ToList();
            var mainInstructorIds = sections.Select(x => x.MainInstructorId).ToList();
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Where(x => mainInstructorIds.Contains(x.Id))
                                             .AsNoTracking()
                                             .ToList();

            var latestBarcode = _db.Barcodes.AsNoTracking()
                                            .Where(x => sectionIdsNullable.Contains(x.SectionId))
                                            .OrderByDescending(x => x.CreatedAt)
                                            .FirstOrDefault();
            
            var activeSectionIdsInBarcode = _db.Barcodes.AsNoTracking()
                                                        .Where(x => x.BarcodeNumber == latestBarcode.BarcodeNumber)
                                                        .Select(x => (long?)x.SectionId).ToList();
            
            sectionIdsNullable = activeSectionIdsInBarcode;

            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new 
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            sectionIdsNullable.AddRange(jointSections.Select(x => (long?)x.Id).ToList());

            // var gradeSkip = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
            //                                       || x.Name.ToUpper() == "AU" )
            //                           .Select(x => (long?)x.Id)
            //                           .ToList();
            // var studentResultRawScore = GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails).Where(x => x.Id != 0 && !(x.IsSkipGrading || x.IsWithdrawal ||x.IsGradePublish || x.TotalScore == null || gradeSkip.Contains(x.GradeId))).ToList();
            var studentResultRawScore = GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails).Where(x => x.Id != 0 && !(x.IsWithdrawal)).ToList();
            
            var studentScoreAllocations = studentResultRawScore.Select(x => new StudentScoreAllocation
                                                                            {
                                                                              StudentId = x.StudentId,
                                                                              Section = x.SectionNumber,
                                                                              SectionId = x.SectionId,
                                                                              CourseId = x.CourseId,
                                                                              CourseCode = x.CourseCode,
                                                                              StudentCode = x.StudentCode,
                                                                              StudentTitle = x.StudentTitle,
                                                                              StudentFirstName = x.StudentFirstNameEn,
                                                                              StudentMidName = x.StudentMidNameEn,
                                                                              StudentLastName = x.StudentLastNameEn,
                                                                              Grade = x.GradeName ?? "",
                                                                              Round = x.Percentage ?? 0,
                                                                              CourseCredit = x.Credit,
                                                                              GradeWeight = x.GradeWeight,
                                                                              GradeId = x.GradeId,
                                                                              IsCalcGrade = x.IsCalcGrade,
                                                                              IsGradePublish = x.IsGradePublish,
                                                                              GradeTemplateId = x.GradeTemplateId,
                                                                              AllocationTotal = x.TotalScore.ToString() ?? "",
                                                                              TotalScore = x.TotalScore,
                                                                              Allocations = x.StudentRawScoreDetails.Select(y => new Allocation
                                                                                                                                 {
                                                                                                                                     Type = y.MarkAllocation?.Name,
                                                                                                                                     Abbreviation = y.MarkAllocation?.Abbreviation,
                                                                                                                                     FullScore = y.Score
                                                                                                                                 }).ToList(),
                                                                              Major = x.DepartmentCode
                                                                            })
                                                               .ToList();

            var sectionIds = studentScoreAllocations.Select(x => x.SectionId).Distinct().ToList();
            var studentSectionAllocations = _db.Sections.Include(x => x.Course).Where(x => sectionIds.Contains(x.Id))
                                                        .Select(x => new StudentSectionAllocation
                                                                     {
                                                                         CourseId = x.CourseId,
                                                                         SectionId = x.Id,
                                                                         Section = x.Number,
                                                                         Course = x.Course.CodeAndSpecialChar,
                                                                         Credit = x.Course.CreditText,
                                                                         SectionType = x.ParentSectionId == null ? "Master" : "Joint"
                                                                     })
                                                        .OrderBy(x => x.Section)
                                                            .ThenByDescending(x => x.SectionType)
                                                            .ThenBy(x => x.Course)
                                                        .ToList();

            var term = _academicProvider.GetTerm(termId);

            var sectionMasterIds = sections.Select(x => x.SectionId).ToList();
            var classStatistics = GetClassStatisticsGradeScoreSummary(studentScoreAllocations);
            var sectionMarkAllocations = new List<SectionMarkAllocationsViewModel>();

            // GRADING CURVE
            var gradeCurves = GetGradingCurveByCourseIdAndTermId(courseId, termId);
            
            var markAllocations = allocation.Select(x => new Allocation
                                                         {
                                                             Id = x.Id,
                                                             Type = x.Name,
                                                             Abbreviation = x.Abbreviation,
                                                             FullScore = x.Score
                                                         })
                                            .ToList();
            foreach(var item in sections)
            {
                var jointSectionIds = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                   .Select(x => x.Id)
                                                   .ToList(); 
                var students = studentScoreAllocations.Where(x => x.SectionId == item.SectionId)
                                                      .ToList();

                var studentJointSections = studentScoreAllocations.Where(x => jointSectionIds.Contains(x.SectionId))
                                                                  .OrderBy(x => x.CourseCode)
                                                                  .ToList();
 
                students.AddRange(studentJointSections);

                var instructorsName = _db.SectionSlots.Where(x => x.SectionId == item.SectionId)
                                                 .Select(x => x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + " " + x.Instructor.LastNameEn)
                                                 .GroupBy(x => x)
                                                 .Select(x => x.First())
                                                 .ToList();
                if(students.Any())
                {
                    sectionMarkAllocations.Add(new SectionMarkAllocationsViewModel
                                            {
                                                StudentScoreAllocations = students,
                                                MarkAllocaiton = markAllocations,
                                                InstructorFullNameEns = instructorsName.Any() ? string.Join(", ",instructorsName) : " - ",
                                                SectionAllocations = studentSectionAllocations.Where(x => x.SectionId == item.SectionId || jointSectionIds.Contains(x.SectionId))
                                                                                                    .OrderByDescending(x => x.SectionType)
                                                                                                        .ThenBy(x => x.Course)
                                                                                                .ToList()
                                            });
                }
            }

            var reportModel = new GradingReportViewModel
                              {
                                  PrintedAt = _dateTimeProvider.ConvertFromUtcToSE(DateTime.UtcNow)?.ToString(StringFormat.ShortDate),
                                  BarcodeNumber = latestBarcode == null ? "N/A" : latestBarcode.BarcodeNumber,
                                  Faculty = course.Faculty == null ? "-" : course.Faculty.NameEn,
                                  Department = course.Department == null ? "-" : course.Department.NameEn,
                                  Semester = term.TermText,
                                  AcademicYear = term.AcademicYear.ToString(),
                                  AcademicTerm = term.AcademicTerm.ToString(),
                                  AcademicLevelId = term.AcademicLevelId,
                                  TermId = term.Id,
                                  CourseId = course.Id,
                                  Course = course.CodeAndName,
                                  CourseCreitText = course.CreditText,
                                  Sections = sections.Select(x => x.SectionNumber).ToList(),
                                  Instructors = instructors,
                                  ClassStatistics = classStatistics,
                                  GradingCurves = gradeCurves.Where(x => x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                  GradingCurvesNotCalc = gradeCurves.Where(x => !x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                  IsCalGradeExist = studentScoreAllocations.Any(x => x.IsCalcGrade),
                                  IsNotCalGradeExist = studentScoreAllocations.Any(x => !x.IsCalcGrade),
                                  GradeNormalCurves = GetGradeNormalCurves(classStatistics),
                                  GradingRanges = GetSummaryGradingCurves(courseId, termId, studentScoreAllocations).ToList(),
                                  GradingFrequencies = GetGradingFrequencies(studentScoreAllocations),
                                  MarkAllocaiton = markAllocations,
                                  StudentScoreAllocations = studentScoreAllocations,
                                  SectionMarkAllocations = sectionMarkAllocations,
                                  StudentRawScores = studentResultRawScore
                              };

            return reportModel;
        }

        public List<GradingReportViewModel> GetGradeApprovalDetailForPreview(List<long> barcodeIds)
        {
            var barcodes = _db.Barcodes.Where(x => barcodeIds.Contains(x.Id))
                                       .Select(x => new
                                                    {
                                                        SectionId = x.SectionId,
                                                        CourseId = x.CourseId,
                                                        TermId = x.Section.TermId,
                                                        InstructorId = x.Section.MainInstructorId
                                                    })
                                       .ToList();

            var barcodeCourseIds = barcodes.Select(x => x.CourseId).ToList();
            var barcodeSectionIds = barcodes.Select(x => x.SectionId).ToList();
            var instructorIds = barcodes.Select(x => x.InstructorId).ToList();
            var termId = barcodes.FirstOrDefault().TermId;
            var term = _academicProvider.GetTerm(termId);
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Where(x => instructorIds.Contains(x.Id))
                                             .ToList();

            var barcodeMarkAllocations = _db.MarkAllocations.Where(x => barcodeCourseIds.Contains(x.CourseId)
                                                                        && x.TermId == termId)
                                                            .ToList();

            var barcodeSections = _db.Sections.Where(x => barcodeSectionIds.Contains(x.Id)).ToList();
            var gradeSkip = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                  || x.Name.ToUpper() == "AU" )
                                      .Select(x => (long?)x.Id)
                                      .ToList();

            var studentRawScores = _db.StudentRawScores.Include(x => x.StudentRawScoreDetails)
                                                            .ThenInclude(x => x.MarkAllocation)
                                                       .Where(x => barcodeIds.Contains(x.BarcodeId ?? 0))
                                                       .Select(x => new StudentRawScoreViewModel
                                                                    {
                                                                        StudentId = x.StudentId,
                                                                        SectionId = x.SectionId,
                                                                        CourseCode = x.Course.CodeAndSpecialChar, 
                                                                        CourseCredit = x.Course.CreditText,
                                                                        CourseName = x.Course.NameEn,
                                                                        CourseId = x.CourseId,
                                                                        StudentCode = x.Student.Code,
                                                                        StudentTitle = x.Student.Title.NameEn,
                                                                        StudentFirstNameEn = x.Student.FirstNameEn,
                                                                        StudentLastNameEn = x.Student.LastNameEn,
                                                                        StudentMidNameEn = x.Student.MidNameEn,
                                                                        SectionNumber = x.Section.Number,
                                                                        SectionType = x.Section.ParentSectionId == null ? "Master" : "Joint",
                                                                        IsPaid = x.RegistrationCourse.IsPaid,
                                                                        IsWithdrawal = x.RegistrationCourse.GradeName == "W" ? true : false,
                                                                        IsGradePublish = x.RegistrationCourse.IsGradePublished,
                                                                        RegistrationCourseId = x.RegistrationCourseId,
                                                                        Id = x.Id,
                                                                        GradeId = x.GradeId,
                                                                        TotalScore = x.TotalScore,
                                                                        IsSkipGrading = x.IsSkipGrading,
                                                                        StudentRawScoreDetails = x.StudentRawScoreDetails,
                                                                        GradeName = x.Grade.Name,
                                                                        GradeWeight = x.Grade.Weight ?? 0,
                                                                        Percentage = x.TotalScore,
                                                                        IsCalcGrade = x.Grade == null ? false : x.Grade.IsCalculateGPA,
                                                                        Credit = x.Course.Credit,
                                                                        DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                                        GradeTemplateId = x.GradeTemplateId
                                                                    })
                                                       .ToList();

            var reports = new List<GradingReportViewModel>();
            foreach (var item in barcodes)
            {
                var instructor = instructors.Where(x => x.Id == item.InstructorId)
                                            .ToList();

                var allocation = barcodeMarkAllocations.Where(x => x.CourseId == item.CourseId)
                                                       .OrderBy(x => x.Sequence)
                                                       .ToList();

                var studentRawScoreDetails = allocation.Select(x => new StudentRawScoreDetail
                                                                    {  
                                                                        MarkAllocationId = x.Id,
                                                                        MarkAllocation = x
                                                                    })
                                                       .ToList();

                var course = _registrationProvider.GetCourse(item.CourseId);
                var sections = barcodeSections.Where(x => x.Id == item.SectionId)
                                              .Select(x => new
                                                           {
                                                               SectionId = x.Id,
                                                               SeatUsed = x.SeatUsed,
                                                               SectionNumber = x.Number,
                                                               CourseName = course.NameEn,
                                                               CourseCode = course.CodeAndSpecialChar
                                                           })
                                              .OrderBy(x => x.SectionNumber)
                                              .ToList();

                var sectionIdsNullable = sections.Select(x => (long?)x.SectionId).ToList();
                var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                                .Select(x => new 
                                                             {
                                                                 Id = x.Id,
                                                                 ParentSectionId = x.ParentSectionId,
                                                                 Number = x.Number,
                                                                 SeatUsed = x.SeatUsed
                                                             })
                                                .ToList();

                sectionIdsNullable.AddRange(jointSections.Select(x => (long?)x.Id).ToList());
                var studentResultRawScore = GetStudentRawScoresBySectionList(sectionIdsNullable, studentRawScoreDetails, studentRawScores).Where(x => x.Id != 0 
                                                                                                                                                      && !( x.IsSkipGrading 
                                                                                                                                                           || x.IsWithdrawal 
                                                                                                                                                           || x.TotalScore == null))
                                                                                                                                          .ToList();

                var studentScoreAllocations = studentResultRawScore.Select(x => new StudentScoreAllocation
                                                                                {
                                                                                  StudentId = x.StudentId,
                                                                                  Section = x.SectionNumber,
                                                                                  SectionId = x.SectionId,
                                                                                  CourseId = x.CourseId,
                                                                                  CourseCode = x.CourseCode,
                                                                                  StudentCode = x.StudentCode,
                                                                                  StudentTitle = x.StudentTitle,
                                                                                  StudentFirstName = x.StudentFirstNameEn,
                                                                                  StudentMidName = x.StudentMidNameEn,
                                                                                  StudentLastName = x.StudentLastNameEn,
                                                                                  Grade = x.GradeName ?? "",
                                                                                  Round = x.Percentage ?? 0,
                                                                                  CourseCredit = x.Credit,
                                                                                  GradeWeight = x.GradeWeight,
                                                                                  GradeId = x.GradeId,
                                                                                  IsCalcGrade = x.IsCalcGrade,
                                                                                  IsGradePublish = x.IsGradePublish,
                                                                                  GradeTemplateId = x.GradeTemplateId,
                                                                                  AllocationTotal = x.TotalScore.ToString() ?? "",
                                                                                  TotalScore = x.TotalScore,
                                                                                  Allocations = x.StudentRawScoreDetails.Select(y => new Allocation
                                                                                                                                     {
                                                                                                                                         Type = y.MarkAllocation?.Name,
                                                                                                                                         Abbreviation = y.MarkAllocation?.Abbreviation,
                                                                                                                                         FullScore = y.Score
                                                                                                                                     }).ToList(),
                                                                                  Major = x.DepartmentCode
                                                                                })
                                                                   .ToList();

                var sectionIds = studentScoreAllocations.Select(x => x.SectionId).Distinct().ToList();
                var studentSectionAllocations = _db.Sections.Where(x => sectionIds.Contains(x.Id))
                                                            .Select(x => new StudentSectionAllocation
                                                                         {
                                                                             CourseId = x.CourseId,
                                                                             SectionId = x.Id,
                                                                             Section = x.Number,
                                                                             Course = x.Course.CodeAndSpecialChar,
                                                                             Credit = x.Course.CreditText,
                                                                             SectionType = x.ParentSectionId == null ? "Master" : "Joint"
                                                                         })
                                                            .OrderBy(x => x.Section)
                                                                .ThenByDescending(x => x.SectionType)
                                                                .ThenBy(x => x.Course)
                                                            .ToList();

                var sectionMasterIds = sections.Select(x => x.SectionId).ToList();
                var classStatistics = GetClassStatisticsGradeScoreSummary(studentScoreAllocations);
                var barcode = _db.Barcodes.Where(x => sectionMasterIds.Contains(x.SectionId)).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                var sectionMarkAllocations = new List<SectionMarkAllocationsViewModel>();

                // GRADING CURVE
                var gradeCurves = GetGradingCurveByCourseIdAndTermId(item.CourseId, termId);
                var markAllocations = allocation.Select(x => new Allocation
                                                             {
                                                                 Id = x.Id,
                                                                 Type = x.Name,
                                                                 Abbreviation = x.Abbreviation,
                                                                 FullScore = x.Score
                                                             })
                                                .ToList();

                foreach(var section in sections)
                {
                    var jointSectionIds = jointSections.Where(x => x.ParentSectionId == section.SectionId)
                                                       .Select(x => x.Id)
                                                       .ToList();

                    var students = studentScoreAllocations.Where(x => x.SectionId == section.SectionId)
                                                          .ToList();

                    var studentJointSections = studentScoreAllocations.Where(x => jointSectionIds.Contains(x.SectionId))
                                                                      .OrderBy(x => x.CourseCode)
                                                                      .ToList();
    
                    students.AddRange(studentJointSections);

                    var instructorsName = _db.SectionSlots.Where(x => x.SectionId == section.SectionId)
                                                          .Select(x => x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + " " + x.Instructor.LastNameEn)
                                                          .GroupBy(x => x)
                                                          .Select(x => x.First())
                                                          .ToList();
                    if(students.Any())
                    {
                        sectionMarkAllocations.Add(new SectionMarkAllocationsViewModel
                                                   {
                                                       StudentScoreAllocations = students,
                                                       MarkAllocaiton = markAllocations,
                                                       InstructorFullNameEns = instructorsName.Any() ? string.Join(", ",instructorsName) : " - ",
                                                       SectionAllocations = studentSectionAllocations.Where(x => x.SectionId == section.SectionId || jointSectionIds.Contains(x.SectionId))
                                                                                                         .OrderByDescending(x => x.SectionType)
                                                                                                             .ThenBy(x => x.Course)
                                                                                                     .ToList()
                                                   });
                    }
                }

                var reportModel = new GradingReportViewModel
                                  {
                                      PrintedAt = _dateTimeProvider.ConvertFromUtcToSE(DateTime.UtcNow)?.ToString(StringFormat.ShortDate),
                                      BarcodeNumber = barcode == null ? "N/A" : barcode.BarcodeNumber,
                                      Faculty = course.Faculty.NameEn,
                                      Department = course.DepartmentId == null ? "-" : course.Department.NameEn,
                                      Semester = term.TermText,
                                      AcademicYear = term.AcademicYear.ToString(),
                                      AcademicTerm = term.AcademicTerm.ToString(),
                                      AcademicLevelId = term.AcademicLevelId,
                                      TermId = term.Id,
                                      CourseId = course.Id,
                                      Course = course.CodeAndName,
                                      CourseCreitText = course.CreditText,
                                      Sections = sections.Select(x => x.SectionNumber).ToList(),
                                      Instructors = instructor,
                                      ClassStatistics = classStatistics,
                                      GradingCurves = gradeCurves.Where(x => x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                      GradingCurvesNotCalc = gradeCurves.Where(x => !x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList(),
                                      IsCalGradeExist = studentScoreAllocations.Any(x => x.IsCalcGrade),
                                      IsNotCalGradeExist = studentScoreAllocations.Any(x => !x.IsCalcGrade),
                                      GradeNormalCurves = GetGradeNormalCurves(classStatistics),
                                      GradingRanges = GetSummaryGradingCurves(item.CourseId, termId, studentScoreAllocations).ToList(),
                                      GradingFrequencies = GetGradingFrequencies(studentScoreAllocations),
                                      MarkAllocaiton = markAllocations,
                                      StudentScoreAllocations = studentScoreAllocations,
                                      SectionMarkAllocations = sectionMarkAllocations,
                                      StudentRawScores = studentResultRawScore
                                  };
                
                reports.Add(reportModel);
            }

            return reports;
        }

        public List<Barcode> GenerateBarcode(long termId, List<long> sectionIds, Course course)
        {
            var term = _academicProvider.GetTerm(termId);
            var currentDateTime = DateTime.Now;
            var year = currentDateTime.Year.ToString();
            var month = currentDateTime.Month.ToString("d2");
            var day = currentDateTime.Day.ToString("d2");
            var time = currentDateTime.ToString("HHmmssfff");
            var barcodeAttemp = (GetBarcodeAttempBytermAndCourse(course.Id, sectionIds.FirstOrDefault()) + 1).ToString("d2");

            var barcodeNumber = $"{term.AcademicYear}{term.AcademicTerm}{course.Code}{year}{month}{day}{time}{barcodeAttemp}";

            var barcodes = new List<Barcode>();
            foreach(var item in sectionIds)
            {
                var jointSectionIds = _db.Sections.Where(x => x.ParentSectionId == item).Select(x => x.Id).ToList();
                var allSectionIds = new List<long>();
                allSectionIds.Add(item);
                allSectionIds.AddRange(jointSectionIds);

                var IsStudentExist = _db.StudentRawScores.Any(x => allSectionIds.Contains(x.SectionId)
                                                                   && !x.RegistrationCourse.IsGradePublished
                                                                   && (x.TotalScore != null || x.GradeId != null));
                
                if (IsStudentExist)
                {
                    var jointSectionIdsJson = JsonConvert.SerializeObject(jointSectionIds);
                    barcodes.Add( new Barcode
                                {
                                    BarcodeNumber = barcodeNumber,
                                    CourseId = course.Id,
                                    SectionId = item,
                                    GeneratedAt = currentDateTime,
                                    SectionIds = jointSectionIdsJson
                                });
                }
            }

            return barcodes;
        }

        public int GetBarcodeAttempBytermAndCourse(long courseId, long sectionId)
        {
            var barcodeAttemp = _db.Barcodes.IgnoreQueryFilters()
                                            .Count(x => sectionId == x.SectionId
                                                        && x.CourseId == courseId);

            return barcodeAttemp;
        }

        #endregion

        public List<StudentRawScoreViewModel> GetStudentRawScoresBySections(List<long?> sectionIdsNullable, List<StudentRawScoreDetail> studentRawScoreDetails)
        {
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Course)
                                                             .Where(x => sectionIdsNullable.Contains(x.SectionId)
                                                                         && x.Status != "d"
                                                                         && x.IsPaid
                                                                         && x.IsActive)
                                                             .AsNoTracking()
                                                             .Select(x => new StudentRawScoreViewModel
                                                                          {
                                                                              StudentId = x.StudentId,
                                                                              SectionId = x.SectionId ?? 0,
                                                                              CourseCode = x.Course.CodeAndSpecialChar,
                                                                              CourseCredit = x.Course.CreditText,
                                                                              CourseName = x.Course.NameEn,
                                                                              CourseId = x.CourseId,
                                                                              StudentCode = x.Student.Code,
                                                                              StudentTitle = x.Student.Title.NameEn,
                                                                              StudentFirstNameEn = x.Student.FirstNameEn,
                                                                              StudentLastNameEn = x.Student.LastNameEn,
                                                                              StudentMidNameEn = x.Student.MidNameEn,
                                                                              SectionNumber = x.Section.Number,
                                                                              SectionType = x.Section.ParentSectionId == null ? "Master" : "Joint",
                                                                              IsPaid = x.IsPaid,
                                                                              IsWithdrawal = x.GradeName == "W" ? true : false,
                                                                              IsGradePublish = x.IsGradePublished,
                                                                              RegistrationCourseId = x.Id,
                                                                              GradeName = x.GradeName,
                                                                              GradeWeight = x.Grade.Weight ?? 0,
                                                                              IsCalcGrade = x.Grade == null ? false : x.Grade.IsCalculateGPA,
                                                                              GradeId = x.GradeId,
                                                                              Credit = x.Course.Credit,
                                                                              Percentage = 0,
                                                                              DepartmentCode = x.Student.AcademicInformation.Department.Code
                                                                          })
                                                             .ToList();

            var studentRawScores = _db.StudentRawScores.Include(x => x.StudentRawScoreDetails)
                                                            .ThenInclude(x => x.MarkAllocation)
                                                       .Where(x => sectionIdsNullable.Contains(x.SectionId))
                                                       .Select(x => new StudentRawScoreViewModel
                                                                    {
                                                                        StudentId = x.StudentId,
                                                                        SectionId = x.SectionId,
                                                                        CourseCode = x.Course.CodeAndSpecialChar, 
                                                                        CourseCredit = x.Course.CreditText,
                                                                        CourseName = x.Course.NameEn,
                                                                        CourseId = x.CourseId,
                                                                        StudentCode = x.Student.Code,
                                                                        StudentTitle = x.Student.Title.NameEn,
                                                                        StudentFirstNameEn = x.Student.FirstNameEn,
                                                                        StudentLastNameEn = x.Student.LastNameEn,
                                                                        StudentMidNameEn = x.Student.MidNameEn,
                                                                        SectionNumber = x.Section.Number,
                                                                        SectionType = x.Section.ParentSectionId == null ? "Master" : "Joint",
                                                                        IsPaid = x.RegistrationCourse.IsPaid,
                                                                        IsWithdrawal = x.RegistrationCourse.GradeName == "W" ? true : false,
                                                                        IsGradePublish = x.RegistrationCourse.IsGradePublished,
                                                                        RegistrationCourseId = x.RegistrationCourseId,
                                                                        Id = x.Id,
                                                                        GradeId = x.GradeId,
                                                                        TotalScore = x.TotalScore,
                                                                        IsSkipGrading = x.IsSkipGrading,
                                                                        StudentRawScoreDetails = x.StudentRawScoreDetails,
                                                                        GradeName = x.Grade.Name,
                                                                        GradeWeight = x.Grade.Weight ?? 0,
                                                                        Percentage = x.TotalScore,
                                                                        IsCalcGrade = x.Grade == null ? false : x.Grade.IsCalculateGPA,
                                                                        Credit = x.Course.Credit,
                                                                        DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                                        GradeTemplateId = x.GradeTemplateId
                                                                    })
                                                       .ToList();

            var studentResultRawScore = new List<StudentRawScoreViewModel>();
            foreach(var item in registrationCourses)
            {
                if(studentRawScores.Any(x => x.RegistrationCourseId == item.RegistrationCourseId))
                {
                    var studentRawScore = studentRawScores.FirstOrDefault(x => x.RegistrationCourseId == item.RegistrationCourseId);
                    foreach(var allocation in studentRawScoreDetails)
                    {
                        if(!studentRawScore.StudentRawScoreDetails.Any(x => x.MarkAllocationId == allocation.MarkAllocationId))
                        {
                            studentRawScore.StudentRawScoreDetails.Add(allocation);
                        }
                    }
                    studentResultRawScore.Add(studentRawScore);
                }
                else
                {
                    var studentRawScoreDetail = studentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                   {
                                                                                       MarkAllocationId = x.MarkAllocationId,
                                                                                       MarkAllocation = x.MarkAllocation,
                                                                                       Score = x.Score
                                                                                   })
                                                                      .ToList();
                    item.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                    var studentRawScore = item;
                    studentResultRawScore.Add(studentRawScore);
                }
            }
            studentResultRawScore = studentResultRawScore.OrderBy(x => x.SectionNumber)
                                                            .ThenByDescending(x => x.SectionType)
                                                            .ThenBy(x => x.CourseCode)
                                                            .ThenBy(x => x.StudentCode)
                                                         .ToList();
            return studentResultRawScore;
        }

        public List<StudentRawScoreViewModel> GetStudentRawScoresBySectionList(List<long?> sectionIdsNullable, List<StudentRawScoreDetail> studentRawScoreDetails, List<StudentRawScoreViewModel> studentRawScores)
        {
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Course)
                                                             .Where(x => sectionIdsNullable.Contains(x.SectionId)
                                                                         && x.Status != "d"
                                                                         && x.IsPaid
                                                                         && x.IsActive)
                                                             .Select(x => new StudentRawScoreViewModel
                                                                          {
                                                                              StudentId = x.StudentId,
                                                                              SectionId = x.SectionId ?? 0,
                                                                              CourseCode = x.Course.CodeAndSpecialChar,
                                                                              CourseCredit = x.Course.CreditText,
                                                                              CourseName = x.Course.NameEn,
                                                                              CourseId = x.CourseId,
                                                                              StudentCode = x.Student.Code,
                                                                              StudentTitle = x.Student.Title.NameEn,
                                                                              StudentFirstNameEn = x.Student.FirstNameEn,
                                                                              StudentLastNameEn = x.Student.LastNameEn,
                                                                              StudentMidNameEn = x.Student.MidNameEn,
                                                                              SectionNumber = x.Section.Number,
                                                                              SectionType = x.Section.ParentSectionId == null ? "Master" : "Joint",
                                                                              IsPaid = x.IsPaid,
                                                                              IsWithdrawal = x.GradeName == "W" ? true : false,
                                                                              IsGradePublish = x.IsGradePublished,
                                                                              RegistrationCourseId = x.Id,
                                                                              GradeName = x.GradeName,
                                                                              GradeWeight = x.Grade.Weight ?? 0,
                                                                              IsCalcGrade = x.Grade == null ? false : x.Grade.IsCalculateGPA,
                                                                              GradeId = x.GradeId,
                                                                              Credit = x.Course.Credit,
                                                                              Percentage = 0,
                                                                              DepartmentCode = x.Student.AcademicInformation.Department.Code
                                                                          })
                                                             .ToList();

            var studentResultRawScore = new List<StudentRawScoreViewModel>();
            foreach(var item in registrationCourses)
            {
                if(studentRawScores.Any(x => x.RegistrationCourseId == item.RegistrationCourseId))
                {
                    var studentRawScore = studentRawScores.FirstOrDefault(x => x.RegistrationCourseId == item.RegistrationCourseId);
                    foreach(var allocation in studentRawScoreDetails)
                    {
                        if(!studentRawScore.StudentRawScoreDetails.Any(x => x.MarkAllocationId == allocation.MarkAllocationId))
                        {
                            studentRawScore.StudentRawScoreDetails.Add(allocation);
                        }
                    }
                    studentResultRawScore.Add(studentRawScore);
                }
                else
                {
                    var studentRawScoreDetail = studentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                   {
                                                                                       MarkAllocationId = x.MarkAllocationId,
                                                                                       MarkAllocation = x.MarkAllocation,
                                                                                       Score = x.Score
                                                                                   })
                                                                      .ToList();
                    item.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                    var studentRawScore = item;
                    studentResultRawScore.Add(studentRawScore);
                }
            }
            studentResultRawScore = studentResultRawScore.OrderBy(x => x.SectionNumber)
                                                            .ThenByDescending(x => x.SectionType)
                                                            .ThenBy(x => x.CourseCode)
                                                            .ThenBy(x => x.StudentCode)
                                                         .ToList();
            return studentResultRawScore;
        }
    }
}