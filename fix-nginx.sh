#!/bin/bash

echo "Parando containers existentes..."
docker-compose down

echo "Limpando containers e redes não utilizados..."
docker system prune -f

echo "Reiniciando serviços..."
docker-compose up -d

echo "Aguardando serviços iniciarem..."
sleep 10

echo "Verificando status dos containers..."
docker ps

echo ""
echo "Verificando logs do nginx..."
docker logs nginx

echo ""
echo "O nginx deve estar funcionando agora!"
echo "Acesse:"
echo "- Frontend: http://localhost"
echo "- Backend API: http://api.localhost"
echo "- Swagger: http://api.localhost/swagger/"
