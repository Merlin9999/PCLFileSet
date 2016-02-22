using System.Collections.Generic;

namespace PCLFileSet
{
    public interface IFileSet
    {
        IFileSet Include(string globPath);
        IFileSet Exclude(string globPath);

        IEnumerable<string> GetFiles();
        IEnumerable<string> GetFolders();
    }
}