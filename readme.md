# F# PowerShell Cmdlets

## Search-File

Grep-like search within files. Help with -full option to get useful help.

Originally from the code referenced in the article ["Rethinking findstr with F# and Powershell"](http://blogs.msdn.com/b/fsharpteam/archive/2012/10/03/rethinking-findstr-with-f-and-powershell.aspx). I tweaked the code to bubble up errors, default to not search executable files, provide rudimentary help, and added an option to exclude files by regular expressions. 

### -Pattern
position 0
required

Regular expression used for matching.

### -Include
position 1
default '*'
comma-separated array

Returns files in current directory matching one or more of the wildcards (not regular expressions).

Example: '\*.fs','\*.fsx'

### -Exclude
comma-separated array

Excludes files matching one or more of the regular expressions (not wild cards).

Example: -exclude '.pdb','.xml'

### -Recurse

Recursively search sub-directories.

### -IncludeExecute

Include executable files '*.exe','*.dll' in search.

### -Encoding
defalult Encoding.ASCII 

I've never tested changing the encoding. Presumably it works.

### -SimpleMatch

Makes search case sensitive.

### -SimpleMatch

Do not use regex, just do a verbatim string search.

## To Dos

- Annoying that Include takes pattern parameters and Exclude takes regular expressions. Pretty hard to remedy, I think.
- I would like to get the dll-help.xml file working. Helpmessage property in the Parameter attribute is second best.
- Ideas for more F# cmdlets.


