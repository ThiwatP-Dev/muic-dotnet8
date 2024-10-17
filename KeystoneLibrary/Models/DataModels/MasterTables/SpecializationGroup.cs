using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class SpecializationGroup : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(500)]
        public string NameTh { get; set; }

        [Required]
        [StringLength(500)]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string? ShortNameTh { get; set; }

        [StringLength(200)]
        public string? ShortNameEn { get; set; }
        
        [StringLength(5)]
        public string? Type { get; set; } // m = minor, c = concentration, a = ability, mdl = module

        public long? MUICId { get; set; }
        public virtual List<SpecializationGroupBlackList> SpecializationGroupBlackLists { get; set; }
        public virtual List<CurriculumSpecializationGroup> CurriculumSpecializationGroups { get; set; }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                     case TYPE_MINOR_CODE:
                        return "Minor";
                    case TYPE_CONCENTRATION_CODE:
                        return "Concentration";
                    case TYPE_ABILITY_CODE:
                        return "Ability";
                    case TYPE_MODULE_CODE:
                        return "Module";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public bool IsForceTrack { get; set; }

        [NotMapped]
        public const string TYPE_ABILITY_CODE = "a";
        [NotMapped]
        public const string TYPE_CONCENTRATION_CODE = "c";
        [NotMapped]
        public const string TYPE_MINOR_CODE = "m";
        [NotMapped]
        public const string TYPE_MODULE_CODE = "mdl";
    }
}