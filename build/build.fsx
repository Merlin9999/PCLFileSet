#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake 
open Fake.Testing.NUnit3
open System.IO

let configuration = "Release"
let srcPath = "./../src"
let solutions = !! (srcPath @@ "**/*.sln")
let solutionPaths = solutions |> Seq.map (fun solutionFile -> Path.GetDirectoryName solutionFile)
let nugetOutPath = "./../nuget"
let nugetWorkPath = nugetOutPath + "/temp"
let sharedAssemblyInfoFile = srcPath @@ "PCLFileSet/Shared/SharedAssemblyInfo.cs"
let nugetPackagesFolder = srcPath @@ "PCLFileSet/packages"

exception UnknownLibraryVersion of string

Target "Clean" (fun _ ->
    solutionPaths |> Seq.iter (fun solutionPath -> CleanDirs !! (solutionPath + "/**/bin/" + configuration))
    solutionPaths |> Seq.iter (fun solutionPath -> CleanDirs !! (solutionPath + "/**/obj/" + configuration))
   
    CleanDirs [nugetOutPath]
)

Target "RestorePackages" (fun _ -> 
    CreateDir nugetPackagesFolder

    for solutionFile in solutions do 
        (RestoreMSSolutionPackages (fun p ->
            { p with
                Sources = "https://nuget.org/api/v2" :: p.Sources
                OutputPath = nugetPackagesFolder
                Retries = 4 })  
            solutionFile) |> ignore
)

Target "Build" (fun _ ->
    let msBuildParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    //"TreatWarningsAsErrors", "True"
                    "Platform", "Any CPU" // "Any CPU" or "x86", or "x64"
                    "DebugSymbols", "True"
                    "Configuration", configuration
                ]
         }

    for solutionFile in solutions do 
        build msBuildParams solutionFile
            |> ignore
)

Target "NUnitTest" (fun _ -> 
    !! ("./../src/**/bin/" + configuration + "/*Tests.dll")
        |> NUnit3 (fun p -> 
            {p with
                ShadowCopy = false;
                //OutputFile = testDir + "TestResults.xml";
                })
)

Target "NuGetPack" (fun _ ->
    let someDllVersion = AssemblyInfoFile.GetAttributeValue "AssemblyVersion" sharedAssemblyInfoFile
    let dllVersion = 
        match someDllVersion with
            | Some x -> x
            | None -> raise (UnknownLibraryVersion "AssemblyVersion not found in Assembly Info file!")

    CreateDir nugetOutPath
    CreateDir nugetWorkPath

    CleanDir nugetWorkPath
    "../src/PCLFileSet/PCLFileSet/PCLFileSet.csproj"
        |> NuGet (fun p -> 
            {p with
                Properties = [("Configuration", configuration)];
                OutputPath = nugetOutPath;
                WorkingDir = nugetWorkPath;
                Version = dllVersion;
                // AccessKey = myAccesskey;
                // Publish = true;
                })
    CleanDir nugetWorkPath
    "../src/PCLFileSet/PCLFileSet.Desktop/PCLFileSet.Desktop.csproj"
        |> NuGet (fun p -> 
            {p with
                Properties = [("Configuration", configuration)];
                OutputPath = nugetOutPath;
                WorkingDir = nugetWorkPath;
                Version = dllVersion;
                // AccessKey = myAccesskey;
                // Publish = true;
                })
    CleanDir nugetWorkPath
    DeleteDir nugetWorkPath
)

Target "Default" (fun _ ->
    DoNothing |> ignore
)

// Target Dependencies...
"Clean"
    ==> "RestorePackages"
    ==> "Build"
    ==> "NUnitTest"
    ==> "NuGetPack"
    ==> "Default"

RunTargetOrDefault "Default"
