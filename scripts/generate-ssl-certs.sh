#!/bin/bash

# Cria o diretório para os certificados, se não existir
mkdir -p ../nginx/certs

# Gera uma chave privada RSA de 2048 bits
echo "Gerando chave privada..."
openssl genrsa -out ../nginx/certs/localhost.key 2048

# Gera um certificado autoassinado (CSR)
echo "Gerando certificado autoassinado..."
openssl req -new -x509 \
  -key ../nginx/certs/localhost.key \
  -out ../nginx/certs/localhost.crt \
  -days 3650 \
  -subj "/C=BR/ST=Sao_Paulo/L=Sao_Paulo/O=Hypesoft/OU=Development/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,DNS:*.localhost,IP:127.0.0.1"

# Gera um arquivo .pem para o Keycloak (combinação de chave e certificado)
cat ../nginx/certs/localhost.key ../nginx/certs/localhost.crt > ../nginx/certs/localhost.pem

# Define permissões adequadas
chmod 600 ../nginx/certs/*

echo "Certificados gerados com sucesso em: $(pwd)/../nginx/certs/"
echo "- localhost.key: Chave privada"
echo "- localhost.crt: Certificado público"
echo "- localhost.pem: Chave e certificado combinados (para Keycloak)"

echo "\nLembre-se de adicionar o certificado localhost.crt às suas autoridades de certificação confiáveis!"
echo "No Windows, clique com o botão direito no arquivo e selecione 'Instalar Certificado'."
echo "No Linux, copie para /usr/local/share/ca-certificates/ e execute 'sudo update-ca-certificates'."
echo "No macOS, use o Keychain Access para importar o certificado e marcá-lo como confiável."
