#!/bin/bash

# Inicia o Keycloak em segundo plano
echo "Iniciando o Keycloak..."
/opt/keycloak/bin/kc.sh start-dev --import-realm --hostname-strict=false &

# Armazena o PID do processo do Keycloak
KEYCLOAK_PID=$!

# Executa o script de configuração
/opt/keycloak/setup-keycloak.sh

# Mantém o contêiner em execução
echo "Keycloak está rodando..."
wait $KEYCLOAK_PID
