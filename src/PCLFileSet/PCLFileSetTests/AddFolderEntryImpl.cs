using System;
using System.Collections.Generic;
using System.Linq;

namespace PCLFileSetTests
{
    public class AddFolderEntryImpl : IAddFolderEntry
    {
        private readonly MemoryFileSystemFake _fileSystemFake;
        private List<string> _pathSegments = new List<string>();
        private List<string> _addedFiles = new List<string>();
        private List<string> _addedFolders = new List<string>();

        public AddFolderEntryImpl(MemoryFileSystemFake fileSystemFake)
        {
            this._fileSystemFake = fileSystemFake;
        }

        public IAddFolderEntry File(string fileName)
        {
            List<string> tempPathSegList = this._pathSegments.ToList();
            tempPathSegList.Add(fileName);
            this._addedFiles.Add(this._fileSystemFake.CombinePath(tempPathSegList));
            return this;
        }

        public IAddFolderEntry Folder(string folderName, Func<IAddFolderEntry, IAddFolderEntry> folderEntryAdderFunc)
        {
            this._pathSegments.Add(folderName);
            folderEntryAdderFunc(this);
            this._addedFolders.Add(this._fileSystemFake.CombinePath(this._pathSegments));
            this._pathSegments.RemoveAt(this._pathSegments.Count - 1);
            return this;
        }

        public string[] GetFilePaths()
        {
            return this._addedFiles.ToArray();
        }

        public string[] GetFolderPaths()
        {
            return this._addedFolders.ToArray();
        }
    }
}