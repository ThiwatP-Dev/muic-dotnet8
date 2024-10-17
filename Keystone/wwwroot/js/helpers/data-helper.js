function isTrue(value) {
    return value.toLowerCase() == "true";
}

function isUndefinedValue(value) {
    return value === undefined;
}

function GetStudentCodeFormat(code) {
    var prefix = code.slice(0, 3);
    var suffix = code.slice(3);
    return prefix.concat('-', suffix);
}

function toCapitalizationFormat(string) {
    return string.replace(/\w\S*/g, function(txt) {
            return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
        }
    );
}

const AddressType = {
    State : "state", 
    Province : "province",
    City : "city",
    District : "district",
    Subdistrict : "subdistrict"
}

const RegularExpressions = {
    AllDigitInParenthesis : /(\d+)/g,
    AllDigitInBracket : /\[(\d+)\]/g,
    AllWhitespace : /\s+/g, // equal to [\r\n\t\f\v ]
    SingleDigitInBracket : /\[(\d+)\]/,
    LastDigitInBracket : /\[([?^\d+]+]*)\][^[]*$/,
    SingleDigit : /\d+/,
    IsIntOrFloat : /^\d*\.?\d+$/g,
}

const ErrorMessage = {
    SaveSuccess : "Saved.",
    InvalidInput : "Unable to save, invalid input in some fields.",
    DataDuplicate : "already exist in database.",
    ServiceUnavailable : "Unable to delete because it is currently in use.",
    RequiredData : "Please fill all required data.",
    StudentNotFound : "Student not found.",
    NegativeData : "Data cannot be negative.",
    EmptyOrSpaceData : "Data cannot be empty or space."
}

const CertificateType = {
    AcademicYearWithGPA : "AcademicYearWithGPA",
    CertifyNewStudent: "CertifyNewStudent",
    ChangingStudentName: "ChangingStudentName",
    ConfirmAcademicYear: "ConfirmAcademicYear",
    ExpensesOutlineCertificate: "ExpensesOutlineCertificate",
    GoingAbroadCertification: "GoingAbroadCertification",
    GraduatedEnglishInstruction: "GraduatedEnglishInstruction",
    GraduatedNormalForm : "GraduatedNormalForm",
    GraduatedWithAdmissionDateAndGraduatedDate: "GraduatedWithAdmissionDateAndGraduatedDate",
    GraduatedWithCeremonyAt : "GraduatedWithCeremonyAt",
    GraduatedWithCurriculum: "GraduatedWithCurriculum",
    GraduatedWithEnglishAssessment: "GraduatedWithEnglishAssessment",
    GraduatingStatusCertification: "GraduatingStatusCertification",
    GraduatingWaitingFinalResult: "GraduatingWaitingFinalResult",
    GraduatingWaitingGrade: "GraduatingWaitingGrade",
    GraduationRankingCertification: "GraduationRankingCertification",
    GraduationWithIELTS: "GraduationWithIELTS",
    StatusCertificateWithAssessment: "StatusCertificateWithAssessment",
    StudentDraftDeferment: "StudentDraftDeferment",
    StudentStatus : "StudentStatus",
    StudentWithAdmissionFromTo : "StudentWithAdmissionFromTo",
    ThaiAirwayCertification: "ThaiAirwayCertification",
    TranslationGraduatedCertificate: "TranslationGraduatedCertificate",
}