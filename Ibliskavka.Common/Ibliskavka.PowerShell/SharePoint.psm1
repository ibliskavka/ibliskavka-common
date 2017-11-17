#
# SharePoint.psm1
# Useful SharePoint 2010 methods. Not specific to any project.
#

Add-PSSnapin Microsoft.Sharepoint.Powershell -EA SilentlyContinue
$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module "$ScriptDir\Common.psm1"

<#
    UploadAndInstallUserSolutions
    Given a input directory and an array of solution names (w/o extention), uploads and installs solutions
#>
function UploadAndInstallUserSolutions([System.String] $url, [System.String] $inputDir, [System.Array] $solutions){
	foreach($solution in $solutions){
        Write-Host "Installing Solution $inputDir\$solution.wsp"
		Add-SPUserSolution -LiteralPath "$inputDir\$solution.wsp" -Site $url
		Install-SPUserSolution -Identity $solution -Site $url
	}
}

<#
	CreateSiteFromTemplate
	Deletes existing site and creates an environment from templates that already exist on the server
    Returns the resulting web
#>
function CreateSiteFromSolution($targetSite, $solutionName, $title){

    RemoveSPWebRecursivelyByUrl -url $targetSite

    Write-Host "Creating site: $targetSite"
    $web = New-SPWeb -url $targetSite
    $templates = $Web.GetAvailableWebTemplates(1033)
    $template = $templates | Where-Object {$_.Title -eq $solutionName}
    $Web.ApplyWebTemplate($template.Name)  
    
    $web.Title = $title
    $web.Update()
    
    return $web
}

<#
	UninstallAndDeleteSolution
	Given a site root and solution name (w/o .wsp extension) function will uninstall and delete the solution
#>
function UninstallAndDeleteSolution([System.String] $url, [System.String] $solutionName){
	Uninstall-SPUserSolution -identity "$solutionName.wsp" -Site $url -Confirm:$false
    Remove-SPUserSolution -identity "$solutionName.wsp" -Site $url -Confirm:$false
}

<#
	RemoveSPWebRecursivelyByUrl
	Deletes the web at a given URL and all sub-webs
#>
function RemoveSPWebRecursivelyByUrl([String]$url)
{
    if((Get-SPWeb $url -ErrorAction SilentlyContinue) -ne $null){
		Write-Host "Removing Site Recursively: $url"
        $web = Get-SPWeb $url
        RemoveSPWebRecursively -web $web
        $web.Dispose()
    }
}

<#
	RemoveSPWebRecursivelyByUrl
	Deletes the given web and all sub-webs
#>
function RemoveSPWebRecursively([Microsoft.SharePoint.SPWeb]$web)
{
   $subwebs = $web.GetSubwebsForCurrentUser()
    
    foreach($subweb in $subwebs)
    {
        RemoveSPWebRecursively($subweb)
        $subweb.Dispose()
    }

    Remove-SPWeb $web -Confirm:$false
}

<#
	Downloads user solutions to the file system.
#>
function DownloadUserSolutions([String]$url, [Array]$solutions, [String]$outDir){
	
	if(!(Test-Path $outDir)){
		echo "Creating physical directory: " + $outDir
		New-Item -ItemType Directory -Force -Path $outDir
	}

	$web = Get-SPWeb $url

	$listTemplate = [Microsoft.SharePoint.SPListTemplateType]::SolutionCatalog
	$solGallery = $web.Site.GetCatalog($listTemplate)
	$solGallery.Items | ForEach-Object {
		foreach($solution in $solutions){
			if($_["Title"] -eq $solution) {
				[System.IO.FileStream]$outStream = New-Object System.IO.FileStream("$outDir/$solution.wsp", [System.IO.FileMode]::Create);
				$fileData = $_.File.OpenBinary();
				$outStream.Write($fileData, 0, $fileData.Length);
				$outStream.Close();
			}
		}
	}

	$web.Dispose()
}

<#
	ExportSiteToFiles
	Exports an environment to files. The target environment is 'tmp' and connection strings are cleared out.
#>
function ExportSiteToFiles([String]$siteCollection, [String]$site, [String]$outDir, [String]$fileName){
	#Default value is 50, my project is near 75.
	Set-MaxTemplateDocSize "$siteCollection/" 100
    
    $fullPath = "$siteCollection/$site"
    Write-Host "Exporting template from $fullPath to $outDir\$fileName. Start Time $(Get-Date)"
    
	#Save temp site as template
    Write-Host "Saving site $fullPath as template with data"
    
	$web = Get-SPWeb $fullPath
	$web.SaveAsTemplate($fileName, $fileName, "", 1)
	$web.Dispose()
    
	#Download templates to disk
	Write-Host "Downloading template"
	DownloadUserSolutions -url $siteCollection -solutions @($fileName) -outDir $outDir
    
	#HouseKeeping
	UninstallAndDeleteSolution -url $siteCollection -solutionName $fileName

	#Reset max file size
	Set-MaxTemplateDocSize "$siteCollection/" 50
    
	Write-Host "ExportSiteToFile End Time $(Get-Date)"
}

<#
	Find-And-Replace
	Case insensitive search of $list $column for $match and replaces it with $replace
#>
Function Find-And-Replace([Microsoft.SharePoint.SPList]$list, [String]$column, [String]$match, [String]$replace){
    foreach($item in $list.Items){
        if($item[$column] -ne $null -and $item[$column].ToLower().Contains($match)){
            Write-Host "Replacing $($list.Title) $column match:$match"
            $item[$column] = Custom-Replace -source $item[$column] -match $match -replace $replace
            $item.Update()
        }
    }
}

<#
	Set-MaxTemplateDocSize
	Sets the maximum template doc size. Also sets max library file size (since template outputs to solution gallery)
#>
Function Set-MaxTemplateDocSize([String]$siteCollection, [int]$newSizeMb){
	$webservice = [Microsoft.SharePoint.Administration.SPWebService]::ContentService
    
    #Set New Limit and update
    $webservice.MaxTemplateDocumentSize = 1024 * 1024 * $newSizeMb
    $webservice.Update()
    
    Set-OSCMaximumFileSize -WebApplication $siteCollection -Size $newSizeMb
}

<#
	Set-OSCMaximumFileSize
	Sets the maximum document library file size
#>
Function Set-OSCMaximumFileSize
{
	[CmdletBinding()]
	param 
	(
        [Parameter(Mandatory=$true,Position=0)]
        [String]$WebApplication,
        [Parameter(Mandatory=$true,Position=1)]
        [String]$Size
    )
    try
    {
		#Get the specified webapplication
        $webapp =  Get-SPWebApplication | where {$_.url -eq $WebApplication }
        If($webapp)
        {
			#Get current maximumfilesize
            $CurrentSize = $webapp.MaximumFileSize

            $webapp.MaximumFileSize = $Size
            $webapp.update()

            write-host "Set MaximumFileSize from $CurrentSize to $Size MB successfully. "
        
        }
        Else
        {
            Write-Error "There is no webapplication '$WebApplication'"
        }
    }
    catch 
    {
        Write-Error $_
    }
}