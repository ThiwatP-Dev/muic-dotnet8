using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class MenuSubgroup 
    {
        public long Id { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public long MenuGroupId { get; set; }
        public int SequenceNo { get; set; }

        [JsonIgnore]
        [ForeignKey("MenuGroupId")]
        public virtual MenuGroup MenuGroup { get; set; }

        [JsonIgnore]
        public virtual List<Menu> Menus { get; set; }
    }
}