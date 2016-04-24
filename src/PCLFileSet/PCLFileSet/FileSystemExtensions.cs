using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCLStorage;

namespace PCLFileSet
{
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static IEnumerable<string> GetFiles(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return fileSet.GetFiles();
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static async Task<IEnumerable<string>> GetFilesAsync(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return await fileSet.GetFilesAsync();
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static IObservable<string> GetFilesAsObservable(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return fileSet.GetFilesAsObservable();
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static async Task<IObservable<string>> GetFilesAsObservableAsync(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return await fileSet.GetFilesAsObservableAsync();
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static IEnumerable<string> GetFolders(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return fileSet.GetFolders();
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static async Task<IEnumerable<string>> GetFoldersAsync(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return await fileSet.GetFoldersAsync();
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static IObservable<string> GetFoldersAsObservable(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return fileSet.GetFoldersAsObservable();
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="globPath">A glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc".</param>
        /// <param name="basePath">The base path. May be a relative or absolute path..</param>
        public static async Task<IObservable<string>> GetFoldersAsObservableAsync(this IFileSystem fileSystem,
            string globPath, string basePath = null)
        {
            var fileSet = new FileSet(fileSystem, basePath: basePath);
            fileSet.Include(globPath);
            return await fileSet.GetFoldersAsObservableAsync();
        }
    }
}