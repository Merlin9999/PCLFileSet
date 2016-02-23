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
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInRoot.zzz";
            const string fileNotFound1 = "FileInFolder.yyy";
            const string fileNameToFind2 = "FileInSubFolder.yyy";
            const string fileNotFound2 = "FileInFolder.zzz";
            string filePathToFind1 = PortablePath.Combine(fileNameToFind1);
            string filePathNotFound1 = PortablePath.Combine("a", fileNotFound1);
            string filePathToFind2 = PortablePath.Combine("a", "b", fileNameToFind2);
            string filePathNotFound2 = PortablePath.Combine("c", fileNotFound2);
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
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(2));
            Assert.That(files, Has.No.Member(filePathNotFound1));
            Assert.That(files, Has.No.Member(filePathNotFound2));
            Assert.That(files, Has.Member(filePathToFind1));
            Assert.That(files, Has.Member(filePathToFind2));
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
        public async Task MultipleIncludesAndAnExclusionsAsObservable()
        {
            var sys = new MemoryFileSystemFake();
            const string fileNameToFind1 = "FileInRoot.zzz";
            const string fileNotFound1 = "FileInFolder.yyy";
            const string fileNameToFind2 = "FileInSubFolder.yyy";
            const string fileNotFound2 = "FileInFolder.zzz";
            string filePathToFind1 = PortablePath.Combine(fileNameToFind1);
            string filePathNotFound1 = PortablePath.Combine("a", fileNotFound1);
            string filePathToFind2 = PortablePath.Combine("a", "b", fileNameToFind2);
            string filePathNotFound2 = PortablePath.Combine("c", fileNotFound2);
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

            IObservable<string> filesObservable = await fs.GetFilesAsObservableAsync();

            List<string> files = new List<string>();
            filesObservable.Subscribe(
                filePath => files.Add(filePath),
                () =>
                {
                    //TODO: The assert below throws an exception, but it's swallowed up and not reported.

                    Assert.That(files.Count, Is.EqualTo(1));
                    //Assert.That(files.Count, Is.EqualTo(2));
                    Assert.That(files, Has.No.Member(filePathNotFound1));
                    Assert.That(files, Has.No.Member(filePathNotFound2));
                    Assert.That(files, Has.Member(filePathToFind1));
                    Assert.That(files, Has.Member(filePathToFind2));
                });
        }
    }
}
