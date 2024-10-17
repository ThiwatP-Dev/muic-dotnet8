using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.Enums
{
    public enum CertificateEnum
    {
        [Display(Name = "รับรองจบระบุวันเข้าศึกษาและวันสำเร็จการศึกษา")]
        GraduatedWithAdmissionAtGraduatedAt,

        [Display(Name = "รับรองจบระบุวันรับปริญญา")]
        GraduatedWithCeremonyAt,

        [Display(Name = "รับรองจบระบุวันเข้าศึกษา สำเร็จการศึกษา")]
        GraduatedWithAdmissionDateAndGraduatedDate,

        [Display(Name = "รับรองจบหลักสูตรเรียนภาษาอังกฤษ ระบุ Assessment")]
        GraduatedWithEnglishAssessment,

        [Display(Name = "รับรองจบฉบับธรรมดา")]
        GraduatedNormalForm,

        [Display(Name = "หนังสือรับรองจบหลักสูตรภาษาอังกฤษ")]
        GraduatedEnglishInstruction,

        [Display(Name = "หนังสือรับรองผ่อนผันเกณฑ์ทหาร")]
        StudentDraftDeferment,

        [Display(Name = "รับรองคาดว่าจะจบ หลักสูตรเรียนภาษาอังกฤษ")]
        GraduatingStatusCertification,

        [Display(Name = "รับรองคาดจบรอผลสอบและรับรองหลักสูตรอังกฤษ")]
        GraduatingWaitingFinalResult,

        [Display(Name = "หนังสือรับรองฐานะปี ระบุ Assessment")]
        StatusCertificateWithAssessment,

        [Display(Name = "หนังสือรับรองสมัครงานการบินไทย")]
        ThaiAirwayCertification,

        [Display(Name = "รับรองคาดจบรอเกรดออก")]
        GraduatingWaitingGrade,

        [Display(Name = "รับรองชั้นปีระบุGPA")]
        AcademicYearWithGPA,

        [Display(Name = "รับรองฐานะปี")]
        ConfirmAcademicYear,

        [Display(Name = "รับรองเคยเป็นนักศึกษาระบุวันเข้าออก")]
        StudentWithAdmissionFromTo,

        [Display(Name = "รับรองเคยเป็นนักศึกษา")]
        StudentStatus,

        [Display(Name = "Transcript")]
        Transcript,

        [Display(Name = "ใบแปลปริญญา")]
        TranslationGraduatedCertificate,

        [Display(Name = "รับรองค่าใช้จ่าย")]
        ExpensesOutlineCertificate,

        [Display(Name = "รับรองจบระบุ IELTS")]
        GraduationWithIELTS,

        [Display(Name = "รับรองระบุไปต่างประเทศ")]
        GoingAbroadCertification,

        [Display(Name = "รับรองนักศึกษาใหม่")]
        CertifyNewStudent,

        [Display(Name = "รับรองจบระบุ Ranking")]
        GraduationRankingCertification,

        [Display(Name = "หนังสือรับรองระบุเป็นบุคคลคนเดียวกันเปลี่ยนชื่อ")]
        ChangingStudentName
    }
}