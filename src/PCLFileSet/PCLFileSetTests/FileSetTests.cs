using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PCLFileSet;
using PCLStorage;

namespace PCLFileSetTests
{
    [TestFixture]
    public class FileSetTests
    {
        [Test]
        public async Task IncludeAllFilesInSubFolderInFolder()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind = "FileInSubFolder.txt";
            string filePathToFind = PortablePath.Combine("a", "b", fileNameToFind);
            sys.AddFiles(x => x
                .File("FileInRoot.txt")
                .Folder("a", a => a
                    .File("FileInFolder.txt")
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File(fileNameToFind)))
                .Folder("c", c => c
                    .File("FileInFolder.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(PortablePath.Combine("a", "b", "*"));
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member(filePathToFind));
        }

        [Test]
        public async Task IncludeAllFilesInSubFolder()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind = "FileInSubFolder.txt";
            string filePathToFind = PortablePath.Combine("b", fileNameToFind);
            sys.AddFiles(x => x
                .File("FileInRoot.txt")
                .Folder("a", a => a
                    .File("FileInFolder.txt")
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File(fileNameToFind)))
                .Folder("c", c => c
                    .File("FileInFolder.txt")));
            var fs = new FileSet(sys, "/" + "a");
            fs.Include(PortablePath.Combine("b", "*"));
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member(filePathToFind));
        }

        [Test]
        public async Task IncludeFolderFilesAndSubFolderFiles()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInFolder.txt";
            const string fileNameToFind2 = "AnotherFileInFolder.txt";
            const string fileNameToFind3 = "FileInSubFolder.txt";
            string filePathToFind1 = PortablePath.Combine("a", fileNameToFind1);
            string filePathToFind2 = PortablePath.Combine("a", fileNameToFind2);
            string filePathToFind3 = PortablePath.Combine("a", "b", fileNameToFind3);
            sys.AddFiles(x => x
                .File("FileInRoot.txt")
                .Folder("a", a => a
                    .File(fileNameToFind1)
                    .File(fileNameToFind2)
                    .Folder("b", b => b
                        .File(fileNameToFind3)))
                .Folder("c", c => c
                    .File("FileInFolder.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(PortablePath.Combine("a", "**", "*"));
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(filePathToFind1));
            Assert.That(files, Has.Member(filePathToFind2));
            Assert.That(files, Has.Member(filePathToFind3));
        }

        [Test]
        public async Task IncludeAllFilesInAnyFolderWithASpecificExtension()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInRoot.zzz";
            const string fileNameToFind2 = "FileInSubFolder.zzz";
            const string fileNameToFind3 = "FileInFolder.zzz";
            string filePathToFind1 = PortablePath.Combine(fileNameToFind1);
            string filePathToFind2 = PortablePath.Combine("a", "b", fileNameToFind2);
            string filePathToFind3 = PortablePath.Combine("c", fileNameToFind3);
            sys.AddFiles(x => x
                .File(fileNameToFind1)
                .Folder("a", a => a
                    .File("FileInFolder.txt")
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File(fileNameToFind2)))
                .Folder("c", c => c
                    .File(fileNameToFind3)));
            var fs = new FileSet(sys);
            fs.Include(PortablePath.Combine("**", "*.zzz"));
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(filePathToFind1));
            Assert.That(files, Has.Member(filePathToFind2));
            Assert.That(files, Has.Member(filePathToFind3));
        }

        [Test]
        public async Task IncludeAllFilesInAnyFolderWithASpecificExtensionWithRootedFileSpec()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInRoot.zzz";
            const string fileNameToFind2 = "FileInSubFolder.zzz";
            const string fileNameToFind3 = "FileInFolder.zzz";
            string filePathToFind1 = PortablePath.Combine(fileNameToFind1);
            string filePathToFind2 = PortablePath.Combine("a", "b", fileNameToFind2);
            string filePathToFind3 = PortablePath.Combine("c", fileNameToFind3);
            sys.AddFiles(x => x
                .File(fileNameToFind1)
                .Folder("a", a => a
                    .File("FileInFolder.txt")
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File(fileNameToFind2)))
                .Folder("c", c => c
                    .File(fileNameToFind3)));
            var fs = new FileSet(sys, "/");
            fs.Include("**/*.zzz");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(filePathToFind1));
            Assert.That(files, Has.Member(filePathToFind2));
            Assert.That(files, Has.Member(filePathToFind3));
        }

        [Test]
        public async Task MultipleIncludesAndAnExclusions()
        {
            string filePathToFind1;
            string filePathNotFound1;
            string filePathToFind2;
            string filePathNotFound2;
            FileSet fs = CreateFileSetForMultipleIncludesAndAnExclusions(
                out filePathToFind1, out filePathToFind2, out filePathNotFound1, out filePathNotFound2);

            List<string> files = (await fs.GetFilesAsync()).ToList();

            ValidateFileSetForMultipleIncludesAndAnExclusions(files,
                filePathToFind1, filePathToFind2, filePathNotFound1, filePathNotFound2);
        }

        [Test]
        public async Task MultipleIncludesAndAnExclusionsAsObservable()
        {
            string filePathToFind1;
            string filePathNotFound1;
            string filePathToFind2;
            string filePathNotFound2;
            FileSet fs = CreateFileSetForMultipleIncludesAndAnExclusions(
                out filePathToFind1, out filePathToFind2, out filePathNotFound1, out filePathNotFound2);

            IObservable<string> filesObservable = await fs.GetFilesAsObservableAsync();

            List<string> files = new List<string>();
            filesObservable
                .Finally(() =>
                {
                    ValidateFileSetForMultipleIncludesAndAnExclusions(files, 
                        filePathToFind1, filePathToFind2, filePathNotFound1, filePathNotFound2);
                })
                .Subscribe(
                    filePath => files.Add(filePath));
        }

        private static FileSet CreateFileSetForMultipleIncludesAndAnExclusions(
            out string filePathToFind1, out string filePathToFind2, 
            out string filePathNotFound1, out string filePathNotFound2)
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInRoot.zzz";
            const string fileNotFound1 = "FileInFolder.yyy";
            const string fileNameToFind2 = "FileInSubFolder.yyy";
            const string fileNotFound2 = "FileInFolder.zzz";
            filePathToFind1 = PortablePath.Combine(fileNameToFind1);
            filePathNotFound1 = PortablePath.Combine("a", fileNotFound1);
            filePathToFind2 = PortablePath.Combine("a", "b", fileNameToFind2);
            filePathNotFound2 = PortablePath.Combine("c", fileNotFound2);
            sys.AddFiles(x => x
                .File(fileNameToFind1)
                .Folder("a", a => a
                    .File(fileNotFound1)
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File(fileNameToFind2)))
                .Folder("c", c => c
                    .File(fileNotFound2)));
            var fs = new FileSet(sys, "/");
            fs.Include("**/*.zzz");
            fs.Include("**/*.yyy");
            fs.Exclude("**/*eInF*z*");
            fs.Exclude("a/*.yyy");
            return fs;
        }

        private static void ValidateFileSetForMultipleIncludesAndAnExclusions(
            List<string> foundFiles, 
            string filePathToFind1, string filePathToFind2, 
            string filePathNotFound1, string filePathNotFound2)
        {
            Assert.That(foundFiles.Count, Is.EqualTo(2));
            Assert.That(foundFiles, Has.No.Member(filePathNotFound1));
            Assert.That(foundFiles, Has.No.Member(filePathNotFound2));
            Assert.That(foundFiles, Has.Member(filePathToFind1));
            Assert.That(foundFiles, Has.Member(filePathToFind2));
        }

        [Test]
        public async Task IncludeAllAndMultipleExcludesWithQuestionMarkWildcard()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind = "FileInFolder.yyy";
            string filePathToFind = PortablePath.Combine("a", fileNameToFind);
            sys.AddFiles(x => x
                .File("FileInRoot.bzz")
                .Folder("a", a => a
                    .File(fileNameToFind)
                    .File("AnotherFileInFolder.txt")
                    .Folder("b", b => b
                        .File("FileInSubFolder.txt")))
                .Folder("c", c => c
                    .File("FileInFolder.azz")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\*");
            fs.Exclude(@"?\?\*");
            fs.Exclude(@"**\Another*");
            fs.Exclude(@"**\*?zz");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member(filePathToFind));
        }

        [Test]
        public async Task IncludeAllFoldersInTree()
        {
            var sys = new MemoryFileSystemFake();
            const string folderNameToFind1 = "Folder1";
            const string folderNameToFind2 = "Folder2";
            const string folderNameToFind3 = "Folder3";
            const string folderNameToFind4 = "Folder4";
            const string folderNameToFind5 = "Folder5";
            string folderPathToFind3 = PortablePath.Combine(folderNameToFind2, folderNameToFind3);
            string folderPathToFind4 = PortablePath.Combine(folderNameToFind2, folderNameToFind4);
            sys.AddFilesAndFolders(x => x
                .Folder(folderNameToFind1, a => a)
                .Folder(folderNameToFind2, a => a
                    .Folder(folderNameToFind3, b => b)
                    .Folder(folderNameToFind4, b => b))
                .Folder(folderNameToFind5, c => c));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\*");
            List<string> files = (await fs.GetFoldersAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(5));
            Assert.That(files, Has.Member(folderNameToFind1));
            Assert.That(files, Has.Member(folderNameToFind2));
            Assert.That(files, Has.Member(folderPathToFind3));
            Assert.That(files, Has.Member(folderPathToFind4));
            Assert.That(files, Has.Member(folderNameToFind5));
        }

        [Test]
        public async Task IncludeAllFoldersInSubFolder()
        {
            var sys = new MemoryFileSystemFake();
            const string folderNameToFind1 = "Folder1";
            const string folderNameToFind2 = "Folder2";
            const string folderNameToFind3 = "Folder3";
            const string folderNameToFind4 = "Folder4";
            const string folderNameToFind5 = "Folder5";
            sys.AddFilesAndFolders(x => x
                .Folder(folderNameToFind1, a => a)
                .Folder(folderNameToFind2, a => a
                    .Folder(folderNameToFind3, b => b)
                    .Folder(folderNameToFind4, b => b))
                .Folder(folderNameToFind5, c => c));
            var fs = new FileSet(sys, "/" + folderNameToFind2);
            fs.Include(@"**\*");
            List<string> files = (await fs.GetFoldersAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(2));
            Assert.That(files, Has.Member(folderNameToFind3));
            Assert.That(files, Has.Member(folderNameToFind4));
        }
    }
}
