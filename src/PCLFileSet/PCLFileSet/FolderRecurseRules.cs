using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PCLStorage;

namespace PCLFileSet
{
    internal class FolderRecurseRules
    {
        private List<string> _globPaths = new List<string>();

        public void AddGlobPath(string pathToAdd)
        {
            this._globPaths.Add(pathToAdd);
        }

        public void AddGlobPaths(params string[] pathsToAdd)
        {
            this.AddGlobPaths((IEnumerable<string>) pathsToAdd);
        }

        public void AddGlobPaths(IEnumerable<string> pathsToAdd)
        {
            foreach (string pathToAdd in pathsToAdd)
                this._globPaths.Add(pathToAdd);
        }

        public List<FolderSegmentRule> GenerateRules(bool arePathsToFolders)
        {
            List<FolderSegmentRule> curRulesSequence = null;
            foreach (string globPath in this._globPaths)
            {
                List<FolderSegmentRule> nextRulesSequence = this.BuildRulesSequence(globPath, arePathsToFolders);
                curRulesSequence = this.MergeRulesSequences(curRulesSequence, nextRulesSequence);
            }

            return curRulesSequence;
        }

        private List<FolderSegmentRule> BuildRulesSequence(string globPath, bool isPathToFolder)
        {
            List<string> globPathSegments =
                globPath.Split(new[] {PortablePath.DirectorySeparatorChar},
                    StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

            // If this is a file path, remove the file part of the path.
            if (!isPathToFolder)
                globPathSegments.RemoveAt(globPathSegments.Count - 1);

            var rulesSequence = new List<FolderSegmentRule>();
            foreach (string pathSeg in globPathSegments)
            {

                if (pathSeg == "**")
                {
                    rulesSequence.Add(new FolderSegmentRule()
                    {
                        Type = EFolderSegmentType.MatchZeroOrMOreFoldersRecursive,
                    });

                    // This is a terminate match rule. It doesn't matter what segments follow
                    // as we do need to recurse all subfolders from this point on.
                    break;
                }

                if (pathSeg == "*")
                {
                    rulesSequence.Add(new FolderSegmentRule()
                    {
                        Type = EFolderSegmentType.MatchAnyFolder,
                    });
                }
                else
                {
                    rulesSequence.Add(new FolderSegmentRule()
                    {
                        Type = EFolderSegmentType.MatchAnyFolderPattern,
                        FolderNameRegexes = new HashSet<string>()
                        {
                            BuildRegexPatternFromPathSegment(pathSeg)
                        }
                    });
                }
            }

            return rulesSequence;
        }

        private static string BuildRegexPatternFromPathSegment(string pathSeg)
        {
            var segmentRegexStr = new StringBuilder();

            segmentRegexStr.Append("^");

            foreach (char curChar in pathSeg)
            {
                if (curChar == '?')
                {
                    segmentRegexStr.Append('.');
                }
                else if (curChar == '*')
                {
                    segmentRegexStr.Append(".+");
                }
                else
                {
                    segmentRegexStr.Append(Regex.Escape(curChar.ToString()));
                }
            }

            segmentRegexStr.Append("$");

            return segmentRegexStr.ToString();
        }

        private List<FolderSegmentRule> MergeRulesSequences(
            List<FolderSegmentRule> rulesSequence1, List<FolderSegmentRule> rulesSequence2)
        {
            if (rulesSequence1 == null)
                return rulesSequence2;

            if (rulesSequence2 == null)
                return rulesSequence1;

            var mergedRulesSequence = new List<FolderSegmentRule>();
            int mergeLength = rulesSequence1.Count < rulesSequence2.Count ? rulesSequence1.Count : rulesSequence2.Count;
            for (int i = 0; i < mergeLength; i++)
            {
                FolderSegmentRule sequence1Seg = rulesSequence1[i];
                FolderSegmentRule sequence2Seg = rulesSequence2[i];

                if (sequence1Seg.Type == EFolderSegmentType.MatchZeroOrMOreFoldersRecursive)
                {
                    mergedRulesSequence.Add(sequence1Seg);
                    return mergedRulesSequence;
                }

                if (sequence2Seg.Type == EFolderSegmentType.MatchZeroOrMOreFoldersRecursive)
                {
                    mergedRulesSequence.Add(sequence2Seg);
                    return mergedRulesSequence;
                }

                if (sequence1Seg.Type == EFolderSegmentType.MatchAnyFolder)
                {
                    mergedRulesSequence.Add(sequence1Seg);
                    continue;
                }

                if (sequence2Seg.Type == EFolderSegmentType.MatchAnyFolder)
                {
                    mergedRulesSequence.Add(sequence2Seg);
                    continue;
                }

                if (sequence1Seg.Type == sequence2Seg.Type 
                    && sequence1Seg.Type == EFolderSegmentType.MatchAnyFolderPattern)
                {
                    mergedRulesSequence.Add(new FolderSegmentRule()
                    {
                        Type = EFolderSegmentType.MatchAnyFolderPattern,
                        FolderNameRegexes = new HashSet<string>(sequence1Seg.FolderNameRegexes
                            .Concat(sequence2Seg.FolderNameRegexes))
                    });
                }
                else
                {
                    throw new ArgumentException("Unexpected segment type!");
                }
            }

            if (rulesSequence1.Count > mergeLength)
                mergedRulesSequence.AddRange(rulesSequence1.Skip(mergeLength));
            else if (rulesSequence2.Count > mergeLength)
                mergedRulesSequence.AddRange(rulesSequence2.Skip(mergeLength));

            return mergedRulesSequence;
        }
    }

    public class FolderSegmentRule
    {
        public EFolderSegmentType Type { get; set; }
        public HashSet<string> FolderNameRegexes { get; set; }
    }

    public enum EFolderSegmentType
    {
        MatchAnyFolderPattern,
        MatchAnyFolder,
        MatchZeroOrMOreFoldersRecursive
    }
}
