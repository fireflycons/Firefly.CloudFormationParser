$script:git = Get-Command -Name Git

function Invoke-Git
{
    param
    (
        [switch]$OutputToPipeline,

        [switch]$SuppressWarnings,

        [Parameter(ValueFromRemainingArguments)]
        [string[]]$GitArgs
    )

    $backupErrorActionPreference = $script:ErrorActionPreference
    $script:ErrorActionPreference = "Continue"

    try
    {
        Write-Host -ForegroundColor Cyan ("git " + ($GitArgs -join ' ').Replace($env:GITHUB_ACCESS_TOKEN, "*****")).Replace($env:GITHUB_EMAIL, "*****")

        & $git $GitArgs 2>&1 |
        ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord])
            {
                # Some git warnings still make it out as error records.
                $msg = $(
                    if (-not ([string]::IsNullOrEmpty($_.ErrorDetails.Message)))
                    {
                        $_.ErrorDetails.Message
                    }
                    else
                    {
                        $_.Exception.Message
                    }
                ).Trim()

                if (-not ($SuppressWarnings -and $msg -ilike 'warning*'))
                {
                    Write-Host $msg
                }
            }
            else
            {
                if (-not ([string]::IsNullOrWhitespace($_)))
                {
                    if ($OutputToPipeline)
                    {
                        $_
                    }
                    else
                    {
                        Write-Host "* $_"
                    }
                }
            }
        }

        $exitcode = $LASTEXITCODE
    }
    finally
    {
        $script:ErrorActionPreference = $backupErrorActionPreference
    }
    if ($exitcode -ne 0)
    {
        throw "GIT finished with exit code $exitcode"
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
Invoke-Git clone -q "https://github.com/$docUriPath" $TEMP_REPO_DIR

if (Test-Path -Path $DOC_SITE_DIR -PathType Container)
{
    Write-Host "Clearing local documentation directory..."
    Set-Location $DOC_SITE_DIR
    Invoke-Git rm -r *
}
else
{
    Write-Host "Creating local documentation directory..."
    New-Item -Path $DOC_SITE_DIR -ItemType Directory | Out-Null
    Set-Location $DOC_SITE_DIR
}


Invoke-Git config core.autocrlf true
Invoke-Git config core.eol lf

Invoke-Git config --global user.email $env:GITHUB_EMAIL
Invoke-Git config --global user.name  $env:APPVEYOR_REPO_COMMIT_AUTHOR

Write-Host "Copying documentation into the local documentation directory..."
Copy-Item -recurse $SOURCE_DIR/docfx/_site/* .

Invoke-Git "add -A ."

Write-Host "Checking if there are changes in the documentation..."
if (-not [string]::IsNullOrEmpty($(Invoke-Git -OutputToPipeline status --porcelain)))
{
    Write-Host "Pushing the new documentation to github.io..."
    Invoke-Git -OutputToPipeline commit -m "AppVeyor Build ${env:APPVEYOR_BUILD_NUMBER}"
    Invoke-Git remote set-url origin "https://$($env:GITHUB_ACCESS_TOKEN)@github.com/$docUriPath"
    Invoke-Git -OutputToPipeline push -q origin
    Write-Host "Documentation updated!"
}
else
{
    Write-Host "Documentation update ignored: No relevant changes in the documentation."
}