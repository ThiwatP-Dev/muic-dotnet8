// SelectList Url

const selectlistBaseUrl = "/SelectList";
const groupRegistrationBaseUrl = "/GroupRegistration";

const SelectListURLDict = {
    GetCurriculumsByDepartmentIds: selectlistBaseUrl + "/GetCurriculumsByDepartmentIds",
    GetCurriculumVersionsByCurriculumIds: selectlistBaseUrl + "/GetCurriculumVersionsByCurriculumIds",
    GetAvailableRoomByDates: selectlistBaseUrl + "/GetAvailableRoomByDates",
    GetAcademicProgramsByAcademicLevelId: selectlistBaseUrl + "/GetAcademicProgramsByAcademicLevelId",
    GetAdmissionRoundByAcademicLevelIdAndTermId: selectlistBaseUrl + "/GetAdmissionRoundByAcademicLevelIdAndTermId",
    GetFacultiesByAcademicLevelId: selectlistBaseUrl + "/GetFacultiesByAcademicLevelId",
    GetExceptionWithdrawalDepartments: selectlistBaseUrl + "/GetExceptionWithdrawalDepartments",
    GetInstructorsByFacultyId: selectlistBaseUrl + "/GetInstructorsByFacultyId",
    GetCurriculumByAcademicLevelId : selectlistBaseUrl + "/GetCurriculumByAcademicLevelId",
    GetTermsByAcademicLevelId : selectlistBaseUrl + "/GetTermsByAcademicLevelId",
    GetExpectedGraduationYearsByAcademicLevelId: selectlistBaseUrl + "/GetExpectedGraduationYearsByAcademicLevelId",
    GetExpectedGraduationTermsByAcademicLevelId: selectlistBaseUrl + "/GetExpectedGraduationTermsByAcademicLevelId",
    GetAdmissionRoundByAcademicLevelId : selectlistBaseUrl + "/GetAdmissionRoundByAcademicLevelId",
    GetAdmissionRoundByTermId : selectlistBaseUrl + "/GetAdmissionRoundByTermId",
    GetEducationBackgroundsByCountryId : selectlistBaseUrl + "/GetEducationBackgroundsByCountryId",
    GetTopicByChannelId : selectlistBaseUrl + "/GetTopicByChannelId",
    GetProvinceByCountryId : selectlistBaseUrl + "/GetProvinceByCountryId",
    GetStatesByCountryId : selectlistBaseUrl + "/GetStatesByCountryId",
    GetBuildingByCampusId : selectlistBaseUrl + "/GetBuildingByCampusId",
    GetRoomByBuildingId : selectlistBaseUrl + "/GetRoomByBuildingId",
    GetAvailableRoom : selectlistBaseUrl + "/GetAvailableRoom",
    GetStateByCountryId : selectlistBaseUrl + "/GetStateByCountryId",
    GetCityByStateIdSelectList : selectlistBaseUrl + "/GetCityByStateIdSelectList",
    GetDistrictByProvinceId : selectlistBaseUrl + "/GetDistrictByProvinceId",
    GetSubdistrictByDistrictId : selectlistBaseUrl + "/GetSubdistrictByDistrictId",
    GetSectionByCourseId : selectlistBaseUrl + "/GetSectionByCourseId",
    GetEvents : selectlistBaseUrl + "/GetEvents",
    GetDepartmentsByFacultyIds : selectlistBaseUrl + "/GetDepartmentsByFacultyIds",
    GetDepartmentsByFacultyId : selectlistBaseUrl + "/GetDepartmentsByFacultyId",
    GetCitiesByCountryId : selectlistBaseUrl + "/GetCitiesByCountryId",
    GetCoursesSelectList : selectlistBaseUrl + "/GetCoursesSelectList",
    GetSectionByCourses : selectlistBaseUrl + "/GetSectionByCourses",
    GetSectionNumbersByCourses : selectlistBaseUrl + "/GetSectionNumbersByCourses",
    GetEndedTermByStartedTerm : selectlistBaseUrl + "/GetEndedTermByStartedTerm",
    GetCoursesByTerm : selectlistBaseUrl + "/GetCoursesByTerm",
    GetCoursesBySectionMasterByTerm: selectlistBaseUrl + "/GetCoursesBySectionMasterByTerm",
    GetCoursesByAcademicLevelId : selectlistBaseUrl + "/GetCoursesByAcademicLevelId",
    GetCurriculumVersion : selectlistBaseUrl + "/GetCurriculumVersion",
    GetCurriculumCourseGroups : selectlistBaseUrl + "/GetCurriculumCourseGroups",
    GetTermInstructorsByCourseId : selectlistBaseUrl + "/GetTermInstructorsByCourseId",
    GetSectionsByInstructorId : selectlistBaseUrl + "/GetSectionsByInstructor",
    GetScholarshipsByScholarshipTypeId : selectlistBaseUrl + "/GetScholarshipsByScholarshipTypeId",
    GetScholarshipExpiryTermId : selectlistBaseUrl + "/GetScholarshipExpiryTermId",
    GetReceiptItemsByCourseId : selectlistBaseUrl + "/GetReceiptItemsByCourseId",
    GetRegistrationTermsByTerm : selectlistBaseUrl + "/GetRegistrationTermsByTerm",
    GetSectionByCourseIdAndStudentId : selectlistBaseUrl + "/GetSectionByCourseIdAndStudentId",
    GetSectionMasterByTermIdAndCourseId : selectlistBaseUrl + "/GetSectionMasterByTermIdAndCourseId",
    GetSectionNumberByRegistrationId : selectlistBaseUrl + "/GetSectionNumberByRegistrationId",
    GetAmountByReceiptId : selectlistBaseUrl + "/GetAmountByReceiptId",
    GetFeeItemDefaultPrice : selectlistBaseUrl + "/GetFeeItemDefaultPrice",
    GetExceptionalCourses : selectlistBaseUrl + "/GetExceptionalCourses",
    GetCurriculumVersionForAdmissionCurriculum : selectlistBaseUrl + "/GetCurriculumVersionForAdmissionCurriculum",
    GetStudentDocumentGroup : selectlistBaseUrl + "/GetStudentDocumentGroup",
    GetStudentDocumentGroupsByCountryId : selectlistBaseUrl + "/GetStudentDocumentGroupsByCountryId",
    GetStudentByCode : selectlistBaseUrl + "/GetStudentByCode",
    GetSpecializationGroupByCurriculumVersionId : selectlistBaseUrl + "/GetSpecializationGroupByCurriculumVersionId",
    GetCurriculumVersionsByCurriculumIdAndStudentId : selectlistBaseUrl + "/GetCurriculumVersionsByCurriculumIdAndStudentId",
    GetPreviousSchoolsByGroupId : selectlistBaseUrl + "/GetPreviousSchoolsByGroupId",
    GetPreviousSchoolsByCountryId : selectlistBaseUrl + "/GetPreviousSchoolsByCountryId",
    GetCurriculumByDepartment : selectlistBaseUrl + "/GetCurriculumByDepartment",
    GetCoursesBySectionStatus : selectlistBaseUrl + "/GetCoursesBySectionStatus",
    GetCoursesByStudentAndTermForCheating : selectlistBaseUrl + "/GetCoursesByStudentAndTermForCheating",
    GetCoursesByStudentCodeAndTermForCheating : selectlistBaseUrl + "/GetCoursesByStudentCodeAndTermForCheating",
    GetDepartmentsByAcademicLevelIdAndFacultyId : selectlistBaseUrl + "/GetDepartmentsByAcademicLevelIdAndFacultyId",
    GetReceiptNumberByStudentCodeAndTerm : selectlistBaseUrl + "/GetReceiptNumberByStudentCodeAndTerm",
    GetCurriculumCourse : selectlistBaseUrl + "/GetCurriculumCourse",
    GetCoursesByAcademicLevelId : selectlistBaseUrl + "/GetCoursesByAcademicLevelId",
    GetExaminationDateByExaminationType : selectlistBaseUrl + "/GetExaminationDateByExaminationType",
    GetExaminationCourseByExaminationDate : selectlistBaseUrl + "/GetExaminationCourseByExaminationDate",
    GetCourseGroupByCurriculumsVersionAndCourse : selectlistBaseUrl + "/GetCourseGroupByCurriculumsVersionAndCourse",
    GetInstructorByCourseId : selectlistBaseUrl + "/GetInstructorByCourseId",
    GetConditionsByType : selectlistBaseUrl + "/GetConditionsByType", 
    GetSelectableCurriculumVersionList : selectlistBaseUrl + "/GetSelectableCurriculumVersionList",
    GetStudentFeeGroups : selectlistBaseUrl + "/GetStudentFeeGroups",
    GetCurriculumVersions : selectlistBaseUrl + "/GetCurriculumVersions",
    GetDegreeNamesByAcademicLevelId : selectlistBaseUrl + "/GetDegreeNamesByAcademicLevelId",
    GetSpecializationGroupByType : selectlistBaseUrl + "/GetSpecializationGroupByType",
    GetConditionsByType : selectlistBaseUrl + "/GetConditionsByType",
    GetCurriculumCourseGroups : selectlistBaseUrl + "/GetCurriculumCourseGroups",
    GetTermsByStudentCode : selectlistBaseUrl + "/GetTermsByStudentCode",
    GetTitleThByTitleEn : selectlistBaseUrl + "/GetTitleThByTitleEn",
    GetFacutiesAndDepartmentsAndCurriculumVersionsByCurriculumId : selectlistBaseUrl + "/GetFacutiesAndDepartmentsAndCurriculumVersionsByCurriculumId",
    GetFilterCourseGroupsByFacultyId : selectlistBaseUrl + "/GetFilterCourseGroupsByFacultyId"
};

