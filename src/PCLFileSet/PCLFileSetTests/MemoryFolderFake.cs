using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCLStorage;

namespace PCLFileSetTests
{
    public class MemoryFolderFake : IFolder
    {
        private readonly MemoryFileSystemFake _fileSys;

        public string Name { get; }
        public string Path { get; }

        public HashSet<string> Files { get; }
        public Dictionary<string, MemoryFolderFake> Folders { get; }
        public MemoryFolderFake Parent { get; }

        public MemoryFolderFake(MemoryFileSystemFake fileSys, string folderPath, MemoryFolderFake parent)
        {
            this._fileSys = fileSys;
            this.Name = folderPath == null ? null : this._fileSys.GetFolderPath(folderPath);
            this.Path = folderPath;
            this.Parent = parent;
            this.Files = new HashSet<string>(this._fileSys.EntryNameComparer);
            this.Folders = new Dictionary<string, MemoryFolderFake>(this._fileSys.EntryNameComparer);
        }

#pragma warning disable CS1998 // Async methods lacks 'await' operators and will run synchronously

        public async Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IFile> GetFileAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IList<IFile>> GetFilesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.Files
                .Select(fp => new MemoryFileFake(this._fileSys, this.Path == null ? fp : PortablePath.Combine(this.Path, fp)))
                .ToList<IFile>();
        }

        public async Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IFolder> GetFolderAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IList<IFolder>> GetFoldersAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.Folders.Values.ToList<IFolder>();
        }

        public async Task<ExistenceCheckResult> CheckExistsAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

#pragma warning restore CS1998 // Async methods lacks 'await' operators and will run synchronously

    }
}