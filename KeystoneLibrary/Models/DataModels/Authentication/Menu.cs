using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class Menu
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TitleTh { get; set; }

        [Required]
        [StringLength(100)]
        public string TitleEn { get; set; }

        [Required]
        [StringLength(2100)]
        public string Url { get; set; }
        public bool IsDeleted { get; set; }
        public long MenuTypeId { get; set; }
        public long MenuGroupId { get; set; }
        public long MenuSubgroupId { get; set; }
        public int SequenceNo { get; set; }

        [JsonIgnore]
        [ForeignKey("MenuTypeId")]
        public virtual MenuType MenuType { get; set; }

        [JsonIgnore]
        [ForeignKey("MenuGroupId")]
        public virtual MenuGroup MenuGroup { get; set; }

        [JsonIgnore]
        [ForeignKey("MenuSubgroupId")]
        public virtual MenuSubgroup MenuSubgroup { get; set; }
    }
}