Write-Host "Parando containers existentes..." -ForegroundColor Green
docker-compose down

Write-Host "Limpando containers e redes não utilizados..." -ForegroundColor Green
docker system prune -f

Write-Host "Reiniciando serviços..." -ForegroundColor Green
docker-compose up -d

Write-Host "Aguardando serviços iniciarem..." -ForegroundColor Green
Start-Sleep -Seconds 10

Write-Host "Verificando status dos containers..." -ForegroundColor Green
docker ps

Write-Host ""
Write-Host "Verificando logs do nginx..." -ForegroundColor Green
docker logs nginx

Write-Host ""
Write-Host "O nginx deve estar funcionando agora!" -ForegroundColor Green
Write-Host "Acesse:" -ForegroundColor Yellow
Write-Host "- Frontend: http://localhost" -ForegroundColor Yellow
Write-Host "- Backend API: http://api.localhost" -ForegroundColor Yellow
Write-Host "- Swagger: http://api.localhost/swagger/" -ForegroundColor Yellow
