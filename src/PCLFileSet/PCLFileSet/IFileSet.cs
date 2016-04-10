using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCLFileSet
{
    public interface IFileSet
    {
        /// <summary>
        /// Specify a glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc"
        /// </summary>
        /// <param name="globPath">The glob path.</param>
        IFileSet Include(string globPath);

        /// <summary>
        /// Specify a glob path to be excluded. Ex: "**\*.txt" and "root/*/a?.doc"
        /// </summary>
        /// <param name="globPath">The glob path.</param>
        IFileSet Exclude(string globPath);

        /// <summary>
        /// Catches the specified exception type. Note: The exception handler delegate is executed on 
        /// <see cref="SynchronizationContext.Current"/> value that was when Catch() was called. 
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be caught and handled.</typeparam>
        /// <param name="exceptionHandler">The exception handler. Commonly and empty block. Ex: ex => { }</param>
        IFileSet Catch<TException>(Action<TException> exceptionHandler) 
            where TException : Exception;

        /// <summary>
        /// Catches the specified exception type. 
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be caught and handled.</typeparam>
        /// <param name="exceptionHandler">The exception handler. Commonly and empty block. Ex: ex => { }</param>
        /// <param name="synchronizationContext">The exception handler delegate is executed on this 
        /// <see cref="SynchronizationContext"/></param>.
        IFileSet Catch<TException>(Action<TException> exceptionHandler, SynchronizationContext synchronizationContext)
            where TException : Exception;

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        IEnumerable<string> GetFiles();

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        IEnumerable<string> GetFolders();

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        Task<IEnumerable<string>> GetFilesAsync();

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        Task<IEnumerable<string>> GetFoldersAsync();

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        IObservable<string> GetFilesAsObservable();

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        IObservable<string> GetFoldersAsObservable();

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        Task<IObservable<string>> GetFilesAsObservableAsync();

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        Task<IObservable<string>> GetFoldersAsObservableAsync();
    }
}