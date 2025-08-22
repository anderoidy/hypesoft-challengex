# Project Verification Script for Hypesoft Challenge
# This script verifies the project structure and checks for missing NuGet packages

# Configuration
$projectRoot = "$PSScriptRoot\backend\src"
$requiredProjects = @(
    "Hypesoft.API",
    "Hypesoft.Application",
    "Hypesoft.Domain",
    "Hypesoft.Infrastructure",
    "Hypesoft.Tests"
)

# Required NuGet packages for each project
$requiredPackages = @{
    "Hypesoft.API" = @(
        "Microsoft.AspNetCore.Mvc.NewtonsoftJson",
        "Microsoft.AspNetCore.Identity.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.Design",
        "Microsoft.EntityFrameworkCore.Tools",
        "Swashbuckle.AspNetCore"
    )
    "Hypesoft.Application" = @(
        "MediatR",
        "AutoMapper",
        "FluentValidation",
        "Ardalis.Result"
    )
    "Hypesoft.Domain" = @(
        "Ardalis.Specification"
    )
    "Hypesoft.Infrastructure" = @(
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.SqlServer",
        "Microsoft.Extensions.Identity.Stores",
        "Microsoft.Extensions.Configuration",
        "Microsoft.Extensions.Options.ConfigurationExtensions"
    )
    "Hypesoft.Tests" = @(
        "xunit",
        "Moq",
        "Microsoft.NET.Test.Sdk",
        "xunit.runner.visualstudio"
    )
}

# Check project structure
Write-Host "Verifying project structure..." -ForegroundColor Cyan
$missingProjects = @()

foreach ($project in $requiredProjects) {
    $projectPath = Join-Path $projectRoot $project
    if (-not (Test-Path $projectPath)) {
        $missingProjects += $project
        Write-Host "  [MISSING] $project" -ForegroundColor Red
    } else {
        Write-Host "  [FOUND]   $project" -ForegroundColor Green
    }
}

if ($missingProjects.Count -gt 0) {
    Write-Host "`nThe following projects are missing: $($missingProjects -join ', ')" -ForegroundColor Yellow
    exit 1
}

# Check NuGet packages
Write-Host "`nVerifying NuGet packages..." -ForegroundColor Cyan

foreach ($project in $requiredProjects) {
    $projectPath = Join-Path $projectRoot $project
    $projectFile = Get-ChildItem -Path $projectPath -Filter "*.csproj" | Select-Object -First 1
    
    if (-not $projectFile) {
        Write-Host "  [ERROR] Could not find .csproj file for $project" -ForegroundColor Red
        continue
    }
    
    Write-Host "`nChecking packages for $project..." -ForegroundColor Cyan
    $projectXml = [xml](Get-Content $projectFile.FullName)
    $installedPackages = $projectXml.Project.ItemGroup.PackageReference | Where-Object { $_.Include } | ForEach-Object { $_.Include }
    
    if ($requiredPackages.ContainsKey($project)) {
        $missingPackages = @()
        
        foreach ($requiredPackage in $requiredPackages[$project]) {
            if ($installedPackages -notcontains $requiredPackage) {
                $missingPackages += $requiredPackage
                Write-Host "  [MISSING] $requiredPackage" -ForegroundColor Yellow
            } else {
                Write-Host "  [FOUND]   $requiredPackage" -ForegroundColor Green
            }
        }
        
        if ($missingPackages.Count -gt 0) {
            Write-Host "`nMissing packages for $($project):" -ForegroundColor Yellow
            $missingPackages | ForEach-Object { Write-Host "  - $_" }
            Write-Host "`nTo install missing packages, run:" -ForegroundColor Cyan
            foreach ($pkg in $missingPackages) {
                Write-Host "  dotnet add $($projectFile.Directory.Name) package $pkg"
            }
        } else {
            Write-Host "  All required packages are installed." -ForegroundColor Green
        }
    } else {
        Write-Host "  No package requirements defined for this project." -ForegroundColor Gray
    }
}

Write-Host "`nVerification complete!" -ForegroundColor Cyan
Write-Host "Run 'dotnet restore' to restore all NuGet packages." -ForegroundColor Cyan
Write-Host "Run 'dotnet build' to build the solution." -ForegroundColor Cyan
