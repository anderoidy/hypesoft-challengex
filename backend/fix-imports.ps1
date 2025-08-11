

# Script para corrigir todos os imports dos testes

Write-Host "🔧 Iniciando correção dos imports dos testes..." -ForegroundColor Green

# 1. Corrigir Testes Unitários - Adicionar import do IApplicationUnitOfWork
$unitTestFiles = @(
    "tests\Hypesoft.UnitTests\Handlers\Commands\CreateProductCommandHandlerTests.cs",
    "tests\Hypesoft.UnitTests\Handlers\Commands\DeleteProductCommandHandlerTests.cs", 
    "tests\Hypesoft.UnitTests\Handlers\Commands\UpdateProductCommandHandlerTests.cs",
    "tests\Hypesoft.UnitTests\Handlers\Queries\GetAllProductsQueryHandlerTests.cs",
    "tests\Hypesoft.UnitTests\Handlers\Queries\GetProductByIdQueryHandlerTests.cs"
)

foreach ($file in $unitTestFiles) {
    if (Test-Path $file) {
        Write-Host "📝 Corrigindo: $file" -ForegroundColor Yellow
        
        $content = Get-Content $file
        $newContent = @()
        $importAdded = $false
        
        foreach ($line in $content) {
            $newContent += $line
            
            # Adicionar o import após os outros using statements e antes do namespace
            if ($line.StartsWith("using ") -and !$importAdded) {
                # Verifica se é a última linha de using antes do namespace
                $nextLineIndex = $content.IndexOf($line) + 1
                if ($nextLineIndex -lt $content.Length) {
                    $nextLine = $content[$nextLineIndex]
                    # Se a próxima linha não é using ou é vazia, adiciona nosso import
                    if (!$nextLine.StartsWith("using ") -or $nextLine.Trim() -eq "") {
                        $newContent += "using Hypesoft.Application.Common.Interfaces;"
                        $importAdded = $true
                        Write-Host "  ✅ Import adicionado" -ForegroundColor Green
                    }
                }
            }
        }
        
        $newContent | Set-Content $file -Encoding UTF8
    } else {
        Write-Host "  ❌ Arquivo não encontrado: $file" -ForegroundColor Red
    }
}

# 2. Corrigir Testes de Integração - Substituir imports antigos
$integrationTestFiles = @(
    "tests\Hypesoft.IntegrationTests\Controllers\ProductsControllerTests.cs",
    "tests\Hypesoft.IntegrationTests\TestBase.cs"
)

foreach ($file in $integrationTestFiles) {
    if (Test-Path $file) {
        Write-Host "📝 Corrigindo: $file" -ForegroundColor Yellow
        
        $content = Get-Content $file
        $newContent = @()
        $persistenceImportAdded = $false
        
        foreach ($line in $content) {
            # Substituir imports antigos
            if ($line -eq "using Hypesoft.Infrastructure.Data;") {
                if (!$persistenceImportAdded) {
                    $newContent += "using Hypesoft.Infrastructure.Persistence;"
                    $persistenceImportAdded = $true
                    Write-Host "  ✅ Import Data substituído por Persistence" -ForegroundColor Green
                }
                continue # Pula a linha original
            }
            elseif ($line -eq "using Hypesoft.Infrastructure.Identity;") {
                # Remove esta linha completamente (já temos Persistence)
                Write-Host "  ✅ Import Identity removido" -ForegroundColor Green
                continue # Pula a linha original
            }
            else {
                $newContent += $line
            }
        }
        
        $newContent | Set-Content $file -Encoding UTF8
    } else {
        Write-Host "  ❌ Arquivo não encontrado: $file" -ForegroundColor Red
    }
}

# 3. Corrigir a classe Program para ser pública (para testes de integração)
$programFile = "src\Hypesoft.API\Program.cs"
if (Test-Path $programFile) {
    Write-Host "📝 Corrigindo visibilidade da classe Program..." -ForegroundColor Yellow
    
    $content = Get-Content $programFile
    $newContent = @()
    
    foreach ($line in $content) {
        if ($line.Trim() -eq "internal class Program" -or $line.Trim() -eq "class Program") {
            $newContent += $line.Replace("internal class Program", "public class Program").Replace("class Program", "public class Program")
            Write-Host "  ✅ Classe Program agora é pública" -ForegroundColor Green
        } else {
            $newContent += $line
        }
    }
    
    $newContent | Set-Content $programFile -Encoding UTF8
} else {
    Write-Host "  ❌ Arquivo Program.cs não encontrado" -ForegroundColor Red
}

Write-Host "`n🎉 Correções concluídas!" -ForegroundColor Green
Write-Host "Execute agora: dotnet clean && dotnet build" -ForegroundColor Cyan