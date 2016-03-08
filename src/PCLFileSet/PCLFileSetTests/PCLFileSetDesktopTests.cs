using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PCLFileSet;
using PCLStorage;

namespace PCLFileSetTests
{
    [TestFixture]
    public class PCLFileSetDesktopTests
    {
        private string TestFilesRoot { get; set; }

        [SetUp]
        public void TestSetup()
        {
            this.TestFilesRoot = PortablePath.Combine(TestHelper.GetTestExecutionPath(), "TestFiles");
        }

        [Test]
        public async Task GetFilesInRoot()
        {
            FileSet fs = new FileSet(new DesktopFileSystem(), basePath: this.TestFilesRoot);
            fs.Include("*");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Member("FileInRoot.txt"));
        }

        [Test]
        public async Task GetAllFiles()
        {
            FileSet fs = new FileSet(new DesktopFileSystem(), basePath: this.TestFilesRoot);
            fs.Include(@"**\*");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(6));
            Assert.That(files, Has.Member(@"FileInRoot.txt"));
            Assert.That(files, Has.Member(@"Folder1\FileInFolder1.txt"));
            Assert.That(files, Has.Member(@"Folder1\SubFolder1\FileInSubFolder1.txt"));
            Assert.That(files, Has.Member(@"Folder2\FileInFolder2.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder2\FileInSubFolder2.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder3\FileInSubFolder3.txt"));
        }

        [Test]
        public async Task GetAllFilesMatchingOnFolderPattern()
        {
            FileSet fs = new FileSet(new DesktopFileSystem(), basePath: this.TestFilesRoot);
            fs.Include(@"**\SubFolder?\*");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(files, Has.Member(@"Folder1\SubFolder1\FileInSubFolder1.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder2\FileInSubFolder2.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder3\FileInSubFolder3.txt"));
        }

        [Test]
        public async Task GetAllFilesWithFolderEndingInSeparatorCharacter()
        {
            FileSet fs = new FileSet(new DesktopFileSystem(), basePath: this.TestFilesRoot + PortablePath.DirectorySeparatorChar);
            fs.Include(@"**\*");
            List<string> files = (await fs.GetFilesAsync()).ToList();

            Assert.That(files.Count, Is.EqualTo(6));
            Assert.That(files, Has.Member(@"FileInRoot.txt"));
            Assert.That(files, Has.Member(@"Folder1\FileInFolder1.txt"));
            Assert.That(files, Has.Member(@"Folder1\SubFolder1\FileInSubFolder1.txt"));
            Assert.That(files, Has.Member(@"Folder2\FileInFolder2.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder2\FileInSubFolder2.txt"));
            Assert.That(files, Has.Member(@"Folder2\SubFolder3\FileInSubFolder3.txt"));
        }

        [Test]
        public async Task AccessDeniedUnhandledIteratingFiles()
        {
            string accessDeniedTest = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Assert.That(accessDeniedTest, Is.Not.Empty);

            FileSet fs = new FileSet(new DesktopFileSystem(), accessDeniedTest);
            fs.Include("**/*");

            Assert.That(async () => (await fs.GetFilesAsync()).ToList(), Throws.TypeOf<UnauthorizedAccessException>()); 
        }

        [Test]
        public async Task AccessDeniedHandledIteratingFiles()
        {
            string accessDeniedTest = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Assert.That(accessDeniedTest, Is.Not.Empty);

            FileSet fs = new FileSet(new DesktopFileSystem(), accessDeniedTest);
            fs.Include("**/*");

            fs.Catch<UnauthorizedAccessException>(ex =>
            {
                // Do nothing. Ignore exception.
            });

            (await fs.GetFilesAsync()).ToList();
        }

        [Test]
        public async Task AccessDeniedUnhandledIteratingFolders()
        {
            string accessDeniedTest = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Assert.That(accessDeniedTest, Is.Not.Empty);

            FileSet fs = new FileSet(new DesktopFileSystem(), accessDeniedTest);
            fs.Include("**/*");

            Assert.That(async () => (await fs.GetFoldersAsync()).ToList(), Throws.TypeOf<UnauthorizedAccessException>());
        }

        [Test]
        public async Task AccessDeniedHandledIteratingFolders()
        {
            string accessDeniedTest = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Assert.That(accessDeniedTest, Is.Not.Empty);

            FileSet fs = new FileSet(new DesktopFileSystem(), accessDeniedTest);
            fs.Include("**/*");

            fs.Catch<UnauthorizedAccessException>(ex =>
            {
                // Do nothing. Ignore exception.
            });

            (await fs.GetFoldersAsync()).ToList();
        }
    }
}
