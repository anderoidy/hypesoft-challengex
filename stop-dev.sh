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

# Função para exibir mensagens de erro
error() {
  echo -e "${RED}[ERRO]${NC} $1"
  exit 1
}

# Função para parar os containers Docker
stop_containers() {
  echo "Parando os containers Docker..."
  
  # Verifica se o docker-compose ou docker compose está disponível
  if command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
  else
    DOCKER_COMPOSE_CMD="docker compose"
  fi
  
  # Para e remove os containers
  $DOCKER_COMPOSE_CMD down --volumes --remove-orphans
  
  if [ $? -eq 0 ]; then
    success "Containers Docker parados e removidos com sucesso!"
  else
    error "Falha ao parar os containers Docker."
  fi
}

# Função para limpar recursos não utilizados
cleanup() {
  echo "Limpando recursos não utilizados..."
  
  # Remove containers parados
  docker container prune -f
  
  # Remove redes não utilizadas
  docker network prune -f
  
  # Remove volumes não utilizados (exceto os volumes nomeados)
  # docker volume prune -f
  
  # Remove imagens intermediárias
  docker image prune -f
  
  success "Limpeza concluída!"
}

# Função para remover entradas dos hosts
# ATENÇÃO: Isso removerá todas as entradas que correspondam ao padrão
remove_hosts() {
  echo "Removendo entradas dos hosts..."
  
  # Lista de hosts para remover
  HOSTS=("api.localhost"
         "auth.localhost"
         "mongo-express.localhost")
  
  # Verifica o sistema operacional
  if [[ "$OSTYPE" == "darwin"* ]] || [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # macOS ou Linux
    for host in "${HOSTS[@]}"; do
      if grep -q "$host" /etc/hosts; then
        echo "Removendo $host do /etc/hosts..."
        sudo sed -i "/$host/d" /etc/hosts
      fi
    done
  elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    # Windows (Git Bash, Cygwin, etc.)
    for host in "${HOSTS[@]}"; do
      if grep -q "$host" /c/Windows/System32/drivers/etc/hosts; then
        echo "Removendo $host do arquivo de hosts..."
        # Cria um backup antes de modificar
        sudo cp /c/Windows/System32/drivers/etc/hosts /c/Windows/System32/drivers/etc/hosts.bak
        # Remove a linha contendo o host
        sudo sed -i "/$host/d" /c/Windows/System32/drivers/etc/hosts
      fi
    done
  else
    warning "Sistema operacional não suportado para remoção automática de hosts."
    echo "Por favor, remova manualmente as seguintes entradas do seu arquivo de hosts:"
    for host in "${HOSTS[@]}"; do
      echo "- 127.0.0.1 $host"
    done
  fi
  
  success "Entradas de hosts removidas com sucesso!"
}

# Função principal
main() {
  echo -e "\n${YELLOW}🛑 Parando o Ambiente de Desenvolvimento Hypesoft${NC}\n"
  
  # Para os containers
  stop_containers
  
  # Pergunta se deseja remover os volumes (dados)
  read -p "Deseja remover todos os volumes? Isso apagará todos os dados do banco de dados! [s/N] " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Ss]$ ]]; then
    echo "Removendo volumes..."
    docker volume prune -f
  else
    echo "Os volumes foram mantidos. Os dados não foram perdidos."
  fi
  
  # Pergunta se deseja limpar recursos não utilizados
  read -p "Deseja limpar recursos não utilizados (containers, redes, imagens)? [s/N] " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Ss]$ ]]; then
    cleanup
  fi
  
  # Pergunta se deseja remover as entradas dos hosts
  read -p "Deseja remover as entradas dos hosts? [s/N] " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Ss]$ ]]; then
    remove_hosts
  fi
  
  echo -e "\n${GREEN}✅ Ambiente de desenvolvimento parado com sucesso!${NC}"
  echo -e "Para reiniciar o ambiente, execute: ${YELLOW}./start-dev.sh${NC}\n"
}

# Executa a função principal
main "$@"
