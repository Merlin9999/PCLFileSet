using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PCLStorage;

namespace PCLFileSet
{
    public class FileSet : IFileSet
    {
        private static string[] FolderSeparatorsStatic => new[] { @"\", @"/" };

        private readonly bool _isCaseSensitive;
        private readonly string[] _alternatePathSeparators;
        private List<string> IncludePaths { get; }
        private List<string> ExcludePaths { get; }

        public readonly string PreferredPathSeparator;
        public readonly string[] PathSeparators;
        public readonly IFileSystem FileSystem;
        public readonly string BasePath;

        public FileSet(IFileSystem fileSystem, string basePath = null, bool isCaseSensitive = false)
        {
            this._isCaseSensitive = isCaseSensitive;
            this.FileSystem = fileSystem;
            this.BasePath = basePath ?? string.Empty;
            this.IncludePaths = new List<string>();
            this.ExcludePaths = new List<string>();

            this.PreferredPathSeparator = PortablePath.DirectorySeparatorChar.ToString();

            var separatorHashSet = new HashSet<string>(FolderSeparatorsStatic);
            separatorHashSet.Add(this.PreferredPathSeparator);
            this.PathSeparators = separatorHashSet.ToArray();

            separatorHashSet.Remove(this.PreferredPathSeparator);
            this._alternatePathSeparators = separatorHashSet.ToArray();
        }

        public IFileSet Include(string globPath)
        {
            this.IncludePaths.Add(this.GetPathWithPreferredSeparator(globPath));

            return this;
        }

        public IFileSet Exclude(string globPath)
        {
            this.ExcludePaths.Add(this.GetPathWithPreferredSeparator(globPath));

            return this;
        }

        public async Task<IEnumerable<string>> GetFilesAsync()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            //var fileList = (await this.EnumerateFilesAsync()).ToList();
            //return fileList
            return (await this.EnumerateFilesAsync())
                .Where(filePath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(filePath)))
                .Where(filePath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(filePath)));
        }

        public async Task<IEnumerable<string>> GetFoldersAsync()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            //var folderList = (await this.EnumerateFoldersAsync()).ToList();
            //return folderList
            return (await this.EnumerateFoldersAsync())
                .Where(folderPath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(folderPath)))
                .Where(folderPath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(folderPath)));
        }

        public async Task<IObservable<string>> GetFilesAsObservableAsync()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            return (await this.GetAllFilesAsObservableAsync())
                .Where(filePath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(filePath)))
                .Where(filePath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(filePath)));
        }

        public async Task<IObservable<string>> GetFoldersAsObservableAsync()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            return (await this.GetAllFoldersAsObservableAsync())
                .Where(folderPath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(folderPath)))
                .Where(folderPath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(folderPath)));
        }

        private async Task<IEnumerable<string>> EnumerateFilesAsync()
        {
            return (await this.GetAllFilesAsObservableAsync()).ToEnumerable();
        }

        private async Task<IEnumerable<string>> EnumerateFoldersAsync()
        {
            return (await this.GetAllFoldersAsObservableAsync()).ToEnumerable();
        }

        private async Task<IObservable<string>> GetAllFilesAsObservableAsync()
        {
            IFolder baseFolder = await this.FileSystem.GetFolderFromPathAsync(this.BasePath);
            
            return Observable.Create(async (IObserver<string> observer) =>
            {
                await this.PostSubfolderFilesToObserverAsync(observer, baseFolder, baseFolder);
                observer.OnCompleted();
            });
        }

        private async Task PostSubfolderFilesToObserverAsync(IObserver<string> observer, IFolder folder, IFolder baseFolder)
        {
            string folderRelativePath = this.BuildRelativePathFromBaseFolder(baseFolder, folder);

            foreach (IFile file in await folder.GetFilesAsync())
                observer.OnNext(folderRelativePath.Length == 0 ? file.Name : string.Join(this.PreferredPathSeparator, folderRelativePath, file.Name));

            foreach (IFolder subfolder in await folder.GetFoldersAsync())
                await this.PostSubfolderFilesToObserverAsync(observer, subfolder, baseFolder);
        }

        private async Task<IObservable<string>> GetAllFoldersAsObservableAsync()
        {
            IFolder baseFolder = await this.FileSystem.GetFolderFromPathAsync(this.BasePath);

            return Observable.Create(async (IObserver<string> observer) =>
            {
                await this.PostSubfoldersToObserverAsync(observer, baseFolder, baseFolder);
                observer.OnCompleted();
            });
        }

        private async Task PostSubfoldersToObserverAsync(IObserver<string> observer, IFolder folder, IFolder baseFolder)
        {
            foreach (IFolder subfolder in await folder.GetFoldersAsync())
            {
                observer.OnNext(this.BuildRelativePathFromBaseFolder(baseFolder, subfolder));
                await this.PostSubfoldersToObserverAsync(observer, subfolder, baseFolder);
            }
        }

        private string BuildRelativePathFromBaseFolder(IFolder baseFolder, IFolder folder)
        {
            string[] baseFolderPathSegments = this.SplitFolderPathIntoSegments(baseFolder);
            string[] pathSegments = this.SplitFolderPathIntoSegments(folder);

            StringComparer segmentNameComparer = this._isCaseSensitive
                ? StringComparer.CurrentCulture
                : StringComparer.CurrentCultureIgnoreCase;
#if DEBUG
            for (int i = 0; i < baseFolderPathSegments.Length; i++)
            {
                if (segmentNameComparer.Compare(baseFolderPathSegments[i], pathSegments[i]) != 0)
                    throw new ArgumentException("Passed base folder is not a base folder of the referenced folder.");
            }
#endif
            if (pathSegments.Length <= baseFolderPathSegments.Length)
                return string.Empty;

            return string.Join(this.PreferredPathSeparator, pathSegments.Skip(baseFolderPathSegments.Length));
        }

        private string[] SplitFolderPathIntoSegments(IFolder folder)
        {
            if (folder.Path == null)
                return new string[0];
            return folder.Path.Split(new[] { this.PreferredPathSeparator }, StringSplitOptions.None);
        }

        private string GetPathWithPreferredSeparator(string path)
        {
            foreach (string sep in this._alternatePathSeparators)
                path = path.Replace(sep, this.PreferredPathSeparator);

            return path;
        }

        private IEnumerable<Regex> BuildPathMatchRegexes(string matchGlobPath)
        {
            string separatorRegexStr = Regex.Escape(this.PreferredPathSeparator);
            string noSeparatorRegexChar = string.Concat("((?!", separatorRegexStr, ").)");

            var pathRoot = matchGlobPath.StartsWith(this.PreferredPathSeparator)
                ? this.PreferredPathSeparator
                : null;
            string pathRootRegexStr = pathRoot == null ? null : Regex.Escape(pathRoot);
            string[] globPathSegments = 
                matchGlobPath.Split(new [] {this.PreferredPathSeparator}, StringSplitOptions.RemoveEmptyEntries);

            var pathRegexStrs = new List<StringBuilder>() { new StringBuilder() };
            var subSeg = new StringBuilder();
            
            pathRegexStrs.Append("^");
            pathRegexStrs.Append(Regex.Escape(pathRootRegexStr ?? string.Empty));

            bool skipNextSegmentSeparator = true;

            foreach (string pathSeg in globPathSegments)
            {
                if (!skipNextSegmentSeparator)
                    pathRegexStrs.Append(separatorRegexStr);
                else
                    skipNextSegmentSeparator = false;

                if (pathSeg == "**")
                {
                    var dupRegexStrs = pathRegexStrs.Select(x => new StringBuilder(x.ToString())).ToList();

                    pathRegexStrs.Append(@".*");
                    pathRegexStrs.Append(separatorRegexStr);
                    pathRegexStrs.AddRange(dupRegexStrs);
                    skipNextSegmentSeparator = true;
                }
                else if (pathSeg == "*")
                {
                    pathRegexStrs.Append(noSeparatorRegexChar).Append('+');
                }
                else
                {
                    subSeg.Clear();

                    foreach (char curChar in pathSeg)
                    {
                        if (curChar == '?')
                        {
                            pathRegexStrs.Append((string) Regex.Escape(subSeg.ToString()));
                            subSeg.Clear();
                            pathRegexStrs.Append(noSeparatorRegexChar);
                        }
                        else if (curChar == '*')
                        {
                            pathRegexStrs.Append((string) Regex.Escape(subSeg.ToString()));
                            subSeg.Clear();
                            pathRegexStrs.Append(noSeparatorRegexChar).Append('*');
                        }
                        else
                        {
                            subSeg.Append(curChar);
                        }
                    }

                    pathRegexStrs.Append((string) Regex.Escape(subSeg.ToString()));
                }
            }

            pathRegexStrs.Append("$");

            return pathRegexStrs.Select(x => new Regex(x.ToString(),
                this._isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
        }
    }
}