<#
.SYNOPSIS
    Runs the integration test suite against a real TwinCAT system service.

.DESCRIPTION
    Sets ADS_TEST_TARGET and invokes `dotnet test` on the test project. Any extra
    arguments are forwarded to `dotnet test`. See
    test/TwinCAT.Ads.Extensions.Tests/README.md for prerequisites (ADS route etc.).

.EXAMPLE
    scripts/run-integration-tests.ps1
    Runs against the loopback TwinCAT (127.0.0.1.1.1).

.EXAMPLE
    scripts/run-integration-tests.ps1 -Target 5.62.31.13.1.1

.EXAMPLE
    scripts/run-integration-tests.ps1 -Target 5.62.31.13.1.1 --filter FullyQualifiedName~RenameFileAsync
#>
param(
    [string]$Target = $(if ($env:ADS_TEST_TARGET) { $env:ADS_TEST_TARGET } else { "127.0.0.1.1.1" }),
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Rest
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\test\TwinCAT.Ads.Extensions.Tests"

Write-Host "Running integration tests against ADS target: $Target"
$env:ADS_TEST_TARGET = $Target
dotnet test $project @Rest
