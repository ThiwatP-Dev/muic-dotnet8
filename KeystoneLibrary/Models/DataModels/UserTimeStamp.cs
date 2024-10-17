using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class UserTimeStamp : Entity
    {
        DateTime _current = DateTime.UtcNow;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string LastUpdate => $"{ UpdatedAt.ToString(StringFormat.ShortDateTime) }";

        [NotMapped]
        public string CreatedAtText => CreatedAt.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string CreatedDateTimeText => CreatedAt.AddHours(7).ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public string UpdatedAtText => UpdatedAt.ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public string UpdatedDateTimeText => UpdatedAt.AddHours(7).ToString(StringFormat.ShortDateTime);

        public override void OnBeforeInsert(string userId = null)
        {
            this.CreatedAt = _current;
            this.CreatedBy = userId;
            this.UpdatedAt = _current;
            this.UpdatedBy = userId;
        }

        public override void OnBeforeUpdate(string userId = null)
        {
            this.UpdatedAt = _current;
            this.UpdatedBy = userId;
        }

        [NotMapped]
        public string CreatedByFullNameEn { get; set; }

        [NotMapped]
        public string ApprovedByFullNameEn { get; set; }

        [NotMapped]
        public string UpdatedByFullName { get; set; }
    }
}