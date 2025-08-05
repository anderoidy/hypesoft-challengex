# Use a imagem oficial do Keycloak como base
FROM quay.io/keycloak/keycloak:24.0

# Instala dependências necessárias
USER root
RUN microdnf -y install jq curl

# Copia os arquivos de configuração
COPY realm-export.json /opt/keycloak/realm-export.json
COPY setup-keycloak.sh /opt/keycloak/setup-keycloak.sh

# Torna o script executável
RUN chmod +x /opt/keycloak/setup-keycloak.sh

# Expõe a porta do Keycloak
EXPOSE 8080

# Define o comando de inicialização
ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start", "--import-realm", "--hostname-strict=false"]

# Adiciona o script de configuração ao entrypoint
CMD ["start-dev"]
