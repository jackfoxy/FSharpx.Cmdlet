// Copyright (c) Microsoft Corporation 2005-2012. 
// This sample code is provided "as is" without warranty of any kind.  
// We disclaim all warranties, either express or implied, including the  
// warranties of merchantability and fitness for a particular purpose.  
 
namespace FSharpx.Cmdlet 
 
open System.Text.RegularExpressions 
 
// holds info about lines of files matching the provided pattern 
type LineMatch (filePath:string, currentPath:string, lineNumber:int, line:string, m:Match) =  
    member val Path = filePath 
    member val RelativePath = filePath.Remove(0, currentPath.Length + 1) 
    member val LineNumber = lineNumber 
    member val Line = line 
    member val Match = m 
 
    // .[] access to match groups 
    member this.Item  
        with get (i:int) =  
            match m.Groups.[i] with 
            | g when g.Success -> g.Value | _ -> null 
 
    member this.Item 
        with get (i:string) =  
            match m.Groups.[i] with 
            | g when g.Success -> g.Value | _ -> null