using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;

namespace PCLFileSet.ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            FileSet fileSet = CreateFileSet();
            IEnumerable<string> matchingFolderItems = fileSet.GetFiles();

            foreach (string fileName in matchingFolderItems)
                Console.WriteLine(fileName);
        }

        private static FileSet CreateFileSet()
        {
            string baseFolder = @".\..\..";
            var fileSet = new FileSet(new DesktopFileSystem(), baseFolder);

            fileSet
                .Include(@"bin\Debug\*.exe")
                .Include(@"bin\Debug\*.dll")
                .Include(@"bin\Debug\*.config")

                .Exclude(@"**\*.vshost.*")

                // Optional: Filter out various exceptions to allow successful completion 
                //           even if access is denied to some files or sub-folders.
                .Catch<SecurityException>(ex => { })
                .Catch<UnauthorizedAccessException>(ex => { })
                .Catch<DirectoryNotFoundException>(ex => { })
                .Catch<IOException>(ex => { });

            return fileSet;
        }
    }
}