// Group Registration
const GroupRegistrationURLDict = {
    SelectPlan: groupRegistrationBaseUrl + "/SelectPlan",
    SearchStudents: groupRegistrationBaseUrl + "/SearchStudents",
    SubmitRegistration: groupRegistrationBaseUrl + "/SubmitRegistration",
    SelectPlannedSchedule: groupRegistrationBaseUrl + "/SelectPlannedSchedule",
    RenderGroupRegistrationSummary: groupRegistrationBaseUrl + "/RenderGroupRegistrationSummary"
};

// Page's Controller Url 
// ***constant name defining rule => Controller name + Action name or What to do + "Url"***
// ***should order by A -> Z
const AddFacultyProgramDirector = "/Faculty/AddFacultyMember";
const AddGroupingCourseUrl = "/GraduatingRequest/AddGroupingCourse";
const AddPetitionUrl = "/PetitionManagement/AddLogs";
const AddStudentFeeGroupUrl = "/AddStudentFeeGroup/GetStudentGroup";
const AdmissionDocumentGroupDetailsUrl = "/AdmissionDocumentGroup/GetDocumentList";
const AdmissionExaminationDetailsUrl = "AdmissionExamination/Details";
const AdmissionStudentGenerateStudentCodeUrl = "/AdmissionStudent/GenerateStudentCode";
const AdmissionStudentCheckExistStudentCodeUrl = "/AdmissionStudent/IsExistStudentCode";
const AdvisorNoteCreateUrl = "/Advisor/Create";
const AgencyDetailUrl = "/Agency/Details";
const BlacklistedStudentInfoUrl = "/BlacklistedStudent/GetStudentInformation";
const CardExpirationOptionUrl = "/CardExpirationOption/GetCardExpirationOption";
const CertificateDetailUrl = "/CertificateManagement/Details";
const ChangeCourseGroupSubmit = "/GraduatingRequest/ChangeCourseGroupSubmit";
const ChangeCourseGroupUrl = "/GraduatingRequest/ChangeCourseGroup";
const ChangeStudentStatusUrl = "/Student/ChangeStudentStatus";
const CheckExaminationReservationUrl = "/ExaminationReservationManagement/CheckRoom";
const CoordinatorSearchByCourseUrl = "/Coordinator/GetInstructorSearchByCourse";
const CourseFeeDetailsUrl = "/CourseFee/Details";
const CourseOfferedChangeInstructorUrl = "/CourseToBeOffered/ChangeMainInstructor";
const CourseOfferedAddJointSection = "/CourseToBeOffered/AddJointSection";
const CourseOfferedDetailsUrl = "/CourseToBeOffered/Details";
const CourseTuitionFeeUrl = "/Course/GetTuitionFee";
const CurriculumPetitionUrl = "/ChangeCurriculumPetitionManagement/Details";
const PaymentLogDetailsUrl = "/PaymentLogReport/Details";
const DeleteGroupingCourse = "/GraduatingRequest/DeleteManuallyCourse";
const DisableGroupingCourse = "/GraduatingRequest/DisableCourse";
const EditAcceptanceLetterFeeUrl = "/AcceptanceLetterFee/EditAcceptanceLetterFee";
const EditCertificateUrl = "/CertificateManagement/Edit";
const EditFacultyProgramDirector = "/Faculty/EditFacultyMember";
const ExaminationValidateUrl = "/Examination/IsModelValid";
const ExaminationCreateUrl = "/Examination/Create";
const ExaminationEditUrl = "/Examination/Edit";
const ExaminationGetUrl = "/Examination/GetExamPeriod";
const ExaminationCoursePeriodCondition = "/ExaminationCoursePeriod/GetExaminationCoursePeriodCondition";
const ExtendedStudentCreateUrl = "/ExtendedStudent/Create";
const ExtendedStudentSendEmailUrl = "/ExtendedStudent/SendEmail";
const FeeTemplateDetailsUrl = "/FeeTemplate/Details";
const FilterCourseGroupGetCourses = "/FilterCourseGroup/GetCourses";
const FilterGroupGetCurriculumVersions = "/FilterCurriculumVersionGroup/GetCurriculumVersions";
const GetAbilityBlacklistDepartmentUrl = "/Ability/GetBlacklistDepartment";
const GetAbilityCourseUrl = "/Ability/GetCourse";
const GetLectureTeachingTypeId = "/CourseToBeOffered/GetLectureTeachingTypeId";
const GetInstructorIdBySectionId = "/ExaminationReservation/GetInstructorIdBySectionId";
const GetCreditAndRegistrationCreditByCourseId = "/Registration/GetCreditAndRegistrationCredit";
const GetMainInstructorAndCheckSeatAvailableBySectionId = "/Registration/GetMainInstructorAndCheckSeatAvailableBySectionId";
const GradeApprovalApprovesUrl = "/GradeApproval/Approves";
const GradeApprovalPreviewUrl = "/GradeApproval/Report";
const GradeBarcodeCancelUrl = "/GradeRecord/Cancel";
const GradeMaintenanceEditGradingLogRemarkUrl = "/GradeMaintenance/EditGradingLogRemark";
const GradeManagementContinueUrl = "/GradeManagement/Continue";
const GradeManagementFinishUrl = "/GradeManagement/Finish";
const GradeManagementSaveUrl = "/GradeManagement/Save";
const GradeManagementExportUrl = "/GradeManagement/Export";
const GradeScoreExportUrl = "/ScoreByInstructor/Export";
const GradeScoreImportUrl = "/ScoreByInstructor/Import";
const GradeManagementGetSectionsUrl = "/GradeManagement/SectionsByCourseIds";
const GradeManagementGetStudentsUrl = "/GradeManagement/AllStudentBySectionIds";
const GradeManagementImportUrl = "/GradeManagement/Import";
const GradeManagementGetGradingLog = "/GradeManagement/GetGradingLog";
const GradeManagementSaveStudentScore = "/GradeManagement/SaveStudentScore";
const GradeManagementGetGradingAllocationUrl = "/GradeManagement/GetGradingAllocation";
const GradeManagementGetGradingStatisticsUrl = "/GradeManagement/GetGradingStatistics";
const GradeManagementGetSelectedStandardGradingGroupsUrl = "/GradeManagement/GetSelectedStandardGradingGroups";
const GradeManagementGradingResultUrl = "/GradeManagement/GradingResult";
const GradeScoreSummaryGetGradeInfo = "/GradeScoreSummary/GetGradingByStudentRawScoreId";
const GradeScoreSummaryGetGradelogs = "/GradeScoreSummary/GetGradeLogsByStudentRawScoreId";
const GradeScoreSummaryUpdateGrade = "/GradeScoreSummary/UpdateGrade";
const GradingLogDetailUrl = "/GradingReport/Details";
const GraduatingRequestChangeStatus = "/GraduatingRequest/ChangeStatus";
const HolidayDetailUrl = "/HolidayCalendar/Details";
const InvoiceDetailUrl = "/Invoice/InvoiceDetails";
const InvoicePartModalDetailUrl = "/Invoice/InvoicePartModalDetails";
const LatePaymentValidateUrl = "/LatePayment/IsModelValid";
const LatePaymentCreateUrl = "/LatePayment/Create";
const LatePaymentEditUrl = "/LatePayment/Edit";
const LatePaymentFindUrl = "/LatePayment/FindLatePaymentTransaction";
const MarkAllocationEditUrl = "/MarkAllocation/EditMarkAllocation";
const MaxSeatLimitBySectionSeatLimitUrl = "/CourseToBeOffered/MaxSeatLimitBySectionSeatLimit";
const GetNextSectionNumberUrl = "/CourseToBeOffered/GetNextSectionNumber";
const PetitionDetailUrl = "/PetitionManagement/Details";
const PlanCourseDetailsUrl = "/Plan/GetCourseDetials";
const PreviousSchoolDetailsUrl = "/PreviousSchool/Details";
const PrintingLogSaveTrackingNumberUrl = "/PrintingLogReport/SaveTrackingNumber";
const PublishGradeApprovalUrl = "/GradeApproval/Publish";
const QuestionnaireApprovalLogUrl = "/QuestionnaireApproval/Details";
const ReceiptDetailsUrl = "/Receipt/Details";
const ReceiptGetCourseByFeeItemUrl = "/Receipt/GetCourseByFeeItemId";
const ReceiptTransectionUrl = "/Receipt/Transaction";
const RefundBankChangeRefundedAtUrl = "/RefundBankReport/SaveRefundedAt";
const RefundCoursesGetInvoiceCourseFeeItemUrl = "/RefundCourses/GetInvoiceCourseFeeItem";
const RegistrationGetSectionsUrl = "/Registration/GetAvailableSections";
const RegistrationAddingUrl = "/Registration/Add";
const RegistrationCheckCredit = "/Registration/CheckCredit";
const RegistrationCheckScheduleConflicts = "/Registration/CheckScheduleConflicts";
const RegistrationCheckExamConflicts = "/Registration/CheckExamConflicts";
const RegistrationCheckPrerequisite = "/Registration/CheckPrerequisite";
const RegistrationCheckCourseCorequisite = "/Registration/CheckCourseCorequisite";
const RegistrationShowSchedule = "/Registration/ShowSchedule";
const RegistrationConditionDetail = "/RegistrationCondition/GetRegistrationConditionDetail";
const RoomDetailsUrl = "/Room/Details";
const ExaminationReservationUrl = "/ExaminationReservationManagement/Edit";
const RoomReservationEditStatusUrl = "/RoomReservationManagement/EditStatus";
const RoomSlotReservationCancelUrl = "/RoomReservationManagement/CancelRoomSlot";
const RoomReservationDetails = "/RoomReservation/Details";
const SectionSlotCreateUrl = "/SectionSlot/CreateModal";
const SectionSlotEditUrl = "/SectionSlot/EditModal";
const SectionSlotRoomEditUrl = "/SectionSlotRoom/EditModal";
const StandardGradingGroupGradeTemplate = "/StandardGradingGroup/GetGradeTemplate";
const StudentCodeRangeIsExistCodeRangeUrl = "/StudentCodeRange/IsExistCodeRange";
const StudentCodeStatusGetCodeRangeUrl = "/StudentCodeStatus/GetStudentCodeRange";
const StudentProbationCreateUrl = "/StudentProbation/Create";
const StudentProbationSendEmailUrl = "/StudentProbation/SendEmail";
const StudentRegistrationCourseByTerm = "DeleteAllCourse/GetRegistrationByStudentAndTerm"
const StudentUploadProfileImageUrl = "/Student/UploadProfileImage";
const StudentsBySections = "/ScoreByInstructor/GetStudentsBySections";
const StudyPlanCreateUrl = "/StudyPlan/Create";
const StudyPlanEditUrl = "/StudyPlan/Edit";
const SeatEditUrl = "/SeatAvailable/EditSeat";
const ScholarshipBudgetTableUrl = "/ScholarshipBudget/GetScholarshipBudgetTable";
const SubmitAddGroupingCourseUrl = "/GraduatingRequest/SubmitAddGroupingCourse";
const EditScholarshipStudentUrl = "/ScholarshipProfile/EditScholarshipStudent";
const GetSpecializationGroupsUrl = "/GraduatingRequest/GetSpecializationGroups";
const GetAcademicHonorsUrl = "/GraduatingRequest/GetAcademicHonors";
const VoucherDetailsUrl = "/Voucher/Details";
const ExaminationReservationDetailsUrl = "/ExaminationReservation/Details";
const TeachingLoadGetInstructorCourseUrl = "/TeachingLoad/GetInstructorCourse";
const TranscriptGetFacutyAndDepartmentUrl = "/Transcript/TranscriptGetFacutyAndDepartment";
const VerificationLetterGetStudentsUrl = "/VerificationLetter/GetStudent";
const WithdrawalExceptionCreateByFacultyUrl = "/WithdrawalException/CreateExceptionByFaculty";
const WithdrawalExceptionCreateByDepartmentsUrl = "/WithdrawalException/CreateExceptionByDepartments";
const WithdrawalExceptionCreateByCourseUrl = "/WithdrawalException/CreateExceptionByCourse";
const WithdrawalExceptionByFacultyUrl = "/WithdrawalException/ExceptionByFaculty";
const WithdrawalExceptionDeleteUrl = "/WithdrawalException/Delete";
const WithdrawalGetApprovedbyUrl = "/Withdrawal/GetApprovedby";
const WithdrawalGetRegistrationCourseByTypeUrl = "/Withdrawal/GetRegistrationCourseByType";
const WithdrawalSearchByCourseUrl = "/Withdrawal/SearchByCourse";
const WithdrawalLogsUrl = "/Withdrawal/Logs"
const ModifyRegistrationCourseLogsUrl = "/ModifyRegistrationCourse/Logs";


