using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Enums;

namespace KeystoneLibrary.Interfaces
{
    public interface ISelectListProvider
    {
        //General
        SelectList GetTitlesEn();
        SelectList GetTitlesTh();
        SelectList GetTitleThByTitleEn(long id);
        SelectList GetRaces();
        SelectList GetNationalities();
        SelectList GetReligions();
        SelectList GetOccupations();
        SelectList GetStudentGroups();
        SelectList GetBankBranches();
        SelectList GetMaritalStatuses();
        SelectList GetLivingStatuses();
        SelectList GetDeformations();
        SelectList GetCalculateTypes();
        SelectList GetStudentCodeStatuses();
        SelectList GetResidentTypes();
        SelectList GetStudentFeeTypes();
        SelectList GetCustomCourseGroups();
        SelectList GetQuestionnaireCourseGroups();
        SelectList GetMonth();
        SelectList GetGradingStatisticReport();
        SelectList GetCheatingStatisticReportTypes();
        SelectList GetCertificateChangeNameTypes(string language = "en");
        SelectList GetPetitions();
        SelectList GetRequiredTypes();
        SelectList GetStudentStatusReportType();
        SelectList GetRoomSlotOrderBy();
        SelectList GetReenterType();
        SelectList GetAttendanceType();
        SelectList GetReceiptInvoice();
        SelectList GetReceiptInvoiceType();
        SelectList GetProgramDirectorTypes();
        SelectList GetStudents();
        SelectList GetStudentCodes();
        SelectList GetRegistrationSummaryReportTypes();
        SelectList GetRegistrationConditions();
        SelectList GetRegistrationTerms();
        SelectList GetRegistrationTermsByTerm(long termId);
        
