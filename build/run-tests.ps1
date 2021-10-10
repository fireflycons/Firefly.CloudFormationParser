$resultsDir = Join-Path $env:APPVEYOR_BUILD_FOLDER TestResults
$dotnet = Get-Command dotnet
$testsPassed = $true

try
{
    $wc = [System.Net.WebClient]::new()

    $(
        Get-ChildItem -Path (Join-Path $env:APPVEYOR_BUILD_FOLDER tests) -Filter *.Tests.Unit.csproj -Recurse
        Get-ChildItem -Path (Join-Path $env:APPVEYOR_BUILD_FOLDER tests) -Filter *.Tests.Integration.csproj -Recurse
    ) |
    Foreach-Object {

        Write-Host
        Write-Host "##" $_.BaseName
        $logFileName = "$($_.BaseName).trx"

        & $dotnet test $_.FullName --results-directory $resultsDir --logger "trx;LogFileName=$logFileName"

        if ($LASTEXITCODE -ne 0)
        {
            $testsPassed = $false
        }

        $wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", (Join-Path $env:APPVEYOR_BUILD_FOLDER $logFileName))
    }
}
finally
{
    $wc.Dispose()
}

if (-not $testsPassed)
{
    throw "Some tests failed"
}