# Backup Script for Hypesoft Challenge Project
# Run this script to create a backup of important project files

# Configuration
$backupRoot = "$PSScriptRoot\backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$sourceRoot = "$PSScriptRoot\backend\src"

# Create backup directories
$backupDirs = @(
    "$backupRoot\Hypesoft.API",
    "$backupRoot\Hypesoft.Application",
    "$backupRoot\Hypesoft.Domain",
    "$backupRoot\Hypesoft.Infrastructure",
    "$backupRoot\Hypesoft.Tests"
)

foreach ($dir in $backupDirs) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

# Backup solution and project files
Copy-Item -Path "$sourceRoot\*.sln" -Destination $backupRoot -Force

# Backup API project
Copy-Item -Path "$sourceRoot\Hypesoft.API\**\*" -Destination "$backupRoot\Hypesoft.API\" -Recurse -Force

# Backup Application project
$appFiles = @("Commands", "Queries", "Handlers", "DTOs", "Interfaces", "Services", "Common")
foreach ($dir in $appFiles) {
    $source = "$sourceRoot\Hypesoft.Application\$dir"
    $dest = "$backupRoot\Hypesoft.Application\$dir"
    if (Test-Path $source) {
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
        Copy-Item -Path "$source\*" -Destination $dest -Recurse -Force
    }
}

# Backup Domain project
$domainFiles = @("Entities", "Enums", "Events", "Exceptions", "Interfaces", "Common", "Specifications")
foreach ($dir in $domainFiles) {
    $source = "$sourceRoot\Hypesoft.Domain\$dir"
    $dest = "$backupRoot\Hypesoft.Domain\$dir"
    if (Test-Path $source) {
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
        Copy-Item -Path "$source\*" -Destination $dest -Recurse -Force
    }
}

# Backup Infrastructure project
$infraFiles = @("Data", "Persistence", "Services", "Configurations")
foreach ($dir in $infraFiles) {
    $source = "$sourceRoot\Hypesoft.Infrastructure\$dir"
    $dest = "$backupRoot\Hypesoft.Infrastructure\$dir"
    if (Test-Path $source) {
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
        Copy-Item -Path "$source\*" -Destination $dest -Recurse -Force
    }
}

# Backup Test projects
if (Test-Path "$sourceRoot\Hypesoft.Tests") {
    Copy-Item -Path "$sourceRoot\Hypesoft.Tests\**\*" -Destination "$backupRoot\Hypesoft.Tests\" -Recurse -Force
}

# Create a manifest file with backup information
$backupInfo = @{
    BackupDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    SourceDirectory = $sourceRoot
    BackupLocation = $backupRoot
    FilesBackedUp = (Get-ChildItem -Path $backupRoot -Recurse -File).Count
}

$backupInfo | ConvertTo-Json -Depth 3 | Out-File "$backupRoot\backup_manifest.json"

Write-Host "Backup completed successfully to: $backupRoot" -ForegroundColor Green
Write-Host "Total files backed up: $($backupInfo.FilesBackedUp)" -ForegroundColor Green
