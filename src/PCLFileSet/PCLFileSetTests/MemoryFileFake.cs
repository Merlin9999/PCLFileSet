using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PCLStorage;
using FileAccess = PCLStorage.FileAccess;

namespace PCLFileSetTests
{
    public class MemoryFileFake : IFile
    {
        private readonly MemoryFileSystemFake _fileSys;

        public string Name { get; }
        public string Path { get; }

        public MemoryFileFake(MemoryFileSystemFake fileSys, string filePath)
        {
            this._fileSys = fileSys;
            this.Name = this._fileSys.GetFolderPath(filePath);
            this.Path = filePath;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task RenameAsync(string newName, NameCollisionOption collisionOption = NameCollisionOption.FailIfExists,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task MoveAsync(string newPath, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}