const StudentDetailsSubURL = {
    AddressEditUrl : "/Address/Edit",
    CheatingStatusDetailsUrl : "/CheatingStatus/Details",
    CheatingStatusEditUrl : "/CheatingStatus/Edit",
    ParentEditUrl : "/Parent/Edit",
    ParentDetailsUrl : "/Parent/Details",
    MaintenanceStatusEditUrl : "/MaintenanceStatus/Edit",
    CurriculumInformationEditUrl : "/CurriculumInformation/Edit",
    CurriculumInformationCreateUrl : "/CurriculumInformation/Create"
};

// Page's SelectList Url
const GetFacultyByCurriculumId = "/Student/GetFacultyByCurriculumId";
const RenderReceiptModal = "/Receipt/RenderReceiptModal";
const RenderMatchCourseInvoice = "/MatchCourse/RenderMatchCourseInvoice";
const TermFeeDetailsModal = "/TermFee/Details";
const GetGradeLogs = "/GradeMaintenance/Details"

// Page's Scholarship Profile
const AddScholarshipStudent = "/ScholarshipProfile/AddScholarshipStudent"
const GetScholarshipLimitBudget = "/ScholarshipProfile/GetLimitBugetByScholarshipId"
const GetGradeById = "/GradeTemplate/GetGrade";

