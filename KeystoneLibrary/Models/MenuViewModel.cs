namespace KeystoneLibrary.Models
{
    public class MenuViewModel
    {
        public string header { get; set; }
        public string icon { get; set; }
        public List<SubmenuViewModel> submenus { get; set; }
    }

    public class SubmenuViewModel
    {
        public string submenus_header { get; set; }
        public List<MenuItemViewModel> submenus_items { get; set; }
    }

    public class MenuItemViewModel
    {
        public string title { get; set; }
        public string url { get; set; }
    }
}