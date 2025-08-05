#!/bin/bash

# Aguarda o Keycloak ficar disponível
echo "Aguardando o Keycloak ficar disponível..."
until curl -f -s "http://localhost:8080/auth/realms/master" >/dev/null 2>&1; do
  echo "Keycloak não está disponível ainda. Aguardando..."
  sleep 5
done

echo "Keycloak está disponível. Iniciando configuração..."

# Obtém o token de acesso do usuário admin
echo "Obtendo token de acesso do admin..."
ACCESS_TOKEN=$(curl -s -d "client_id=admin-cli" -d "username=$KEYCLOAK_ADMIN" -d "password=$KEYCLOAK_ADMIN_PASSWORD" -d "grant_type=password" "http://localhost:8080/auth/realms/master/protocol/openid-connect/token" | jq -r '.access_token')

if [ -z "$ACCESS_TOKEN" ] || [ "$ACCESS_TOKEN" = "null" ]; then
  echo "Falha ao obter token de acesso. Verifique as credenciais do admin."
  exit 1
fi

# Verifica se o realm já existe
echo "Verificando se o realm 'hypesoft' já existe..."
REALM_EXISTS=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $ACCESS_TOKEN" "http://localhost:8080/auth/admin/realms/hypesoft")

if [ "$REALM_EXISTS" -eq 200 ]; then
  echo "O realm 'hypesoft' já existe. Atualizando..."
  # Atualiza o realm existente
  curl -X PUT -H "Content-Type: application/json" -H "Authorization: Bearer $ACCESS_TOKEN" -d @/opt/keycloak/realm-export.json "http://localhost:8080/auth/admin/realms/hypesoft"
  echo "Realm 'hypesoft' atualizado com sucesso!"
else
  echo "Criando novo realm 'hypesoft'..."
  # Cria um novo realm
  curl -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $ACCESS_TOKEN" -d @/opt/keycloak/realm-export.json "http://localhost:8080/auth/admin/realms"
  echo "Realm 'hypesoft' criado com sucesso!"
fi

echo "Configuração do Keycloak concluída!"
