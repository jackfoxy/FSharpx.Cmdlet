// Copyright (c) Microsoft Corporation 2005-2012. 
// This sample code is provided "as is" without warranty of any kind.  
// We disclaim all warranties, either express or implied, including the  
// warranties of merchantability and fitness for a particular purpose.  
 
namespace FSharpx.Cmdlet 
 
open Prelude
open System.Collections.Concurrent 
open System.IO 
open System.Text 
open System.Text.RegularExpressions 
open System.Threading.Tasks 

type ProcessException = MalformedRegex

/// Class for doing regex searches of files on disk. 
type FileSearcher
   (pattern:string, startingDirectory:string, includePatterns:string [], 
    excludePatterns:string [], recurse:bool, caseSensitive:bool, 
    simpleMatch:bool, encoding:Encoding, IncludeExecute:bool) = 

    /// Compiled regex object which will be used for matching. 
    let regex = 
        try match pattern, caseSensitive, simpleMatch with 
            | p, false, false   -> Regex (p, RegexOptions.Compiled ||| RegexOptions.IgnoreCase) |> Success
            | p, false, true    -> Regex (Regex.Escape p, RegexOptions.Compiled ||| RegexOptions.IgnoreCase) |> Success
            | p, true,  false   -> Regex (p, RegexOptions.Compiled) |> Success
            | p, true,  true    -> Regex (Regex.Escape p,  RegexOptions.Compiled) |> Success
        with _ -> Failure MalformedRegex
 
    /// Collection into which matching lines will be fed. 
    let bc = new BlockingCollection<LineMatch>()     
    let accumError msg = bc.Add (LineMatch ("Error:" + msg , "", 0, "", (Regex "").Match ""))

    /// Error reporting
    let reportError msg = accumError msg; bc.CompleteAdding ()

    let excludeRegexes =
        let rec loop regexs xs =
            match xs with
            | [] -> Success regexs
            | hd::tl ->
                try loop (Regex(hd)::regexs) tl
                with _ ->   accumError ("Malformed regular expression in exclude regex " + hd)
                            Failure MalformedRegex
        excludePatterns |> Array.toList |> loop []    
 
    /// Returns all files in the specified directory matching 
    ///    one or more of the wildcards in "includePatterns." 
    let GetIncludedFiles dir (excludeRegexes:Regex list) = 
        seq {for patt in includePatterns do 
                yield! Directory.EnumerateFiles(dir, patt) 
        } |> Seq.filter (fun s -> excludeRegexes |> List.exists (fun x -> x.IsMatch s) |> not)
     
    /// Pattern which matches a directory path with either 
    ///   Contents(<enumerable of files in the directory>, <enumerable of directories in the directory>) 
    ///   AccessDenied if unable to obtain directory contents 
    let (|Contents|AccessDenied|) (dir, (excludeRegexes:Regex list)) =  
            try Success (GetIncludedFiles dir excludeRegexes,  
                    if recurse then Directory.EnumerateDirectories dir
                    else Seq.empty) 
            with :? System.UnauthorizedAccessException -> AccessDenied 
    
    /// Enumerates all accessible files in or under the specified directory. 
    let rec GetFiles (excludeRegexes:Regex list) dir  =  
        seq { match (dir, excludeRegexes) with 
                | Contents (files, directories) -> 
                    yield! files 
                    yield! directories |> Seq.collect (GetFiles excludeRegexes)
                | AccessDenied -> accumError ("Access denied directory " + dir)
        } 
 
    /// Scans the specified file for lines matching the specified pattern and 
    ///    inserts them into the blocking collection. 
    let CollectLineMatches (file:string) (regex:Regex) = 
        try 
            if not IncludeExecute && (file.ToLower().EndsWith ".exe" || file.ToLower().EndsWith ".dll") then () else
            File.ReadAllLines (file, encoding) |> Array.iteri (fun i line ->            
            match regex.Match line with 
            | m when m.Success -> bc.Add (LineMatch (file, startingDirectory, i + 1, line, m)) 
            | _ -> ()) 
        with
            | :? IOException -> reportError ("IOException on file " + file)
            | e -> reportError (sprintf "%s, exception processing file %s" e.Message file) 
     
    /// Initiates the search for matching file content, returning an enumerable of matching lines. 
    /// Note that the search is executed in parallel and thus the order of results is not guaranteed. 
    member this.Search () = 
        match regex with
        | Success regex ->
            match excludeRegexes with
            | Success excludes ->
                Task.Factory.StartNew (fun () -> 
                    let tasks =  
                        GetFiles excludes startingDirectory
                        |> Seq.map (fun file -> Task.Factory.StartNew (fun () -> CollectLineMatches file regex)) 
                        |> Seq.toArray 
                    if tasks.Length = 0 then bc.CompleteAdding () else 
                    Task.Factory.ContinueWhenAll (tasks, fun _ -> bc.CompleteAdding ()) |> ignore 
                ) |> ignore   
            | Failure _ -> bc.CompleteAdding ()
        | Failure _ -> reportError ("Malformed regular expression in search pattern " + pattern)
        bc.GetConsumingEnumerable ()