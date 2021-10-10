function Get-PropertyNameFromSlug
{
    <#
        .SYNOPSIS
            Gets the MSBuild property name from tag slug e.g. cfn/1.0.0
    #>
    param
    (
        [Parameter(Mandatory)]
        [string] $tagSlug
    )

    switch ($tagSlug)
    {
        'core' { "Generate_CloudFormationParser" }
        'cfn' { "Generate_CloudFormationParserCfn" }
        's3' { "Generate_CloudFormationParserS3" }
        default { throw "Invalid tag slug." }
    }
}

function Update-PackagesGeneration
{
    <#
        .SYNOPSIS
            Update the PackagesGeneration.props based on given tag name.
    #>
    param
    (
        [Parameter(Mandatory)]
        [string] $propertyName
    )

    # Update the package generation props to enable package generation of the right package
    $genPackagesFilePath = "./build/PackagesGeneration.props"
    $genPackagesContent = Get-Content $genPackagesFilePath
    $newGenPackagesContent = $genPackagesContent -replace "<$propertyName>\w+<\/$propertyName>", "<$propertyName>true</$propertyName>"
    $newGenPackagesContent | Set-Content $genPackagesFilePath

    # Check content changes (at least one property changed
    $genPackagesContentStr = $genPackagesContent | Out-String
    $newGenPackagesContentStr = $newGenPackagesContent | Out-String
    if ($genPackagesContentStr -eq $newGenPackagesContentStr)
    {
        throw "MSBuild property $propertyName does not exist in $genPackagesFilePath or content not updated."
    }
}

function Update-AllPackagesGeneration
{
    <#
        .SYNOPSIS
            Update the PackagesGeneration.props to generate all packages.
    #>
    # Update the package generation props to enable package generation of the right package
    $genPackagesFilePath = "./build/PackagesGeneration.props"
    $genPackagesContent = Get-Content $genPackagesFilePath
    $newGenPackagesContent = $genPackagesContent -replace "false", "true"
    $newGenPackagesContent | Set-Content $genPackagesFilePath
}

function Update-DeployBuild
{
    <#
        .SYNOPSIS
            Update the DeployDuild.props to make the build a deploy build.
    #>
    # Update the package generation props to enable package generation of the right package
    $genPackagesFilePath = "./build/DeployBuild.props"
    $genPackagesContent = Get-Content $genPackagesFilePath
    $newGenPackagesContent = $genPackagesContent -replace "false", "true"
    $newGenPackagesContent | Set-Content $genPackagesFilePath
}

########################################################################

# Update .props based on git tag status & setup build version
if ($env:APPVEYOR_REPO_TAG -eq "true")
{
    Update-DeployBuild
    $tagParts = $env:APPVEYOR_REPO_TAG_NAME.split("/", 2)

    # Full release
    if ($tagParts.Length -eq 1) # X.Y.Z
    {
        Update-AllPackagesGeneration
        $env:Build_Version = $env:APPVEYOR_REPO_TAG_NAME
        $env:Release_Name = $env:Build_Version
    }
    # Partial release
    else # Slug/X.Y.Z
    {
        # Retrieve MSBuild property name for which enabling package generation
        $tagSlug = $tagParts[0]
        $propertyName = Get-PropertyNameFromSlug $tagSlug
        $tagVersion = $tagParts[1]

        Update-PackagesGeneration $propertyName
        $env:Build_Version = $tagVersion
        $projectName = $propertyName -replace "Generate_", ""
        $projectName = $projectName -replace "_", "."
        $env:Release_Name = "$projectName $tagVersion"
    }

    $env:IsFullIntegrationBuild = $false # Run only tests on deploy builds (not coverage, etc.)
}
else
{
    Update-AllPackagesGeneration
    $env:Build_Version = "$($env:APPVEYOR_BUILD_VERSION)"
    $env:Release_Name = $env:Build_Version

    $env:IsFullIntegrationBuild = "$env:APPVEYOR_PULL_REQUEST_NUMBER" -eq "" -And $env:Configuration -eq "Release"
}

$env:Build_Assembly_Version = "$env:Build_Version" -replace "\-.*", ""

"Building version: $env:Build_Version"
"Building assembly version: $env:Build_Assembly_Version"

if ($env:IsFullIntegrationBuild -eq $true)
{
    "With full integration"

    $env:PATH = "C:\Program Files\Java\jdk15\bin;$($env:PATH)"
    $env:JAVA_HOME_11_X64 = 'C:\Program Files\Java\jdk15'
    $env:JAVA_HOME = 'C:\Program Files\Java\jdk15'
}
else
{
    "Without full integration"
}