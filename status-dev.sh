#!/bin/bash

# Cores para saída
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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
}

# Função para exibir informações
info() {
  echo -e "${BLUE}[INFO]${NC} $1"
}

# Função para verificar a saúde de um serviço HTTP/HTTPS
check_http_service() {
  local name=$1
  local url=$2
  local method=${3:-GET}
  
  echo -n "Verificando $name ($url)... "
  
  # Usa curl para verificar o status HTTP
  response=$(curl -s -o /dev/null -w "%{http_code}" -X $method $url 2>/dev/null)
  
  if [ "$response" = "" ]; then
    echo -e "${RED}❌ Fora do ar${NC}"
    return 1
  elif [ $response -ge 200 ] && [ $response -lt 400 ]; then
    echo -e "${GREEN}✅ Online (HTTP $response)${NC}"
    return 0
  else
    echo -e "${YELLOW}⚠️  Com problemas (HTTP $response)${NC}"
    return 2
  fi
}

# Função para verificar um serviço Docker
check_docker_service() {
  local name=$1
  
  echo -n "Verificando $name... "
  
  # Verifica se o container está em execução
  container_id=$(docker ps -q -f "name=^${name}$")
  
  if [ -z "$container_id" ]; then
    echo -e "${RED}❌ Parado${NC}"
    return 1
  else
    # Verifica o status de saúde do container, se disponível
    health_status=$(docker inspect --format='{{.State.Health.Status}}' $name 2>/dev/null || echo "no healthcheck")
    
    if [ "$health_status" = "healthy" ]; then
      echo -e "${GREEN}✅ Em execução (saudável)${NC}"
      return 0
    elif [ "$health_status" = "no healthcheck" ]; then
      echo -e "${GREEN}✅ Em execução${NC}"
      return 0
    elif [ "$health_status" = "starting" ]; then
      echo -e "${YELLOW}🔄 Iniciando...${NC}"
      return 2
    else
      echo -e "${YELLOW}⚠️  Em execução (não saudável: $health_status)${NC}"
      return 2
    fi
  fi
}

# Função para verificar o uso de recursos do sistema
check_system_resources() {
  echo -e "\n${BLUE}📊 Uso de Recursos do Sistema:${NC}"
  
  # Uso de CPU
  cpu_usage=$(top -bn1 | grep "Cpu(s)" | awk '{print $2 + $4}' | awk -F. '{print $1}')
  echo -n "CPU: "
  if [ $cpu_usage -gt 90 ]; then
    echo -e "${RED}${cpu_usage}% (Alto)${NC}"
  elif [ $cpu_usage -gt 70 ]; then
    echo -e "${YELLOW}${cpu_usage}% (Médio)${NC}"
  else
    echo -e "${GREEN}${cpu_usage}% (Normal)${NC}"
  fi
  
  # Uso de memória
  total_mem=$(free -m | awk '/Mem:/ {print $2}')
  used_mem=$(free -m | awk '/Mem:/ {print $3}')
  mem_percent=$((used_mem * 100 / total_mem))
  
  echo -n "Memória: "
  if [ $mem_percent -gt 90 ]; then
    echo -e "${RED}${mem_percent}% (${used_mem}MB/${total_mem}MB - Crítico)${NC}"
  elif [ $mem_percent -gt 70 ]; then
    echo -e "${YELLOW}${mem_percent}% (${used_mem}MB/${total_mem}MB - Alto)${NC}"
  else
    echo -e "${GREEN}${mem_percent}% (${used_mem}MB/${total_mem}MB - Normal)${NC}"
  fi
  
  # Uso de disco
  disk_usage=$(df -h / | awk 'NR==2 {print $5}' | tr -d '%')
  echo -n "Disco (sistema de arquivos raiz): "
  if [ $disk_usage -gt 90 ]; then
    echo -e "${RED}${disk_usage}% (Crítico)${NC}"
  elif [ $disk_usage -gt 70 ]; then
    echo -e "${YELLOW}${disk_usage}% (Alto)${NC}"
  else
    echo -e "${GREEN}${disk_usage}% (Normal)${NC}"
  fi
}

# Função principal
main() {
  echo -e "\n${BLUE}🔄 Verificando o status do Ambiente de Desenvolvimento Hypesoft${NC}\n"
  
  # Verifica se o Docker está em execução
  if ! docker info &> /dev/null; then
    error "O Docker não está em execução. Por favor, inicie o Docker e tente novamente."
    exit 1
  fi
  
  # Verifica os serviços Docker
  echo -e "${BLUE}🐳 Status dos Containers Docker:${NC}"
  
  # Lista de serviços para verificar
  services=("mongodb" "postgres" "keycloak" "backend" "frontend" "nginx" "mongo-express")
  
  # Contadores para resumo
  total_services=${#services[@]}
  running_services=0
  warning_services=0
  error_services=0
  
  # Verifica cada serviço
  for service in "${services[@]}"; do
    if check_docker_service "$service"; then
      ((running_services++))
    elif [ $? -eq 2 ]; then
      ((warning_services++))
    else
      ((error_services++))
    fi
  done
  
  # Verifica os serviços HTTP/HTTPS
  echo -e "\n${BLUE}🌐 Status dos Serviços Web:${NC}"
  
  # Lista de serviços web para verificar
  declare -A web_services=(
    ["Frontend"]="https://localhost:3000"
    ["Backend API"]="https://api.localhost/health"
    ["Keycloak Admin"]="https://auth.localhost/realms/master"
    ["Mongo Express"]="http://mongo-express.localhost:8081"
  )
  
  # Verifica cada serviço web
  for service in "${!web_services[@]}"; do
    check_http_service "$service" "${web_services[$service]}"
    status_code=$?
    
    if [ $status_code -eq 0 ]; then
      ((running_services++))
    elif [ $status_code -eq 2 ]; then
      ((warning_services++))
    else
      ((error_services++))
    fi
  done
  
  # Exibe o resumo
  echo -e "\n${BLUE}📊 Resumo do Ambiente:${NC}"
  echo -e "${GREEN}✅ $running_services serviços em execução${NC}"
  
  if [ $warning_services -gt 0 ]; then
    echo -e "${YELLOW}⚠️  $warning_services serviços com avisos${NC}"
  fi
  
  if [ $error_services -gt 0 ]; then
    echo -e "${RED}❌ $error_services serviços com erros${NC}"
  fi
  
  # Verifica recursos do sistema
  check_system_resources
  
  # Dicas de solução de problemas
  if [ $error_services -gt 0 ] || [ $warning_services -gt 0 ]; then
    echo -e "\n${YELLOW}🔧 Dicas para Solução de Problemas:${NC}"
    
    if [ $error_services -gt 0 ]; then
      echo "- Verifique os logs dos serviços com problemas: docker logs <nome_do_container>"
      echo "- Verifique se todos os serviços estão em execução: docker ps -a"
    fi
    
    if [ $warning_services -gt 0 ]; then
      echo "- Alguns serviços podem estar iniciando. Aguarde alguns instantes e execute este script novamente."
    fi
    
    echo "- Para reiniciar o ambiente: ./start-dev.sh"
    echo "- Para parar o ambiente: ./stop-dev.sh"
  fi
  
  echo -e "\n${GREEN}✅ Verificação concluída em $(date)${NC}"
}

# Executa a função principal
main "@"
