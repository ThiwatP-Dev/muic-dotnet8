using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class MenuGroup 
    {
        /// Menu group defines types of menu in information based angle
        /// It tells which group of information the related menu is, such as student information, instructor information or course information.
        /// Code is used as a static value to be used to compare aginst the checking value to lessen typo error chances.
        public long Id { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Icon { get; set; }
        public int SequenceNo { get; set; }

        [JsonIgnore]
        public virtual List<Menu> Menus { get; set; }

        [JsonIgnore]
        public virtual List<MenuSubgroup> MenuSubgroups { get; set; }
    }
}