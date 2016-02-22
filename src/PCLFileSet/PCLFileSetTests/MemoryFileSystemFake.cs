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
        private Folder RootFolder { get; }
        private Folder CurrentWorkingFolder { get; set; }

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
            this.RootFolder = new Folder(this, null, null);
            this.CurrentWorkingFolder = this.RootFolder;

            // Not needed for current unit tests.
            this.LocalStorage = null;
            this.RoamingStorage = null;
        }

        public void AddFiles(params string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                string folderPath = this.GetFolderPath(filePath);
                Folder folder = this.FindFolder(folderPath, createIfDoesNotExist: true);
                folder.Files.Add(this.GetFileName(filePath));
            }
        }

        public void AddFiles(Func<IAddFolderEntry, IAddFolderEntry> folderEntryAdderFunc)
        {
            var addFolderEntryImpl = new AddFolderEntryImpl(this);
            folderEntryAdderFunc(addFolderEntryImpl);
            this.AddFiles(addFolderEntryImpl.GetFilePaths());
        }

        public IEnumerable<string> EnumerateFiles(string basePath)
        {
            Folder basePathFolder = this.FindFolder(basePath);
            if (basePathFolder != null)
                foreach (string file in this.EnumerateFiles(basePathFolder, string.Empty))
                    yield return file;
        }

        public async Task<IFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            string folderPath = this.GetFolderPath(path);
            string fileName = this.GetFileName(path);
            Folder folder = this.FindFolder(folderPath);

            if (folder.Files.Any(file => this.EntryNameComparer.Compare(file, fileName) != 0))
            {
                if (this.ThrowOnError)
                    throw new FileNotFoundException($"File not found: \"{path}\"");
                return null;
            }

            return new MemoryFileFake(this, path);
        }

        public async Task<IFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            return new MemoryFolderFake(this, path);
        }

        private IEnumerable<string> EnumerateFiles(Folder folder, string nestedPath)
        {
            foreach (string file in folder.Files)
            {
                if (nestedPath.Length > 0)
                    yield return this.CombinePath(nestedPath, file);
                else
                    yield return file;
            }

            foreach (Folder subFolder in folder.Folders.Values)
            {
                string subFolderPath;

                if (nestedPath.Length > 0)
                    subFolderPath = this.CombinePath(nestedPath, subFolder.Name);
                else
                    subFolderPath = subFolder.Name;

                foreach (string filePath in this.EnumerateFiles(subFolder, subFolderPath))
                    yield return filePath;
            }
        }

        public IEnumerable<string> EnumerateFolders(string basePath)
        {
            Folder basePathFolder = this.FindFolder(basePath);
            if (basePathFolder != null)
                foreach (string folderPath in this.EnumerateFolders(basePathFolder, string.Empty))
                    yield return folderPath;
        }

        private IEnumerable<string> EnumerateFolders(Folder folder, string nestedPath)
        {
            foreach (Folder subFolder in folder.Folders.Values)
            {
                string subFolderPath;

                if (nestedPath.Length > 0)
                    subFolderPath = this.CombinePath(nestedPath, subFolder.Name);
                else
                    subFolderPath = subFolder.Name;

                yield return subFolderPath;

                foreach (string folderPath in this.EnumerateFolders(subFolder, subFolderPath))
                    yield return folderPath;
            }
        }

        public bool IsPathRooted(string path)
        {
            path = path.TrimStart();
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

        private Folder FindFolder(string folderPath, bool createIfDoesNotExist = false)
        {
            if (folderPath == null)
                return this.CurrentWorkingFolder;

            string[] pathSegments = this.GetPathSegments(folderPath);

            Folder curFolder =
                this.IsPathRooted(folderPath)
                    ? this.RootFolder
                    : this.CurrentWorkingFolder;

            for (int index = 0; index < pathSegments.Length; index++)
            {
                string curSegment = pathSegments[index];
                Folder foundFolder;

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
                        foundFolder = new Folder(this, curSegment, curFolder);
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

        private string[] GetPathSegments(Folder folder)
        {
            var nameSegments = new Stack<string>();

            while (!string.IsNullOrWhiteSpace(folder.Name))
            {
                nameSegments.Push(folder.Name);
                folder = folder.Parent;
            }

            return nameSegments.ToArray();
        }

        private string BuildPathString(IEnumerable<string> segments, bool makeRooted)
        {
            string path = makeRooted
                ? string.Join(this.PreferredFolderSeparator, new [] {string.Empty}, segments)
                : string.Join(this.PreferredFolderSeparator, segments);

            if (makeRooted && path.Length == 0)
                return this.PreferredFolderSeparator;

            return path;
        }

        private class Folder
        {
            private readonly MemoryFileSystemFake _fileSystem;
            public string Name { get; set; }
            public Folder Parent { get; }

            public Folder(MemoryFileSystemFake fileSystem, string name, Folder parent)
            {
                this._fileSystem = fileSystem;
                this.Name = name;
                this.Parent = parent;
                this.Files = new HashSet<string>(this._fileSystem.EntryNameComparer);
                this.Folders = new Dictionary<string, Folder>(this._fileSystem.EntryNameComparer);
            }

            public HashSet<string> Files { get; }
            public Dictionary<string, Folder> Folders { get; }
        }
    }
}
