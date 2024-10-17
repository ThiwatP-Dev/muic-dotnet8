using System.Runtime.InteropServices;

namespace KeystoneLibrary.Helpers
{
    public class FolderHelper
    {
        public static class KnownFolder
        {
            public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        }

        [DllImport("shell32.dll", CharSet=CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        public static string GetFolderPath(Guid folder)
        {
            SHGetKnownFolderPath(folder, 0, IntPtr.Zero, out string path);
            return path;
        }
    }
}