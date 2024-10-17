using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ChangeCurriculumPetitionViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        [Required]
        public string Status { get; set; }
        public string Remark { get; set; }
    }
}