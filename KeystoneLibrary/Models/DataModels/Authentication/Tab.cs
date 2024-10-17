using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class Tab
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
        public long MenuId { get; set; }
        public int SequenceNo { get; set; }

        [JsonIgnore]
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }

        [JsonIgnore]
        public virtual List<TabPermission> Tabs { get; set; }
    }
}