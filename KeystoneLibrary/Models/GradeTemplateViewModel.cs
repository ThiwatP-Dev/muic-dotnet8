using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class GradeTemplateViewModel
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(500)]
        public string GradeIds
        {
            get
            {
                return JsonConvert.SerializeObject(Grades.Select(x => x.Id));
            }
        }

        public List<Grade> Grades { get; set; }
    }
}