using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using PCLStorage;

namespace PCLFileSetTests
{
    public class MemoryFileSystemFake : IFileSystem
    {
        private MemoryFolderFake RootFolder { get; }
        private MemoryFolderFake CurrentWorkingFolder { get; set; }

        public bool IsCaseSensitive { get; }
        public static string PreferredFolderSeparatorStatic => @"\";
        public static string[] FolderSeparatorsStatic => new[] { @"\", @"/" };


        public string PreferredFolderSeparator => PreferredFolderSeparatorStatic;
        public string[] FolderSeparators => FolderSeparatorsStatic;
        public StringComparer EntryNameComparer { get; }

        public bool ThrowOnError { get; set; }

        public IFolder LocalStorage { get; }
        public IFolder RoamingStorage { get; }

        public MemoryFileSystemFake(bool isCaseInsensitive = false)
        {
            this.IsCaseSensitive = isCaseInsensitive;
            this.EntryNameComparer = isCaseInsensitive
                ? StringComparer.CurrentCulture
                : StringComparer.CurrentCultureIgnoreCase;
            this.RootFolder = new MemoryFolderFake(this, null, null);
            this.CurrentWorkingFolder = this.RootFolder;

            // Not needed for current unit tests.
            this.LocalStorage = null;
            this.RoamingStorage = null;
        }

        public void AddFiles(params string[] filePaths)
        {
            AddFilesAndFolders(filePaths, new string[] {});
        }

        public void AddFilesAndFolders(string[] filePaths, string[] folderPaths)
        {
            foreach (string filePath in filePaths)
            {
                string folderPath = this.GetFolderPath(filePath);
                MemoryFolderFake folder = this.FindFolder(folderPath, createIfDoesNotExist: true);
                folder.Files.Add(this.GetFileName(filePath));
            }

            foreach (string folderPath in folderPaths)
            {
                this.FindFolder(folderPath, createIfDoesNotExist: true);
            }
        }


        public void AddFiles(Func<IAddFolderEntry, IAddFolderEntry> folderEntryAdderFunc)
        {
            var addFolderEntryImpl = new AddFolderEntryImpl(this);
            folderEntryAdderFunc(addFolderEntryImpl);
            this.AddFiles(addFolderEntryImpl.GetFilePaths());
        }

        public void AddFilesAndFolders(Func<IAddFolderEntry, IAddFolderEntry> folderEntryAdderFunc)
        {
            var addFolderEntryImpl = new AddFolderEntryImpl(this);
            folderEntryAdderFunc(addFolderEntryImpl);
            this.AddFilesAndFolders(addFolderEntryImpl.GetFilePaths(), addFolderEntryImpl.GetFolderPaths());
        }

#pragma warning disable CS1998 // Async methods lacks 'await' operators and will run synchronously

        public async Task<IFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            string folderPath = this.GetFolderPath(path);
            string fileName = this.GetFileName(path);
            MemoryFolderFake folder = this.FindFolder(folderPath);

            if (folder.Files.All(file => this.EntryNameComparer.Compare(file, fileName) != 0))
            {
                if (this.ThrowOnError)
                    throw new FileNotFoundException($"File not found: \"{path}\"");
                return null;
            }

            return new MemoryFileFake(this, path);
        }

        public async Task<IFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.FindFolder(path);
        }

#pragma warning restore CS1998 // Async methods lacks 'await' operators and will run synchronously

        public bool IsPathRooted(string path)
        {
            return this.FolderSeparators.Any(sep => path.StartsWith(sep));
        }

        public string[] GetPathSegments(string folderPath)
        {
            string[] pathSegments = folderPath.Split(this.FolderSeparators, StringSplitOptions.RemoveEmptyEntries);
            return pathSegments;
        }

        public string CombinePath(params string[] pathSegments)
        {
            return string.Join(this.PreferredFolderSeparator, pathSegments);
        }

        public string CombinePath(IEnumerable<string> pathSegments)
        {
            return string.Join(this.PreferredFolderSeparator, pathSegments);
        }

        public string GetFolderPath(string path)
        {
            bool pathIsRooted = this.IsPathRooted(path);
            string[] pathSegments = this.GetPathSegments(path);
            return this.BuildPathString(pathSegments.Take(pathSegments.Length - 1), pathIsRooted);
        }

        public string GetFileName(string path)
        {
            string[] pathSegments = this.GetPathSegments(path);
            return pathSegments.Last();
        }

        internal MemoryFolderFake FindFolder(string folderPath, bool createIfDoesNotExist = false)
        {
            if (folderPath == null)
                return this.CurrentWorkingFolder;

            string[] pathSegments = this.GetPathSegments(folderPath);

            MemoryFolderFake curFolder =
                this.IsPathRooted(folderPath)
                    ? this.RootFolder
                    : this.CurrentWorkingFolder;

            for (int index = 0; index < pathSegments.Length; index++)
            {
                string curSegment = pathSegments[index];
                MemoryFolderFake foundFolder;

                if (curSegment == ".")
                {
                    continue;
                }

                if (curSegment == "..")
                {
                    if (curFolder.Parent == null)
                    {
                        if (this.ThrowOnError)
                            throw new DirectoryNotFoundException("Root folder does NOT have a parent");
                        return null;
                    }

                    curFolder = curFolder.Parent;
                    continue;
                }

                if (!curFolder.Folders.TryGetValue(curSegment, out foundFolder))
                {
                    if (createIfDoesNotExist)
                    {
                        foundFolder = new MemoryFolderFake(this, curFolder.Path == null ? curSegment : PortablePath.Combine(curFolder.Path, curSegment), curFolder);
                        curFolder.Folders.Add(curSegment, foundFolder);
                    }
                    else if (this.ThrowOnError)
                    {
                        throw new DirectoryNotFoundException(
                            $"Folder not found: {this.BuildPathString(pathSegments.Take(index + 1), false)}");
                    }
                    else
                    {
                        return null;
                    }
                }

                curFolder = foundFolder;
            }

            return curFolder;
        }

        //private string[] GetPathSegments(MemoryFolderFake folder)
        //{
        //    var nameSegments = new Stack<string>();

        //    while (!string.IsNullOrWhiteSpace(folder.Name))
        //    {
        //        nameSegments.Push(folder.Name);
        //        folder = folder.Parent;
        //    }

        //    return nameSegments.ToArray();
        //}

        private string BuildPathString(IEnumerable<string> segments, bool makeRooted)
        {
            string path = makeRooted
                ? string.Join(this.PreferredFolderSeparator, new [] {string.Empty}, segments)
                : string.Join(this.PreferredFolderSeparator, segments);

            if (makeRooted && path.Length == 0)
                return this.PreferredFolderSeparator;

            return path;
        }
    }
}
