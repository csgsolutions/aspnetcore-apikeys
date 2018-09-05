#!/usr/bin/env powershell
#requires -version 4
#
# CSG Build Script
# Copyright 2017 Cornerstone Solutions Group
Param(
	[alias("c")][string]
	$Configuration = "Release",
	[string]
	$BuildToolsVersion = "1.0.0-latest",
	[switch]
	$NoTest,
	[string]
	$BuildNumber=""
)

$Solution =  "$(Get-Item -Path *.sln | Select-Object -First 1)"
$OutputPackages = @(
	".\src\Csg.AspNetCore.Authentication.ApiKey\Csg.AspNetCore.Authentication.ApiKey.csproj"
)
$TestProjects = Get-Item -Path tests\**\*Tests.csproj | %{ $_.FullName }

Write-Host "=============================================================================="
Write-Host "The Build Script"
Write-Host "=============================================================================="

if ($BuildNumber) {
	$BuildNumber = $BuildNumber.PadLeft(5, "0")
}

try {
	. "$PSScriptRoot/bootstrap.ps1"	
	Get-BuildTools -Version $BuildToolsVersion | Out-Null
	
	# RESTORE
	Write-Host "Restoring Packages..." -ForegroundColor Magenta
	dotnet restore $SOLUTION
	if ($LASTEXITCODE -ne 0) {
		throw "Package restore failed with exit code $LASTEXITCODE."
	}

	# BUILD SOLUTION
	Write-Host "Performing build..." -ForegroundColor Magenta	
	dotnet build $SOLUTION --configuration $Configuration /p:BuildNumber=$BuildNumber
	if ($LASTEXITCODE -ne 0) {
		throw "Build failed with exit code $LASTEXITCODE."
	}

	# RUN TESTS
	if ( !($NoTest.IsPresent) -and $TestProjects.Length -gt 0 ) {
		Write-Host "Performing tests..." -ForegroundColor Magenta
		foreach ($test_proj in $TestProjects) {
			Write-Host "Testing $test_proj"			
			#Note: The --logger parameter is for specifically for mstest to make it output test results
			dotnet test $test_proj --no-build --configuration $Configuration --logger "trx;logfilename=TEST-$(get-date -format yyyyMMddHHmmss).xml"
			if ($LASTEXITCODE -ne 0) {
				throw "Test failed with code $LASTEXITCODE"
			}
		}
	}

	# CREATE NUGET PACKAGES
	if ( $OutputPackages.Length -gt 0 ) {
		Write-Host "Packaging..."  -ForegroundColor Magenta
		foreach ($pack_proj in $OutputPackages){
			Write-Host "Packing $pack_proj"
			dotnet pack $pack_proj --no-build --configuration $Configuration /p:BuildNumber=$BuildNumber
			if ($LASTEXITCODE -ne 0) {
				throw "Pack failed with code $result"
			}
		}
	}

	Write-Host "All Done. This build is great! (as far as I can tell)" -ForegroundColor Green
	exit 0
} catch {
	Write-Host "ERROR: An error occurred and the build was aborted." -ForegroundColor White -BackgroundColor Red
	Write-Error $_	
	exit 3
} finally {
	Remove-Module 'BuildTools' -ErrorAction Ignore
}