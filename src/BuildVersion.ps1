param
(
    [string] $ProjectFile
)

Write-Host "Generating Build Number"

#Get the version from the csproj file
$xml = [Xml] (Get-Content $ProjectFile)
$initialVersion = [Version] $xml.Project.PropertyGroup.Version
Write-Host "Initial version read from project file: $initialVersion" 
$splitVersion = $initialVersion -Split "\."

#Get the build number (number of days since January 1, 2000)
$baseDate = [datetime]"01/01/2000"
$currentDate = $(Get-Date)
$interval = (NEW-TIMESPAN -Start $baseDate -End $currentDate)
$buildNumber = $interval.Days
#Write-Host $buildNumber

#Get the revision number (number seconds (divided by two) into the day on which the compilation was performed)
$StartDate=[datetime]::Today
$EndDate=(GET-DATE)
$revisionNumber = [math]::Round((New-TimeSpan -Start $StartDate -End $EndDate).TotalSeconds / 2,0)
#Write-Host $revisionNumber

#Final version number, using the Major, Minor and Patch versions, and then appends build and revision number together
$finalBuildVersion = "$($splitVersion[0]).$($splitVersion[1]).$($splitVersion[2]).$($buildNumber)$($revisionNumber.ToString("00000"))"
#Write-Host "Major.Minor,patch,Build+Revision"
Write-Host "Final build number: $finalBuildVersion" 
#Writing final version number back to Github variable
#Write-Host "Writing final version $finalBuildVersion back to Github variable"
echo "::set-env name=buildVersion::$finalBuildVersion"
#echo "buildVersion=$finalBuildVersion" >> $GITHUB_ENV

