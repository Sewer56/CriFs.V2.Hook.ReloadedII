using System.Runtime.InteropServices;

namespace CriFs.V2.Hook.Utilities;

public class Native
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(int hWnd, String text, String caption, uint type);
}