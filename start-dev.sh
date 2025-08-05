#!/bin/bash

# Cores para saída
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para exibir mensagens de sucesso
success() {
  echo -e "${GREEN}[SUCESSO]${NC} $1"
}

# Função para exibir mensagens de aviso
warning() {
  echo -e "${YELLOW}[AVISO]${NC} $1"
}

# Função para exibir mensagens de erro e sair
error() {
  echo -e "${RED}[ERRO]${NC} $1"
  exit 1
}

# Verifica se o Docker está instalado e em execução
check_docker() {
  echo "Verificando se o Docker está instalado e em execução..."
  if ! command -v docker &> /dev/null; then
    error "Docker não encontrado. Por favor, instale o Docker e tente novamente."
  fi

  if ! docker info &> /dev/null; then
    error "Docker não está em execução. Por favor, inicie o Docker e tente novamente."
  fi
  success "Docker está instalado e em execução."
}

# Verifica se o Docker Compose está instalado
check_docker_compose() {
  echo "Verificando se o Docker Compose está instalado..."
  if ! command -v docker-compose &> /dev/null && ! command -v docker compose &> /dev/null; then
    error "Docker Compose não encontrado. Por favor, instale o Docker Compose e tente novamente."
  fi
  success "Docker Compose está instalado."
}

# Verifica se o OpenSSL está instalado
check_openssl() {
  echo "Verificando se o OpenSSL está instalado..."
  if ! command -v openssl &> /dev/null; then
    error "OpenSSL não encontrado. Por favor, instale o OpenSSL e tente novamente."
  fi
  success "OpenSSL está instalado."
}

# Gera certificados SSL autoassinados
generate_ssl_certs() {
  echo "Verificando certificados SSL..."
  CERTS_DIR="./nginx/certs"
  
  if [ ! -f "$CERTS_DIR/localhost.crt" ] || [ ! -f "$CERTS_DIR/localhost.key" ]; then
    warning "Certificados SSL não encontrados. Gerando novos certificados autoassinados..."
    
    # Cria o diretório se não existir
    mkdir -p "$CERTS_DIR"
    
    # Gera a chave privada
    openssl genrsa -out "$CERTS_DIR/localhost.key" 2048
    
    # Gera o certificado autoassinado
    openssl req -new -x509 \
      -key "$CERTS_DIR/localhost.key" \
      -out "$CERTS_DIR/localhost.crt" \
      -days 3650 \
      -subj "/C=BR/ST=Sao_Paulo/L=Sao_Paulo/O=Hypesoft/OU=Development/CN=localhost" \
      -addext "subjectAltName=DNS:localhost,DNS:*.localhost,IP:127.0.0.1"
    
    # Cria o arquivo .pem para o Keycloak
    cat "$CERTS_DIR/localhost.key" "$CERTS_DIR/localhost.crt" > "$CERTS_DIR/localhost.pem"
    
    # Define permissões adequadas
    chmod 600 "$CERTS_DIR/"*
    
    success "Certificados SSL gerados com sucesso em $CERTS_DIR/"
  else
    success "Certificados SSL já existem."
  fi
}

# Configura os hosts no arquivo /etc/hosts
setup_hosts() {
  echo "Configurando hosts locais..."
  HOSTS=("127.0.0.1 localhost"
         "127.0.0.1 api.localhost"
         "127.0.0.1 auth.localhost"
         "127.0.0.1 mongo-express.localhost")
  
  for host in "${HOSTS[@]}"; do
    if ! grep -q "^$host" /etc/hosts; then
      echo "Adicionando $host ao /etc/hosts..."
      echo "$host" | sudo tee -a /etc/hosts > /dev/null
    fi
  done
  success "Hosts configurados com sucesso."
}

# Inicia os containers Docker
start_containers() {
  echo "Iniciando os containers Docker..."
  
  # Verifica se o docker-compose ou docker compose está disponível
  if command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
  else
    DOCKER_COMPOSE_CMD="docker compose"
  fi
  
  # Constrói e inicia os containers
  $DOCKER_COMPOSE_CMD up -d --build
  
  if [ $? -eq 0 ]; then
    success "Containers Docker iniciados com sucesso!"
    echo -e "\n${GREEN}✅ Ambiente de desenvolvimento pronto!${NC}"
    echo -e "\nAcesse as seguintes URLs:"
    echo -e "- Frontend:      ${GREEN}https://localhost:3000${NC}"
    echo -e "- Backend API:   ${GREEN}https://api.localhost/swagger${NC}"
    echo -e "- Keycloak Admin:${GREEN} https://auth.localhost/admin${NC} (usuário: admin, senha: admin)"
    echo -e "- MongoDB Express:${GREEN} http://mongo-express.localhost:8081${NC} (usuário: admin, senha: admin)"
    echo -e "\nPara parar o ambiente, execute: ${YELLOW}docker-compose down${NC}"
  else
    error "Falha ao iniciar os containers Docker."
  fi
}

# Função principal
main() {
  echo -e "\n${GREEN}🛠️  Configuração do Ambiente de Desenvolvimento Hypesoft${NC}\n"
  
  # Verifica as dependências
  check_docker
  check_docker_compose
  check_openssl
  
  # Configura o ambiente
  generate_ssl_certs
  setup_hosts
  
  # Inicia os containers
  start_containers
}

# Executa a função principal
main "$@"
