using System.Text;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
/// Helper methods for dealing with paths.
/// </summary>
internal static class PathHelper
{
    public static string GetFirstDirectory(this string path, int startIndex, out bool hasBackslash)
    {
        int firstBackSlash = path.IndexOf('\\', startIndex);
        int firstForwardSlash = path.IndexOf('/', startIndex);

        // If neither slash is found, return the whole path
        hasBackslash = true;
        if (firstBackSlash == -1 && firstForwardSlash == -1)
        {
            hasBackslash = false;
            return path;
        }

        // If one of the slashes is not found, use the one that's found
        int firstSlash = (firstBackSlash == -1) ? firstForwardSlash : 
            (firstForwardSlash == -1) ? firstBackSlash :
            Math.Min(firstBackSlash, firstForwardSlash); 

        return path.Substring(0, firstSlash);
    }
    
    public static void TrimFinalNewline(this StringBuilder sb)
    {
        while (sb.Length > 0 && sb[^1] == '\n')
            sb.Length--;
    }
    
    public static bool IsSymbolicLink(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.LinkTarget != null;
    }
}