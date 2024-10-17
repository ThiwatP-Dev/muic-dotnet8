using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.Authentication
{
    public class MenuType
    {
        /// Menu type defines types of menu in technical angle
        /// It tells which kind of menu it is, such as page or tab.
        /// Code is used as a static value to be used to compare aginst the checking value to lessen typo error chances.
        public long Id { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public int SequenceNo { get; set; }
    }
}