using System.IO;

namespace CRViewer
{
    public static class PathHelper
    {
        public static string GetParentPath(string path)
        {
            return Path.GetFullPath(System.IO.Path.Combine(path, ".."));
        }
    }
}
