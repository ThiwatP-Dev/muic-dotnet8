using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels;

namespace Keystone.Controllers
{
    public class SelectListController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly IAddressProvider _addressProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly ICardProvider _cardProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IFeeProvider _feeProvider;
        
        public SelectListController(ApplicationDbContext db,
                                    ISelectListProvider selectListProvider,
                                    IStudentProvider studentProvider,
                                    IAcademicProvider academicProvider,
                                    IAdmissionProvider admissionProvider,
                                    IAddressProvider addressProvider,
                                    ICurriculumProvider curriculumProvider,
                                    IInstructorProvider instructorProvider,
                                    IRoomProvider roomProvider,
                                    IRegistrationProvider registrationProvider,
                                    ICacheProvider cacheProvider,
                                    IScholarshipProvider scholarshipProvider,
                                    IReceiptProvider receiptProvider,
                                    ICardProvider cardProvider,
                                    IDateTimeProvider dateTimeProvider,
                                    IFeeProvider feeProvider) : base(db, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _academicProvider = academicProvider;
            _admissionProvider = admissionProvider;
            _addressProvider = addressProvider;
            _curriculumProvider = curriculumProvider;
            _instructorProvider = instructorProvider;
            _roomProvider = roomProvider;
            _registrationProvider = registrationProvider;
            _cacheProvider = cacheProvider;
            _scholarshipProvider = scholarshipProvider;
            _receiptProvider = receiptProvider;
            _cardProvider = cardProvider;
            _dateTimeProvider = dateTimeProvider;
            _feeProvider = feeProvider;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId, long facultyId)
        {
            var departments = _academicProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            return Json(departments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetFacultiesByAcademicLevelId(long id)
        {
            var faculties = _academicProvider.GetFacultiesByAcademicLevelId(id);
            return Json(faculties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExceptionWithdrawalDepartments(long facultyId)
        {
            var departments = _academicProvider.GetExceptionWithdrawalDepartments(facultyId);
            return Json(departments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDepartmentsByFacultyIds(long academicLevelId, List<long> ids)
        {
            var departments = _academicProvider.GetDepartmentsByFacultyIds(academicLevelId, ids);
            return Json(departments);
        }

        public JsonResult GetDepartmentsByFacultyId(long facultyId)
        {
            var departments = _selectListProvider.GetDepartments(facultyId);
            return Json(departments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCurriculumsByDepartmentIds(long academicLevelId, List<long> facultyIds, List<long> departmentIds)
        {
            var curriculum = _academicProvider.GetCurriculumsByDepartmentIds(academicLevelId, facultyIds, departmentIds);
            return Json(curriculum);
        }

        public JsonResult GetFilterCourseGroupsByFacultyId(long facultyId)
        {
            var filterCourseGroups = _selectListProvider.GetFilterCourseGroupsByFacultyId(facultyId);
            return Json(filterCourseGroups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCurriculumVersionsByCurriculumIds(long academicLevelId, List<long> curriculumIds)
        {
            var curriculumVersion = _academicProvider.GetCurriculumVersionsByCurriculumIds(academicLevelId, curriculumIds);
            return Json(curriculumVersion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetProvinceByCountryId(long id)
        {
            var provinces = _addressProvider.GetProvinceByCountryId(id);
            return Json(provinces);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetStateByCountryId(long id)
        {
            var states = _db.States.Where(x => x.CountryId == id)
                                   .ToList();
            return Json(states);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetStatesByCountryId(long id)
        {
            var states = _addressProvider.GetStatesByCountryId(id);
            return Json(states);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDistrictByProvinceId(long id)
        {
            var districts = _addressProvider.GetDistrictByProvinceId(id);
            return Json(districts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSubdistrictByDistrictId(long id)
        {
            var subdistricts = _addressProvider.GetSubdistrictByDistrictId(id);
            return Json(subdistricts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTopicByChannelId(long id)
        {
            var topics = _academicProvider.GetTopicByChannelId(id);
            return Json(topics);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetEvents(long id)
        {
            var events = _db.Events.Where(x => x.EventCategoryId == id)
                                   .ToList();
            return Json(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCityByStateIdSelectList(long id)
        {
            var cities = _db.Cities.Where(x => x.StateId == id)
                                   .ToList();
            return Json(cities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTermsByAcademicLevelId(long id)
        {
            var term = _academicProvider.GetTermsByAcademicLevelId(id);
            return Json(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExpectedGraduationYearsByAcademicLevelId(long id)
        {
            var term = _selectListProvider.GetExpectedGraduationYears(id);
            return Json(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExpectedGraduationTermsByAcademicLevelId(long id)
        {
            var term = _selectListProvider.GetExpectedGraduationTerms(id);
            return Json(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAdmissionRoundByAcademicLevelId(long id)
        {
            var admissionRound = _selectListProvider.GetAdmissionRoundByAcademicLevelId(id);
            return Json(admissionRound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAcademicProgramsByAcademicLevelId(long id)
        {
            var academicProgram = _selectListProvider.GetAcademicProgramsByAcademicLevelId(id);
            return Json(academicProgram);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAdmissionRoundByTermId(long id)
        {
            var admissionRound = _selectListProvider.GetAdmissionRoundByTermId(id);
            return Json(admissionRound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAdmissionRoundByAcademicLevelIdAndTermId(long academicLevelId, long termId)
        {
            var admissionRound = _selectListProvider.GetAdmissionRounds(academicLevelId, termId);
            return Json(admissionRound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetStudentDocumentGroupsByCountryId(long academicLevelId, long? facultyId, long? departmentId, long? previousSchoolCountryId)
        {
            var admissionDocumentGroup = _selectListProvider.GetStudentDocumentGroupsByCountryId(academicLevelId, facultyId, departmentId, previousSchoolCountryId);
            return Json(admissionDocumentGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetEndedTermByStartedTerm(long id)
        {
            var terms = _selectListProvider.GetEndedTermByStartedTerm(id);
            return Json(terms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTermsByStudentCode(string code)
        {
            var terms = _selectListProvider.GetTermsByStudentCode(code);
            return Json(terms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTitleThByTitleEn(long id)
        {
            var titles = _selectListProvider.GetTitleThByTitleEn(id);
            return Json(titles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionByCourseId(long termId, long courseId)
        {
            var sections = _registrationProvider.GetSectionByCourseId(termId, courseId);
            var sectionSelectList = _selectListProvider.GetSections(termId, courseId);
            if (sections.Count() > 0 && sectionSelectList.Count() == 0)
            {
                return Json(sections);
            }
            else
            {
                return Json(sectionSelectList);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionMasterByTermIdAndCourseId(long termId, long courseId)
        {

            var sectionSelectList = _selectListProvider.GetSectionMasterByTermIdAndCourseId(termId, courseId);

            return Json(sectionSelectList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionByCourses(long termId, List<long> courseIds)
        {
            var sections = _selectListProvider.GetSections(termId, courseIds);
            return Json(sections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCoursesBySectionStatus(long termId, string sectionStatus)
        {
            var courses = _selectListProvider.GetCoursesBySectionStatus(termId, sectionStatus);
            return Json(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCoursesBySectionMasterByTerm(long termId)
        {
            var courses = _selectListProvider.GetCoursesFromSectionMasterByTermId(termId);
            return Json(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionsByInstructor(long instructorId, long termId, long courseId)
        {
            var sections = _registrationProvider.GetSectionsByInstructorId(instructorId, termId, courseId);
            return Json(sections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExaminationDateByExaminationType(string studentCode, string examinationType)
        {
            var examinationDate = _selectListProvider.GetExaminationDateByExaminationType(studentCode, examinationType);
            return Json(examinationDate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExaminationCourseByExaminationDate(string examinationDate, string studentCode, string examinationType)
        {
            var examinationCourse = _selectListProvider.GetExaminationCourseByExaminationDate(examinationDate, studentCode, examinationType);
            return Json(examinationCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetEducationBackgroundsByCountryId(long id)
        {
            var educationBackgrounds = _admissionProvider.GetEducationBackgroundsByCountryId(id);
            return Json(educationBackgrounds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCitiesByCountryId(long id)
        {
            var cities = _db.Cities.Where(x => x.CountryId == id)
                                   .ToList();
            return Json(cities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetConditionsByType(string conditionType)
        {
            var conditions = _selectListProvider.GetConditionsByType(conditionType);
            return Json(conditions);
        }

        //Building
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetBuildingByCampusId(long id)
        {
            var buildings = _roomProvider.GetBuildingByCampusId(id);
            return Json(buildings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAvailableRoom(DateTime date, string start, string end, string type)
        {
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(start);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(end);
            var rooms = new List<Room>();
            if (startedAt != null && endedAt != null)
            {
                rooms = _roomProvider.GetAvailableRoom(date, startedAt.Value, endedAt.Value, type);
            }
            return Json(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAvailableRoomByDates(RoomReservation roomReservation)
        {
            var rooms = _selectListProvider.GetAvailableRoomByDates(roomReservation);
            return Json(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetRoomByBuildingId(long id)
        {
            var rooms = _roomProvider.GetRoomByBuildingId(id);
            return Json(rooms);
        }

        //Course & Section
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionNumbersByCourses(long termId, string ids)
        {
            var sections = new List<string>();
            long temp = 0;
            List<long> courses = String.IsNullOrEmpty(ids) ? null : ids.Split(',').Where(x => Int64.TryParse(x, out temp))
                                                                                  .Select(x => temp)
                                                                                  .ToList();

            sections = courses == null ? _registrationProvider.GetSectionNumbers() :
                                         _registrationProvider.GetSectionNumbersByCourses(termId, courses);
            return Json(sections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCourseGroupByCurriculumsVersionAndCourse(long curriculumVersionId, long courseId)
        {
            var courseGroups = _selectListProvider.GetCourseGroupByCurriculumsVersionAndCourse(curriculumVersionId, courseId);
            return Json(courseGroups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExceptionalCourses()
        {
            var courses = _selectListProvider.GetExceptionalCourses();
            return Json(courses);
        }

        //Instructor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetInstructorsByFacultyId(long id)
        {
            var instructors = _instructorProvider.GetInstructorsByFacultyId(id);
            return Json(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTermInstructorsByCourseId(long termId, long courseId)
        {
            var instructors = _instructorProvider.GetTermInstructorsByCourseId(termId, courseId);
            return Json(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetCoursesSelectList(long termId, long facultyId, long departmentId)
        {
            var courses = _selectListProvider.GetCoursesBySectionGroup(termId, facultyId, departmentId);
            return Json(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetInstructorByCourseId(long termId, long courseId)
        {
            var instructors = _selectListProvider.GetInstructorByCourseId(termId, courseId);
            return Json(instructors);
        }

        //Curriculum
        public JsonResult GetCurriculumByAcademicLevelId(long id)
        {
            var curriculums = _curriculumProvider.GetCurriculumByAcademicLevelId(id);
            return Json(curriculums);
        }

        public JsonResult GetCurriculumByDepartment(long academicLevelId, long facultyId, long departmentId)
        {
            var curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            return Json(curriculums);
        }

        public JsonResult GetCurriculumVersion(long curriculumId)
        {
            var curriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            return Json(curriculumVersions);
        }

        public JsonResult GetCurriculumCourse(long versionId)
        {
            var curriculumCourse = _selectListProvider.GetCurriculumCourse(versionId);
            return Json(curriculumCourse);
        }

        public JsonResult GetSelectableCurriculumVersionList(long curriculumId, string studentCode)
        {
            var curriculumVersions = _selectListProvider.GetSelectableCurriculumVersionList(curriculumId, studentCode);
            return Json(curriculumVersions);
        }

        public JsonResult GetSpecializationGroupByCurriculumVersionId(long curriculumVersionId)
        {
            var specializationGroup = _selectListProvider.GetSpecializationGroupByCurriculumVersionId(curriculumVersionId);
            return Json(specializationGroup);
        }

        [HttpPost]
        public JsonResult GetCoursesByTerm(long termId)
        {
            var courses = _selectListProvider.GetCoursesByTerm(termId);
            return Json(courses);
        }

        [HttpPost]
        public JsonResult GetCoursesByAcademicLevelId(long academicLevelId)
        {
            var courses = _selectListProvider.GetCoursesByAcademicLevelId(academicLevelId);
            return Json(courses);
        }

        [HttpPost]
        public JsonResult GetCoursesByStudentAndTermForCheating(Guid studentId, long termId)
        {
            var courses = _selectListProvider.GetCoursesByStudentAndTermForCheating(studentId, termId);
            return Json(courses);
        }

        [HttpPost]
        public JsonResult GetCoursesByStudentCodeAndTermForCheating(string studentCode, long termId)
        {
            var studentId = _studentProvider.GetStudentByCode(studentCode)?.Id;
            var courses = _selectListProvider.GetCoursesByStudentAndTermForCheating(studentId, termId);
            return Json(courses);
        }

        [HttpPost]
        public JsonResult GetCurriculumVersionForAdmissionCurriculum(long termId, long admissionRoundId, long facultyId, long? departmentId = null)
        {
            var curriculumVersions = _selectListProvider.GetCurriculumVersionForAdmissionCurriculum(termId, admissionRoundId, facultyId, departmentId);
            return Json(curriculumVersions);
        }

        public JsonResult GetCurriculumVersionsByCurriculumIdAndStudentId(Guid studentId, long curriculumId)
        {
            var curriculumVersions = _selectListProvider.GetCurriculumVersionsByCurriculumIdAndStudentId(studentId, curriculumId);
            return Json(curriculumVersions);
        }

        [HttpPost]
        public JsonResult GetPreviousSchoolsByGroupId(long schoolGroupId)
        {
            var previousSchools = _selectListProvider.GetPreviousSchools(schoolGroupId);
            return Json(previousSchools);
        }

        [HttpPost]
        public JsonResult GetPreviousSchoolsByCountryId(long countryId)
        {
            var previousSchools = _selectListProvider.GetPreviousSchoolsByCountryId(countryId);
            return Json(previousSchools);
        }

        [HttpPost]
        public JsonResult GetCurriculumCourseGroups(long versionId)
        {
            var groups = _selectListProvider.GetCurriculumCourseGroups(versionId);
            return Json(groups);
        }

        [HttpPost]
        public JsonResult GetCurriculumCourseGroupsForChangeCurriculum(long versionId)
        {
            var groups = _selectListProvider.GetCurriculumCourseGroupsForChangeCurriculum(versionId);
            return Json(groups);
        }

        [HttpPost]
        public JsonResult GetDegreeNamesByAcademicLevelId(long academicLevelId)
        {
            var degreeNames = _selectListProvider.GetDegreeNamesByAcademicLevelId(academicLevelId);
            return Json(degreeNames);
        }

        [HttpPost]
        public JsonResult GetSpecializationGroupByType(string type)
        {
            var types = _selectListProvider.GetSpecializationGroupByType(type);
            return Json(types);
        }

        //Scholarship
        [HttpPost]
        public JsonResult GetScholarshipsByScholarshipTypeId(long scholarshipTypeId)
        {
            var scholarships = _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId);
            return Json(scholarships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetScholarshipExpiryTermId(long academicLevelId, long selectedTermId, int scholarshipId)
        {
            var terms = _academicProvider.GetTermsByAcademicLevelId(academicLevelId);
            var scholarship = _scholarshipProvider.GetScholarshipById(scholarshipId);
            var selectedTerm = selectedTermId == 0 ? _cacheProvider.GetCurrentTerm(academicLevelId)
                                                   : terms.SingleOrDefault(x => x.Id == selectedTermId);

            var expiredAcademicYear = selectedTerm.AcademicYear + ((scholarship?.TotalYear ?? 0) - 1);
            // ex. start 2020 with 4 year --> expire at 2023 last term (2020, 2021, 2022, 2023)
            var expiredAcademicTerm = 1;
            var expiredTerm = _academicProvider.GetTermByAcademicYear(expiredAcademicYear).FirstOrDefault();

            if (expiredTerm == null || scholarship.TotalYear == null || scholarship.TotalYear == 0)
            {
                var lastedTerm = terms.FirstOrDefault(); // Order by Descending Year and Term
                expiredAcademicYear = lastedTerm.AcademicYear;
                expiredAcademicTerm = lastedTerm.AcademicTerm;
            }
            else
            {
                expiredAcademicTerm = expiredTerm.AcademicTerm;
            }

            terms.Select(x =>
            {
                x.IsScholarshipExpiryTerm = (x.AcademicYear == expiredAcademicYear && x.AcademicTerm == expiredAcademicTerm);
                return x;
            }).ToList();

            return Json(terms);
        }

        //Refund
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetReceiptItemsByCourseId(long registrationId)
        {
            var feeItems = _selectListProvider.GetReceiptItemsByCourseId(registrationId);
            return Json(feeItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSectionNumberByRegistrationId(long registrationId)
        {
            var section = _registrationProvider.GetSectionNumberByRegistrationId(registrationId);
            return Json(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetAmountByReceiptId(long receiptItemId)
        {
            var amount = _receiptProvider.GetAmountByReceiptId(receiptItemId);
            return Json(amount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetFeeItemDefaultPrice(long feeItemId)
        {
            var price = _feeProvider.GetFeeItem(feeItemId)?.DefaultPrice ?? 0;
            return Json(price);
        }

        //Receipt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetReceiptNumberByStudentCodeAndTerm(string code, int year, int term)
        {
            var numbers = _selectListProvider.GetReceiptNumberByStudentCodeAndTerm(code, year, term);
            return Json(numbers);
        }

        //Student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetStudentByCode(string studentCode)
        {
            var student = _studentProvider.GetStudentByCode(studentCode);
            return Json(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetStudentFeeGroups(long academicLevelId, long facultyId, long departmentId, long curriculumId,
                                              long curriculumVersionId, long nationalityId, int batch, long studentGroupId,
                                              long studentFeeTypeId)
        {
            var feeGroups = _selectListProvider.GetStudentFeeGroups(academicLevelId, facultyId, departmentId, curriculumId,
                                                                    curriculumVersionId, nationalityId, batch, studentGroupId,
                                                                    studentFeeTypeId);
            return Json(feeGroups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetRegistrationTermsByTerm(long termId)
        {
            var registrationTerms = _selectListProvider.GetRegistrationTermsByTerm(termId);
            return Json(registrationTerms);
        }
    }
}