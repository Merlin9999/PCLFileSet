using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using PCLFileSet;
using PCLStorage;

namespace PCLFileSetTests
{
    [TestFixture]
    public class FolderRecurseRulesTests
    {
        [Test]
        public void NoFolders()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(0));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NamedFolder()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NamedFolderWithPattern()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"?X*Z\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^.X.+Z$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^.X.+Z$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NestedNamedFolder()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\?X*Z\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.X.+Z$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.X.+Z$"));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NestedStarFolder()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\*\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NestedQuestionMarkFolder()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\?\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.$"));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void NestedNamedFolderWithNestedDoubleStar()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\**\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member(@"^Folder$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void NestedNamedFolderWithInitialDoubleStar()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"**\Folder\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeFolderPatterns()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\Subfolder1\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\Subfolder2\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder1$"));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder2$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder1$"));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder2$"));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void MergeStarFolderWithStarFolderAddedFirst()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\*\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\Subfolder1\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));


            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        public void MergeStarFolderWithStarFolderAddedSecond()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\Subfolder1\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\*\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));
            
            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolder));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void MergeDoubleStarFolderWithDoubleStarFolderAddedFirst()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\**\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\Subfolder1\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
            
            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeDoubleStarFolderWithDoubleStarFolderAddedSecond()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\Subfolder1\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\**\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeWithFirstPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\Subfolder\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder$"));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void MergeWithSecondPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\Subfolder\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder$"));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(3));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member("^Subfolder$"));
            Assert.That(folderPathSegmentRules[1].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
            Assert.That(folderPathSegmentRules[2].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[2].FolderNameRegexes, Has.Member(@"^.+\.txt$"));
        }

        [Test]
        public void MergeDoubleStarWithBeyondCommonLengthFistPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\**\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeDoubleStarBeyondCommonLengthWithSecondPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder1\*.txt");
            rulesFactory.AddGlobPath(@"Folder2\**\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(filePathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(filePathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(2));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchAnyFolderPattern));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder1$"));
            Assert.That(folderPathSegmentRules[0].FolderNameRegexes, Has.Member("^Folder2$"));
            Assert.That(folderPathSegmentRules[1].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeDoubleStarWithWithinCommonLengthFistPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"**\SubFolder\*.txt");
            rulesFactory.AddGlobPath(@"Folder\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

        [Test]
        public void MergeDoubleStarWithinCommonLengthWithSecondPathLonger()
        {
            var rulesFactory = new FolderRecurseRules();
            rulesFactory.AddGlobPath(@"Folder\*.txt");
            rulesFactory.AddGlobPath(@"**\SubFolder\*.txt");

            List<FolderSegmentRule> filePathSegmentRules = rulesFactory.GenerateRules(false);
            Assert.That(filePathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(filePathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));

            List<FolderSegmentRule> folderPathSegmentRules = rulesFactory.GenerateRules(true);
            Assert.That(folderPathSegmentRules.Count, Is.EqualTo(1));
            Assert.That(folderPathSegmentRules[0].Type, Is.EqualTo(EFolderSegmentType.MatchZeroOrMOreFoldersRecursive));
        }

    }
}
