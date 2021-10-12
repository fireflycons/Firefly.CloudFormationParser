function Invoke-Git
{
    <#
    .SYNOPSIS
        Invoke git, handling its quirky stderr that isn't error

    .OUTPUTS
        Git messages, and lastly the exit code

    .EXAMPLE
        Invoke-Git push

    .EXAMPLE
        Invoke-Git "add ."
    #>
    param
    (
        [Parameter(Mandatory)]
        [string] $Command
    )

    try
    {
        $exitCode = 0
        $path = [System.IO.Path]::GetTempFileName()

        Invoke-Expression "git $Command 2> $path"
        $exitCode = $LASTEXITCODE

        if ( $exitCode -gt 0 )
        {
            Write-Error (Get-Content $path).ToString()
        }
        else
        {
            Get-Content $path | Select-Object -First 1
        }

        $exitCode
    }
    catch
    {
        Write-Host "Error: $_`n$($_.ScriptStackTrace)"
    }
    finally
    {
        if ( Test-Path $path )
        {
            Remove-Item $path
        }
    }
}

$ErrorActionPreference = "Stop"

# Only master Release
if ($env:Configuration -ne "Release")
{
    Write-Host "Documentation update ignored: Not Release build."
    return
}

if ($env:APPVEYOR_REPO_BRANCH -ne "master")
{
    Write-Host "Documentation update ignored: Not master branch."
    return
}

if (-not [string]::IsNullOrEmpty($env:APPVEYOR_PULL_REQUEST_NUMBER))
{
    # Secure variables not set during PR, so we couldn't do this even if we wanted to.
    Write-Host "Documentation update ignored: Pull request."
    return
}

# Chocolatey DocFX
cinst docfx --version $env:DocFXVersion -y  --limit-output |
Foreach-Object {
    if ($_ -inotlike 'Progress*Saving*')
    {
        Write-Host $_
    }
}

Write-Host "Generating documentation site..."
docfx ./docfx/docfx.json

$githubAccount, $repoName = $env:APPVEYOR_REPO_NAME -split '/'
$docUriPath = "$($githubAccount)/$($githubAccount).github.io.git"

$SOURCE_DIR = $env:APPVEYOR_BUILD_FOLDER
$TEMP_REPO_DIR = Join-Path ([IO.Path]::GetTempPath()) "$githubAccount.github.io"
$DOC_SITE_DIR = Join-Path $TEMP_REPO_DIR $repoName

if (Test-Path $TEMP_REPO_DIR)
{
    Write-Host "Removing temporary documentation directory $TEMP_REPO_DIR..."
    Remove-Item -recurse $TEMP_REPO_DIR
}

New-Item -Path $TEMP_REPO_DIR -ItemType Directory | Out-Null

Write-Host "Cloning the documentation site."
git clone -q "https://github.com/$docUriPath" $TEMP_REPO_DIR

if (Test-Path -Path $DOC_SITE_DIR -PathType Container)
{
    Write-Host "Clearing local documentation directory..."
    Set-Location $DOC_SITE_DIR
    git rm -r *
}
else
{
    Write-Host "Creating local documentation directory..."
    New-Item -Path $DOC_SITE_DIR -ItemType Directory | Out-Null
    Set-Location $DOC_SITE_DIR
}


git config core.autocrlf true
git config core.eol lf

git config --global user.email $env:GITHUB_EMAIL
git config --global user.name  $env:APPVEYOR_REPO_COMMIT_AUTHOR

Write-Host "Copying documentation into the local documentation directory..."
Copy-Item -recurse $SOURCE_DIR/docfx/_site/* .

Invoke-Git "add -A ."

Write-Host "Checking if there are changes in the documentation..."
if (-not [string]::IsNullOrEmpty($(git status --porcelain)))
{
    Write-Host "Pushing the new documentation to github.io..."
    git commit -m "AppVeyor Build ${env:APPVEYOR_BUILD_NUMBER}"
    git remote set-url origin "https://$($env:GITHUB_ACCESS_TOKEN)@github.com/$docUriPath"
    git push -q origin
    Write-Host "Documentation updated!"
}
else
{
    Write-Host "Documentation update ignored: No relevant changes in the documentation."
}