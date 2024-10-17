using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KeystoneLibrary.Models.Api
{
    /// <summary>
    /// Create Holiday Object
    /// </summary>
    public class CreateHolidayViewModel
    {
        [Required]
        public long MuicId { get; set; }

        [Required]        
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }
        
        public bool IsAllowMakeup { get; set; }

        /// <summary>
        /// Format of [ dd/MM/yyyy HH:mm ] year is AD (2024). Specified same date with `EndedAt` equal to same day. Must be less than `EndedAt`
        /// </summary>
        [Required]
        public string StartedAt { get; set; }

        /// <summary>
        /// Format of [ dd/MM/yyyy HH:mm ] year is AD (2024)
        /// </summary>
        [Required]
        public string EndedAt { get; set; }

        public List<long> CancelSectionSlotIds { get; set; }

        [Required]
        public string CreatedBy { get; set; }
    }

    public class HolidayViewModel
    {
        public long Id { get; set; }
        public long MuicId { get; set; }

        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        public bool IsAllowMakeup { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndedAt { get; set; }
        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}