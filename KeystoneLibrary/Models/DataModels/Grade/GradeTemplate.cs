using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class GradeTemplate : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)] 
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string GradeIds { get; set; }

        [NotMapped]
        public List<Grade> Grades { get; set; }

        [NotMapped]
        public List<long> GradeIdsLong => JsonConvert.DeserializeObject<List<long>>(GradeIds);
    }
}