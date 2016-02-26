using System.Collections.Generic;
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
    }
}
