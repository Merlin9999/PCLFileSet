using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCLFileSet
{
    public interface IFileSet
    {
        IFileSet Include(string globPath);
        IFileSet Exclude(string globPath);

        IFileSet Catch<TException>(Action<TException> exceptionHandler) 
            where TException : Exception;

        IFileSet Catch<TException>(Action<TException> exceptionHandler, SynchronizationContext synchronizationContext)
            where TException : Exception;

        Task<IEnumerable<string>> GetFilesAsync();
        Task<IEnumerable<string>> GetFoldersAsync();

        Task<IObservable<string>> GetFilesAsObservableAsync();
        Task<IObservable<string>> GetFoldersAsObservableAsync();
    }
}