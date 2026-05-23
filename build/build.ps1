[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")

$sourceProjects = Get-ChildItem -Path (Join-Path $root "src") -Recurse -Filter "*.csproj" -File
$testProjects = Get-ChildItem -Path (Join-Path $root "tests") -Recurse -Filter "*.csproj" -File
$sampleProjects = if (Test-Path -LiteralPath (Join-Path $root "samples")) {
    Get-ChildItem -Path (Join-Path $root "samples") -Recurse -Filter "*.csproj" -File
}
else {
    @()
}
$benchmarkProjects = if (Test-Path -LiteralPath (Join-Path $root "benchmarks")) {
    Get-ChildItem -Path (Join-Path $root "benchmarks") -Recurse -Filter "*.csproj" -File
}
else {
    @()
}

$restoreProjects = @(@($sourceProjects) + @($testProjects) + @($sampleProjects) + @($benchmarkProjects)) |
    Sort-Object -Property FullName -Unique

Push-Location $root
try {
    foreach ($project in $restoreProjects) {
        dotnet restore $project.FullName
    }

    foreach ($project in $sourceProjects) {
        dotnet build $project.FullName --configuration $Configuration --no-restore
    }

    foreach ($project in $sampleProjects) {
        dotnet build $project.FullName --configuration $Configuration --no-restore /p:UseSharedCompilation=false
    }

    foreach ($project in $benchmarkProjects) {
        dotnet build $project.FullName --configuration $Configuration --no-restore /p:UseSharedCompilation=false
    }

    if (-not $SkipTests) {
        foreach ($project in $testProjects) {
            $trxFileName = "{0}.trx" -f $project.BaseName
            dotnet test $project.FullName --configuration $Configuration --no-restore --logger "trx;LogFileName=$trxFileName"
        }
    }
}
finally {
    Pop-Location
}
