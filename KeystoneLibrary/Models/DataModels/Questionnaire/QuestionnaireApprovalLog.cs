using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class QuestionnaireApprovalLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long QuestionnaireApprovalId { get; set; }

        [JsonIgnore] 
        [ForeignKey("QuestionnaireApprovalId")]
        public virtual QuestionnaireApproval QuestionnaireApproval { get; set; }
        // MUIC : STAFF (FIRST APPROVED), PD (SECOND APPROVED)
        //          w = WAITING STAFF APRROVE
        //          s = WAITING PD APRROVE
        //          p = PD APRROVE
        public string? Status { get; set; }  
    }
}