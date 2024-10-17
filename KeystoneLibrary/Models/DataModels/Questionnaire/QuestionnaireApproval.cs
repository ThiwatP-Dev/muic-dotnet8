using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class QuestionnaireApproval : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public long? InstructorId { get; set; }
        public long TermId { get; set; }
        public int TotalSurvey { get; set; }
        public int TotalEnrolled { get; set; }
        public decimal TotalSectionScore { get; set; } // Per Each Instructor
        public decimal TotalSectionMJScore { get; set; } // Per Each Instructor
        public decimal TotalSectionSD { get; set; } // Per Each Instructor
        public decimal TotalSectionMJSD { get; set; } 
        public decimal TotalAnswerValue { get; set; }
        public decimal TotalQuestion { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        
        // MUIC : STAFF (FIRST APPROVED), PD (SECOND APPROVED)
        //          w = Waiting for Staff Approval
        //          s = Waiting for PD Approval
        //          p = Approved by PD
        public string? Status { get; set; }  
    }
}