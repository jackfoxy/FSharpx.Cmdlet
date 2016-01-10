# ==============================================================================================
# 
#	NAME	: FSharpx.Cmdlet.psm1
#	AUTHORS : Jack Foxy, Jared Hester
#
#		Example Posh Module File
#
# ============================================================================================== 

$Script:memory = "this script scoped variable will maintain its state"


function internal-helper-function {
	"This function will not be exported"
}

## Alternate names for our standard command
New-Alias -Name grep -Value Search-File


Export-ModuleMember -Function Search-File
Export-ModuleMember -Alias grep