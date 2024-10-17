using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class MenuPermission : UserTimeStamp
    {
        public long Id { get; set; }
        public string? RoleId { get; set; }
        public string? UserId { get; set; }

        [Required]
        public long MenuId { get; set; }
        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [JsonIgnore]
        [ForeignKey("RoleId")]
        public virtual IdentityRole? Role { get; set; }
        
        [JsonIgnore]
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
    }
}