using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class Barcode : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(200)] 
        public string? BarcodeNumber { get; set; }

        public long CourseId { get; set; }
        public long SectionId { get; set; }

        [StringLength(500)] 
        public string? SectionIds { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime GeneratedAt { get; set; }

        public bool IsPublished { get; set; }

        [StringLength(200)]
        public string? PublishedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PublishedAt { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [NotMapped]
        public string Sections { get; set; }

        [NotMapped]
        public string GeneratedAtText => GeneratedAt.ToLocalTime().ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public string PublishedAtText => PublishedAt?.ToLocalTime().ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public string ApprovedAtText => ApprovedAt?.ToLocalTime().ToString(StringFormat.ShortDateTime);
    }
}