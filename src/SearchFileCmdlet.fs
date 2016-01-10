// Copyright (c) Microsoft Corporation 2005-2012. 
// This sample code is provided "as is" without warranty of any kind.  
// We disclaim all warranties, either express or implied, including the  
// warranties of merchantability and fitness for a particular purpose.  
 
namespace FSharpx.Cmdlet 
 
open System.Management.Automation 
open System.Text 
 
(* 
  Powershell cmdlet Search-File 

  http://blogs.msdn.com/b/fsharpteam/archive/2012/10/03/rethinking-findstr-with-f-and-powershell.aspx
 
  To use - build this project, open Windows Powershell, and type "Import-Module <path to project>\bin\debug\FSUtils.dll" 
  To import additional formatting configuration (so that output looks like findstr output) type "Update-FormatData -Prepend <path to project>\bin\debug\FSUtils.format.ps1xml" 
   
  Note - if you are running Powershell v2, you will need to manually enable loading .NET 4 assemblies. 
    To do this, edit or create a file at $pshome\powershell.exe.config with the below content 
 
    <?xml version="1.0" encoding="utf-8"?> 
    <configuration> 
        <startup> 
          <supportedRuntime version="v4.0.30319" /> 
        </startup> 
    </configuration> 
 
  Examples: 
 
  # recursive search of all *.cs files for given regex 
  PS C:\users\john\documents> Search-File '(public|private) class' *.cs -Recurse 
 
  # recursive search of all *.cs and *.cpp files 
  PS C:\users\john\documents> Search-File 'TODO' '*.cs','*.cpp' -Recurse 
 
  # non-recursive search for literal string 'parsley' in *.txt files 
  PS C:\users\john\documents\recipes> Search-File 'parsley' *.txt -SimpleMatch -CaseSensitive 
*) 
 
/// Grep-like cmdlet Search-File 
[<Cmdlet ("Search", "File")>] 
type SearchFileCmdlet () = 
    inherit PSCmdlet () 
 
    /// Regex pattern used in the search. 
    [<Parameter (Mandatory = true, Position = 0, HelpMessage = "regular expression which will be used for matching" )>] 
    [<ValidateNotNullOrEmpty>] 
    member val Pattern : string = null with get, set 
     
    /// Array of filename wildcards. 
    [<Parameter (Position = 1, HelpMessage = "returns files in current directory matching one or more of the wildcards (not regular expressions); e.g. '*.fs','*.fsx'; default '*'" )>] 
    [<ValidateNotNull>] 
    member val Include = [|"*"|] with get, set 

    /// Array of filename excludes. 
    [<Parameter (HelpMessage = "excludes files matching one or more of the regular expressions (not wild cards); e.g. '.pdb','.xml'" )>] 
    [<ValidateNotNull>] 
    member val Exclude = [||] with get, set 
     
    /// Whether or not to recurse from the current directory. 
    [<Parameter (HelpMessage = "recursively search sub-directories" )>] 
    member val Recurse : SwitchParameter = SwitchParameter false with get, set 

    /// Whether or not to include executable files exe and dll. 
    [<Parameter (HelpMessage = "include executable files '*.exe','*.dll' in search" )>] 
    member val IncludeExecute : SwitchParameter = SwitchParameter false with get, set 
 
    /// Endcoding to use when reading the files. 
    [<Parameter>] 
    member val Encoding = Encoding.ASCII with get, set 
     
    /// Toggle for case-sensitive search. 
    [<Parameter (HelpMessage = "makes search case sensitive")>] 
    member val CaseSensitive : SwitchParameter = SwitchParameter false with get, set 
 
    /// Do not use regex, just do a verbatim string search. 
    [<Parameter (HelpMessage = "do not use regex, just do a verbatim string search")>] 
    member val SimpleMatch : SwitchParameter = SwitchParameter false with get, set 
     
    /// Called once per object coming from the pipeline. 
    override this.ProcessRecord () = 
        let searcher = 
            FileSearcher (this.Pattern, this.SessionState.Path.CurrentFileSystemLocation.Path, 
                this.Include, this.Exclude, this.Recurse.IsPresent, this.CaseSensitive.IsPresent, 
                this.SimpleMatch.IsPresent, this.Encoding, this.IncludeExecute.IsPresent) 
        searcher.Search () |> Seq.iter (fun item -> this.WriteObject item)