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
        private MemoryFileSystemFake _fileSys;

        public string Name { get; }
        public string Path { get; }

        public MemoryFolderFake(MemoryFileSystemFake fileSys, string folderPath)
        {
            this._fileSys = fileSys;
            this.Name = this._fileSys.GetFolderPath(folderPath);
            this.Path = folderPath;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IFile> GetFileAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task<IList<IFile>> GetFilesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this._fileSys.EnumerateFiles(this.Path)
                .Select(fp => new MemoryFileFake(this._fileSys, fp))
                .ToList<IFile>();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task<IFolder> GetFolderAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task<IList<IFolder>> GetFoldersAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this._fileSys.EnumerateFolders(this.Path)
                .Select(fp => new MemoryFolderFake(this._fileSys, fp))
                .ToList<IFolder>();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ExistenceCheckResult> CheckExistsAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
 #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
   }
}