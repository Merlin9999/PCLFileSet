# PCLFileSet

PCLFileSet is a c# library inspired by [nAnt's](http://nant.sourceforge.net/) [&lt;fileset&gt;](http://nant.sourceforge.net/release/latest/help/types/fileset.html). A file set allows you to create a sequence of files (or folders) by specifying one or more file specifications that may utilize wildcards. Multiple file specifications for inclusion and exclusion can be specified. Exclusion file specifications filter files (or folders) that would otherwise be included.

## File Specifications
File specifications can use wildcards such as `*`, `?`, and `**`. 
* `*` matches 1 or more of any character
* `?` matches any one character
* `**` when used as a folder name, matches zero or more sub-folders

Supported folder path separator characters include both `/` and `\`.

Examples:
* `"**\*.txt"` finds all txt files in the specified base folder and any sub-folder.
* `"a?.doc"` finds all 2 character doc files in the base folder where the first character is 'a'.
* `"root/*/*"` finds all files in any immediate sub-folder of the 'root' folder, but finds no files in the base folder nor in the 'root' folder.

## Code Example

```c#
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
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
            var fileSet = new FileSet(baseFolder);

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
```

## Async and Observable Support

The `GetFiles()` method in the code example above returns an [`IEnumerable<string>`](https://msdn.microsoft.com/en-us/library/9eekhta0(v=vs.110).aspx). There are `async` versions of the `GetFiles...()` methods and versions of this method that return [`IObservable<string>`](https://msdn.microsoft.com/en-us/library/dd990377(v=vs.110).aspx). Additionally, there is a full set of `GetFolders...()` methods. Truth be known, the non-async and the non-observable versions of these methods are wrappers around the observable async methods.

## Note
From what I've seen, PCLFileSet's main use cases are in desktop applications and utilities such as my own [FileFind](https://github.com/Merlin9999/FileFind) utility. However, I wanted to explore creating a Portable Class Library (PCL) and I was very interested in the [PCLStorage](https://github.com/dsplaisted/pclstorage) project on GitHub. 

Additionally, using PCLStorage provided a very nice file system abstraction (see:  [IFileSystem](https://github.com/dsplaisted/PCLStorage/blob/master/src/PCLStorage.Abstractions/IFileSystem.cs)). With it, I could create an in-memory file system implementation for use in unit tests. This allowed for better testing without being overcome by having to create actual folder and file structures for each test (Baaa!). This made for a considerably more sane developer experience. :-)
 
