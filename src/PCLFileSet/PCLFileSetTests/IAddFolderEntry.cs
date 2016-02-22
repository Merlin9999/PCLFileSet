using System;

namespace PCLFileSetTests
{
    public interface IAddFolderEntry
    {
        IAddFolderEntry File(string fileName);
        IAddFolderEntry Folder(string folderName, Func<IAddFolderEntry, IAddFolderEntry> folderEntryAdderFunc);
    }
}