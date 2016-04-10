using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        private Dictionary<Type,List<FileSet.ExceptionHandlerInfo>> ExceptionHandlers { get; }

        public readonly string PreferredPathSeparator;
        public readonly string[] PathSeparators;
        public readonly IFileSystem FileSystem;

        public readonly string BasePath;
        private IFolder _baseFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSet"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="basePath">The base folder path.</param>
        /// <param name="isCaseSensitive">if set to <c>true</c> [is case sensitive].</param>
        public FileSet(IFileSystem fileSystem, string basePath = null, bool isCaseSensitive = false) 
            : this(fileSystem, isCaseSensitive)
        {
            this.BasePath = this.GetPathWithPreferredSeparator(basePath ?? ".");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSet"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system implementation.</param>
        /// <param name="baseFolder">The base folder.</param>
        /// <param name="isCaseSensitive">if set to <c>true</c> [is case sensitive].</param>
        public FileSet(IFileSystem fileSystem, IFolder baseFolder, bool isCaseSensitive = false) 
            : this(fileSystem, isCaseSensitive)
        {
            this.BaseFolder = baseFolder;
        }

        private FileSet(IFileSystem fileSystem, bool isCaseSensitive)
        {
            this._isCaseSensitive = isCaseSensitive;
            this.FileSystem = fileSystem;
            this.IncludePaths = new List<string>();
            this.ExcludePaths = new List<string>();
            this.ExceptionHandlers = new Dictionary<Type, List<FileSet.ExceptionHandlerInfo>>();

            this.PreferredPathSeparator = PortablePath.DirectorySeparatorChar.ToString();

            var separatorHashSet = new HashSet<string>(FolderSeparatorsStatic);
            separatorHashSet.Add(this.PreferredPathSeparator);
            this.PathSeparators = separatorHashSet.ToArray();

            separatorHashSet.Remove(this.PreferredPathSeparator);
            this._alternatePathSeparators = separatorHashSet.ToArray();
        }

        public IFolder BaseFolder
        {
            get
            {
                if (this._baseFolder == null)
                {
                    Task<IFolder> folderTask = this.FileSystem.GetFolderFromPathAsync(this.BasePath);
                    folderTask.WaitForTaskAndTranslateAggregateExceptions();
                    this._baseFolder = folderTask.Result;
                }

                if (this._baseFolder == null)
                    throw new InvalidOperationException($"Base path \"{this.BasePath}\" is not valid.");

                return this._baseFolder;
            }
            set { this._baseFolder = value; }
        }

        /// <summary>
        /// Specify a glob path to be included. Ex: "**\*.txt" and "root/*/a?.doc"
        /// </summary>
        /// <param name="globPath">The glob path.</param>
        /// <returns></returns>
        public IFileSet Include(string globPath)
        {
            this.IncludePaths.Add(this.GetPathWithPreferredSeparator(globPath));

            return this;
        }

        /// <summary>
        /// Specify a glob path to be excluded. Ex: "**\*.txt" and "root/*/a?.doc"
        /// </summary>
        /// <param name="globPath">The glob path.</param>
        /// <returns></returns>
        public IFileSet Exclude(string globPath)
        {
            this.ExcludePaths.Add(this.GetPathWithPreferredSeparator(globPath));

            return this;
        }

        /// <summary>
        /// Catches the specified exception type. Note: The exception handler delegate is executed on
        /// <see cref="SynchronizationContext.Current" /> value that was when Catch() was called.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be caught and handled.</typeparam>
        /// <param name="exceptionHandler">The exception handler. Commonly and empty block. Ex: ex =&gt; { }</param>
        /// <returns></returns>
        public IFileSet Catch<TException>(Action<TException> exceptionHandler)
            where TException : Exception
        {
            return this.Catch(exceptionHandler, SynchronizationContext.Current);
        }

        /// <summary>
        /// Catches the specified exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be caught and handled.</typeparam>
        /// <param name="exceptionHandler">The exception handler. Commonly and empty block. Ex: ex =&gt; { }</param>
        /// <param name="synchronizationContext">The exception handler delegate is executed on this
        /// <see cref="SynchronizationContext" /></param>
        /// <returns></returns>
        /// .
        public IFileSet Catch<TException>(Action<TException> exceptionHandler, SynchronizationContext synchronizationContext)
            where TException : Exception
        {
            List<FileSet.ExceptionHandlerInfo> handlerList;
            if (!this.ExceptionHandlers.TryGetValue(typeof (TException), out handlerList))
            {
                handlerList = new List<FileSet.ExceptionHandlerInfo>();
                this.ExceptionHandlers.Add(typeof(TException), handlerList);
            }
            handlerList.Add(new FileSet.ExceptionHandlerInfo()
            {
                ExceptionHandler = (Exception exc) => exceptionHandler((TException) exc),
                SynchronizationContext = synchronizationContext,
            });
            return this;
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFiles()
        {
            Task<IEnumerable<string>> filesTask = this.GetFilesAsync();
            filesTask.WaitForTaskAndTranslateAggregateExceptions();
            return filesTask.Result;
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFolders()
        {
            Task<IEnumerable<string>> foldersTask = this.GetFoldersAsync();
            foldersTask.WaitForTaskAndTranslateAggregateExceptions();
            return foldersTask.Result;
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <returns></returns>
        public IObservable<string> GetFilesAsObservable()
        {
            Task<IObservable<string>> filesTask = this.GetFilesAsObservableAsync();
            filesTask.WaitForTaskAndTranslateAggregateExceptions();
            return filesTask.Result;
        }

        /// <summary>
        /// Gets the specified folders.
        /// </summary>
        /// <returns></returns>
        public IObservable<string> GetFoldersAsObservable()
        {
            Task<IObservable<string>> foldersTask = this.GetFoldersAsObservableAsync();
            foldersTask.WaitForTaskAndTranslateAggregateExceptions();
            return foldersTask.Result;
        }

        /// <summary>
        /// Gets the specified files.
        /// </summary>
        /// <returns></returns>
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
            var folderRecurseRules = new FolderRecurseRules();
            folderRecurseRules.AddGlobPaths(this.IncludePaths);
            List<FolderSegmentRule> rules = folderRecurseRules.GenerateRules(false);

            return Observable.Create(async (IObserver<string> observer) =>
            {
                try
                { 
                    await this.PostSubfolderFilesToObserverAsync(observer, this.BaseFolder, this.BaseFolder, rules, 0);
                }
                catch (Exception exc)
                {
                    var handled = this.CallMatchingExceptionHandlers(exc);

                    if (!handled)
                        throw;
                }

                observer.OnCompleted();
            });
        }

        private async Task PostSubfolderFilesToObserverAsync(IObserver<string> observer, IFolder folder,
            IFolder baseFolder, List<FolderSegmentRule> rules, int folderSegmentLevel)
        {
            string folderRelativePath = this.BuildRelativePathFromBaseFolder(baseFolder, folder);

            foreach (IFile file in await folder.GetFilesAsync())
            {
                observer.OnNext(folderRelativePath.Length == 0
                    ? file.Name
                    : string.Join(this.PreferredPathSeparator, folderRelativePath, file.Name));
            }

            List<Regex> folderPatternMatchRegexes = null;
            foreach (IFolder subfolder in await folder.GetFoldersAsync())
            {
                try
                {
                    if (this.ShouldRecurseIntoFolder(subfolder, rules, folderSegmentLevel, ref folderPatternMatchRegexes))
                    {
                        await this.PostSubfolderFilesToObserverAsync(observer, subfolder, baseFolder, rules,
                            folderSegmentLevel + 1);
                    }
                }
                catch (Exception exc)
                {
                    var handled = this.CallMatchingExceptionHandlers(exc);

                    if (!handled)
                        throw;
                }
            }
        }

        private async Task<IObservable<string>> GetAllFoldersAsObservableAsync()
        {
            var folderRecurseRules = new FolderRecurseRules();
            folderRecurseRules.AddGlobPaths(this.IncludePaths);
            List<FolderSegmentRule> rules = folderRecurseRules.GenerateRules(false);

            return Observable.Create(async (IObserver<string> observer) =>
            {
                try
                {
                    await this.PostSubfoldersToObserverAsync(observer, this.BaseFolder, this.BaseFolder, rules, 0);
                }
                catch (Exception exc)
                {
                    var handled = this.CallMatchingExceptionHandlers(exc);

                    if (!handled)
                        throw;
                }

                observer.OnCompleted();
            });
        }

        private async Task PostSubfoldersToObserverAsync(IObserver<string> observer, IFolder folder, IFolder baseFolder, 
            List<FolderSegmentRule> rules, int folderSegmentLevel)
        {
            List<Regex> folderPatternMatchRegexes = null;
            foreach (IFolder subfolder in await folder.GetFoldersAsync())
            {
                observer.OnNext(this.BuildRelativePathFromBaseFolder(baseFolder, subfolder));

                try
                {
                    if (this.ShouldRecurseIntoFolder(subfolder, rules, folderSegmentLevel, ref folderPatternMatchRegexes))
                    {
                        await this.PostSubfoldersToObserverAsync(observer, subfolder, baseFolder, 
                            rules, folderSegmentLevel + 1);
                    }
                }
                catch (Exception exc)
                {
                    var handled = this.CallMatchingExceptionHandlers(exc);

                    if (!handled)
                        throw;
                }
            }
        }

        private bool ShouldRecurseIntoFolder(IFolder folder, List<FolderSegmentRule> rules, int folderSegmentLevel, ref List<Regex> folderPatternMatchRegexes)
        {
            if (folderSegmentLevel < rules.Count)
            {
                switch (rules[folderSegmentLevel].Type)
                {
                    case EFolderSegmentType.MatchAnyFolder:
                    case EFolderSegmentType.MatchZeroOrMOreFoldersRecursive:
                        return true;

                    case EFolderSegmentType.MatchAnyFolderPattern:
                        if (folderPatternMatchRegexes == null)
                        {
                            folderPatternMatchRegexes = rules[folderSegmentLevel].FolderNameRegexes
                                .Select(s =>
                                    new Regex(s,
                                        this._isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase))
                                .ToList();
                        }

                        if (folderPatternMatchRegexes.Any(r => r.IsMatch(folder.Name)))
                            return true;
                        break;
                }
            }
            else if (rules.Any() && rules.Last().Type == EFolderSegmentType.MatchZeroOrMOreFoldersRecursive)
                return true;

            return false;
        }

        private bool CallMatchingExceptionHandlers(Exception exc)
        {
            bool handled = false;
            foreach (var exceptionHandlerKVP in this.ExceptionHandlers)
            {
                if (exceptionHandlerKVP.Key.GetTypeInfo().IsAssignableFrom(exc.GetType().GetTypeInfo()))
                {
                    foreach (FileSet.ExceptionHandlerInfo handlerInfo in exceptionHandlerKVP.Value)
                    {
                        if (handlerInfo.SynchronizationContext != null)
                            handlerInfo.SynchronizationContext.Post((ex) => handlerInfo.ExceptionHandler((Exception)ex), exc);
                        else
                            handlerInfo.ExceptionHandler(exc);
                        handled = true;
                    }
                }
            }
            return handled;
        }

        private string BuildRelativePathFromBaseFolder(IFolder baseFolder, IFolder folder)
        {
            string[] baseFolderPathSegments = this.SplitFolderPathIntoSegments(baseFolder);
            string[] pathSegments = this.SplitFolderPathIntoSegments(folder);

            StringComparer segmentNameComparer = this.GetNameComparer();
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

        private StringComparer GetNameComparer()
        {
            return this._isCaseSensitive
                ? StringComparer.CurrentCulture
                : StringComparer.CurrentCultureIgnoreCase;
        }

        private StringComparison GetNameComparison()
        {
            return this._isCaseSensitive
                ? StringComparison.CurrentCulture
                : StringComparison.CurrentCultureIgnoreCase;
        }

        internal string[] SplitFolderPathIntoSegments(IFolder folder)
        {
            if (folder.Path == null)
                return new string[0];

            return this.TrimSeparatorsFromEnd(folder.Path)
                .Split(new[] {this.PreferredPathSeparator}, StringSplitOptions.None);
        }

        private string TrimSeparatorsFromEnd(string path)
        {
            bool found;
            StringComparer nameComparer = this.GetNameComparer();

            do
            {
                found = false;
                foreach (string pathSeparator in this.PathSeparators)
                {
                    if (path.EndsWith(pathSeparator, this.GetNameComparison()))
                    {
                        path = path.Substring(0, path.Length - pathSeparator.Length);
                        found = true;
                    }
                }
            } while (found);

            return path;
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

        private class ExceptionHandlerInfo
        {
            public Action<Exception> ExceptionHandler { get; set; }
            public SynchronizationContext SynchronizationContext { get; set; }
        }
    }
}