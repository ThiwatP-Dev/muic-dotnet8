using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels
{
    public class Configuration
    {
        public long Id { get; set; }

        [StringLength(100)]
        public string? Key { get; set; }

        public string? Value { get; set; }

        public string? Remark { get; set; }
    }
}