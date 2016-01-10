#r @"..\packages\Microsoft.PowerShell.5.ReferenceAssemblies\lib\net4\System.Management.Automation.dll" 
#r @"..\posh-module\FSharpx.Cmdlet.dll" 
 
open FSharpx.Cmdlet 
open System.IO 
open System 
open System.Linq 
 
// set up some test files 
let temp = Path.Combine (Path.GetTempPath (), "FSUtilsTest") 
if Directory.Exists temp then Directory.Delete (temp, true) 
Directory.CreateDirectory temp |> ignore 
Directory.CreateDirectory (Path.Combine (temp, "subdir")) |> ignore 
 
let fileNames = 
    [| "File1.txt"; "File2.log"; "subdir\File3.bat"; 
        "subdir\File4.ini"; "subdir\FileEncoding.txt" 
    |] |> Array.map (fun path -> Path.Combine (temp, path)) 
 
fileNames 
|> Array.iteri (fun i path -> 
    let encoding =  
        if   i <> fileNames.Length - 1 
        then Text.Encoding.ASCII 
        else Text.Encoding.Unicode 
    File.AppendAllText (path, sprintf "Line1 %s\r\n" path, encoding) 
    File.AppendAllText (path, sprintf "Line2 %s\r\n" path, encoding) 
    File.AppendAllText (path, sprintf "Line3 %s\r\n" path, encoding) 
    ) 
 
// helpers 
let test name func = 
    try 
        printfn "Running test - %s" name; func () 
    with 
    | Failure message -> printfn "\tTest failed: %A" message 
    | e -> printfn "\tTest failed: %A" e.Message 
 
let doSearch (searcher:FileSearcher) expected =  
    match searcher.Search().Count () with 
    | n when n = expected -> () 
    | n -> failwith <| sprintf "Wrong number of results returned: %d" n 
 
 
// tests 
test "Recursive/non-recursive search" (fun () -> 
    doSearch (FileSearcher ("^Line1", temp, [|"*.*"|], [||], false, false, false, Text.Encoding.ASCII, false)) 2 
    doSearch (FileSearcher ("^Line1", temp, [|"*.*"|], [||], true, false, false,  Text.Encoding.ASCII, false)) 5 
) 
 
test "Case sensitivity" (fun () -> 
    doSearch (FileSearcher ("^Line1", temp, [|"*.*"|], [||], true, true, false, Text.Encoding.ASCII, false)) 5 
    doSearch (FileSearcher ("^line1", temp, [|"*.*"|], [||], true, true, false, Text.Encoding.ASCII, false)) 0 
) 
 
test "Simple string match" (fun () -> 
    doSearch (FileSearcher ("^Line1", temp, [|"*.*"|], [||], true, true, true, Text.Encoding.ASCII, false)) 0 
    doSearch (FileSearcher ("Line1", temp,  [|"*.*"|], [||], true, true, true, Text.Encoding.ASCII, false)) 5 
    doSearch (FileSearcher ("\\", temp,     [|"*.*"|], [||], true, true, true, Text.Encoding.ASCII, false)) 15 
) 
 
test "File extensions" (fun () -> 
    doSearch (FileSearcher ("^Line1", temp, [|"*.txt"|], [||], false, false, false, Text.Encoding.ASCII, false)) 1 
    doSearch (FileSearcher ("^Line1", temp, [|"*.txt"; "*.bat"|], [||], false, false, false, Text.Encoding.ASCII, false)) 1 
    doSearch (FileSearcher ("^Line1", temp, [|"*.txt"; "*.bat"; "*.log"|], [||], false, false, false, Text.Encoding.ASCII, false)) 2 
    doSearch (FileSearcher ("^Line1", temp, [|"*.foo"|], [||], false, false, false, Text.Encoding.ASCII, false)) 0 
) 
 
test "Encoding" (fun () -> 
    doSearch (FileSearcher ("^Line\d", temp, [|"*.*"|], [||], true, false, false, Text.Encoding.Unicode, false)) 3 // only the 3 lines from the unicode file 
) 
 
test "Returned FileMatch objects" (fun () -> 
    let s = FileSearcher ("^Line(?<linenum>\d)", temp, [|"*.txt"|], [||], false, false, false, Text.Encoding.ASCII, false) 
    s.Search () |> Seq.iteri (fun i lm -> 
        let expected = sprintf "Line%d" (i+1) 
        if not (lm.Line.StartsWith expected) then failwith "Result line does not start with expected string" 
        if not (lm.["linenum"] = (i+1).ToString()) then failwith "Named capture group not as expected" 
        if not (lm.[0] = expected) then failwith "Numbered capture group not as expected" 
        if lm.RelativePath <> "File1.txt" then failwith "Unexpected relative path" 
      ) 
)