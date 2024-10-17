namespace KeystoneLibrary.Models
{
    public enum UploadSubDirectory
    {
        [StringValue("StudentDocuments")] 
        STUDENT_DOCUMENTS,

        [StringValue("Signatory")] 
        SIGNATORY,
        
        [StringValue("StudentProfileImage")] 
        STUDENT_PROFILE_IMAGE,

        [StringValue("ChangeName")] 
        CHANGE_NAME,

        [StringValue("AdmissionProfileImage")] 
        ADMISSION_PROFILE_IMAGE,

        [StringValue("AdmissionStudentDocuments")] 
        ADMISSION_STUDENT_DOCUMENTS,
        
        [StringValue("GradeMaintenance")] 
        GRADE_MAINTENANCE,

        [StringValue("TuitionFeeReport")]
        TUITION_FEE_REPORT
    }
}