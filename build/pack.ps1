[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$VersionSuffix
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$packageProject = ".\src\Observation\Observation.csproj"
$testProject = ".\tests\Observation.UnitTests\Observation.UnitTests.csproj"

Push-Location $root
try {
    dotnet restore $packageProject
    dotnet restore $testProject
    dotnet test $testProject --configuration $Configuration --no-restore

    $packArgs = @(
        "pack",
        $packageProject,
        "--configuration",
        $Configuration,
        "--no-restore",
        "--output",
        ".\artifacts\packages",
        "-p:EnablePackageValidation=true"
    )

    if ($VersionSuffix) {
        $packArgs += "/p:VersionSuffix=$VersionSuffix"
    }

    dotnet @packArgs
}
finally {
    Pop-Location
}
