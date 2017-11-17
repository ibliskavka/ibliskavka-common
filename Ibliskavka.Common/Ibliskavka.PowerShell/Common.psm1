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