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
            var fs = new FileSet(sys, "/a");
            fs.Include(PortablePath.Combine("b", "*"));
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member(filePathToFind));
        }

        [Test]
        public async Task IncludeAllFilesInSubFolderWithBaseFolderEndingInSeparatorChar()
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
            var fs = new FileSet(sys, "/a/");
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
                .Subscribe(filePath => files.Add(filePath));
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

        [Test]
        public async Task MatchOnFolderAfterStarStarWithFolderMatchOnStar()
        {
            var sys = new MemoryFileSystemFake();
            const string folderToMatchOn = "FolderToMatchOn";
            const string foundFileName1 = "IncludedFile1.txt";
            const string foundFileName2 = "IncludedFile2.txt";
            const string foundFileName3 = "IncludedFile3.txt";
            string foundFilePath1 = PortablePath.Combine("B", "C", folderToMatchOn, "MatchByStar1", foundFileName1);
            string foundFilePath2 = PortablePath.Combine("B", "C", folderToMatchOn, "MatchByStar1", foundFileName2);
            string foundFilePath3 = PortablePath.Combine("B", "C", folderToMatchOn, "MatchByStar2", foundFileName3);
            sys.AddFilesAndFolders(x => x
                .Folder("A", a => a
                    .File("ExcludedFile.txt"))
                .Folder("B", b => b
                    .File("ExcludedFile.txt")
                    .Folder("C", c => c
                        .Folder(folderToMatchOn, d => d
                            .File("ExcludedFile.txt")
                            .Folder("MatchByStar1", e => e
                                .File(foundFileName1)
                                .File(foundFileName2)
                                .Folder("ExcludedFolder", f => f
                                    .File("ExcludedFile.txt")))
                            .Folder("MatchByStar2", f => f
                                .File(foundFileName3))
                            .File("AnotherExcludedFile.txt")
                        )))
                .Folder("Z", z => z
                    .File("ExcludedFile.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\" + folderToMatchOn + @"\*\*");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
        }

        [Test]
        public async Task MatchOnFolderAfterStarStarWithAnotherStarStar()
        {
            var sys = new MemoryFileSystemFake();
            const string folderToMatchOn = "FolderToMatchOn";
            const string foundFileName1 = "IncludedFile.1zz";
            const string foundFileName2 = "IncludedFile.2zz";
            const string foundFileName3 = "IncludedFile.3zz";
            const string foundFileName4 = "IncludedFile.4zz";
            string foundFilePath1 = PortablePath.Combine("B", "C", folderToMatchOn, foundFileName1);
            string foundFilePath2 = PortablePath.Combine("B", "C", folderToMatchOn, "D", foundFileName2);
            string foundFilePath3 = PortablePath.Combine("B", "C", folderToMatchOn, "D", "E", foundFileName3);
            string foundFilePath4 = PortablePath.Combine("B", "C", folderToMatchOn, "F", foundFileName4);
            sys.AddFilesAndFolders(x => x
                .Folder("A", a => a
                    .File("ExcludedFile.txt"))
                .Folder("B", b => b
                    .File("ExcludedFile.xzz")
                    .Folder("C", c => c
                        .Folder(folderToMatchOn, d => d
                            .File(foundFileName1)
                            .Folder("D", e => e
                                .File("ExcludedFile.a1z")
                                .File(foundFileName2)
                                .Folder("E", f => f
                                    .File(foundFileName3)))
                            .Folder("F", f => f
                                .File("ExcludedFile.txt")
                                .File(foundFileName4))
                            .File("AnotherExcludedFile.txt")
                        )))
                .Folder("Z", z => z
                    .File("ExcludedFile.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\" + folderToMatchOn + @"\**\*.?zz");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(4));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
            Assert.That(files, Has.Member(foundFilePath4));
        }

        [Test]
        public async Task CaseInsensitiveFileMatch()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.txt";
            string foundFilePath2 = PortablePath.Combine("A", foundFileName);
            string foundFilePath3 = PortablePath.Combine("B", foundFileName);
            string foundFilePath4 = PortablePath.Combine("B", "C", foundFileName);
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File("ExcludedFile.txt")
                    .File(foundFileName))
                .Folder("B", b => b
                    .File(foundFileName)
                    .File("ExcludedFile.txt")
                    .Folder("C", c => c
                        .File(foundFileName)
                        .File("ExcludedFile.txt")
                    ))
                .Folder("Z", z => z
                    .File("ExcludedFile.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\" + foundFileName.ToLower());
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(4));
            Assert.That(files, Has.Member(foundFileName));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
            Assert.That(files, Has.Member(foundFilePath4));
        }

        [Test]
        public async Task CaseInsensitiveFolderMatchGettingFiles()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.Txt";
            string foundFilePath1 = PortablePath.Combine("A", "zxcASDFqwe", foundFileName);
            string foundFilePath2 = PortablePath.Combine("A", "qweASDFzxc", foundFileName);
            string foundFilePath3 = PortablePath.Combine("C", "zxcASDFqwe", foundFileName);
            string foundFilePath4 = PortablePath.Combine("C", "zxcASDFqwe", "zzzASDFzzz", foundFileName);
            string foundFilePath5 = PortablePath.Combine("C", "zxcASDFqwe", "qweASDFzxc", foundFileName);
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File(foundFileName)
                    .Folder("zxcASDFqwe", zxc => zxc
                        .File(foundFileName))
                    .Folder("B", b => b
                        .File(foundFileName))
                    .Folder("qweASDFzxc", qwe => qwe
                        .File(foundFileName)))
                .Folder("C", c => c
                    .File(foundFileName)
                    .Folder("zxcASDFqwe", zxc => zxc
                        .File(foundFileName)
                        .Folder("zzzASDFzzz", zzz => zzz
                            .File(foundFileName))
                        .Folder("D", d => d
                            .File(foundFileName))
                        .Folder("qweASDFzxc", qwe => qwe
                            .File(foundFileName))))
                .Folder("Z", z => z
                    .File("ExcludedFile.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\*asdf*\" + foundFileName.ToLower());
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(5));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
            Assert.That(files, Has.Member(foundFilePath4));
            Assert.That(files, Has.Member(foundFilePath5));
        }

        [Test]
        public async Task CaseInsensitiveFolderMatchGettingFolders()
        {
            var sys = new MemoryFileSystemFake();
            const string ignoredFileName = "IncludedFile.Txt";
            string foundFolderPath1 = PortablePath.Combine("A", "zxcASDFqwe");
            string foundFolderPath2 = PortablePath.Combine("A", "qweASDFzxc");
            string foundFolderPath3 = PortablePath.Combine("C", "zxcASDFqwe");
            string foundFolderPath4 = PortablePath.Combine("C", "zxcASDFqwe", "zzzASDFzzz");
            string foundFolderPath5 = PortablePath.Combine("C", "zxcASDFqwe", "qweASDFzxc");
            sys.AddFilesAndFolders(x => x
                .File(ignoredFileName)
                .Folder("A", a => a
                    .File(ignoredFileName)
                    .Folder("zxcASDFqwe", zxc => zxc
                        .File(ignoredFileName))
                    .Folder("B", b => b
                        .File(ignoredFileName))
                    .Folder("qweASDFzxc", qwe => qwe
                        .File(ignoredFileName)))
                .Folder("C", c => c
                    .File(ignoredFileName)
                    .Folder("zxcASDFqwe", zxc => zxc
                        .File(ignoredFileName)
                        .Folder("zzzASDFzzz", zzz => zzz
                            .File(ignoredFileName))
                        .Folder("D", d => d
                            .File(ignoredFileName))
                        .Folder("qweASDFzxc", qwe => qwe
                            .File(ignoredFileName))))
                .Folder("Z", z => z
                    .File("ExcludedFile.txt")));
            var fs = new FileSet(sys, "/");
            fs.Include(@"**\*asdf*");
            List<string> folders = (await fs.GetFoldersAsync()).ToList();

            Assert.That(folders.Count, Is.EqualTo(5));
            Assert.That(folders, Has.Member(foundFolderPath1));
            Assert.That(folders, Has.Member(foundFolderPath2));
            Assert.That(folders, Has.Member(foundFolderPath3));
            Assert.That(folders, Has.Member(foundFolderPath4));
            Assert.That(folders, Has.Member(foundFolderPath5));
        }

        [Test]
        public async Task BasePathSupportsDotDotAndDotFolders()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.Txt";
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File(foundFileName)
                    .Folder("B", b => b
                        .File(foundFileName))
                    .Folder("C", c => c
                        .File(foundFileName)))
                .Folder("D", d => d
                    .File(foundFileName)
                    .Folder("E", e => e
                        .File(foundFileName))
                    .Folder("F", f => f
                        .File(foundFileName)))
                .Folder("G", g => g
                    .File(foundFileName)));
            var fs1 = new FileSet(sys, @"A\C\..\B\..\..\.\D\E\.\..\.\F");
            fs1.Include(@"*");
            List<string> files = (await fs1.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member(foundFileName));

            string foundFilePath1 = PortablePath.Combine("E", foundFileName);
            string foundFilePath2 = PortablePath.Combine("F", foundFileName);
            var fs2 = new FileSet(sys, @"A\C\..\B\..\..\.\D\E\.\..\.");
            fs2.Include(@"*\*");
            files = (await fs2.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(2));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
        }

        [Test]
        public async Task GetFilesStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.Txt";
            string foundFilePath2 = PortablePath.Combine("A", foundFileName);
            string foundFilePath3 = PortablePath.Combine("A", "B", foundFileName);
            string foundFilePath4 = PortablePath.Combine("A", "C", foundFileName);
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File(foundFileName)
                    .Folder("B", b => b
                        .File(foundFileName))
                    .Folder("C", c => c
                        .File(foundFileName))));

            List<string> files = (await FileSet.GetFilesAsync(sys, @"**\*")).ToList();

            Assert.That(files.Count, Is.EqualTo(4));
            Assert.That(files, Has.Member(foundFileName));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
            Assert.That(files, Has.Member(foundFilePath4));
        }

        [Test]
        public async Task GetFilesWithBaseFolderStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.Txt";
            string foundFilePath2 = PortablePath.Combine("B", foundFileName);
            string foundFilePath3 = PortablePath.Combine("C", foundFileName);
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File(foundFileName)
                    .Folder("B", b => b
                        .File(foundFileName))
                    .Folder("C", c => c
                        .File(foundFileName))));

            List<string> files = (await FileSet.GetFilesAsync(sys, @"**\*", basePath: @"\A")).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(foundFileName));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
        }

        [Test]
        public async Task GetFilesAsObservableStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            const string foundFileName = "IncludedFile.Txt";
            string foundFilePath2 = PortablePath.Combine("A", foundFileName);
            string foundFilePath3 = PortablePath.Combine("A", "B", foundFileName);
            string foundFilePath4 = PortablePath.Combine("A", "C", foundFileName);
            sys.AddFilesAndFolders(x => x
                .File(foundFileName)
                .Folder("A", a => a
                    .File(foundFileName)
                    .Folder("B", b => b
                        .File(foundFileName))
                    .Folder("C", c => c
                        .File(foundFileName))));

            IObservable<string> filesObservable = await FileSet.GetFilesAsObservableAsync(sys, @"**\*");
            List<string> files = new List<string>();

            filesObservable
                .Finally(() =>
                {
                    Assert.That(files.Count, Is.EqualTo(4));
                    Assert.That(files, Has.Member(foundFileName));
                    Assert.That(files, Has.Member(foundFilePath2));
                    Assert.That(files, Has.Member(foundFilePath3));
                    Assert.That(files, Has.Member(foundFilePath4));
                })
                .Subscribe(filePath => files.Add(filePath));
        }

        [Test]
        public async Task GetFoldersStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            string foundFilePath1 = PortablePath.Combine("A");
            string foundFilePath2 = PortablePath.Combine("A", "B");
            string foundFilePath3 = PortablePath.Combine("A", "C");
            sys.AddFilesAndFolders(x => x
                .Folder("A", a => a
                    .Folder("B", b => b)
                    .Folder("C", c => c)));

            List<string> files = (await FileSet.GetFoldersAsync(sys, @"**\*")).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
            Assert.That(files, Has.Member(foundFilePath3));
        }

        [Test]
        public async Task GetFoldersWithBaseFolderStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            string foundFilePath1 = PortablePath.Combine("B");
            string foundFilePath2 = PortablePath.Combine("C");
            sys.AddFilesAndFolders(x => x
                .Folder("A", a => a
                    .Folder("B", b => b)
                    .Folder("C", c => c)));

            List<string> files = (await FileSet.GetFoldersAsync(sys, @"**\*", basePath: @"\A")).ToList();

            Assert.That(files.Count, Is.EqualTo(2));
            Assert.That(files, Has.Member(foundFilePath1));
            Assert.That(files, Has.Member(foundFilePath2));
        }

        [Test]
        public async Task GetFoldersAsObservableStaticMethod()
        {
            var sys = new MemoryFileSystemFake();
            string foundFilePath1 = PortablePath.Combine("A");
            string foundFilePath2 = PortablePath.Combine("A", "B");
            string foundFilePath3 = PortablePath.Combine("A", "C");
            sys.AddFilesAndFolders(x => x
                .Folder("A", a => a
                    .Folder("B", b => b)
                    .Folder("C", c => c)));

            IObservable<string> filesObservable = await FileSet.GetFoldersAsObservableAsync(sys, @"**\*");
            List<string> files = new List<string>();

            filesObservable
                .Finally(() =>
                {
                    Assert.That(files.Count, Is.EqualTo(3));
                    Assert.That(files, Has.Member(foundFilePath1));
                    Assert.That(files, Has.Member(foundFilePath2));
                    Assert.That(files, Has.Member(foundFilePath3));
                })
                .Subscribe(filePath => files.Add(filePath));
        }
    }
}
