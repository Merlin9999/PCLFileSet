using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCLFileSet
{
    public interface IFileSet
    {
        IFileSet Include(string globPath);
        IFileSet Exclude(string globPath);

        Task<IEnumerable<string>> GetFilesAsync();
        Task<IEnumerable<string>> GetFoldersAsync();
    }
}