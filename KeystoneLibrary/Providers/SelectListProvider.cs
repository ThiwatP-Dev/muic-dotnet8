using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class SelectListProvider : BaseProvider, ISelectListProvider
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly IGradeProvider _gradeProvider;
        protected readonly IStudentProvider _studentProvider;
        private readonly ICacheProvider _cacheProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IFeeProvider _feeProvider;
        protected readonly IPrerequisiteProvider _prerequisiteProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly IReservationProvider _reservationProvider;

        public SelectListProvider(ApplicationDbContext db,
                                  IAcademicProvider academicProvider,
                                  IInstructorProvider instructorProvider,
                                  IRegistrationProvider registrationProvider,
                                  IGradeProvider gradeProvider,
                                  IRoomProvider roomProvider,
                                  IStudentProvider studentProvider,
                                  ICacheProvider cacheProvider,
                                  IReceiptProvider receiptProvider,
                                  IAdmissionProvider admissionProvider,
                                  ICurriculumProvider curriculumProvider,
                                  IFeeProvider feeProvider,
                                  IPrerequisiteProvider prerequisiteProvider,
                                  IDateTimeProvider dateTimeProvider,
                                  IReservationProvider reservationProvider,
                                  RoleManager<IdentityRole> roleManager) : base(db)
        {
            _academicProvider = academicProvider;
            _instructorProvider = instructorProvider;
            _roomProvider = roomProvider;
            _registrationProvider = registrationProvider;
            _gradeProvider = gradeProvider;
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _receiptProvider = receiptProvider;
            _admissionProvider = admissionProvider;
            _curriculumProvider = curriculumProvider;
            _feeProvider = feeProvider;
            _prerequisiteProvider = prerequisiteProvider;
            _dateTimeProvider = dateTimeProvider;
            _roleManager = roleManager;
            _reservationProvider = reservationProvider;
        }

        #region General
        public SelectList GetTitlesEn()
        {
            var titles = _db.Titles.OrderBy(x => x.Order)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameEn,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(titles, "Value", "Text");
        }

        public SelectList GetTitlesTh()
        {
            var titles = _db.Titles.OrderBy(x => x.Order)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameTh,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(titles, "Value", "Text");
        }

        public SelectList GetTitleThByTitleEn(long id)
        {
            var titles = _db.Titles.Where(x => x.Id == id)
                                   .OrderBy(x => x.Order)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameTh,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(titles, "Value", "Text");
        }

        public SelectList GetRaces()
        {
            var races = _db.Races.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(races, "Value", "Text");
        }

        public SelectList GetNationalities()
        {
            var nationalities = _db.Nationalities.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(nationalities, "Value", "Text");
        }

        public SelectList GetReligions()
        {
            var religions = _db.Religions.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(religions, "Value", "Text");
        }

        public SelectList GetStudentGroups()
        {
            var studentGroups = _db.StudentGroups.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
                                                 .OrderBy(x => x.Text);
            return new SelectList(studentGroups, "Value", "Text");
        }

        public SelectList GetRelationships()
        {
            var relationships = _db.Relationships.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(relationships, "Value", "Text");
        }

        public SelectList GetOccupations()
        {
            var occupations = _db.Occupations.Select(x => new SelectListItem
                                                          {
                                                              Text = x.NameEn,
                                                              Value = x.Id.ToString()
                                                          });
            return new SelectList(occupations, "Value", "Text");
        }

        public SelectList GetBankBranches()
        {
            var bankbranches = _db.BankBranches.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(bankbranches, "Value", "Text");
        }

        public SelectList GetMaritalStatuses()
        {
            var maritalStatuses = new List<SelectListItem>
                                  {
                                      new SelectListItem { Text = "Single", Value = "s" },
                                      new SelectListItem { Text = "Married", Value = "m" },
                                      new SelectListItem { Text = "Other", Value = "o" }
                                  };

            return new SelectList(maritalStatuses, "Value", "Text");
        }

        public SelectList GetLivingStatuses()
        {
            var livingStatuses = new List<SelectListItem>
                                 {
                                     new SelectListItem { Text = "Alive", Value = "a" },
                                     new SelectListItem { Text = "Death", Value = "d" },
                                     new SelectListItem { Text = "Other", Value = "o" }
                                 };

            return new SelectList(livingStatuses, "Value", "Text");
        }

        public SelectList GetDeformations()
        {
            var deformations = _db.Deformations.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            })
                                               .OrderBy(x => x.Text);

            return new SelectList(deformations, "Value", "Text");
        }

        public SelectList GetCalculateTypes()
        {
            var calculateType = new List<SelectListItem>
                                {
                                    new SelectListItem { Text = "All", Value = "" },
                                    new SelectListItem { Text = "Not Specify", Value = "N/A" },
                                    new SelectListItem { Text = "Per Stundent", Value = "s" },
                                    new SelectListItem { Text = "Per Hour", Value = "h" }
                                };

            return new SelectList(calculateType, "Value", "Text");
        }

        public SelectList GetStudentCodeStatuses()
        {
            var check = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "all" },
                            new SelectListItem { Text = "Used", Value = "true" },
                            new SelectListItem { Text = "Not Used", Value = "false" }
                        };
            return new SelectList(check, "Value", "Text");
        }

        public SelectList GetResidentTypes()
        {
            var types = _db.ResidentTypes.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetStudentFeeTypes()
        {
            var types = _db.StudentFeeTypes.Select(x => new SelectListItem
                                                        {
                                                            Text = x.NameEn,
                                                            Value = x.Id.ToString()
                                                        });
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetCustomCourseGroups()
        {
            var courseGroups = _db.CustomCourseGroups.Select(x => new SelectListItem
                                                                  {
                                                                      Text = x.Name,
                                                                      Value = x.Id.ToString()
                                                                  });
            return new SelectList(courseGroups, "Value", "Text");
        }

        public SelectList GetQuestionnaireCourseGroups()
        {
            var courseGroups = _db.QuestionnaireCourseGroups.Select(x => new SelectListItem
                                                                         {
                                                                             Text = x.Name,
                                                                             Value = x.Id.ToString()
                                                                         });
            return new SelectList(courseGroups, "Value", "Text");
        }

        public SelectList GetGradingStatisticReport()
        {
            var check = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Classified by Academic Level", Value = "a" },
                            new SelectListItem { Text = "Classified by Department", Value = "d" },
                            new SelectListItem { Text = "Classified by Course", Value = "c" },
                        };
            return new SelectList(check, "Value", "Text");
        }

        public SelectList GetCheatingStatisticReportTypes()
        {
            var report = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Term", Value = "t" },
                             new SelectListItem { Text = "Faculty", Value = "f" },
                             new SelectListItem { Text = "Batch", Value = "b" }
                         };
            return new SelectList(report, "Value", "Text");
        }

    public SelectList GetMonth()
        {
            var report = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "January", Value = "1" },
                             new SelectListItem { Text = "Fabuary", Value = "2" },
                             new SelectListItem { Text = "March", Value = "3" },
                             new SelectListItem { Text = "April", Value = "4" },
                             new SelectListItem { Text = "May", Value = "5" },
                             new SelectListItem { Text = "June", Value = "6" },
                             new SelectListItem { Text = "July", Value = "7" },
                             new SelectListItem { Text = "August", Value = "8" },
                             new SelectListItem { Text = "September", Value = "9" },
                             new SelectListItem { Text = "October", Value = "10" },
                             new SelectListItem { Text = "November", Value = "11" },
                             new SelectListItem { Text = "December", Value = "12" },
                         };
            return new SelectList(report, "Value", "Text");
        }

        public SelectList GetCertificateChangeNameTypes(string language = "en")
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = language == "th" ? "เปลี่ยนชื่อ" : "Only Name", Value = "name" },
                            new SelectListItem { Text = language == "th" ? "เปลี่ยนนามสกุล" : "Only Surname", Value = "surname" },
                            new SelectListItem { Text = language == "th" ? "เปลี่ยนชื่อและสกุล" : "Name and Surname", Value = "name and surname" },
                            new SelectListItem { Text = language == "th" ? "เปลี่ยนตัวสะกด" : "Change Spelling", Value = "spelling" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetReenterType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Re-Admission", Value = "ra" },
                            new SelectListItem { Text = "Re-Enter", Value = "re" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetAttendanceType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "ID", Value = "i" },
                            new SelectListItem { Text = "Name", Value = "n" },
                            new SelectListItem { Text = "Picture", Value = "p" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetReceiptInvoice()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Receipt", Value = "r" },
                            new SelectListItem { Text = "Invoice", Value = "i" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetReceiptInvoiceType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Registration", Value = "r" },
                            new SelectListItem { Text = "Add", Value = "a" },
                            new SelectListItem { Text = "Delete", Value = "d" },
                            new SelectListItem { Text = "Refund", Value = "rf" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetProgramDirectorTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Program Director", Value = "pd" },
                            new SelectListItem { Text = "Program Coordinator", Value = "pc" }
                        };
            return new SelectList(types, "Value", "Text");
        }
        #endregion

        #region Academic
        public SelectList GetCampuses()
        {
            var campuses = _db.Campuses.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(campuses, "Value", "Text");
        }

        public SelectList GetAcademicPrograms()
        {
            var academicProgram = _db.AcademicPrograms.Select(x => new SelectListItem
                                                      {
                                                          Text = x.NameEn,
                                                          Value = x.Id.ToString()
                                                      })
                                                      .OrderBy(x => x.Text);

            return new SelectList(academicProgram, "Value", "Text");
        }
        
        public SelectList GetAcademicProgramsByAcademicLevelId(long academicLevelId)
        {
            var academicProgram = _db.AcademicPrograms.Where(x => x.AcademicLevelId == academicLevelId)
                                                      .Select(x => new SelectListItem
                                                      {
                                                          Text = x.NameEn,
                                                          Value = x.Id.ToString()
                                                      })
                                                      .OrderBy(x => x.Text);
            return new SelectList(academicProgram, "Value", "Text");
        }


        public SelectList GetAcademicYearByScholarshipStudents()
        {
            var academicYear = _db.ScholarshipStudents.Include(x => x.EffectivedTerm)
                                                      .Select(x => x.EffectivedTerm.AcademicYear)
                                                      .Distinct()
                                                      .Select(x => new SelectListItem
                                                                   {
                                                                       Text = x.ToString(),
                                                                       Value = x.ToString()
                                                                   })
                                                      .OrderBy(x => x.Text);
            return new SelectList(academicYear, "Value", "Text");
        }

        public SelectList GetSectionPrograms()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "All" },
                               new SelectListItem { Text = "Morning", Value = "false" },
                               new SelectListItem { Text = "Evening", Value = "true" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetCoursesBySectionStatus(long termId, string sectionStatus)
        {
            var courseSection = new List<Course>();
            if (sectionStatus == "all")
            {
                courseSection = _cacheProvider.GetCourses();
            }
            else if (sectionStatus == "true")
            {
                courseSection = _registrationProvider.GetCoursesByOpenSection(termId, sectionStatus);
            }
            else
            {
                courseSection = _registrationProvider.GetCoursesByCloseSection(termId, sectionStatus);
            }

            var coursesSelectList = courseSection.Select(x => new SelectListItem
            {
                Text = x.CourseAndCredit,
                Value = x.Id.ToString()
            });
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetCoursesFromSectionMasterByTermId(long termId)
        {

            var courseIds = _db.Sections.Where(x => x.TermId == termId  
                                                   && x.ParentSectionId == null
                                                   && x.Status == "a")
                                       .Select(x => x.CourseId)
                                       .GroupBy(x => x)
                                       .Select(x => x.FirstOrDefault())
                                       .ToList();

            var courses = _db.Courses.Where(x => courseIds.Contains(x.Id))
                                     .Select(x => new 
                                                  {
                                                      Course = x.CourseAndCredit,
                                                      Id = x.Id
                                                  })
                                     .ToList();

            var coursesSelectList = courses.Select(x => new SelectListItem
                                                         {
                                                             Text = x.Course,
                                                             Value = x.Id.ToString()
                                                         });
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetSectionMasterByTermIdAndCourseId(long termId, long courseId)
        {
            var sections = _db.Sections.Where(x => x.TermId == termId  
                                                   && x.CourseId == courseId
                                                   && x.ParentSectionId == null
                                                   && x.Status == "a")
                                       .ToList();

            var coursesSelectList = sections.Select(x => new SelectListItem
                                                             {
                                                                 Text = x.Number,
                                                                 Value = x.Id.ToString()
                                                             });
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetAcademicLevels(string language = "en")
        {
            var levels = _db.AcademicLevels.Select(x => new SelectListItem
            {
                Text = language == "th" ? x.NameTh : x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(levels, "Value", "Text");
        }

        public SelectList GetFaculties(string language = "en")
        {
            var faculties = _cacheProvider.GetFaculties();
            var facultiesSelectList = faculties.OrderBy(x => x.Code)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = language == "th" ? x.CodeAndNameTh : x.CodeAndName,
                                                   Value = x.Id.ToString()
                                               });
            return new SelectList(facultiesSelectList, "Value", "Text");
        }

        public SelectList GetFacultiesByAcademicLevelId(long academicLevelId)
        {
            var faculties = _academicProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            var facultiesSelectList = faculties.OrderBy(x => x.Code)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.CodeAndName,
                                                   Value = x.Id.ToString()
                                               });
            return new SelectList(facultiesSelectList, "Value", "Text");
        }

        public SelectList GetAcademicHonors()
        {
            var academicHonors = _db.AcademicHonors.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(academicHonors, "Value", "Text");
        }

        public SelectList GetDepartments(string language = "en")
        {
            var departments = _db.Departments.Select(x => new SelectListItem
            {
                Text = language == "th" ? x.CodeAndNameTh : x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(departments, "Value", "Text");
        }

        public SelectList GetDepartmentsByFacultyIds(long academicLevelId, List<long> facultyIds)
        {
            var departments = _academicProvider.GetDepartmentsByFacultyIds(academicLevelId, facultyIds)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.CodeAndName,
                                                   Value = x.Id.ToString()
                                               });
            return new SelectList(departments, "Value", "Text");
        }

        public SelectList GetDepartments(long id, string language = "en")
        {
            var departments = _cacheProvider.GetDepartments();
            var departmentsSelectList = departments.Where(x => x.FacultyId == id)
                                                   .OrderBy(x => x.Code)
                                                   .Select(x => new SelectListItem
                                                   {
                                                       Text = language == "th" ? x.CodeAndNameTh : x.CodeAndName,
                                                       Value = x.Id.ToString()
                                                   });
            return new SelectList(departmentsSelectList, "Value", "Text");
        }

        public SelectList GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId)
        {
            var departments = _academicProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId);
            var departmentsSelectList = departments.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(departmentsSelectList, "Value", "Text");
        }

        public SelectList GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId, long facultyId)
        {
            var departments = _academicProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            var departmentsSelectList = departments.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(departmentsSelectList, "Value", "Text");
        }

        public SelectList GetAdmissionRoundByAcademicLevelId(long id)
        {
            var admissionRound = _admissionProvider.GetAdmissionRoundByAcademicLevelId(id);
            if (admissionRound != null)
            {
                var admissionRoundSelectList = admissionRound.Select(x => new SelectListItem
                {
                    Text = x.TermRoundText,
                    Value = x.Id.ToString()
                });
                return new SelectList(admissionRoundSelectList, "Value", "Text");
            }
            else
            {
                return null;
            }
        }

        public SelectList GetAdmissionRoundByTermId(long termId)
        {
            var admissionRound = _admissionProvider.GetAdmissionRoundByTermId(termId);
            if (admissionRound != null)
            {
                var admissionRoundSelectList = admissionRound.Select(x => new SelectListItem
                {
                    Text = $"{ x.AdmissionTerm.TermText } Round { x.Round }",
                    Value = x.Id.ToString()
                });
                return new SelectList(admissionRoundSelectList, "Value", "Text");
            }
            else
            {
                return null;
            }
        }

        public SelectList GetSubmittedStatus()
        {
            var checkedDate = new List<SelectListItem>
                              {
                                  new SelectListItem { Text = "All", Value = "all" },
                                  new SelectListItem { Text = "Submitted", Value = "true" },
                                  new SelectListItem { Text = "No submitted", Value = "false"}
                              };

            return new SelectList(checkedDate, "Value", "Text");
        }

        public SelectList GetReceivedStatus()
        {
            var receivedDate = new List<SelectListItem>
                              {
                                  new SelectListItem { Text = "All", Value = "all" },
                                  new SelectListItem { Text = "Received", Value = "true" },
                                  new SelectListItem { Text = "No received", Value = "false"}
                              };

            return new SelectList(receivedDate, "Value", "Text");
        }

        public SelectList GetTermsByAcademicLevelId(long id)
        {
            var terms = _academicProvider.GetTermsByAcademicLevelId(id)
                                         .OrderByDescending(x => x.AcademicYear)
                                         .ThenByDescending(x => x.AcademicTerm)
                                         .Select(x => new SelectListItem
                                         {
                                             Text = x.TermText,
                                             Value = x.Id.ToString()
                                         });
            return new SelectList(terms, "Value", "Text");
        }

        public SelectList GetExpectedGraduationTerms(long id)
        {
            var terms = _academicProvider.GetTermsByAcademicLevelId(id)
                                         .OrderBy(x => x.AcademicTerm)
                                         .GroupBy(x => x.AcademicTerm)
                                         .Select(x => new SelectListItem
                                                      {
                                                          Text = x.FirstOrDefault().AcademicTerm.ToString(),
                                                          Value = x.FirstOrDefault().AcademicTerm.ToString()
                                                      })
                                         .ToList();

            return new SelectList(terms, "Value", "Text");
        }

        public SelectList GetExpectedGraduationYears(long id)
        {
            var terms = _academicProvider.GetTermsByAcademicLevelId(id);
            var yearList = terms.GroupBy(x => x.AcademicYear)
                                .Select(x => x.FirstOrDefault().AcademicYear)
                                .ToList();

            var lastYear = yearList.OrderByDescending(x => x).FirstOrDefault();
            for (var i = lastYear; i < (lastYear + 3); i++)
            {
                yearList.Add((i + 1));
            }

            var termSelectList = yearList.Select(x => new SelectListItem
                                                      {
                                                          Text = x.ToString().Substring(2) + "-" +(x + 1).ToString().Substring(2),
                                                          Value = x.ToString()
                                                      })
                                         .OrderBy(x => x.Text)
                                         .ToList();
                                         
            return new SelectList(termSelectList, "Value", "Text");
        }

        public SelectList GetInvoicesByStudentCodeAndTermId(string studentCode, long termId)
        {
            var invoices = _receiptProvider.GetInvoicesByStudentCodeAndTermId(studentCode, termId)
                                           .Select(x => new SelectListItem
                                                        {
                                                            Text = $"{ x.Number } ({ x.CreatedAtText })",
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(invoices, "Value", "Text");
        }

        public SelectList GetInvoiceType()
        {
            var invoiceType = new List<SelectListItem>
                                      {
                                          new SelectListItem { Text = "All", Value = "All"},
                                          new SelectListItem { Text = Invoice.TYPE_REGISTRATION, Value = Invoice.TYPE_REGISTRATION},
                                          new SelectListItem { Text = Invoice.TYPE_ADD_DROP, Value = Invoice.TYPE_ADD_DROP}
                                      };

            return new SelectList(invoiceType, "Value", "Text");
        }

        public SelectList GetInvoiceRefundType()
        {
            var invoiceType = new List<SelectListItem>
                                      {
                                          new SelectListItem { Text = "All", Value = "All"},
                                          new SelectListItem { Text = Invoice.REFUND_TYPE_NON_REFUND, Value = Invoice.REFUND_TYPE_NON_REFUND},
                                          new SelectListItem { Text = Invoice.REFUND_TYPE_REFUND, Value = Invoice.REFUND_TYPE_REFUND},
                                          new SelectListItem { Text = Invoice.REFUND_TYPE_BALANCE, Value = Invoice.REFUND_TYPE_BALANCE}
                                      };

            return new SelectList(invoiceType, "Value", "Text");
        }

        public SelectList GetTermTypes()
        {
            var termTypes = _db.TermTypes.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(termTypes, "Value", "Text");
        }

        public SelectList GetCurriculumTermTypes()
        {
            var curriculumTermTypes = new List<SelectListItem>
                                      {
                                          new SelectListItem { Text = "Semester", Value = "s"},
                                          new SelectListItem { Text = "Trimester", Value = "t"}
                                      };

            return new SelectList(curriculumTermTypes, "Value", "Text");
        }

        public SelectList GetEndedTermByStartedTerm(long id)
        {
            var term = _academicProvider.GetTerm(id);
            var terms = _db.Terms.Where(x => x.AcademicYear > term.AcademicYear
                                             || (x.AcademicYear == term.AcademicYear
                                                 && x.AcademicTerm >= term.AcademicTerm))
                                 .Select(x => new SelectListItem
                                 {
                                     Text = x.TermText,
                                     Value = x.Id.ToString()
                                 })
                                 .OrderBy(x => x.Text);
            return new SelectList(terms, "Value", "Text");
        }

        public SelectList GetTermsByStudentCode(string code)
        {
            var student = _studentProvider.GetStudentInformationByCode(code);
            if (student != null)
            {
                var studentTerms = _academicProvider.GetTermsByAcademicLevelId(student.AcademicInformation.AcademicLevelId);
                var terms = studentTerms.Select(x => new SelectListItem
                {
                    Text = x.TermText,
                    Value = x.Id.ToString()
                });
                return new SelectList(terms, "Value", "Text");
            }

            return null;
        }

        public SelectList GetCurriculum()
        {
            var statuses = _db.Curriculums.Include(x => x.AcademicLevel)
                                          .OrderBy(x => x.AcademicLevel.Code)
                                              .ThenBy(x => x.NameEn)
                                          .Select(x => new SelectListItem
                                          {
                                              Text = x.CodeAndName,
                                              Value = x.Id.ToString()
                                          });
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetCurriculumByAcademicLevelId(long academicLevelId)
        {
            var statuses = _db.Curriculums.Include(x => x.AcademicLevel)
                                          .Where(x => x.AcademicLevelId == academicLevelId)
                                          .OrderBy(x => x.AcademicLevel.Code)
                                              .ThenBy(x => x.NameEn)
                                          .Select(x => new SelectListItem
                                          {
                                              Text = x.CodeAndName,
                                              Value = x.Id.ToString()
                                          });
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetCurriculumByCurriculumInformation(Guid studentId)
        {
            var curriculum = _db.CurriculumInformations.IgnoreQueryFilters()
                                                       .Include(x => x.CurriculumVersion)
                                                           .ThenInclude(x => x.Curriculum)
                                                       .Where(x => x.StudentId == studentId)
                                                       .OrderBy(x => x.CurriculumVersion.Curriculum.NameEn)
                                                       .Select(x => new SelectListItem
                                                                    {
                                                                        Text = x.CurriculumVersion.NameEn,
                                                                        Value = x.CurriculumVersionId.ToString()
                                                                    });
            return new SelectList(curriculum, "Value", "Text");
        }

        public SelectList GetCurriculumByDepartment(long academicLevelId, long facultyId, long departmentId)
        {
            var statuses = _db.Curriculums.Include(x => x.AcademicLevel)
                                          .Where(x => x.AcademicLevelId == academicLevelId
                                                      && x.FacultyId == facultyId
                                                      && x.DepartmentId == departmentId)
                                          .Select(x => new SelectListItem
                                          {
                                              Text = x.CodeAndName,
                                              Value = x.Id.ToString()
                                          })
                                          .OrderBy(x => x.Text);
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetCurriculumsByDepartmentIds(long academicLevelId, List<long> facultyIds, List<long> departmentIds)
        {
            var statuses = _academicProvider.GetCurriculumsByDepartmentIds(academicLevelId, facultyIds, departmentIds)
                                            .Select(x => new SelectListItem
                                            {
                                                Text = x.CodeAndName,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text);
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetCurriculumByOpenedTermAndClosedTerm(long academicLevelId, long? openedTermId, long? closedTermId)
        {
            var curriculums = _db.Curriculums.Include(x => x.AcademicLevel)
                                             .Include(x => x.CurriculumVersions)
                                             .Where(x => x.AcademicLevelId == academicLevelId
                                                         && x.CurriculumVersions.Any(y => y.OpenedTermId == null || y.OpenedTermId == openedTermId)
                                                         && x.CurriculumVersions.Any(y => y.ClosedTermId == null || y.ClosedTermId == closedTermId))
                                             .Select(x => new SelectListItem
                                             {
                                                 Text = x.CodeAndName,
                                                 Value = x.Id.ToString()
                                             })
                                             .OrderBy(x => x.Text);
            return new SelectList(curriculums, "Value", "Text");
        }

        public SelectList GetCurriculumVersions()
        {
            var curriculumVersions = _db.CurriculumVersions.Select(x => new SelectListItem
                                                                        {
                                                                            Text = x.CodeAndName,
                                                                            Value = x.Id.ToString()
                                                                        })
                                                           .OrderBy(x => x.Text);

            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetCurriculumVersion(long curriculumId)
        {
            var curriculumVersions = _db.CurriculumVersions.Where(x => x.CurriculumId == curriculumId)
                                                           .OrderBy(x => x.NameEn)
                                                           .Select(x => new SelectListItem
                                                           {
                                                               Text = x.CodeAndName,
                                                               Value = x.Id.ToString()
                                                           });

            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetCurriculumVersionsByCurriculumIds(long academicLevelId, List<long> curriculumIds)
        {
            var curriculumVersions = _academicProvider.GetCurriculumVersionsByCurriculumIds(academicLevelId, curriculumIds)
                                                      .Select(x => new SelectListItem
                                                      {
                                                          Text = x.CodeAndName,
                                                          Value = x.Id.ToString()
                                                      })
                                                      .OrderBy(x => x.Text);

            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetCurriculumVersionsByCurriculumId(long curriculumId)
        {
            var curriculumVersions = _db.CurriculumVersions.Where(x => x.CurriculumId == curriculumId)
                                                            .Select(x => new SelectListItem
                                                            {
                                                                Text = x.CodeAndName,
                                                                Value = x.Id.ToString()
                                                            })
                                                            .OrderBy(x => x.Text);

            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetCurriculumVersionsByCurriculumIdAndStudentId(Guid studentId, long curriculumId)
        {
            var curriculumVersions = _curriculumProvider.GetCurriculumVersionsByCurriculumIdAndStudentId(studentId, curriculumId)
                                                        .Select(x => new SelectListItem
                                                                     {
                                                                         Text = x.CodeAndName,
                                                                         Value = x.Id.ToString()
                                                                     })
                                                        .OrderBy(x => x.Text);
            
            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetCurriculumVersionsByCurriculumInformation(string studentCode, string language)
        {
            var curriculumVersions = _db.CurriculumInformations.Include(x => x.CurriculumVersion)
                                                               .Where(x => x.Student.Code == studentCode)
                                                               .IgnoreQueryFilters()
                                                               .Select(x => new SelectListItem
                                                                            {
                                                                                Text = language == "en" ? x.CurriculumVersion.CodeAndName
                                                                                                        : x.CurriculumVersion.CodeAndNameTh,
                                                                                Value = x.CurriculumVersionId.ToString()
                                                                            })
                                                               .OrderBy(x => x.Text);
            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetSelectableCurriculumList(string studentCode)
        {
            var studentExist = _studentProvider.IsExistStudent(studentCode);
            if (studentExist)
            {
                var student = _studentProvider.GetStudentByCode(studentCode);
                if (student != null)
                {
                    var versionCurriculumIds = _db.CurriculumVersions.Where(x => x.Id != student.AcademicInformation.CurriculumVersionId)
                                                                     .Select(x => x.CurriculumId)
                                                                     .ToList();

                    var version = _db.Curriculums.Where(x => x.AcademicLevelId == student.AcademicInformation.AcademicLevelId
                                                             && versionCurriculumIds.Contains(x.Id))
                                                 .Distinct()
                                                 .Select(x => new SelectListItem
                                                 {
                                                     Text = x.CodeAndName,
                                                     Value = x.Id.ToString()
                                                 })
                                                 .OrderBy(x => x.Text);

                    return new SelectList(version, "Value", "Text"); ;
                }
            }

            return null;
        }


        public SelectList GetSelectableCurriculumVersionList(long curriculumId, string studentCode)
        {
            var studentExist = _studentProvider.IsExistStudent(studentCode);
            if (studentExist)
            {
                var student = _studentProvider.GetStudentByCode(studentCode);
                var batch = student.AcademicInformation.Batch;
                var admissionTerm = _db.AdmissionInformations.Include(x => x.AdmissionTerm)
                                                             .FirstOrDefault(x => x.StudentId == student.Id)?.AdmissionTerm;
                if (student != null)
                {
                    var studentAdmissionTerm = (from admission in _db.AdmissionInformations
                                                join term in _db.Terms on admission.AdmissionTermId equals term.Id
                                                where admission.StudentId == student.Id
                                                select term).SingleOrDefault();

                    var curriculumVersions = _db.CurriculumVersions.Include(x => x.OpenedTerm)
                                                                   .Include(x => x.ClosedTerm)
                                                                   .Where(x => x.CurriculumId == curriculumId
                                                                               && (batch == 0 
                                                                                   || ((!x.StartBatch.HasValue || x.StartBatch <=  batch) 
                                                                                            &&  (!x.EndBatch.HasValue || x.EndBatch >= batch))
                                                                                   )
                                                                               && (x.OpenedTerm == null
                                                                                   || (studentAdmissionTerm.AcademicYear > x.OpenedTerm.AcademicYear
                                                                                        || (studentAdmissionTerm.AcademicYear == x.OpenedTerm.AcademicYear && studentAdmissionTerm.AcademicTerm >= x.OpenedTerm.AcademicTerm)))
                                                                               && (x.ClosedTerm == null
                                                                                   || (studentAdmissionTerm.AcademicYear < x.ClosedTerm.AcademicYear
                                                                                       || (studentAdmissionTerm.AcademicYear == x.ClosedTerm.AcademicYear && studentAdmissionTerm.AcademicTerm <= x.ClosedTerm.AcademicTerm))))
                                                                   .Distinct()
                                                                   .Select(x => new SelectListItem
                                                                   {
                                                                       Text = x.CodeAndName,
                                                                       Value = x.Id.ToString()
                                                                   })
                                                                   .OrderBy(x => x.Text);

                    return new SelectList(curriculumVersions, "Value", "Text");
                }
            }
            return null;
        }

        public SelectList GetCurriculumCourseGroups(long versionId)
        {
            var groups = _curriculumProvider.GetCourseGroups(versionId, 0, 0)
                                            .Select(x => new SelectListItem
                                                         {
                                                             Text = x.NameEn,
                                                             Value = x.Id.ToString()
                                                         })
                                            .OrderBy(x => x.Text);

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetCurriculumCourseGroupsAndChildren(long versionId)
        {
            var groups = _curriculumProvider.GetCourseGroupsAndChildren(versionId, 0, 0)
                                            .Select(x => new SelectListItem
                                                         {
                                                             Text = x.FullPathName,
                                                             Value = x.Id.ToString()
                                                         })
                                            .OrderBy(x => x.Text);

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetCurriculumCourseGroupsForChangeCurriculum(long versionId)
        {
            var groups = _curriculumProvider.GetCourseGroupsForChangeCurriculum(versionId, 0, 0)
                                            .Select(x => new SelectListItem
                                            {
                                                Text = x.NameEn,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text);

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetCourseGroupByCurriculumsVersionAndCourse(long curriculumVersionId, long courseId)
        {
            var groups = (from courseGroup in _db.CourseGroups
                          join curriculumCourse in _db.CurriculumCourses on courseGroup.Id equals curriculumCourse.CourseGroupId
                          where courseGroup.CurriculumVersionId == curriculumVersionId
                                && curriculumCourse.CourseId == courseId
                          orderby courseGroup.NameEn
                          select new SelectListItem
                          {
                              Text = courseGroup.NameEn,
                              Value = courseGroup.Id.ToString()
                          });

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetFilterCourseGroups()
        {
            var groups = _db.FilterCourseGroups.Select(x => new SelectListItem
                                                            {
                                                                Text = x.Name,
                                                                Value = x.Id.ToString()
                                                            })
                                               .OrderBy(x => x.Text);

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetFilterCourseGroupsByFacultyId(long facultyId)
        {
            var groups = _db.FacultyMembers.Where(x => x.FilterCourseGroupId != null
                                                       && (facultyId == 0
                                                           || x.FacultyId == facultyId))
                                           .GroupBy(x => x.FilterCourseGroupId)
                                           .Select(x => new SelectListItem
                                                        {
                                                            Text = x.FirstOrDefault().FilterCourseGroup.Name,
                                                            Value = x.Key.ToString()
                                                        })
                                           .OrderBy(x => x.Text);

            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetSectionStatuses()
        {
            var sections = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Confirm", Value = "c"},
                               new SelectListItem { Text = "Approved", Value = "a"}
                           };

            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetCourseToBeOfferedSectionStatuses()
        {
            var sections = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Confirm", Value = "c"}
                           };

            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSectionTypes()
        {
            var sections = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Master", Value = "m"},
                               new SelectListItem { Text = "Joint", Value = "j"},
                               new SelectListItem { Text = "Ghost", Value = "g"},
                               new SelectListItem { Text = "Outbound", Value = "o"}
                           };

            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetCourseGroupTypes()
        {
            var courseGroupTypes = new List<SelectListItem>
                                   {
                                       new SelectListItem { Text = "Required", Value = "r"},
                                       new SelectListItem { Text = "Elective", Value = "e"}
                                   };

            return new SelectList(courseGroupTypes, "Value", "Text");
        }

        public SelectList GetChangeNameType()
        {
            var changeType = new List<SelectListItem>
                             {
                                 new SelectListItem { Text = "Change", Value = "c" },
                                 new SelectListItem { Text = "Spelling", Value = "s" }
                             };

            return new SelectList(changeType, "Value", "Text");
        }

        public SelectList GetChangeFlagType()
        {
            var flagType = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Firstname", Value = "f" },
                               new SelectListItem { Text = "Lastname", Value = "l" },
                               new SelectListItem { Text = "Both", Value = "b" }
                           };

            return new SelectList(flagType, "Value", "Text");
        }

        public SelectList GetMinorsByCurriculumVersionId(long versionId)
        {
            var minors = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                         .Where(x => x.CurriculumVersionId == versionId
                                                     && x.SpecializationGroupId != null
                                                     && x.SpecializationGroup.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                         .Select(x => new
                                         {
                                             Name = x.SpecializationGroup.NameEn,
                                             Id = x.SpecializationGroup.Id
                                         })
                                         .Distinct()
                                         .ToList();

            var selectList = minors.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(selectList, "Value", "Text");
        }

        public SelectList GetMinors()
        {
            var minors = _db.SpecializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                 .Select(x => new SelectListItem
                                                 {
                                                     Text = x.NameEn,
                                                     Value = x.Id.ToString()
                                                 })
                                                 .OrderBy(x => x.Text);
            return new SelectList(minors, "Value", "Text");
        }

        public SelectList GetConcentrationsByCurriculumVersionId(long versionId)
        {
            var concentrations = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                                 .Where(x => x.CurriculumVersionId == versionId
                                                             && x.SpecializationGroupId != null
                                                             && x.SpecializationGroup.Type == SpecializationGroup.TYPE_CONCENTRATION_CODE)
                                                 .Select(x => new
                                                 {
                                                     Name = x.SpecializationGroup.NameEn,
                                                     Id = x.SpecializationGroup.Id
                                                 })
                                                 .Distinct()
                                                 .ToList();

            var selectList = concentrations.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(selectList, "Value", "Text");
        }

        public SelectList GetConcentrations()
        {
            var concentrations = _db.SpecializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_CONCENTRATION_CODE)
                                                         .Select(x => new SelectListItem
                                                         {
                                                             Text = x.NameEn,
                                                             Value = x.Id.ToString()
                                                         });
            return new SelectList(concentrations, "Value", "Text");
        }

        public SelectList GetModulesByCurriculumVersionId(long versionId)
        {
            var concentrations = _db.CourseGroups.Include(x => x.SpecializationGroup)
                                                 .Where(x => x.CurriculumVersionId == versionId
                                                             && x.SpecializationGroupId != null
                                                             && x.SpecializationGroup.Type == SpecializationGroup.TYPE_MODULE_CODE)
                                                 .Select(x => new
                                                 {
                                                     Name = x.SpecializationGroup.NameEn,
                                                     Id = x.SpecializationGroup.Id
                                                 })
                                                 .Distinct()
                                                 .ToList();

            var selectList = concentrations.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(selectList, "Value", "Text");
        }

        public SelectList GetModules()
        {
            var concentrations = _db.SpecializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_MODULE_CODE)
                                                         .Select(x => new SelectListItem
                                                         {
                                                             Text = x.NameEn,
                                                             Value = x.Id.ToString()
                                                         });
            return new SelectList(concentrations, "Value", "Text");
        }

        public SelectList GetAbilities()
        {
            var abilities = _db.SpecializationGroups.Where(x => x.Type == SpecializationGroup.TYPE_ABILITY_CODE)
                                                    .Select(x => new SelectListItem
                                                    {
                                                        Text = x.NameEn,
                                                        Value = x.Id.ToString()
                                                    });
            return new SelectList(abilities, "Value", "Text");
        }

        public SelectList GetSpecializationGroups()
        {
            var specializationGroup = _curriculumProvider.GetSpecializationInformations()
                                                         .Select(x => new SelectListItem
                                                                      {
                                                                          Text = $"{x.TypeText} : {x.NameEn}",
                                                                          Value = x.Id.ToString()
                                                                      });
            return new SelectList(specializationGroup, "Value", "Text");
        }

        public SelectList GetSpecializationGroupByCurriculumVersionId(long curriculumVersionId)
        {
            var specializationGroup = _curriculumProvider.GetSpecializationInformationByCurriculumVersionId(curriculumVersionId)
                                                         .Select(x => new SelectListItem
                                                                      {
                                                                          Text = $"{x.TypeText} : {x.NameEn}",
                                                                          Value = x.Id.ToString()
                                                                      });
            return new SelectList(specializationGroup, "Value", "Text");
        }

        public SelectList GetCurriculumVersionForAdmissionCurriculum(long termId, long admissionRoundId, long facultyId, long? departmentId = null)
        {
            var curriculumVersions = _admissionProvider.GetCurriculumVersionForAdmissionCurriculum(termId, admissionRoundId, facultyId, departmentId)
                                                       .Select(x => new SelectListItem
                                                       {
                                                           Text = x.CodeAndName,
                                                           Value = x.Id.ToString()
                                                       });
            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetFilterCurriculumVersionGroups()
        {
            var groups = _db.FilterCurriculumVersionGroups.Select(x => new SelectListItem
                                                                       {
                                                                           Text = x.Name,
                                                                           Value = x.Id.ToString()
                                                                       });
            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetDependencyTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Corequisite", Value = "Corequisite" },
                            new SelectListItem { Text = "Course Equivalence", Value = "Equivalence" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetCorequisites()
        {
            var corequisites = _db.Corequisites.Include(x => x.FirstCourse)
                                               .Include(x => x.SecondCourse)
                                               .Select(x => new SelectListItem
                                                            {
                                                                Text = $"{ x.FirstCourse.CourseAndCredit } [{ x.FirstCourse.MUICId }] and { x.SecondCourse.CourseAndCredit } [{ x.SecondCourse.MUICId }]",
                                                                Value = x.Id.ToString()
                                                            });
            return new SelectList(corequisites, "Value", "Text");
        }

        public SelectList GetCourseEquivalents()
        {
            var equivalents = _db.CourseEquivalents.Include(x => x.Course)
                                                   .Include(x => x.EquilaventCourse)
                                                   .Select(x => new SelectListItem
                                                                {
                                                                    Text = $"{ x.Course.CourseAndCredit } [{ x.Course.MUICId }] and { x.EquilaventCourse.CourseAndCredit } [{ x.EquilaventCourse.MUICId }]",
                                                                    Value = x.Id.ToString()
                                                                });
            return new SelectList(equivalents, "Value", "Text");
        }

        public SelectList GetProbations()
        {
            var probations = _db.Probations.Select(x => new SelectListItem
            {
                Text = x.ProbationGPA,
                Value = x.Id.ToString()
            });
            return new SelectList(probations, "Value", "Text");
        }

        public SelectList GetRetires()
        {
            var retires = _db.Retires.Select(x => new SelectListItem
                                                  {
                                                      Text = x.Name,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(retires, "Value", "Text");
        }

        public SelectList GetSortByForStudentProbation()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "Student Code", Value = "s" });
            list.Add(new SelectListItem { Text = "Major", Value = "m" });
            list.Add(new SelectListItem { Text = "GPA", Value = "g" });
            list.Add(new SelectListItem { Text = "Advisor", Value = "a" });
            return new SelectList(list, "Value", "Text"); ;
        }

        public SelectList GetSortByForSignatureSheet()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "ID", Value = "i" });
            list.Add(new SelectListItem { Text = "Name", Value = "n" });
            return new SelectList(list, "Value", "Text"); ;
        }

        public SelectList GetStudentStatisticByProvinceAndSchoolReportType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Province", Value = "p" },
                            new SelectListItem { Text = "School", Value = "s" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetBatches()
        {
            var batch = _db.AcademicInformations.OrderByDescending(x => x.Batch)
                                                .Select(x => x.Batch)
                                                .Distinct()
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.ToString(),
                                                    Value = x.ToString()
                                                });
            return new SelectList(batch, "Value", "Text");
        }

        public SelectList GetMaintenanceTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "all" },
                            new SelectListItem { Text = "Change", Value = "change" },
                            new SelectListItem { Text = "Add", Value = "add" },
                            new SelectListItem { Text = "Delete", Value = "delete" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetTransactionFinalGradeTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "s" },
                            new SelectListItem { Text = "Date", Value = "d" },
                            new SelectListItem { Text = "Month", Value = "m" },
                            new SelectListItem { Text = "Term", Value = "t" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetConditionTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "And", Value = "and"},
                            new SelectListItem { Text = "Or", Value = "or"},
                            new SelectListItem { Text = "Course Group", Value = "coursegroup" },
                            new SelectListItem { Text = "Credit", Value = "credit" },
                            new SelectListItem { Text = "GPA", Value = "gpa" },
                            new SelectListItem { Text = "Grade", Value = "grade" },
                            new SelectListItem { Text = "Term", Value = "term" },
                            new SelectListItem { Text = "Total Course Group", Value = "totalcoursegroup" },
                            new SelectListItem { Text = "Batch", Value = "batch" },
                            new SelectListItem { Text = "Ability", Value = "ability" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetConditionsByType(string conditionType)
        {
            var conditions = new List<SelectListItem>();

            if (conditionType == "and")
            {
                conditions = _prerequisiteProvider.GetAndConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Description + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .Distinct()
                                                  .ToList();
            }
            else if (conditionType == "or")
            {
                conditions = _prerequisiteProvider.GetOrConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Description + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .Distinct()
                                                  .ToList();
            }
            else if (conditionType == "coursegroup")
            {
                conditions = _prerequisiteProvider.GetCourseGroupConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.CourseGroupConditionName + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "credit")
            {
                conditions = _prerequisiteProvider.GetCreditConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.CreditConditionName  + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "gpa")
            {
                conditions = _prerequisiteProvider.GetGPAConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.GPAConditionName + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "grade")
            {
                conditions = _prerequisiteProvider.GetGradeConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.GradeConditionName + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "term")
            {
                conditions = _prerequisiteProvider.GetTermConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.TermConditionName + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "totalcoursegroup")
            {
                conditions = _prerequisiteProvider.GetTotalCourseGroupConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.TotalCourseGroupConditionName + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "batch")
            {
                conditions = _prerequisiteProvider.GetBatchConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Batch.ToString() + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else if (conditionType == "ability")
            {
                conditions = _prerequisiteProvider.GetAbilityConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Ability.NameEn.ToString() + "[" + x.Id.ToString() + "]",
                                                      Value = x.Id.ToString()
                                                  })
                                                  .ToList();
            }
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetGradeConditions()
        {
            var conditions = _prerequisiteProvider.GetGradeConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.GradeConditionName,
                                                      Value = x.Id.ToString()
                                                  });

            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetCourseGroupConditions()
        {
            var conditions = _prerequisiteProvider.GetCourseGroupConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.CourseGroupConditionName,
                                                      Value = x.Id.ToString()
                                                  });

            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetTermConditions()
        {
            var conditions = _prerequisiteProvider.GetTermConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.TermConditionName,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetGPAConditions()
        {
            var conditions = _prerequisiteProvider.GetGPAConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.GPAConditionName,
                                                      Value = x.Id.ToString()
                                                  });

            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetCreditConditions()
        {
            var conditions = _prerequisiteProvider.GetCreditConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.CreditConditionName,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetTotalCourseGroupConditions()
        {
            var conditions = _prerequisiteProvider.GetTotalCourseGroupConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.TotalCourseGroupConditionName,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetBatchConditions()
        {
            var conditions = _prerequisiteProvider.GetBatchConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Batch.ToString(),
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetAbilityConditions()
        {
            var conditions = _prerequisiteProvider.GetAbilityConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Ability.NameEn.ToString(),
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetAndConditions()
        {
            var conditions = _prerequisiteProvider.GetAndConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Description,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetOrConditions()
        {
            var conditions = _prerequisiteProvider.GetOrConditions()
                                                  .Select(x => new SelectListItem
                                                  {
                                                      Text = x.Description,
                                                      Value = x.Id.ToString()
                                                  });
            return new SelectList(conditions, "Value", "Text");
        }

        public SelectList GetTransferUniversities()
        {
            var transferUniversities = _db.TransferUniversities.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(transferUniversities, "Value", "Text");
        }

        public SelectList GetImplementedCurriculumVersionsByStudentId(Guid studentId, long facultyId = 0, long departmentId = 0)
        {
            var curriculumVersions = _curriculumProvider.GetImplementedCurriculumVersionsByStudentId(studentId, facultyId, departmentId)
                                                        .Select(x => new SelectListItem
                                                        {
                                                            Text = x.CodeAndName,
                                                            Value = x.Id.ToString()
                                                        });
            return new SelectList(curriculumVersions, "Value", "Text");
        }

        public SelectList GetDegreeNamesByAcademicLevelId(long academicLevelId)
        {
            var degreeNames = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                    .Where(x => x.Curriculum.AcademicLevelId == academicLevelId)
                                                    .Select(x => x.DegreeNameEn)
                                                    .Distinct()
                                                    .OrderBy(x => x)
                                                    .Select(x => new SelectListItem
                                                    {
                                                        Text = x,
                                                        Value = x
                                                    });
            return new SelectList(degreeNames, "Value", "Text");
        }

        public SelectList GetProjectTypes()
        {
            var projects = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Thesis", Value = "t" },
                               new SelectListItem { Text = "Dissertation", Value = "d" }
                           };

            return new SelectList(projects, "Value", "Text");
        }
        #endregion

        #region Grade
        public SelectList GetGrades()
        {
            var grades = _gradeProvider.GetGrades().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(grades, "Value", "Text");
        }

        public SelectList GetWeightGrades()
        {
            var grades = _gradeProvider.GetWeightGrades().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(grades, "Value", "Text");
        }

        public SelectList GetNonWeightGrades()
        {
            var grades = _gradeProvider.GetNonWeightGrades().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(grades, "Value", "Text");
        }

        public SelectList GetGradesExcept(long gradeId, List<long> gradesInTemplate)
        {
            var grades = _db.Grades.Where(x => (x.Id != gradeId
                                                || gradeId == 0)
                                                && gradesInTemplate.Contains(x.Id))
                                   .OrderByDescending(x => x.Weight)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.Name,
                                       Value = x.Id.ToString()
                                   }).ToList();

            if (gradeId != 0)
            {
                grades.Add(new SelectListItem() { Text = "N/A", Value = null });
            }

            return new SelectList(grades, "Value", "Text");
        }

        public SelectList GetGradeTemplates()
        {
            var gradeTemplates = _db.GradeTemplates.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(gradeTemplates, "Value", "Text");
        }

        public SelectList GetTransferedGrades()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Same Previous Grade", Value = "s" },
                            new SelectListItem { Text = "T", Value = "T" },
                            new SelectListItem { Text = "t", Value = "t" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetStandardGradingScoreType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Score", Value = "s" },
                            new SelectListItem { Text = "Percentage", Value = "p" }
                        };
            return new SelectList(types, "Value", "Text");
        }
        
        public SelectList GetStandardGradingGroups()
        {
            var groups = _db.StandardGradingGroups.Select(x => new SelectListItem
                                                               {
                                                                   Text = x.Name,
                                                                   Value = x.Id.ToString()
                                                               });
            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetGradingByPageScoreInstructor()
        {
            var groups = _db.Grades.Where(x => x.Name.ToUpper() == "I" || x.Name.ToUpper() == "AU")
                                   .Select(x => new SelectListItem
                                                {
                                                    Text = x.Name,
                                                    Value = x.Id.ToString()
                                                });
            return new SelectList(groups, "Value", "Text");
        }
        #endregion

        #region Admission
        public SelectList GetAdmissionTypes()
        {
            var admissiontypes = _db.AdmissionTypes.Select(x => new SelectListItem
                                                                {
                                                                    Text = x.NameEn,
                                                                    Value = x.Id.ToString()
                                                                });
            return new SelectList(admissiontypes, "Value", "Text");
        }

        public SelectList GetPreviousSchools(long schoolGroupId = 0)
        {
            var preSchools = _db.PreviousSchools.Where(x => schoolGroupId == 0
                                                            || x.SchoolGroupId == schoolGroupId)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = $"{ x.NameEn } / { x.NameTh }",
                                                    Value = x.Id.ToString()
                                                })
                                                .OrderBy(x => x.Text);
            return new SelectList(preSchools, "Value", "Text");
        }

        public SelectList GetPreviousSchoolsByCountryId(long countryId)
        {
            var preSchools = _admissionProvider.GetPreviousSchoolsByCountryId(countryId)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = $"{ x.NameEn } / { x.NameTh }",
                                                   Value = x.Id.ToString()
                                               })
                                               .OrderBy(x => x.Text);
            return new SelectList(preSchools, "Value", "Text");
        }

        public SelectList GetSchoolTypes()
        {
            var schoolTypes = _db.SchoolTypes.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(schoolTypes, "Value", "Text");
        }

        public SelectList GetSchoolTerritories()
        {
            var schoolTerritories = _db.SchoolTerritories.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(schoolTerritories, "Value", "Text");
        }

        public SelectList GetEducationBackground()
        {
            var educations = _db.EducationBackgrounds.Select(x => new SelectListItem
            {
                Text = $"{ x.NameEn } / { x.NameTh }",
                Value = x.Id.ToString()
            });
            return new SelectList(educations, "Value", "Text");
        }

        public SelectList GetSchoolGroup()
        {
            var schoolGroup = _db.SchoolGroups.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                              .OrderBy(x => x.Text);
            return new SelectList(schoolGroup, "Value", "Text");
        }

        public SelectList GetDocuments()
        {
            var documents = _db.Documents.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(documents, "Value", "Text");
        }

        public SelectList GetAdmissionRounds(long academicLevelId, long termId = 0)
        {
            var admissionRounds = _admissionProvider.GetAdmissionRoundByAcademicLevelIdAndTermId(academicLevelId, termId)
                                                    .Select(x => new SelectListItem
                                                    {
                                                        Text = $"{ x.AdmissionTerm.TermText } Round { x.Round }",
                                                        Value = x.Id.ToString()
                                                    });
            return new SelectList(admissionRounds, "Value", "Text");
        }

        public SelectList GetEntranceExamResults()
        {
            var results = new List<SelectListItem>
                          {
                              new SelectListItem { Text = "Pass", Value = "p" },
                              new SelectListItem { Text = "Fail", Value = "f" },
                              new SelectListItem { Text = "Non-test or no result", Value = "n" }
                          };
            return new SelectList(results, "Value", "Text");
        }

        public SelectList GetAdmissionChannels()
        {
            var channels = _db.AdmissionChannels.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                                 .OrderBy(x => x.Text);
            return new SelectList(channels, "Value", "Text");
        }

        public SelectList GetAdmissionPlaces()
        {
            var places = _db.AdmissionPlaces.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                             .OrderBy(x => x.Text);
            return new SelectList(places, "Value", "Text");
        }

        public SelectList GetAgencies()
        {
            var agencies = _db.Agencies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
                                       .OrderBy(x => x.Text);
            return new SelectList(agencies, "Value", "Text");
        }

        public SelectList GetExemptedAdmissionExaminations()
        {
            var examinations = _db.ExemptedAdmissionExaminations.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                                               .OrderBy(x => x.Text);
            return new SelectList(examinations, "Value", "Text");
        }

        public SelectList GetAdmissionExaminationTypes()
        {
            var examinations = _db.AdmissionExaminationTypes.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                                            .OrderBy(x => x.Text);
            return new SelectList(examinations, "Value", "Text");
        }

        public SelectList GetStudentStatuses(GetStudentStatusesEnum type = GetStudentStatusesEnum.Default)
        {
            var statuses = new List<SelectListItem>();
            if(type == GetStudentStatusesEnum.Default)
            {
                statuses = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "" },
                            new SelectListItem { Text = "Admission", Value = "a" },
                            new SelectListItem { Text = "Studying", Value = "s" },
                            new SelectListItem { Text = "Deleted", Value = "d" },
                            new SelectListItem { Text = "Blacklist", Value = "b" },
                            new SelectListItem { Text = "Resign", Value = "rs" },
                            new SelectListItem { Text = "Dismiss", Value = "dm" },
                            new SelectListItem { Text = "Passed all required course", Value = "prc" },
                            new SelectListItem { Text = "Passed away", Value = "pa" },
                            new SelectListItem { Text = "Graduated", Value = "g" },
                            new SelectListItem { Text = "Graduated with first class honor", Value = "g1" },
                            new SelectListItem { Text = "Graduated with second class honor", Value = "g2" },
                            new SelectListItem { Text = "Exchange", Value = "ex" },
                            new SelectListItem { Text = "Transferred to other university", Value = "tr" },
                            new SelectListItem { Text = "Leave of absence", Value = "la" },
                            new SelectListItem { Text = "No Report", Value = "np" }
                        };
            } 
            else if (type == GetStudentStatusesEnum.DefaultWithoutAll)
            {
                statuses = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Admission", Value = "a" },
                            new SelectListItem { Text = "Studying", Value = "s" },
                            new SelectListItem { Text = "Deleted", Value = "d" },
                            new SelectListItem { Text = "Blacklist", Value = "b" },
                            new SelectListItem { Text = "Resign", Value = "rs" },
                            new SelectListItem { Text = "Dismiss", Value = "dm" },
                            new SelectListItem { Text = "Passed all required course", Value = "prc" },
                            new SelectListItem { Text = "Passed away", Value = "pa" },
                            new SelectListItem { Text = "Graduated", Value = "g" },
                            new SelectListItem { Text = "Graduated with first class honor", Value = "g1" },
                            new SelectListItem { Text = "Graduated with second class honor", Value = "g2" },
                            new SelectListItem { Text = "Exchange", Value = "ex" },
                            new SelectListItem { Text = "Transferred to other university", Value = "tr" },
                            new SelectListItem { Text = "Leave of absence", Value = "la" },
                            new SelectListItem { Text = "No Report", Value = "np" }
                        };
            }
            else if(type == GetStudentStatusesEnum.DefaultStudying)
            {
                statuses = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Studying", Value = "s" },
                            new SelectListItem { Text = "All", Value = "" },
                            new SelectListItem { Text = "Admission", Value = "a" },
                            new SelectListItem { Text = "Deleted", Value = "d" },
                            new SelectListItem { Text = "Blacklist", Value = "b" },
                            new SelectListItem { Text = "Resign", Value = "rs" },
                            new SelectListItem { Text = "Dismiss", Value = "dm" },
                            new SelectListItem { Text = "Passed all required course", Value = "prc" },
                            new SelectListItem { Text = "Passed away", Value = "pa" },
                            new SelectListItem { Text = "Graduated", Value = "g" },
                            new SelectListItem { Text = "Graduated with first class honor", Value = "g1" },
                            new SelectListItem { Text = "Graduated with second class honor", Value = "g2" },
                            new SelectListItem { Text = "Exchange", Value = "ex" },
                            new SelectListItem { Text = "Transferred to other university", Value = "tr" },
                            new SelectListItem { Text = "Leave of absence", Value = "la" },
                            new SelectListItem { Text = "No Report", Value = "np" }
                        };
            }
            else  if(type == GetStudentStatusesEnum.StudentProfile)
            {
                statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Admission", Value = "a" },
                               new SelectListItem { Text = "Studying", Value = "s" },
                               new SelectListItem { Text = "Deleted", Value = "d" },
                               new SelectListItem { Text = "Blacklist", Value = "b" },
                               //new SelectListItem { Text = "Resign", Value = "rs" },
                               new SelectListItem { Text = "Dismiss", Value = "dm" },
                               new SelectListItem { Text = "Passed all required course", Value = "prc" },
                               new SelectListItem { Text = "Passed away", Value = "pa" },
                               new SelectListItem { Text = "Exchange", Value = "ex" },
                               new SelectListItem { Text = "Transferred to other university", Value = "tr" },
                               new SelectListItem { Text = "Leave of absence", Value = "la" },
                               new SelectListItem { Text = "No Report", Value = "np" }
                           };
            }
            else if (type == GetStudentStatusesEnum.GraduationRequest)
            {
                statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Graduated", Value = "g" },
                               new SelectListItem { Text = "Graduated with first class honor", Value = "g1" },
                               new SelectListItem { Text = "Graduated with second class honor", Value = "g2" }
                           };
            }
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetStudentProfileStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Admission", Value = "a" },
                               new SelectListItem { Text = "Studying", Value = "s" },
                               new SelectListItem { Text = "Deleted", Value = "d" },
                               new SelectListItem { Text = "Blacklist", Value = "b" },
                               new SelectListItem { Text = "Resign", Value = "rs" },
                               new SelectListItem { Text = "Dismiss", Value = "dm" },
                               new SelectListItem { Text = "Passed all required course", Value = "prc" },
                               new SelectListItem { Text = "Passed away", Value = "pa" },
                               new SelectListItem { Text = "Graduated", Value = "g" },
                               new SelectListItem { Text = "Graduated with first class honor", Value = "g1" },
                               new SelectListItem { Text = "Graduated with second class honor", Value = "g2" },
                               new SelectListItem { Text = "Exchange", Value = "ex" },
                               new SelectListItem { Text = "Transferred to other university", Value = "tr" },
                               new SelectListItem { Text = "Leave of absence", Value = "la" },
                               new SelectListItem { Text = "No Report", Value = "np" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetStudents()
        {
            var students = _db.Students.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(students, "Value", "Text");
        }

        public SelectList GetStudentCodes()
        {
            var students = _db.Students.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Code
            });
            return new SelectList(students, "Value", "Text");
        }

        public SelectList GetRegistrationSummaryReportTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Course", Value = "c" },
                            new SelectListItem { Text = "Student", Value = "s" }
                        };
                        
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetRegistrationTermsByTerm(long termId)
        {
            var registrationTerms = _db.RegistrationTerms.Where(x => x.TermId == termId)
                                                         .Select(x => new SelectListItem
                                                                      {
                                                                          Text = x.Name,
                                                                          Value = x.Id.ToString()
                                                                      });

            return new SelectList(registrationTerms, "Value", "Text");
        }
        public SelectList GetRegistrationTerms()
        {
            var registrationTerms = _db.RegistrationTerms.Select(x => new SelectListItem
                                                                      {
                                                                          Text = x.Name,
                                                                          Value = x.Id.ToString()
                                                                      });

            return new SelectList(registrationTerms, "Value", "Text");
        }

        public SelectList GetRegistrationConditions()
        {
            var registrationTerms = _db.RegistrationConditions.Select(x => new SelectListItem
                                                                      {
                                                                          Text = x.Name,
                                                                          Value = x.Id.ToString()
                                                                      });

            return new SelectList(registrationTerms, "Value", "Text");
        }

        public SelectList GetStudentDocumentGroups()
        {
            var studentDocumentGroup = _db.AdmissionDocumentGroups.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(studentDocumentGroup, "Value", "Text");
        }

        public SelectList GetStudentDocumentGroupsByCountryId(long academicLevelId, long? facultyId, long? departmentId, long? previousSchoolCountryId)
        {
            var studentDocumentGroup = _admissionProvider.GetStudentDocumentGroupsByCountryId(academicLevelId, facultyId, departmentId, previousSchoolCountryId)
                                                         .Select(x => new SelectListItem
                                                         {
                                                             Text = x.Name,
                                                             Value = x.Id.ToString()
                                                         });
            return new SelectList(studentDocumentGroup, "Value", "Text");
        }

        public SelectList GetAdmissionStatuses()
        {
            var approved = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = AdmissionStatusEnum.WaitingDocument.GetDisplayName(), Value = AdmissionStatusEnum.WaitingDocument.GetDisplayName() },
                               new SelectListItem { Text = AdmissionStatusEnum.PaymentSuccess.GetDisplayName(), Value = AdmissionStatusEnum.PaymentSuccess.GetDisplayName() },
                               new SelectListItem { Text = AdmissionStatusEnum.Completed.GetDisplayName(), Value = AdmissionStatusEnum.Completed.GetDisplayName() }
                           };
            return new SelectList(approved, "Value", "Text");
        }
        #endregion

        #region Course
        public SelectList GetCourses(bool showMUICId = true)
        {
            var courses = _cacheProvider.GetCourses();
            var coursesSelectList = courses.Select(x => new SelectListItem
                                                        {
                                                            Text = x.CourseAndCredit + (!showMUICId || x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetAllowAddedCourses(long includedCourseId = 0,bool showMUICId = true)
        {
            var courses = _db.Courses.AsNoTracking()
                                     .IgnoreQueryFilters()
                                     .Where(x => x.TransferUniversityId == null)
                                     .ToList();
            if (includedCourseId > 0)
            {
                courses = courses.Where(x => (x.IsActive && x.IsAllowAddNewSection) || x.Id == includedCourseId).ToList();
            }
            else
            {
                courses = courses.Where(x => x.IsActive && x.IsAllowAddNewSection).ToList();
            }
            var coursesSelectList = courses.Select(x => new SelectListItem
            {
                Text = x.CourseAndCredit + (!showMUICId || x.MUICId == null ? "" : " [" + x.MUICId + "]"),
                Value = x.Id.ToString()
            })
                                           .OrderBy(x => x.Text);
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetCoursesByAcademicLevelId(long academicLevelId)
        {
            var courses = _db.Courses.Where(x => x.AcademicLevelId == academicLevelId
                                                 && x.TransferUniversityId == null)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.CourseAndCredit + (x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCoursesByAcademicLevelAndTransferUniversity(long academicLevelId, long transferUniversityId)
        {
            var courses = _db.Courses.Where(x => x.AcademicLevelId == academicLevelId
                                                 && x.TransferUniversityId == transferUniversityId)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.CourseAndCredit + (x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCoursesByAcademicLevelAndTerm(long academicLevelId, long termId)
        {
            var courses = _db.Sections.Where(x => x.Course.AcademicLevelId == academicLevelId
                                                  && x.TermId == termId)
                                      .Select(x => new 
                                                   {
                                                       Name = x.Course.CodeAndName,
                                                       Id = x.Course.Id 
                                                   })
                                      .Distinct()
                                      .Select(x => new SelectListItem
                                                   {
                                                       Text = x.Name,
                                                       Value = x.Id.ToString()
                                                   });
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCourses(long facultyId, long departmentId)
        {
            var courses = _db.Courses.Where(x => (facultyId == 0
                                                  || x.FacultyId == facultyId)
                                                  && (departmentId == 0
                                                  || x.DepartmentId == departmentId)
                                                 && x.TransferUniversityId == null)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.CourseAndCredit + (x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                         Value = x.Id.ToString()
                                     });
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCoursesByCurriculumVersion(long academicLevelId, long curriculumVersionId)
        {
            var courses = (from courseGroup in _db.CourseGroups
                           join curriculumCourse in _db.CurriculumCourses on courseGroup.Id equals curriculumCourse.CourseGroupId
                           join course in _db.Courses on curriculumCourse.CourseId equals course.Id
                           where courseGroup.CurriculumVersionId == curriculumVersionId
                           select new SelectListItem
                           {
                               Text = course.CourseAndCredit + (course.MUICId == null ? "" : " [" + course.MUICId +"]"),
                               Value = course.Id.ToString()
                           });
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetExternalCourses()
        {
            var courses = _cacheProvider.GetExternalCourses();
            var coursesSelectList = courses.Select(x => new SelectListItem
                                                        {
                                                            Text = x.CourseAndCredit + (x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetCourseAndTransferCourse()
        {
            var courses = _cacheProvider.GetCourseAndTransferCourse();
            var coursesSelectList = courses.Select(x => new SelectListItem
                                                        {
                                                            Text = x.CourseAndCredit + (x.MUICId == null ? "" : " [" + x.MUICId +"]"),
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(coursesSelectList, "Value", "Text");
        }

        public SelectList GetCoursesByStudentAndTerm(Guid studentId, long termId)
        {
            var courses = _studentProvider.GetRegistrationCourseByStudentId(studentId, termId)
                                          .Select(x => new SelectListItem
                                                       {
                                                           Text = x.Course.CourseAndCredit + (x.Course.MUICId == null ? "" : " [" + x.Course.MUICId +"]"),
                                                           Value = x.CourseId.ToString()
                                                       });
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCoursesByStudentAndTermForCheating(Guid? studentId, long termId)
        {
            var courses = (from registration in _db.RegistrationCourses
                           join course in _db.Courses on registration.CourseId equals course.Id
                           join section in _db.Sections on registration.SectionId equals section.Id
                           where registration.IsPaid
                                 && (registration.Status == "a" || registration.Status == "r")
                                 && registration.TermId == termId
                                 && registration.StudentId == studentId
                           select new SelectListItem
                           {
                               Text = $"{course.CodeAndName} ({section.Number})",
                               Value = registration.Id.ToString()
                           }).ToList();

            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCourseRates()
        {
            var courseRates = _db.CourseRates.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(courseRates, "Value", "Text");
        }

        public SelectList GetCoursesByTerm(long termId)
        {
            var courses = _db.Sections.Include(x => x.Course)
                                      .Where(x => x.TermId == termId)
                                      .Select(x => new
                                      {
                                          Name = x.Course.CourseAndCredit + (x.Course.MUICId == null ? "" : " [" + x.Course.MUICId +"]"),
                                          Id = x.Course.Id
                                      })
                                      .Distinct()
                                      .OrderBy(x => x.Name)
                                      .Select(x => new SelectListItem
                                      {
                                          Text = x.Name,
                                          Value = x.Id.ToString()
                                      });

            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetCoursesByCourseGroup(long courseGroupId)
        {
            var courses = _db.CurriculumCourses.Include(x => x.Course)
                                               .Where(x => x.CourseGroupId == courseGroupId)
                                               .Select(x => new
                                                            {
                                                                Name = x.Course.CourseAndCredit + (x.Course.MUICId == null ? "" : " [" + x.Course.MUICId +"]"),
                                                                Id = x.Course.Id
                                                            })
                                               .Distinct()
                                               .Select(x => new SelectListItem
                                                            {
                                                                Text = x.Name,
                                                                Value = x.Id.ToString()
                                                            });

            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetTeachingCourseByInstructorId(long instructorId, long termId)
        {
            // For AU
            // var courseIds = _db.InstructorSections.Include(x => x.SectionDetail)
            //                                       .ThenInclude(x => x.Section)
            //                                       .Where(x => !x.SectionDetail.Section.IsClosed
            //                                                   && x.InstructorId == instructorId
            //                                                   && x.SectionDetail.Section.TermId == termId)
            //                                       .Select(x => x.SectionDetail.Section.CourseId)
            //                                       .Distinct()
            //                                       .ToList();
            
            // For MUIC
            var courses = _db.Sections.Include(x => x.Course)
                                        .Where(x => x.MainInstructorId == instructorId
                                                   && x.TermId == termId)
                                        .Select(x => x.Course)
                                        .Distinct()
                                        .OrderBy(x => x.CourseAndCredit);
            return new SelectList(courses.Select(x => new SelectListItem
                                        {
                                        Text = x.CourseAndCredit,
                                        Value = x.Id.ToString()
                                        }), 
                                    "Value", 
                                    "Text");
        }

        public SelectList GetExaminationTypes()
        {
            var examinationType = _db.ExaminationTypes.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(examinationType, "Value", "Text");
        }

        public SelectList GetExaminationDateByExaminationType(string studentCode, string examinationType)
        {
            var student = _studentProvider.GetStudentByCode(studentCode);
            var currentTermId = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId)?.Id ?? 0;

            var query = _db.RegistrationCourses.Include(x => x.Section)
                                               .AsNoTracking()
                                               .Where(x => x.StudentId == student.Id
                                                           && x.TermId == currentTermId
                                                           && x.Status != "d");

            if (examinationType == "Midterm")
            {
                query = query.Where(x => x.Section.MidtermDate.HasValue
                                         && x.Section.MidtermStart.HasValue
                                         && x.Section.MidtermEnd.HasValue);
            }
            else
            {
                query = query.Where(x => x.Section.FinalDate.HasValue
                                         && x.Section.FinalStart.HasValue
                                         && x.Section.FinalEnd.HasValue);
            }

            var registrations = query.ToList();

            var dates = (from registration in registrations
                         group registration by examinationType == "Midterm" ? registration.Section.MidtermDate.Value : registration.Section.FinalDate.Value
                         into g
                         let date = g.Key.ToString(StringFormat.ShortDate)
                         orderby g.Key
                         select new SelectListItem
                         {
                            Text = date,
                            Value = date
                         })
                        .ToList();

            return new SelectList(dates, "Value", "Text");
        }

        public SelectList GetExaminationCourseByExaminationDate(string examinationDate, string studentCode, string examinationType)
        {
            var examDate = _dateTimeProvider.ConvertStringToDateTime(examinationDate);
            var course = new List<SelectListItem>().AsQueryable();
            if (examinationType == "Midterm")
            {
                course = _db.RegistrationCourses.Include(x => x.Section)
                                                .Include(x => x.Student)
                                                .Where(x => x.Section.MidtermDate == examDate
                                                            && x.Student.Code == studentCode)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.Course.CourseAndCredit,
                                                    Value = x.CourseId.ToString()
                                                });
            }
            else
            {
                course = _db.RegistrationCourses.Include(x => x.Section)
                                                .Include(x => x.Student)
                                                .Where(x => x.Section.FinalDate == examDate
                                                            && x.Student.Code == studentCode)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.Course.CourseAndCredit,
                                                    Value = x.CourseId.ToString()
                                                });
            }

            return new SelectList(course, "Value", "Text");
        }

        public SelectList GetMidtermAndFinal()
        {
            var results = new List<SelectListItem>
                          {
                              new SelectListItem { Text = "Midterm", Value = "Midterm" },
                              new SelectListItem { Text = "Final", Value = "Final" }
                          };
            return new SelectList(results, "Value", "Text");
        }

        public SelectList GetIncidents()
        {
            var incidents = _db.Incidents.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(incidents, "Value", "Text");
        }

        public SelectList GetSections()
        {
            var sections = _db.Sections.Select(x => new SelectListItem
            {
                Text = x.Number,
                Value = x.Id.ToString()
            });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSections(long termId, List<long> courseIds)
        {
            var sections = _db.Sections.Where(x => !x.IsParent
                                                   && x.TermId == termId
                                                   && (courseIds == null
                                                   || courseIds.Contains(x.CourseId)))
                                       .GroupBy(x => x.Number)
                                       .Select(x => new SelectListItem
                                       {
                                           Text = x.Key,
                                           Value = x.Key.ToString()
                                       });

            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSections(long courseId)
        {
            var sections = _db.Sections.Where(x => x.CourseId == courseId)
                                       .Select(x => new SelectListItem
                                       {
                                           Text = x.Number,
                                           Value = x.Id.ToString()
                                       });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSections(long termId, long courseId)
        {
            var sections = _db.Sections.Where(x => x.CourseId == courseId
                                                   && x.TermId == termId)
                                       .Select(x => new SelectListItem
                                       {
                                           Text = x.Number,
                                           Value = x.Id.ToString()
                                       });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetParentSections(long courseId, long termId)
        {
            var sections = _db.Sections.Where(x => x.TermId == termId
                                                   && x.CourseId == courseId
                                                   && x.IsParent)
                                       .Select(x => new SelectListItem
                                       {
                                           Text = x.Number,
                                           Value = x.Id.ToString()
                                       });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSectionNumbers()
        {
            var sections = _registrationProvider.GetSectionNumbers().Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSectionsByInstructorId(long instructorId, long termId, long courseId)
        {
            var sections = _registrationProvider.GetSectionsByInstructorId(instructorId, termId, courseId)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.Number,
                                                    Value = x.Id.ToString()
                                                });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSectionByCourseId(long termId, long courseId)
        {
            var sections = _registrationProvider.GetSectionByCourseId(termId, courseId)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.Number,
                                                    Value = x.Id.ToString()
                                                });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetSectionSlotStatus()
        {
            var status = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Waiting", Value = "w" },
                             new SelectListItem { Text = "Taught", Value = "p" },
                             new SelectListItem { Text = "Cancel", Value = "c" }
                         };

            return new SelectList(status, "Value", "Text");
        }

        public SelectList GetSeatAvailableStatuses()
        {
            var seatAvailables = new List<SelectListItem>
                                 {
                                     new SelectListItem { Text = "All", Value = "" },
                                     new SelectListItem { Text = "Seat Available", Value = "true" },
                                     new SelectListItem { Text = "No Seat Available", Value = "false" }
                                 };
            return new SelectList(seatAvailables, "Value", "Text");
        }

        public SelectList GetCreditTypes()
        {
            var creditTypes = new List<SelectListItem>
                              {
                                  new SelectListItem { Text = "All", Value = "all" },
                                  new SelectListItem { Text = "Academic", Value = "academic" },
                                  new SelectListItem { Text = "Lecture", Value = "lecture" },
                                  new SelectListItem { Text = "Lab", Value = "lab" },
                                  new SelectListItem { Text = "Self Study", Value = "self" },
                                  new SelectListItem { Text = "Registration", Value = "registration" },
                                  new SelectListItem { Text = "Free Elective", Value = "free" }
                              };
            return new SelectList(creditTypes, "Value", "Text");
        }

        public SelectList GetSpecializationGroupTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Minor", Value = "m" },
                            new SelectListItem { Text = "Concentration", Value = "c" },
                            new SelectListItem { Text = "Ability", Value = "a" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetSpecializationGroupByType(string type)
        {
            var groups = _db.SpecializationGroups.Where(x => x.Type == type)
                                                 .Select(x => new SelectListItem
                                                              {
                                                                  Text = x.NameEn,
                                                                  Value = x.Id.ToString()
                                                              });
            
            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetQuestionnaireApprovalStatuses()
        {
            var status = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Waiting for Staff Approval", Value = "w" },
                            new SelectListItem { Text = "Waiting for PD Approval", Value = "s" },
                            new SelectListItem { Text = "Approved by PD", Value = "p" }
                        };

            return new SelectList(status, "Value", "Text");
        }
        #endregion

        #region Instructor
        public SelectList GetCodeFaculties(long id)
        {
            var faculties = _db.Faculties.Select(x => new SelectListItem
            {
                Selected = id == 0 ? false : (x.Id == id),
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(faculties, "Value", "Text");
        }

        public SelectList GetCodeDepartments(long id)
        {
            var departments = _db.Departments.Select(x => new SelectListItem
            {
                Selected = id == 0 ? false : (x.Id == id),
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            });
            return new SelectList(departments, "Value", "Text");
        }

        public SelectList GetInstructors()
        {
            var instructors = _db.Instructors.Select(x => new SelectListItem
                                                          {
                                                              Text = $"{ x.Title.NameEn } { x.FirstNameEn } { x.LastNameEn }",
                                                              Value = x.Id.ToString()
                                                          });
            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetInstructorById(long instructorId)
        {
            var instructors = _db.Instructors.AsNoTracking()
                                             .Where(x => (instructorId == 0 || x.Id == instructorId))
                                             .Select(x => new SelectListItem
                                                          {
                                                              Text = $"{ x.Title.NameEn } { x.FirstNameEn } { x.LastNameEn }",
                                                              Value = x.Id.ToString()
                                                          });
            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetInstructorsForCheating()
        {
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Select(x => new SelectListItem
                                                          {
                                                              Text = x.FullNameEn,
                                                              Value = x.CodeAndName
                                                          });
            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetCourseByInstructorId(long instructorId, long termId)
        {
            var sections = _db.InstructorSections.Include(x => x.SectionDetail)
                                                     .ThenInclude(x => x.Section)
                                                     .ThenInclude(x => x.Course)
                                                 .Where(x => x.InstructorId == instructorId
                                                             && x.SectionDetail.Section.TermId == termId)
                                                 .Select(x => new SelectListItem
                                                 {
                                                     Text = x.SectionDetail.Section.Course.CourseAndCredit,
                                                     Value = x.SectionDetail.Section.CourseId.ToString()
                                                 });
            return new SelectList(sections, "Value", "Text");
        }

        public SelectList GetInstructorsByFacultyId(long id)
        {
            var instructors = _instructorProvider.GetInstructorsByFacultyId(id).Select(x => new SelectListItem
                                                                                            {
                                                                                                Text = x.FullNameEn,
                                                                                                Value = x.Id.ToString()
                                                                                            });
            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetTermInstructorsByCourseId(long termId, long courseId)
        {
            var instructors = _instructorProvider.GetTermInstructorsByCourseId(termId, courseId)
                                                 .Select(x => new SelectListItem
                                                              {
                                                                  Text = x.CodeAndName,
                                                                  Value = x.Id.ToString()
                                                              });
            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetInstructorByCourseId(long termId, long courseId)
        {
            var instructors = _db.Sections.Include(x => x.MainInstructor)
                                          .Where(x => x.TermId == termId
                                                       && x.CourseId == courseId)
                                          .Select(x => x.MainInstructor)
                                          .Distinct()
                                          .Select(x => new SelectListItem
                                                       {
                                                           Text = x.FullNameEn,
                                                           Value = x.Id.ToString()
                                                       });

            return new SelectList(instructors, "Value", "Text");
        }

        public SelectList GetInstructorBySectionId(long termId, long courseId, long sectionId)
        {
            // Get Instructor For muic
            var instructors = _db.SectionSlots.Include(x => x.Instructor)
                                              .Include(x => x.Section)
                                              .Where(x => x.Section.TermId == termId
                                                           && (courseId == 0 || x.Section.CourseId == courseId)
                                                           && (sectionId == 0 || x.SectionId == sectionId))
                                              .Select(x => x.Instructor)
                                              .Distinct()
                                              .Select(x => new SelectListItem
                                                           {
                                                               Text = x.FullNameEn,
                                                               Value = x.Id.ToString()
                                                           });

            return new SelectList(instructors, "Value", "Text");
        }
        #endregion

        #region Address
        public SelectList GetCountries()
        {
            var countries = _db.Countries.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(countries, "Value", "Text");
        }

        public SelectList GetCities()
        {
            var cities = _db.Cities.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(cities, "Value", "Text");
        }

        public SelectList GetCities(long countryId)
        {
            var cities = _db.Cities.Where(x => x.CountryId == countryId)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameEn,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(cities, "Value", "Text");
        }

        public SelectList GetCities(long countryId, long stateId)
        {
            var cities = _db.Cities.Where(x => x.CountryId == countryId
                                               && x.StateId == stateId)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameEn,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(cities, "Value", "Text");
        }

        public SelectList GetCitiesByStateId(long stateId)
        {
            var cities = _db.Cities.Where(x => x.StateId == stateId)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.NameEn,
                                       Value = x.Id.ToString()
                                   });
            return new SelectList(cities, "Value", "Text");
        }

        public SelectList GetProvinces()
        {
            var provinces = _db.Provinces.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(provinces, "Value", "Text");
        }

        public SelectList GetProvinces(long countryId)
        {
            var provinces = _db.Provinces.Where(x => x.CountryId == countryId)
                                         .Select(x => new SelectListItem
                                         {
                                             Text = x.NameEn,
                                             Value = x.Id.ToString()
                                         });
            return new SelectList(provinces, "Value", "Text");
        }

        public SelectList GetDistricts()
        {
            var districts = _db.Districts.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(districts, "Value", "Text");
        }

        public SelectList GetDistricts(long provinceId)
        {
            var districts = _db.Districts.Where(x => x.ProvinceId == provinceId)
                                         .Select(x => new SelectListItem
                                         {
                                             Text = x.NameEn,
                                             Value = x.Id.ToString()
                                         });
            return new SelectList(districts, "Value", "Text");
        }

        public SelectList GetSubdistricts()
        {
            var subdistricts = _db.Subdistricts.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(subdistricts, "Value", "Text");
        }

        public SelectList GetSubdistricts(long districtId)
        {
            var subdistricts = _db.Subdistricts.Where(x => x.DistrictId == districtId)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.NameEn,
                                                   Value = x.Id.ToString()
                                               });
            return new SelectList(subdistricts, "Value", "Text");
        }

        public SelectList GetStates()
        {
            var states = _db.States.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(states, "Value", "Text");
        }

        public SelectList GetStates(long countryId)
        {
            var states = _db.States.Where(x => x.CountryId == countryId)
                                .Select(x => new SelectListItem
                                {
                                    Text = x.NameEn,
                                    Value = x.Id.ToString()
                                });
            return new SelectList(states, "Value", "Text");
        }
        #endregion

        #region Registration
        public SelectList GetRegistrationStatuses()
        {
            var statuses = _db.RegistrationStatuses.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetSlotType()
        {
            var slots = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Registration", Value = "r" },
                            new SelectListItem { Text = "Registration Payment", Value = "p" },
                            new SelectListItem { Text = "Add/Drop", Value = "a" },
                            new SelectListItem { Text = "Add/Drop Payment", Value = "ap" },
                            new SelectListItem { Text = "Withdrawal", Value = "w"},
                            new SelectListItem { Text = "Evaluation", Value = "e"},
                            new SelectListItem { Text = "Grade Submission", Value = "g"},
                            new SelectListItem { Text = "Change Curriculum", Value = "cc"},
                        };
            return new SelectList(slots, "Value", "Text");
        }

        public SelectList GetLockedRegistrationStatuses()
        {
            var block = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "" },
                            new SelectListItem { Text = "No Lock", Value = "false" },
                            new SelectListItem { Text = "Locked", Value = "true" }
                        };
            return new SelectList(block, "Value", "Text");
        }

        public SelectList GetStudentRegistrationTerms(string studentCode)
        {
            var terms = _registrationProvider.GetStudentRegistrationTerms(studentCode)
                                             .Select(x => new SelectListItem
                                             {
                                                 Text = x.TermText,
                                                 Value = x.Id.ToString()
                                             });
            return new SelectList(terms, "Value", "Text");
        }

        public SelectList GetStudentStates()
        {
            var states = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Registration", Value = "REG" },
                             new SelectListItem { Text = "Payment for Registration", Value = "PAY_REG" },
                             new SelectListItem { Text = "Add/Drop", Value = "ADD" },
                             new SelectListItem { Text = "Payment for Add/Drop", Value = "PAY_ADD" }
                         };

            return new SelectList(states, "Value", "Text");
        }
        #endregion

        #region Announcement
        public SelectList GetChannels()
        {
            var channels = _db.Channels.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(channels, "Value", "Text");
        }

        public SelectList GetTopics()
        {
            var topics = _db.Topics.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(topics, "Value", "Text");
        }

        public SelectList GetTopics(List<long> selectedTopic)
        {
            var topics = _db.Topics.Select(x => new SelectListItem
            {
                Selected = (selectedTopic.Contains(x.Id)),
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(topics, "Value", "Text");
        }
        #endregion

        #region Parent
        public SelectList GetRelations()
        {
            var relations = _db.Relationships.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(relations, "Value", "Text");
        }
        #endregion

        #region Maintenance Status
        public SelectList GetMaintenanceFees(long? facultyId, long? departmentId, long? academicLevelId, long? studentGroupId)
        {
            var maintenanceFees = _db.MaintenanceFees.Include(x => x.AcademicLevel)
                                                     .Include(x => x.Faculty)
                                                     .Include(x => x.Department)
                                                     .Where(x => (x.FacultyId == null
                                                                  || facultyId == x.FacultyId)
                                                                  && (x.DepartmentId == null
                                                                      || departmentId == null
                                                                      || departmentId == x.DepartmentId)
                                                                  && academicLevelId == x.AcademicLevelId)
                                                     .Select(x => new SelectListItem
                                                     {
                                                         Text = $"{ x.Faculty.NameEn }/{ x.Department.NameEn } - { x.Fee.ToString(StringFormat.TwoDecimal) }",
                                                         Value = x.Id.ToString()
                                                     });
            return new SelectList(maintenanceFees, "Value", "Text");
        }
        #endregion

        #region Building
        public SelectList GetBuildings()
        {
            var buildings = _db.Buildings.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(buildings, "Value", "Text");
        }

        public SelectList GetBuildings(long campusId)
        {
            var buildings = _db.Buildings.Where(x => x.CampusId == campusId)
                                         .Select(x => new SelectListItem
                                         {
                                             Text = x.NameEn,
                                             Value = x.Id.ToString()
                                         })
                                         .OrderBy(x => x.Text); ;
            return new SelectList(buildings, "Value", "Text");
        }

        public SelectList GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type)
        {
            var rooms = _roomProvider.GetAvailableRoom(date, start, end, type)
                                     .Select(x => new SelectListItem
                                                  {
                                                      Text = x.NameEn + $"[{ x.Capacity }]",
                                                      Value = x.Id.ToString()
                                                  })
                                     .OrderBy(x => x.Text);
            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type, long roomId = 0)
        {
            var rooms = _roomProvider.GetAvailableRoom(date, start, end, type);
            if(roomId != 0)
            {
                rooms.Add(
                    _db.Rooms.SingleOrDefault(x => x.Id == roomId)
                );
            }
            var selectRoom = rooms.Select(x => new SelectListItem
                                               {
                                                   Text = x.NameEn + $"[{ x.Capacity }]",
                                                   Value = x.Id.ToString()
                                               })
                                  .OrderBy(x => x.Text);
            return new SelectList(selectRoom, "Value", "Text");
        }

        public SelectList GetRooms()
        {
            var rooms = _db.Rooms.Select(x => new SelectListItem
                                              {
                                                  Text = x.NameEn + $"[{ x.Capacity }]",
                                                  Value = x.Id.ToString()
                                              })
                                 .OrderBy(x => x.Text);
                                 
            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetRoomsBySeatPlaning(int seatPlaning)
        {
            var rooms = _db.Rooms.Where(x => x.Capacity >= seatPlaning)
                                 .Select(x => new SelectListItem
                                              {
                                                  Text = x.NameEn + $"[{ x.Capacity }]",
                                                  Value = x.Id.ToString()
                                              })
                                 .OrderBy(x => x.Text);

            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetRooms(long buildingId)
        {
            var rooms = _db.Rooms.Where(x => x.BuildingId == buildingId)
                                 .Select(x => new SelectListItem
                                 {
                                     Text = x.NameEn,
                                     Value = x.Id.ToString()
                                 })
                                 .OrderBy(x => x.Text);
            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetAvailableRoomBySectionSlotIds(List<long> sectionSlotIds)
        {
            var rooms = _roomProvider.GetAvailableRoomBySectionSlotIds(sectionSlotIds)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.NameEn + $"[{ x.Capacity }]",
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(rooms, "Value", "Text");
        }
        public SelectList GetAvailableRoomBySectionSlotIdsAndInSectionSlot(List<long> sectionSlotIds)
        {
            var rooms = _roomProvider.GetAvailableRoomAndRoomInSectionSlot(sectionSlotIds)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.NameEn + $"[{ x.Capacity }]",
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetAvailableRoomByDates(RoomReservation roomReservation)
        {
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(roomReservation.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(roomReservation.EndTimeText);
            var dates = _reservationProvider.GenerateSelectedDate(roomReservation);
            var rooms = _roomProvider.GetAvailableRoomByDates(dates, startedAt, endedAt, roomReservation.Id)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.NameEn + $"[{ x.Capacity }]",
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(rooms, "Value", "Text");
        }

        public SelectList GetRoomTypes()
        {
            var roomTypes = _db.RoomTypes.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return new SelectList(roomTypes, "Value", "Text");
        }

        public SelectList GetFacilities()
        {
            var facility = _db.Facilities.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(facility, "Value", "Text");
        }

        public SelectList GetUsingTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Studying", Value = "s" },
                            new SelectListItem { Text = "Activity", Value = "a" },
                            new SelectListItem { Text = "Examination", Value = "e" },
                            new SelectListItem { Text = "Meeting", Value = "m" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetAllReservationStatuses()
        {
            var status = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Waiting", Value = "w" },
                            new SelectListItem { Text = "Approved", Value = "a" },
                            new SelectListItem { Text = "Reject", Value = "r" },
                            new SelectListItem { Text = "Cancel", Value = "c" }
                        };
            return new SelectList(status, "Value", "Text");

        }

        public SelectList GetReservationStatuses(string currentStatus)
        {
            var status = new List<SelectListItem>();
            if (currentStatus == "a")
            {
                status.Add(new SelectListItem { Text = "Waiting", Value = "w" });
                status.Add(new SelectListItem { Text = "Reject", Value = "r" });
            }
            else if (currentStatus == "w")
            {
                status.Add(new SelectListItem { Text = "Approved", Value = "a" });
                status.Add(new SelectListItem { Text = "Reject", Value = "r" });
            }
            else if (currentStatus == "r")
            {
                status.Add(new SelectListItem { Text = "Approved", Value = "a" });
                status.Add(new SelectListItem { Text = "Waiting", Value = "w" });
            }
            else if (currentStatus == "defaultWaiting")
            {
                status.Add(new SelectListItem { Text = "Waiting", Value = "w" });
                status.Add(new SelectListItem { Text = "All", Value = "all"});
                status.Add(new SelectListItem { Text = "Approved", Value = "a" });
                status.Add(new SelectListItem { Text = "Reject", Value = "r" });
                status.Add(new SelectListItem { Text = "Confirm", Value = "c" });
                status.Add(new SelectListItem { Text = "No Exam", Value = "ne" });
            }
            else
            {
                status.Add(new SelectListItem { Text = "Approved", Value = "a" });
                status.Add(new SelectListItem { Text = "Waiting", Value = "w" });
                status.Add(new SelectListItem { Text = "Reject", Value = "r" });
            }

            return new SelectList(status, "Value", "Text", "Disabled");
        }

        public SelectList GetSenderTypes()
        {
            var type = new List<SelectListItem>
                       {
                           new SelectListItem { Text = "Student", Value = "s" },
                           new SelectListItem { Text = "Instructor", Value = "i" },
                           new SelectListItem { Text = "Admin/Staff", Value = "a" }
                       };
            return new SelectList(type, "Value", "Text");
        }
        #endregion

        #region Academic Calendar
        public SelectList GetEventCategories()
        {
            var eventCategories = _db.EventCategories.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(eventCategories, "Value", "Text");
        }

        public SelectList GetEvents()
        {
            var events = _db.Events.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(events, "Value", "Text");
        }

        public SelectList GetDayOfWeek()
        {
            var days = new List<SelectListItem>();
            days.Add(new SelectListItem() { Text = "Sunday", Value = "0" });
            days.Add(new SelectListItem() { Text = "Monday", Value = "1" });
            days.Add(new SelectListItem() { Text = "Tuesday", Value = "2" });
            days.Add(new SelectListItem() { Text = "Wednesday", Value = "3" });
            days.Add(new SelectListItem() { Text = "Thursday", Value = "4" });
            days.Add(new SelectListItem() { Text = "Friday", Value = "5" });
            days.Add(new SelectListItem() { Text = "Saturday", Value = "6" });
            return new SelectList(days, "Value", "Text");
        }

        public SelectList GetCalendarViewLevel()
        {
            var views = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "" },
                new SelectListItem { Text = "Student", Value = "s" },
                new SelectListItem { Text = "Admission", Value = "a" },
                new SelectListItem { Text = "Instructor", Value = "i" },
                new SelectListItem { Text = "Admin", Value = "ad" }
            };
            return new SelectList(views, "Value", "Text");
        }
        #endregion

        #region Teaching Type
        public SelectList GetTeachingTypes()
        {
            var types = _db.TeachingTypes.Where(x => !x.IsExamination)
                                         .Select(x => new SelectListItem
                                                      {
                                                          Text = x.NameEn,
                                                          Value = x.Id.ToString()
                                                      });
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetInstructorTypes()
        {
            var types = _db.InstructorTypes.Select(x => new SelectListItem
                                                        {
                                                            Text = x.NameEn,
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetInstructorRankings()
        {
            var types = _db.InstructorRankings.Select(x => new SelectListItem
                                                           {
                                                               Text = x.NameEn,
                                                               Value = x.Id.ToString()
                                                           })
                                              .OrderBy(x => x.Text);
            return new SelectList(types, "Value", "Text");
        }
        #endregion

        #region Withdrawal
        public SelectList GetWithdrawalTypes()
        {
            var withdrawalTypes = new List<SelectListItem>
            {
                //new SelectListItem() { Text = "Debarment", Value = "d" },
                //new SelectListItem() { Text = "Petition", Value = "p" },
                new SelectListItem() { Text = "Application", Value = "u" }
            };
            return new SelectList(withdrawalTypes, "Value", "Text");
        }

        public SelectList GetWithdrawalStatuses()
        {
            var withdrawalTypes = new List<SelectListItem>
            {
                new SelectListItem() { Text = "Waiting", Value = "w" },
                new SelectListItem() { Text = "Approved", Value = "a" },
                new SelectListItem() { Text = "Reject", Value = "r" }
            };
            return new SelectList(withdrawalTypes, "Value", "Text");
        }

        public SelectList GetWithdrawalPeriodTypes()
        {
            var withdrawalTypes = new List<SelectListItem>
                                  {
                                      new SelectListItem { Text = "Debarment", Value = "d"},
                                      new SelectListItem { Text = "Petition", Value = "p"},
                                      new SelectListItem { Text = "Application", Value = "u"},
                                  };
            return new SelectList(withdrawalTypes, "Value", "Text");
        }

        public SelectList GetExceptionalCourses()
        {
            var courses = _db.Courses.Include(x => x.ExceptionalCourse)
                                     .Where(x => x.ExceptionalCourse == null
                                                 && x.TransferUniversityId == null)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.CodeAndName,
                                         Value = x.Id.ToString()
                                     })
                                     .OrderBy(x => x.Text);
            return new SelectList(courses, "Value", "Text");
        }

        public SelectList GetExceptionalDepartments(long facultyId)
        {
            var departments = _academicProvider.GetExceptionWithdrawalDepartments(facultyId)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.CodeAndName,
                                                   Value = x.Id.ToString()
                                               })
                                               .OrderBy(x => x.Text);
            return new SelectList(departments, "Value", "Text");
        }
        #endregion

        #region Seat Available
        public SelectList GetCoursesBySectionGroup(long termId, long facultyId, long departmentId)
        {
            var courses = _db.Sections.Include(x => x.Course)
                                      .Where(x => x.TermId == termId
                                                  && (x.Course.FacultyId == facultyId
                                                  || facultyId == 0)
                                                  && (x.Course.DepartmentId == departmentId
                                                  || departmentId == 0))
                                      .GroupBy(x => new { x.CourseId, x.TermId, x.Course.CourseAndCredit })
                                      .Select(y => new SelectListItem
                                      {
                                          Text = y.Key.CourseAndCredit,
                                          Value = y.Key.CourseId.ToString()
                                      });
            return new SelectList(courses, "Value", "Text");
        }
        #endregion

        #region Payment
        public SelectList GetLatePaymentPermissionTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Petition", Value = "p" },
                            new SelectListItem { Text = "Memo", Value = "m" },
                            new SelectListItem { Text = "Contact", Value = "c" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetLatePaymentStatuses()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "all" },
                            new SelectListItem { Text = "No Late Payment", Value = "false" },
                            new SelectListItem { Text = "Late Payment", Value = "true" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetPaymentStatusesWithOutAll()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Paid", Value = "p" },
                               new SelectListItem { Text = "Unpaid", Value = "u" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetPaymentStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "a" },
                               new SelectListItem { Text = "Paid", Value = "p" },
                               new SelectListItem { Text = "Unpaid", Value = "u" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetPaymentMethods()
        {
            var payments = _db.PaymentMethods.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                             .ToList();
            return new SelectList(payments, "Value", "Text");
        }
        #endregion

        #region Student
        public SelectList GetStudentForLatedPayment(long termId)
        {
            var students = _studentProvider.GetStudentForLatedPayment(termId)
                                           .Select(x => new SelectListItem
                                           {
                                               Text = x.CodeAndName,
                                               Value = x.Id.ToString()
                                           })
                                           .ToList();
            return new SelectList(students, "Value", "Text");
        }

        public SelectList GetStudyPlanByCurriculumVersion(long curriculumVersionId)
        {
            var studyPlans = _curriculumProvider.GetStudyPlansByCurriculumVersion(curriculumVersionId)
                                                .Select(x => new SelectListItem
                                                {
                                                    Text = x.YearText,
                                                    Value = "" // TODO: use value of study plan id
                                                });
            return new SelectList(studyPlans, "Value", "Text");
        }

        public SelectList GetResignReasons()
        {
            var resignReasons = _db.ResignReasons.Select(x => new SelectListItem
            {
                Text = x.DescriptionEn,
                Value = x.Id.ToString()
            });
            return new SelectList(resignReasons, "Value", "Text");
        }

        public SelectList GetStudentsByAcademicLevelId(long academicLevelId)
        {
            var studentCode = _db.Students.Include(x => x.AcademicInformation)
                                          .Where(x => x.AcademicInformation.AcademicLevelId == academicLevelId)
                                          .Select(x => new SelectListItem
                                                       {
                                                           Text = x.CodeAndName,
                                                           Value = x.Code.ToString()
                                                       });
            return new SelectList(studentCode, "Value", "Text");
        }

        public SelectList GetScholarshipStudents()
        {
            var studentId = _db.Students.Where(x => x.StudentStatus == "s"
                                                    && _db.ScholarshipStudents.Any(y => y.StudentId == x.Id))
                                        .Select(x => new SelectListItem
                                        {
                                            Text = x.CodeAndName,
                                            Value = x.Id.ToString()
                                        });
            return new SelectList(studentId, "Value", "Text");
        }

        public SelectList GetChangedNameLogStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Approved", Value = "a" },
                               new SelectListItem { Text = "Waiting", Value = "w" },
                               new SelectListItem { Text = "Reject", Value = "r" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetReenterReasons()
        {
            var reenterReason = _db.ReEnterReasons.Select(x => new SelectListItem
            {
                Text = x.DescriptionEn,
                Value = x.Id.ToString()
            });
            return new SelectList(reenterReason, "Value", "Text");
        }

        public SelectList GetScholarshipStudentsByStudentCode(string studentCode)
        {
            var scholarshipStudents = _db.ScholarshipStudents.Include(x => x.Scholarship)
                                                             .Include(x => x.Student)
                                                             .Where(x => x.Student.Code == studentCode
                                                                         && x.IsApproved
                                                                         && x.LimitedAmount > 0)
                                                             .Select(x => new SelectListItem
                                                                          {
                                                                              Text = x.Scholarship.NameEn,
                                                                              Value = x.Id.ToString()
                                                                          });

            return new SelectList(scholarshipStudents, "Value", "Text");
        }
        #endregion

        #region Fee
        public SelectList GetPaidTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "One term", Value = "o" },
                            new SelectListItem { Text = "Recurring", Value = "r" }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetTuitionFeeTypes()
        {
            var tuitionFeeTypes = _db.TuitionFeeTypes.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
                                                     .OrderBy(x => x.Text);
            return new SelectList(tuitionFeeTypes, "Value", "Text");
        }

        public SelectList GetTuitionFeeFormulas()
        {
            var tuitionFeeFormulas = _db.TuitionFeeFormulas.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
                                                           .OrderBy(x => x.Text);
            return new SelectList(tuitionFeeFormulas, "Value", "Text");
        }

        public SelectList GetFeeItems()
        {
            var items = _db.FeeItems.Select(x => new SelectListItem
            {
                Text = x.CodeAndName,
                Value = x.Id.ToString()
            })
                                    .OrderBy(x => x.Text);
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetReceiptItemsByCourseId(long registrationId)
        {
            var items = _receiptProvider.GetReceiptItemsByCourseId(registrationId)
                                        .Select(x => new SelectListItem
                                        {
                                            Text = x.FeeItemName,
                                            Value = x.Id.ToString()
                                        })
                                        .OrderBy(x => x.Text);
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetCurriculumCourse(long versionId)
        {
            var items = _curriculumProvider.GetCurriculumCourse(versionId)
                                            .Select(x => new SelectListItem
                                            {
                                                Text = x.CourseAndCredit,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text);
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetReceiptNumberByStudentCodeAndTerm(string code, int year, int term)
        {
            var items = _receiptProvider.GetReceiptNumberByStudentCodeAndTerm(code, year, term)
                                        .Select(x => new SelectListItem
                                        {
                                            Text = x.Number,
                                            Value = x.Id.ToString()
                                        })
                                        .OrderBy(x => x.Text);
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetTermFeeItemsByStudentAndTermId(Guid studentId, long termId)
        {
            var items = _receiptProvider.GetTermFeeItemsByStudentAndTermId(studentId, termId)
                                        .Select(x => new SelectListItem
                                        {
                                            Text = x.FeeItemName,
                                            Value = x.Id.ToString()
                                        })
                                        .OrderBy(x => x.Text);
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetTermFeeCalculateTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "One Time", Value = "o" },
                            new SelectListItem { Text = "Per Term", Value = "t" },
                            new SelectListItem { Text = "Per Year", Value = "y" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetReceiptsCourses(Guid studentId, long termId)
        {
            var receipts = _receiptProvider.GetReceiptsCourses(studentId, termId);
            if (receipts != null && receipts.Any())
            {
                var items = receipts.Select(x => new SelectListItem
                {
                    Text = x.Course?.Code ?? "",
                    Value = x.Id.ToString()
                })
                                    .OrderBy(x => x.Text);

                return new SelectList(items, "Value", "Text");
            }

            var nullSelectList = new List<SelectListItem>
                                 {
                                     new SelectListItem { Text = "N/A", Value = "-1" }
                                 };

            return new SelectList(nullSelectList, "Value", "Text");
        }

        public SelectList GetFeeItemFromReceiptPreview(List<FeeItemViewModel> receiptItems)
        {
            var items = receiptItems.Select(x => new SelectListItem
            {
                Text = x.FeeItemName,
                Value = x.FeeItemId.ToString()
            });
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetFeeGroups()
        {
            var items = _db.FeeGroups.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(items, "Value", "Text");
        }

        public SelectList GetPercentages()
        {
            var percentages = new List<SelectListItem>
                              {
                                  new SelectListItem { Text = "N/A", Value = "-1", Selected = true }
                              };

            percentages.AddRange(_db.Percentages.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Value.ToString()
            }));

            return new SelectList(percentages, "Value", "Text");
        }

        public SelectList GetScholarshipPercentages()
        {
            var percentages = _db.Percentages.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Value.ToString()
            });

            return new SelectList(percentages, "Value", "Text");
        }

        public SelectList GetVoucherStatus()
        {
            var voucherStatus = new List<SelectListItem>
                                   {
                                       new SelectListItem { Text = "Request", Value = "r" },
                                       new SelectListItem { Text = "Paid", Value = "p" }
                                   };

            return new SelectList(voucherStatus, "Value", "Text");
        }

        public SelectList GetFullAmountScholarship()
        {
            var scholarship = _db.Scholarships.Where(x => x.IsFullAmount)
                                              .Select(x => new SelectListItem
                                                           {
                                                               Text = x.NameEn,
                                                               Value = x.Id.ToString()
                                                           });
            return new SelectList(scholarship, "Value", "Text");
        }

        public SelectList GetMultiplyStatuses()
        {
            var multiplyStatuses = new List<SelectListItem>
                                   {
                                       new SelectListItem { Text = "Multiply", Value = "m" },
                                       new SelectListItem { Text = "Fix", Value = "f" }
                                   };

            return new SelectList(multiplyStatuses, "Value", "Text");
        }

        public SelectList GetStudentFeeType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Normal", Value = "s" },
                            new SelectListItem { Text = "Resident", Value = "r" },
                            new SelectListItem { Text = "Nonresident", Value = "n" },
                            new SelectListItem { Text = "Outbound", Value = "o" },
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetStudentFeeGroups()
        {
            var studentFeeGroups = _db.StudentFeeGroups.Select(x => new SelectListItem
                                                                    {
                                                                        Text = x.Name,
                                                                        Value = x.Id.ToString()
                                                                    });
            return new SelectList(studentFeeGroups, "Value", "Text");
        }

        public SelectList GetStudentFeeGroups(long academicLevelId, long facultyId, long departmentId, long curriculumId,
                                              long curriculumVersionId, long nationalityId, int batch, long studentGroupId,
                                              long studentFeeTypeId)
        {
            var studentFeeGroups = _feeProvider.GetStudentFeeGroups(academicLevelId, facultyId, departmentId, curriculumId,
                                                                    curriculumVersionId, nationalityId, batch, studentGroupId,
                                                                    studentFeeTypeId)
                                               .Select(x => new SelectListItem
                                                            {
                                                                Text = x.Name,
                                                                Value = x.Id.ToString()
                                                            });
            return new SelectList(studentFeeGroups, "Value", "Text");
        }

        public SelectList GetFeeTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Other Fee", Value = "o" },
                            new SelectListItem { Text = "Registration Fee", Value = "r" }
                        };
                        
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetReceiptCreatedBy()
        {
            var types = _receiptProvider.GetReceiptCreatedBy()
                                        .Select(x => new SelectListItem
                                                     {
                                                         Text = x,
                                                         Value = x
                                                     });
            return new SelectList(types, "Value", "Text");
        }
        #endregion

        #region Other
        public SelectList GetActiveStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Active", Value = "true" },
                               new SelectListItem { Text = "Inactive", Value = "false" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetOpenCloseStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Open", Value = "false" },
                               new SelectListItem { Text = "Close", Value = "true" }
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetRoles()
        {
            var roles = _db.Roles.OrderBy(x => x.Name)
                                 .Select(x => new SelectListItem
                                 {
                                     Text = x.Name,
                                     Value = x.Id
                                 });
            return new SelectList(roles, "Value", "Text");
        }

        public SelectList GetUsers()
        {
            var users = _db.Users.Select(x => new SelectListItem
            {
                Text = x.UserName,
                Value = x.Id
            });

            return new SelectList(users, "Value", "Text");
        }

        public SelectList GetUsersFullNameEn()
        {
            var users = (from user in _db.Users
                         join instructor in _db.Instructors on user.UserName equals instructor.Code into instructors
                         from instructor in instructors.DefaultIfEmpty()
                         join title in _db.Titles on instructor.TitleId equals title.Id
                         select new 
                                {
                                    Id = user.Id,
                                    FullNameEn = title.NameEn + " " + instructor.FirstNameEn + " " + instructor.LastNameEn,
                                    FirstNameEn = instructor.FirstNameEn ?? string.Empty,
                                    UserName = user.UserName
                                }
                        )
                        .Select(x => new SelectListItem
                        {
                            Text = string.IsNullOrEmpty(x.FirstNameEn) ? x.UserName : x.FullNameEn,
                            Value = x.Id
                        });

            return new SelectList(users, "Value", "Text");
        }

        public SelectList GetYesNoAnswer()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Yes", Value = "true" },
                               new SelectListItem { Text = "No", Value = "false" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetNoYesAnswer()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "No", Value = "false" },
                               new SelectListItem { Text = "Yes", Value = "true" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetAllYesNoAnswer()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Yes", Value = "true" },
                               new SelectListItem { Text = "No", Value = "false" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetAllYesNoValue()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "all" },
                               new SelectListItem { Text = "Yes", Value = "true" },
                               new SelectListItem { Text = "No", Value = "false" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetThaiStatuses()
        {
            var programs = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Thai", Value = "true" },
                               new SelectListItem { Text = "Non-Thai", Value = "false" }
                           };

            return new SelectList(programs, "Value", "Text");
        }

        public SelectList GetApprovedStatuses()
        {
            var approved = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Approve", Value = "true" },
                               new SelectListItem { Text = "Unapprove", Value = "false" }
                           };
            return new SelectList(approved, "Value", "Text");
        }

        public SelectList GetEffectivedStatuses()
        {
            var approved = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Effective", Value = "true" },
                               new SelectListItem { Text = "Expired", Value = "false" }
                           };
            return new SelectList(approved, "Value", "Text");
        }

        public SelectList GetSignatories(string language = "en")
        {
            var signatories = _db.Signatories.Select(x => new SelectListItem
            {
                Text = language == "en" ? x.FullNameEn : x.FullNameTh,
                Value = x.Id.ToString()
            });

            return new SelectList(signatories, "Value", "Text");
        }

        public SelectList GetSignatorieNames(string language = "en")
        {
            var signatories = _db.Signatories.Select(x => new SelectListItem
            {
                Text = language == "en" ? x.FullNameEn : x.FullNameTh,
                Value = x.FullNameEn
            });

            return new SelectList(signatories, "Value", "Text");
        }

        public SelectList GetPetitions()
        {
            var petitions = _db.Petitions.Select(x => new SelectListItem
                                                      {
                                                          Text = x.NameEn,
                                                          Value = x.Id.ToString()
                                                      });
            return new SelectList(petitions, "Value", "Text");
        }

        public SelectList GetRequiredTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Required", Value = "rq"},
                            new SelectListItem { Text = "Recomment", Value = "rc"}
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetStudentStatusReportType()
        {
            var report = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Full Report", Value = "f" },
                             new SelectListItem { Text = "Simple Report", Value = "s" }
                         };

            return new SelectList(report, "Value", "Text");
        }

        public SelectList GetRoomSlotOrderBy()
        {
            var orderBy = new List<SelectListItem>
                          {
                              new SelectListItem { Text = "Date", Value = "d" },
                              new SelectListItem { Text = "Building", Value = "b" },
                              new SelectListItem { Text = "Room", Value = "r" },
                              new SelectListItem { Text = "Time", Value = "t" }
                          };

            return new SelectList(orderBy, "Value", "Text");
        }

        public SelectList GetPetitionStatuses()
        {
            var status = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Request", Value = "r" },
                             new SelectListItem { Text = "Accept", Value = "a" },
                             new SelectListItem { Text = "Reject", Value = "j" }
                         };

            return new SelectList(status, "Value", "Text");
        }

        public SelectList GetChangeCurriculumPetitionStatuses()
        {
            var status = new List<SelectListItem>
                         {
                             new SelectListItem { Text = "Pending", Value = "p" },
                             new SelectListItem { Text = "Cancel", Value = "c" },
                             new SelectListItem { Text = "Approved", Value = "a" },
                             new SelectListItem { Text = "Rejected", Value = "r" }
                         };

            return new SelectList(status, "Value", "Text");
        }

        public SelectList GetPetitionChannels()
        {
            var channels = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Web", Value = "w" },
                               new SelectListItem { Text = "App", Value = "a" }
                           };

            return new SelectList(channels, "Value", "Text");
        }

        public SelectList GetChangeDepartmentPetitionStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "PENDING", Value = "PENDING" },
                               new SelectListItem { Text = "CANCEL", Value = "CANCEL" },
                               new SelectListItem { Text = "APPROVED", Value = "APPROVED" },
                               new SelectListItem { Text = "REJECTED", Value = "REJECTED" }
                           };

            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetExaminationCourseConditions()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "Same Day", Value = "s" },
                            new SelectListItem { Text = "Different Day", Value = "d" }
                        };

            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetCertificateTypes()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "หนังสือรับรองชั้นปีระบุเกรดเฉลี่ยสะสม" , Value = Certificate.AcademicYearWithGPA },
                            new SelectListItem { Text = "หนังสือรับรองจบการศึกษาระบุวันรับปริญญา", Value = Certificate.GraduatedWithCeremonyAt },
                            new SelectListItem { Text = "รับรองคาดจบรอเกรดออก", Value = Certificate.GraduatingWaitingGrade },
                            new SelectListItem { Text = "หนังสือรับรองจบการศึกษาฉบับธรรมดา", Value = Certificate.GraduatedNormalForm },
                            new SelectListItem { Text = "รับรองจบระบุวันเข้าศึกษา สำเร็จการศึกษา", Value = Certificate.GraduatedWithAdmissionDateAndGraduatedDate},
                            new SelectListItem { Text = "รับรองจบระบุ Assessment", Value = Certificate.GraduatedWithEnglishAssessment },
                            new SelectListItem { Text = "รับรองจบระบุ Ranking", Value = Certificate.GraduationRankingCertification },
                            new SelectListItem { Text = "รับรองจบระบุ IELTS", Value = Certificate.GraduationWithIELTS },
                            new SelectListItem { Text = "หนังสือรับรองคาดจบหลักสูตรเรียนเป็นภาษาอังกฤษ", Value = Certificate.GraduatingStatusCertification },
                            new SelectListItem { Text = "หนังสือรับรองคาดจบรอผลสอบและรับรองหลักสูตรอังกฤษ", Value = Certificate.GraduatingWaitingFinalResult },
                            new SelectListItem { Text = "หนังสือรับรองเคยเป็นนักศึกษา", Value = Certificate.StudentStatus },
                            new SelectListItem { Text = "หนังสือรับรองกระทรวง", Value = Certificate.GraduatedEnglishInstruction },
                            new SelectListItem { Text = "หนังสือรับรองเคยเป็นนักศึกษาระบุวันเข้าออก" , Value = Certificate.StudentWithAdmissionFromTo },
                            new SelectListItem { Text = "ใบแปลปริญญา", Value = Certificate.TranslationGraduatedCertificate },
                            new SelectListItem { Text = "หนังสือรับรองผ่อนผันเกณฑ์ทหาร", Value = Certificate.StudentDraftDeferment },
                            new SelectListItem { Text = "รับรองฐานะปี", Value = Certificate.ConfirmAcademicYear},
                            new SelectListItem { Text = "หนังสือรับรองฐานะปี ระบุ Assessment", Value = Certificate.StatusCertificateWithAssessment },
                            new SelectListItem { Text = "หนังสือรับรองสมัครงานการบินไทย", Value = Certificate.ThaiAirwayCertification },
                            new SelectListItem { Text = "หนังสือรับรองระบุเป็นบุคคลคนเดียวกันเปลี่ยนชื่อ", Value = Certificate.ChangingStudentName },
                            new SelectListItem { Text = "รับรองค่าใช้จ่าย", Value = Certificate.ExpensesOutlineCertificate },
                            new SelectListItem { Text = "รับรองระบุไปต่างประเทศ", Value = Certificate.GoingAbroadCertification },
                            new SelectListItem { Text = "รับรองนักศึกษาใหม่", Value = Certificate.CertifyNewStudent }
                        };
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetLanguages()
        {
            var languages = new List<SelectListItem>
                            {
                                new SelectListItem { Text = "English", Value = "en" },
                                new SelectListItem { Text = "ภาษาไทย", Value = "th" },
                            };
            return new SelectList(languages, "Value", "Text");
        }

        public SelectList GetLanguage()
        {
            var languages = new List<SelectListItem>
                            {
                                new SelectListItem { Text = "All", Value = "" },
                                new SelectListItem { Text = "English", Value = "en" },
                                new SelectListItem { Text = "ภาษาไทย", Value = "th" },
                            };
            return new SelectList(languages, "Value", "Text");
        }

        public SelectList GetCertificatePurposes()
        {
            var purposes = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "For Students", Value = "s" },
                               new SelectListItem { Text = "For Instructors", Value = "i" },
                               new SelectListItem { Text = "For Officers", Value = "o" },
                           };
            return new SelectList(purposes, "Value", "Text");
        }

        public SelectList GetUrgentStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Normal", Value = "false" },
                               new SelectListItem { Text = "Urgent", Value = "true" },
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetPrintStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Request", Value = "r" },
                               new SelectListItem { Text = "Printed", Value = "p" },
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetPaidStatuses()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Paid", Value = "true" },
                               new SelectListItem { Text = "Unpaid", Value = "false" },
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetExcludeBalanceInvoice()
        {
            var statuses = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Only Refund", Value = "true" },
                               new SelectListItem { Text = "Only Balance", Value = "false" },
                           };
            return new SelectList(statuses, "Value", "Text");
        }

        public SelectList GetGender()
        {
            var gender = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "All", Value = "" },
                               new SelectListItem { Text = "Male", Value = "1" },
                               new SelectListItem { Text = "Female", Value = "2" },
                               new SelectListItem { Text = "Undefined", Value = "0" },
                           };
            return new SelectList(gender, "Value", "Text");
        }

        public SelectList GetNativeLanguages()
        {
            var languages = _db.Languages.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(languages, "Value", "Text");
        }

        public SelectList GetGenderForAdmission()
        {
            var gender = new List<SelectListItem>
                           {
                               new SelectListItem { Text = "Male", Value = "1" },
                               new SelectListItem { Text = "Female", Value = "2" },
                               new SelectListItem { Text = "Other", Value = "0" },
                           };
            return new SelectList(gender, "Value", "Text");
        }

        public SelectList GetDistributionMethods()
        {
            var methods = _db.DistributionMethods.Select(x => new SelectListItem
                                                              {
                                                                  Text = x.NameEn,
                                                                  Value = x.Id.ToString()
                                                              });
            return new SelectList(methods, "Value", "Text");
        }
        #endregion

        #region Scholarship 
        public SelectList GetScholarshipTypes()
        {
            var types = _db.ScholarshipTypes.Select(x => new SelectListItem
                                                         {
                                                             Text = x.NameEn,
                                                             Value = x.Id.ToString()
                                                         });
            return new SelectList(types, "Value", "Text");
        }

        public SelectList GetScholarships()
        {
            var scholarships = _db.Scholarships.Select(x => new SelectListItem
                                                            {
                                                                Text = x.NameEn,
                                                                Value = x.Id.ToString()
                                                            });
            return new SelectList(scholarships, "Value", "Text");
        }

        public SelectList GetScholarshipsByScholarshipTypeId(long scholarshipTypeId)
        {
            var scholarshipsbytype = _db.Scholarships.Where(x => (x.ScholarshipTypeId == 0
                                                                  || x.ScholarshipTypeId == scholarshipTypeId))
                                                     .Select(x => new SelectListItem
                                                     {
                                                         Text = x.NameEn,
                                                         Value = x.Id.ToString()
                                                     });
            return new SelectList(scholarshipsbytype, "Value", "Text");
        }

        public SelectList GetSponsors()
        {
            var sponsors = _db.Sponsors.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            });
            return new SelectList(sponsors, "Value", "Text");
        }
        #endregion

        #region Grade
        public SelectList GetGradeOptions()
        {
            var options = _gradeProvider.GetGradeOptions().Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            });
            return new SelectList(options, "Value", "Text");
        }

        public SelectList GetTransferType()
        {
            var types = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "All", Value = "ALL" },
                            new SelectListItem { Text = "Transferred", Value = "TRANSFERRED" },
                            new SelectListItem { Text = "Not Transferred", Value = "NOTTRANSFERRED" },
                            new SelectListItem { Text = "Not Complete", Value = "NOTCOMPLETE" }
                        };

            return new SelectList(types, "Value", "Text");
        }
        #endregion

        #region Questtionaire
        public SelectList GetRespondType()
        {
            var respondTypes = new List<SelectListItem>
                               {
                                   new SelectListItem { Text = "Student", Value = "Student" },
                                   new SelectListItem { Text = "Instructor", Value = "Instructor" },
                                   new SelectListItem { Text = "Officer", Value = "Officer" }
                               };

            return new SelectList(respondTypes, "Value", "Text");
        }

        public SelectList GetQuestionnaire()
        {
            var questionnaire = _db.Questionnaires.Select(x => new SelectListItem
            {
                Text = x.NameEn,
                Value = x.Id.ToString()
            })
                                                  .OrderBy(x => x.Text);
            return new SelectList(questionnaire, "Value", "Text");
        }

        public SelectList GetQuestionGroups()
        {
            var groups = _db.QuestionGroups.Select(x => new SelectListItem
                                                        {
                                                            Text = x.NameEn,
                                                            Value = x.Id.ToString()
                                                        })
                                           .OrderBy(x => x.Text);
            return new SelectList(groups, "Value", "Text");
        }

        public SelectList GetCourses(long? academicLevelId)
        {
            var courses = _db.Courses.Where(x => x.AcademicLevelId == academicLevelId)
                                     .Select(x => new SelectListItem
                                     {
                                         Text = x.CodeAndName,
                                         Value = x.Id.ToString()
                                     });
            return new SelectList(courses, "Value", "Text");
        }
        #endregion

        #region Graduation
        public SelectList GetGraduatingStatuses()
        {
            var respondTypes = new List<SelectListItem>
                               {
                                   new SelectListItem { Text = "Submitted", Value = "w" },
                                   new SelectListItem { Text = "Accepted", Value = "a" },
                                   new SelectListItem { Text = "Checking in progress", Value = "p" },
                                   new SelectListItem { Text = "Completed", Value = "c" },
                                   new SelectListItem { Text = "Returned", Value = "t" },
                                   new SelectListItem { Text = "Rejected", Value = "r" }
                               };
            return new SelectList(respondTypes, "Value", "Text");
        }
        #endregion
    }
}