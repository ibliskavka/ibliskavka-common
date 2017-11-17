#
# Common.psm1
# For basic utilities
#

<#
	Custom-Replace
	Case-insensitive replace - Uses regex and escapes special chars.
#>
function Custom-Replace([String]$source, [String]$match, [String]$replace){
	return $source -ireplace [regex]::Escape($match), $replace
}

<#
	Creates a remote P$ 2.0 profile
#>
function Create-PS2-Profile([String]$computerName){
	$s = New-PSSession -ComputerName $computerName
	Invoke-Command -Session $s -ScriptBlock { 
    	Register-PSSessionConfiguration -Name PS2 -PSVersion 2.0
	}
}

<#
	Example of executing a remote powershell script using the P$ 2.0
#>
function Example-Remote-Invoke([String]$computerName, [String]$remoteScriptPath){
	$s = New-PSSession -ComputerName $computerName -ConfigurationName PS2
	Invoke-Command -Session $s -ScriptBlock { Invoke-Expression $remoteScriptPath}
}