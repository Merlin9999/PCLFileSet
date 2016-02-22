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

        private bool _isCaseSensitive;
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

        public IEnumerable<string> GetFiles()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            return this.EnumerateFiles()
                .Where(filePath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(filePath)))
                .Where(filePath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(filePath)));
        }

        public IEnumerable<string> GetFolders()
        {
            var includeRegexList = this.IncludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();
            var excludeRegexList = this.ExcludePaths.SelectMany(p => this.BuildPathMatchRegexes(p)).ToList();

            return this.EnumerateFolders()
                .Where(filePath => includeRegexList.Any(includeRegex => includeRegex.IsMatch(filePath)))
                .Where(filePath => !excludeRegexList.Any(excludeRegex => excludeRegex.IsMatch(filePath)));
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
                            StringBuilderListExtensions.Append(pathRegexStrs, (string) Regex.Escape(subSeg.ToString()));
                            subSeg.Clear();
                            pathRegexStrs.Append(noSeparatorRegexChar);
                        }
                        else if (curChar == '*')
                        {
                            StringBuilderListExtensions.Append(pathRegexStrs, (string) Regex.Escape(subSeg.ToString()));
                            subSeg.Clear();
                            pathRegexStrs.Append(noSeparatorRegexChar).Append('*');
                        }
                        else
                        {
                            subSeg.Append(curChar);
                        }
                    }

                    StringBuilderListExtensions.Append(pathRegexStrs, (string) Regex.Escape(subSeg.ToString()));
                }
            }

            pathRegexStrs.Append("$");

            return pathRegexStrs.Select(x => new Regex(x.ToString(),
                this._isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
        }

        private IEnumerable<string> EnumerateFiles()
        {
            Task<IObservable<IFile>> task = this.GetFilesAsObservable();
            task.Wait();
            return task.Result.ToEnumerable().Select(x => x.Path);
        }

        private IEnumerable<string> EnumerateFolders()
        {
            Task<IObservable<IFolder>> task = this.GetFoldersAsObservable();
            task.Wait();
            return task.Result.ToEnumerable().Select(x => x.Path);
        }

        private async Task<IObservable<IFile>> GetFilesAsObservable()
        {
            IFolder folder = await this.FileSystem.GetFolderFromPathAsync(this.BasePath);
            IList<IFile> files = await folder.GetFilesAsync();
            
            return Observable.Create((IObserver<IFile> observer) =>
            {
                foreach (IFile file in files)
                    observer.OnNext(file);
                observer.OnCompleted();
                return Disposable.Create(() => { });
            });
        }

        private async Task<IObservable<IFolder>> GetFoldersAsObservable()
        {
            IFolder folder = await this.FileSystem.GetFolderFromPathAsync(this.BasePath);
            IList<IFolder> folders = await folder.GetFoldersAsync();

            return Observable.Create((IObserver<IFolder> observer) =>
            {
                foreach (IFolder subFolder in folders)
                    observer.OnNext(subFolder);
                observer.OnCompleted();
                return Disposable.Create(() => { });
            });
        }


        private string GetPathWithPreferredSeparator(string path)
        {
            foreach (string sep in this._alternatePathSeparators)
                path = path.Replace(sep, this.PreferredPathSeparator);

            return path;
        }
    }
}