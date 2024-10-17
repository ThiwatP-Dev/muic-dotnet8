using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Withdrawals
{
    public class WithdrawalException : UserTimeStamp
    {
        public long Id { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CourseId { get; set; }
        public Guid? StudentId { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}