        //Academic
        SelectList GetAcademicPrograms();
        SelectList GetAcademicProgramsByAcademicLevelId(long academicLevelId);
        SelectList GetAcademicYearByScholarshipStudents();
        SelectList GetSectionPrograms();
        SelectList GetCoursesBySectionStatus(long termId, string section);
        SelectList GetCoursesFromSectionMasterByTermId(long termId);
        SelectList GetAcademicLevels(string language = "en");
        SelectList GetAcademicHonors();
        SelectList GetFaculties(string language = "en");
        SelectList GetFacultiesByAcademicLevelId(long academicLevelId);
        SelectList GetDepartments(string language = "en");
        SelectList GetDepartments(long id, string language = "en");
        SelectList GetDepartmentsByFacultyIds(long academicLevelId, List<long> facultyIds);
        SelectList GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId);
        SelectList GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId, long facultyId);
        SelectList GetTermTypes();
        SelectList GetTermsByAcademicLevelId(long id);
        SelectList GetExpectedGraduationTerms(long id);
        SelectList GetExpectedGraduationYears(long id);
        SelectList GetInvoicesByStudentCodeAndTermId(string studentCode, long termId);
        SelectList GetInvoiceType();
        SelectList GetInvoiceRefundType();
        SelectList GetCurriculumTermTypes();
        SelectList GetAdmissionRoundByAcademicLevelId(long id);
        SelectList GetAdmissionRoundByTermId(long termId);
        SelectList GetSubmittedStatus();
        SelectList GetReceivedStatus();
        SelectList GetEndedTermByStartedTerm(long id);
        SelectList GetTermsByStudentCode(string code);
        SelectList GetCurriculum();
        SelectList GetCurriculumCourse(long versionId);
        SelectList GetCurriculumByCurriculumInformation(Guid studentId);
        SelectList GetCurriculumByDepartment(long academicLevelId, long facultyId, long departmentId);
        SelectList GetCurriculumsByDepartmentIds(long academicLevelId, List<long> facultyIds, List<long> departmentIds);
        SelectList GetCurriculumByAcademicLevelId(long academicLevelId);
        SelectList GetCurriculumByOpenedTermAndClosedTerm(long academicLevelId, long? openedTermId, long? closedTermId);
        SelectList GetCurriculumVersions();
        SelectList GetCurriculumVersion(long curriculumId);
        SelectList GetCurriculumVersionsByCurriculumIds(long academicLevelId, List<long> curriculumIds);
        SelectList GetCurriculumVersionsByCurriculumId(long curriculumId);
        SelectList GetCurriculumVersionsByCurriculumInformation(string studentCode, string language);
        SelectList GetCurriculumVersionsByCurriculumIdAndStudentId(Guid studentId, long curriculumId);
        SelectList GetSelectableCurriculumList(string studentCode);
        SelectList GetSelectableCurriculumVersionList(long curriculumId, string studentCode); 
        SelectList GetCurriculumCourseGroupsForChangeCurriculum(long versionId);
        SelectList GetCurriculumCourseGroupsAndChildren(long versionId);
        SelectList GetCurriculumCourseGroups(long versionId);
        SelectList GetCourseGroupTypes();
        SelectList GetMinors();
        SelectList GetConcentrations();
        SelectList GetModules();
        SelectList GetModulesByCurriculumVersionId(long versionId);
        SelectList GetMinorsByCurriculumVersionId(long versionId);
        SelectList GetConcentrationsByCurriculumVersionId(long versionId);
        SelectList GetAbilities();
        SelectList GetSpecializationGroups();
        SelectList GetSpecializationGroupByCurriculumVersionId(long curriculumVersionId);
        SelectList GetCurriculumVersionForAdmissionCurriculum(long termId, long admissionRoundId, long facultyId, long? departmentId = null);
        SelectList GetFilterCurriculumVersionGroups();
        SelectList GetDependencyTypes();
        SelectList GetCorequisites();
        SelectList GetCourseEquivalents();
        SelectList GetProbations();
        SelectList GetRetires();
        SelectList GetSortByForStudentProbation();
        SelectList GetSortByForSignatureSheet();
        SelectList GetStudentStatisticByProvinceAndSchoolReportType();
        SelectList GetBatches();
        SelectList GetImplementedCurriculumVersionsByStudentId(Guid studentId, long facultyId = 0, long departmentId = 0);
        SelectList GetMaintenanceTypes();
        SelectList GetTransactionFinalGradeTypes();
        SelectList GetGradeConditions();
        SelectList GetCourseGroupConditions();
        SelectList GetTermConditions();
        SelectList GetGPAConditions();
        SelectList GetCreditConditions();
        SelectList GetTotalCourseGroupConditions();
        SelectList GetBatchConditions();
        SelectList GetAbilityConditions();
        SelectList GetConditionTypes();
        SelectList GetAndConditions();
        SelectList GetOrConditions();
        SelectList GetConditionsByType(string conditionTypes);
        SelectList GetTransferUniversities();
        SelectList GetDegreeNamesByAcademicLevelId(long academicLevelId);
        SelectList GetProjectTypes();
        
        //Grade 
        SelectList GetGrades();
        SelectList GetWeightGrades();
        SelectList GetNonWeightGrades();
        SelectList GetGradesExcept(long gradeId, List<long> gradesInTemplate);
        SelectList GetGradeTemplates();
        SelectList GetTransferedGrades();
        SelectList GetStandardGradingScoreType();
        SelectList GetStandardGradingGroups();
        SelectList GetGradingByPageScoreInstructor();

        //Admission
        SelectList GetAdmissionTypes();
        SelectList GetPreviousSchools(long schoolGroupId = 0);
        SelectList GetPreviousSchoolsByCountryId(long countryId);
        SelectList GetSchoolTypes();
        SelectList GetSchoolTerritories();
        SelectList GetEducationBackground();
        SelectList GetSchoolGroup();
        SelectList GetDocuments();
        SelectList GetAdmissionRounds(long academicLevelId, long termId = 0);
        SelectList GetEntranceExamResults();
        SelectList GetAdmissionChannels();
        SelectList GetAdmissionPlaces();
        SelectList GetAgencies();
        SelectList GetExemptedAdmissionExaminations();
        SelectList GetAdmissionExaminationTypes();
        SelectList GetStudentStatuses(GetStudentStatusesEnum type = GetStudentStatusesEnum.Default);
        SelectList GetStudentProfileStatuses();
        SelectList GetStudentDocumentGroups();
        SelectList GetStudentDocumentGroupsByCountryId(long academicLevelId, long? facultyId, long? departmentId, long? previousSchoolCountryId);
        SelectList GetAdmissionStatuses();

        //Course 
        SelectList GetCourses(bool showMUICId = true);
        SelectList GetAllowAddedCourses(long includedCourseId = 0, bool showMUICId = true);
        SelectList GetCoursesByAcademicLevelId(long academicLevelId);
        SelectList GetCoursesByAcademicLevelAndTransferUniversity(long academicLevelId, long transferUniversityId);
        SelectList GetCourses(long? academicLevelId);
        SelectList GetCoursesByAcademicLevelAndTerm(long academicLevelId, long termId);
        SelectList GetCourses(long facultyId, long departmentId);
        SelectList GetCoursesByStudentAndTerm(Guid studentId, long termId);
        SelectList GetCoursesByStudentAndTermForCheating(Guid? studentId, long termId);
        SelectList GetCourseRates();
        SelectList GetCoursesByTerm(long termId);
        SelectList GetCoursesByCourseGroup(long courseGroupId);
        SelectList GetCoursesByCurriculumVersion(long academicLevelId, long curriculumVersionId);
        SelectList GetCourseAndTransferCourse();
        SelectList GetExternalCourses();
        SelectList GetTeachingCourseByInstructorId(long instructorId, long termId);
        SelectList GetSections();
        SelectList GetSections(long courseId);
        SelectList GetSections(long termId, long courseId);
        SelectList GetSections(long termId, List<long> coursesId = null);
        SelectList GetParentSections(long courseId, long termId);
        SelectList GetCourseByInstructorId(long instructorId, long termId);
        SelectList GetSectionNumbers();
        SelectList GetSectionMasterByTermIdAndCourseId(long termId, long courseId);
        SelectList GetSectionsByInstructorId(long instructorId, long termId, long courseId);
        SelectList GetSectionByCourseId(long termId, long courseId);
        SelectList GetSectionSlotStatus();
        SelectList GetSeatAvailableStatuses();
        SelectList GetCourseGroupByCurriculumsVersionAndCourse(long curriculumVersionId, long courseId);
        SelectList GetFilterCourseGroups();
        SelectList GetFilterCourseGroupsByFacultyId(long facultyId);
        SelectList GetSectionStatuses();
        SelectList GetCourseToBeOfferedSectionStatuses();
        SelectList GetSectionTypes();
        SelectList GetCreditTypes();
        SelectList GetSpecializationGroupTypes();
        SelectList GetSpecializationGroupByType(string type);
        SelectList GetQuestionnaireApprovalStatuses();
        
        //Instructor
        SelectList GetInstructors();
        SelectList GetInstructorById(long instructorId);
        SelectList GetInstructorsForCheating();
        SelectList GetExaminationTypes();
        SelectList GetExaminationDateByExaminationType(string studentCode, string examinationType);
        SelectList GetExaminationCourseByExaminationDate(string examinationDate, string studentCode, string examinationType);
        SelectList GetMidtermAndFinal();
        SelectList GetIncidents();
        SelectList GetInstructorsByFacultyId(long id);
        SelectList GetTermInstructorsByCourseId(long termId, long courseId);
        SelectList GetInstructorByCourseId(long termId, long courseId);
        SelectList GetInstructorBySectionId(long termId, long courseId, long sectionId);
        SelectList GetCodeFaculties(long id);
        SelectList GetCodeDepartments(long id);

        //Address
        SelectList GetCountries();
        SelectList GetProvinces();
        SelectList GetProvinces(long countryId);
        SelectList GetDistricts();
        SelectList GetDistricts(long provinceId);
        SelectList GetSubdistricts();
        SelectList GetSubdistricts(long districtId);
        SelectList GetStates();
        SelectList GetStates(long countryId);
        SelectList GetCities();
        SelectList GetCitiesByStateId(long stateId);
        SelectList GetCities(long countryId);
        SelectList GetCities(long countryId, long stateId);

        //Announcement
        SelectList GetChannels();
        SelectList GetTopics();
        SelectList GetTopics(List<long> selectedTopic);

        //Registration
        SelectList GetRegistrationStatuses();
        SelectList GetSlotType();
        SelectList GetLockedRegistrationStatuses();
        SelectList GetStudentRegistrationTerms(string studentCode);
        SelectList GetStudentStates();
        
        //Academic Calendar 
        SelectList GetDayOfWeek();
        SelectList GetEvents();
        SelectList GetEventCategories();
        SelectList GetCalendarViewLevel();

        //Parent
        SelectList GetRelationships();
        
        //Maintenance Status
        SelectList GetMaintenanceFees(long? facultyId, long? departmentId, long? academicLevelId, long? studentGroupId);

        //Building
        SelectList GetCampuses();
        SelectList GetBuildings();
        SelectList GetBuildings(long campusId);
        SelectList GetRoomTypes();
        SelectList GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type);
        SelectList GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type, long roomId = 0);
        SelectList GetAvailableRoomByDates(RoomReservation roomReservation);
        SelectList GetRooms();
        SelectList GetRooms(long buildingId);
        SelectList GetRoomsBySeatPlaning(int seatPlaning);
        SelectList GetAvailableRoomBySectionSlotIds(List<long> sectionSlotIds);
        SelectList GetAvailableRoomBySectionSlotIdsAndInSectionSlot(List<long> sectionSlotIds);
        SelectList GetFacilities();
        SelectList GetUsingTypes();

        // Reservation
        SelectList GetAllReservationStatuses();
        SelectList GetReservationStatuses(string currentStatus);
        SelectList GetSenderTypes();

        //Teaching Load
        SelectList GetTeachingTypes();
        SelectList GetInstructorTypes();
        SelectList GetInstructorRankings();

        //Withdrawal
        SelectList GetWithdrawalTypes();
        SelectList GetWithdrawalStatuses();
        SelectList GetWithdrawalPeriodTypes();
        
        SelectList GetExceptionalCourses();
        
        SelectList GetExceptionalDepartments(long facultyId);

        //Seat Available
        SelectList GetCoursesBySectionGroup(long termId, long facultyId, long departmentId);

        //Payment
        SelectList GetLatePaymentPermissionTypes();
        SelectList GetLatePaymentStatuses();
        SelectList GetPaymentStatusesWithOutAll();
        SelectList GetPaymentStatuses();
        SelectList GetPaymentMethods();

        // Student
        SelectList GetStudentForLatedPayment(long termId);
        SelectList GetStudyPlanByCurriculumVersion(long curriculumVersionId);
        SelectList GetResignReasons();
        SelectList GetStudentsByAcademicLevelId(long academicLevelId);
        SelectList GetScholarshipStudents();
        SelectList GetChangeNameType();
        SelectList GetChangeFlagType();
        SelectList GetReenterReasons();
        SelectList GetScholarshipStudentsByStudentCode(string studentCode);
        
        // Fee
        SelectList GetChangedNameLogStatuses();
        SelectList GetPaidTypes();
        SelectList GetTuitionFeeTypes();
        SelectList GetTuitionFeeFormulas();
        SelectList GetFeeItems();
        SelectList GetFeeGroups();
        SelectList GetPercentages();
        SelectList GetMultiplyStatuses();
        SelectList GetReceiptItemsByCourseId(long registrationId);
        SelectList GetReceiptNumberByStudentCodeAndTerm(string code, int year, int term);
        SelectList GetTermFeeItemsByStudentAndTermId(Guid studentId, long termId);
        SelectList GetTermFeeCalculateTypes();
        SelectList GetReceiptsCourses(Guid studentId, long termId);
        SelectList GetFeeItemFromReceiptPreview(List<FeeItemViewModel> receiptItems);
        SelectList GetStudentFeeType();
        SelectList GetStudentFeeGroups();
        SelectList GetStudentFeeGroups(long academicLevelId, long facultyId, long departmentId, long curriculumId,
                                       long curriculumVersionId, long nationalityId, int batch, long studentGroupId,
                                       long studentFeeTypeId);
        SelectList GetFeeTypes();
        SelectList GetReceiptCreatedBy();

        // Other
        SelectList GetActiveStatuses();
        SelectList GetOpenCloseStatuses();
        SelectList GetYesNoAnswer();
        SelectList GetNoYesAnswer();
        SelectList GetAllYesNoAnswer();
        SelectList GetAllYesNoValue();
        SelectList GetThaiStatuses();
        SelectList GetApprovedStatuses();
        SelectList GetEffectivedStatuses();
        SelectList GetCertificateTypes();
        SelectList GetLanguages();
        SelectList GetLanguage();
        SelectList GetCertificatePurposes();
        SelectList GetUrgentStatuses();
        SelectList GetPrintStatuses();
        SelectList GetPaidStatuses();
        SelectList GetExcludeBalanceInvoice();
        SelectList GetGender();
        SelectList GetSignatories(string language = "en");
        SelectList GetSignatorieNames(string language = "en");
        SelectList GetPetitionStatuses();
        SelectList GetChangeCurriculumPetitionStatuses();
        SelectList GetPetitionChannels();
        SelectList GetChangeDepartmentPetitionStatuses();
        SelectList GetNativeLanguages();

        // Authen
        SelectList GetRoles();
        SelectList GetUsers();
        SelectList GetUsersFullNameEn();
        SelectList GetExaminationCourseConditions();
        SelectList GetGenderForAdmission();
        SelectList GetDistributionMethods();

        //Scolarship 
        SelectList GetScholarshipTypes();
        SelectList GetScholarships();
        SelectList GetScholarshipsByScholarshipTypeId(long scholarshipTypeId);
        SelectList GetSponsors();
        SelectList GetScholarshipPercentages();
        SelectList GetVoucherStatus();
        SelectList GetFullAmountScholarship();

        // Grade
        SelectList GetGradeOptions();
        SelectList GetTransferType();

        // Questionnaire
        SelectList GetQuestionnaire();
        SelectList GetQuestionGroups();
        SelectList GetRespondType();

        //Graduation
        SelectList GetGraduatingStatuses();
    }
}