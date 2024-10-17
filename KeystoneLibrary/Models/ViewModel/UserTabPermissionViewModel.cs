namespace KeystoneLibrary.Models.ViewModels
{
    public class UserTabPermissionViewModel
    {
        public string Tab { get; set; }
        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
    }
}