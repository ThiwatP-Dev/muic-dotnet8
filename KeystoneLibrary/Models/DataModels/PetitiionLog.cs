using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class PetitionLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long StudentPetitionId { get; set; }

        [Required]
        [StringLength(5000)]
        public string Comment { get; set; }

        [ForeignKey("StudentPetitionId")]
        public virtual StudentPetition StudentPetition { get; set; }
    }
}