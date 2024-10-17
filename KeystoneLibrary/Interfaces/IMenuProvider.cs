using KeystoneLibrary.Models;

namespace KeystoneLibrary.Interfaces
{
    public interface IMenuProvider
    {
        List<MenuViewModel> GetAllMenu();
        List<MenuViewModel> GetAllMenu(string userId, List<string> roleIds);
    }
}