// AdmissionStudent
const GetAdmissionFirstClassDate = "/AdmissionStudent/GetAdmissionFirstClassDate"

// UserRole
const GetRoleById = "/Role/GetRole";
const GetUserRoleById = "/Role/GetUserRole";

//USpark
const USparkCalculateFee = "/USparkCalculateFee/Calculate";
const USparkQuestionaire = "/USparkQuestionnaire/Questionnaire"

//SectionMenagementApi
const GetSectionApi = "/SectionAPI/GetSections";
const GetRoomApi = "/SectionAPI/GetRooms";
const GetTeachingApi = "/SectionAPI/GetTeachingTypes";
const AddMakeUp = "/SectionAPI/CreateMakeUp";
const UpdateStatusSectionSlotApi = "/SectionAPI/UpdateStatusSectionSlot";
const UpdateExamApi = "/SectionAPI/UpdateExam";
const GetExaminationReservation = "SectionAPI/GetExaminationReservations"
const UpdateTeachingTypeSectionSlot = "SectionAPI/UpdateTeachingTypeSectionSlot"
const UpdateStatusExam = "SectionAPI/UpdateStatusExam"
const GetInstructorOfCourse = "SectionAPI/GetInstructorOfTerm"

//StudentAPI
const AddStudents ="/Student/Adds";

//Invoice
const UpdateIsPaid ="Invoice/Update";
