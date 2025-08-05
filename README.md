# Hypesoft Challenge - Sistema de Gestão de Produtos

Bem-vindo ao desafio técnico da Hypesoft! Este projeto consiste no desenvolvimento de um sistema completo de gestão de produtos, demonstrando habilidades em arquitetura moderna, boas práticas de desenvolvimento e tecnologias de ponta.

## 🚀 Tecnologias

### Backend
- .NET 9 com C#
- Clean Architecture + DDD (Domain-Driven Design)
- CQRS + MediatR pattern
- Entity Framework Core com MongoDB
- Keycloak para autenticação/autorização
- Swagger/OpenAPI para documentação

### Frontend
- Next.js 14 (App Router)
- TypeScript
- TailwindCSS + Shadcn/ui
- React Query para gerenciamento de estado
- React Hook Form + Zod para validação
- Recharts para gráficos

### Infraestrutura
- Docker + Docker Compose
- MongoDB como banco de dados
- Keycloak para autenticação
- Nginx como reverse proxy

## 📋 Pré-requisitos

- Docker Desktop 4.0+ (com WSL2 ativado no Windows)
- .NET 9 SDK (para desenvolvimento local)
- Node.js 18+ (para desenvolvimento frontend)
- Git

## 🛠️ Configuração do Ambiente

1. **Clone o repositório**
   ```bash
   git clone https://github.com/seu-usuario/hypesoft-challengex.git
   cd hypesoft-challengex
   ```

2. **Configure as variáveis de ambiente**
   ```bash
   cp .env.example .env
   ```
   Edite o arquivo `.env` conforme necessário.

3. **Inicie os containers**
   ```bash
   docker-compose up -d
   ```
   Isso irá iniciar todos os serviços em containers Docker:
   - Backend API (http://localhost:5000)
   - Frontend (http://localhost:3000)
   - MongoDB (porta 27017)
   - MongoDB Express (http://localhost:8081)
   - Keycloak (http://localhost:8080)
   - Nginx (http://localhost:80, https://localhost:443)

## 🔧 Desenvolvimento

### Backend

1. Acesse o diretório do backend:
   ```bash
   cd backend
   ```

2. Restaure os pacotes e execute o projeto:
   ```bash
   dotnet restore
   dotnet run --project Hypesoft.API
   ```

3. Acesse a documentação da API em http://localhost:5000/swagger

### Frontend

1. Acesse o diretório do frontend:
   ```bash
   cd frontend
   ```

2. Instale as dependências e inicie o servidor de desenvolvimento:
   ```bash
   npm install
   npm run dev
   ```

3. Acesse a aplicação em http://localhost:3000

## 🔐 Configuração do Keycloak

1. Acesse o console do Keycloak em http://localhost:8080
2. Faça login com as credenciais:
   - Usuário: `admin`
   - Senha: `admin` (ou a senha definida no .env)
3. Came um novo Realm chamado `hypesoft`
4. Crie dois clients:
   - `hypesoft-frontend` (público)
   - `hypesoft-backend` (confidencial)
5. Configure os redirect URIs apropriados

## 🧪 Testes

### Backend
```bash
cd backend
dotnet test
```

### Frontend
```bash
cd frontend
npm test
```

## 📦 Build para Produção

```bash
# Build dos containers
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# Iniciar em produção
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## ✨ Agradecimentos

- Equipe Hypesoft pelo desafio técnico
- Comunidade de código aberto por todas as tecnologias incríveis utilizadas
