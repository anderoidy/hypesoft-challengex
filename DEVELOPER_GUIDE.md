# Guia do Desenvolvedor - Hypesoft Challenge

Este guia fornece informações detalhadas sobre como configurar, executar e trabalhar com o ambiente de desenvolvimento do projeto Hypesoft Challenge.

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado em sua máquina:

- **Docker** (versão 20.10.0 ou superior)
- **Docker Compose** (versão 1.29.0 ou superior)
- **Git** (para controle de versão)
- **OpenSSL** (para geração de certificados)
- **Node.js** (versão 18 ou superior, para desenvolvimento frontend)
- **.NET 9 SDK** (para desenvolvimento backend)

## 🚀 Configuração Inicial

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/hypesoft-challengex.git
cd hypesoft-challengex
```

### 2. Configure as variáveis de ambiente

1. Faça uma cópia do arquivo `.env.example` para `.env`:
   ```bash
   cp .env.example .env
   ```

2. Edite o arquivo `.env` conforme necessário para o seu ambiente de desenvolvimento.

### 3. Gere os certificados SSL

Execute o script para gerar certificados autoassinados:

```bash
./scripts/generate-ssl-certs.sh
```

> **Nota:** No Windows, você pode precisar executar isso no Git Bash ou WSL.

## 🏃‍♂️ Executando o Ambiente

### Iniciando todos os serviços

Para iniciar todo o ambiente de desenvolvimento, execute:

```bash
./start-dev.sh
```

Este script irá:
1. Verificar as dependências
2. Gerar certificados SSL (se necessário)
3. Configurar os hosts locais
4. Iniciar todos os containers Docker

### Acessando os serviços

Após a inicialização, você poderá acessar:

- **Frontend**: https://localhost:3000
- **Backend API**: https://api.localhost/swagger
- **Keycloak Admin Console**: https://auth.localhost/admin
  - Usuário: `admin`
  - Senha: `admin` (ou a senha definida em `KEYCLOAK_ADMIN_PASSWORD`)
- **MongoDB Express**: http://mongo-express.localhost:8081
  - Usuário: `admin` (ou o valor de `MONGO_EXPRESS_USERNAME`)
  - Senha: `admin` (ou o valor de `MONGO_EXPRESS_PASSWORD`)

### Verificando o status dos serviços

Para verificar o status de todos os serviços em execução:

```bash
./status-dev.sh
```

### Parando o ambiente

Para parar todos os serviços e limpar os recursos:

```bash
./stop-dev.sh
```

## 🛠️ Desenvolvimento

### Estrutura do Projeto

```
hypesoft-challengex/
├── backend/              # Código-fonte do backend (.NET 9)
├── frontend/             # Código-fonte do frontend (Next.js 14)
├── nginx/                # Configurações do Nginx
│   ├── conf.d/           # Configurações de virtual hosts
│   └── nginx.conf        # Configuração principal do Nginx
├── keycloak/             # Configurações do Keycloak
│   ├── realm-export.json # Configuração do Realm
│   └── setup-keycloak.sh # Script de configuração
├── scripts/              # Scripts úteis
├── .env.example          # Exemplo de variáveis de ambiente
├── docker-compose.yml    # Configuração do Docker Compose
└── README.md             # Documentação principal
```

### Desenvolvimento Backend

O backend foi desenvolvido em .NET 9 seguindo os princípios da Clean Architecture e CQRS.

#### Estrutura do Backend

```
backend/
├── Hypesoft.Application/    # Camada de aplicação (CQRS, DTOs, Interfaces)
├── Hypesoft.Domain/         # Camada de domínio (Entidades, Interfaces)
├── Hypesoft.Infrastructure/ # Camada de infraestrutura (Repositórios, Banco de Dados)
└── Hypesoft.API/            # Camada de API (Controllers, Middlewares)
```

#### Executando o backend localmente

1. Navegue até o diretório do backend:
   ```bash
   cd backend
   ```

2. Restaure as dependências e execute o projeto:
   ```bash
   dotnet restore
   dotnet run --project Hypesoft.API
   ```

3. A API estará disponível em: https://localhost:5001

### Desenvolvimento Frontend

O frontend foi desenvolvido com Next.js 14, TypeScript, Tailwind CSS e Shadcn/ui.

#### Estrutura do Frontend

```
frontend/
├── public/          # Arquivos estáticos
├── src/
│   ├── app/         # Páginas e rotas (App Router)
│   ├── components/  # Componentes reutilizáveis
│   ├── lib/         # Utilitários e configurações
│   └── styles/      # Estilos globais
└── package.json     # Dependências e scripts
```

#### Executando o frontend localmente

1. Navegue até o diretório do frontend:
   ```bash
   cd frontend
   ```

2. Instale as dependências e execute o projeto:
   ```bash
   npm install
   npm run dev
   ```

3. O frontend estará disponível em: http://localhost:3000

## 🔒 Autenticação e Autorização

O projeto utiliza Keycloak para autenticação e autorização. O fluxo de autenticação é baseado em OpenID Connect (OIDC).

### Configuração do Keycloak

1. Acesse o Console de Administração do Keycloak em: https://auth.localhost/admin
2. Faça login com as credenciais de administrador
3. Navegue até o Realm "hypesoft"
4. Configure os clients, usuários e roles conforme necessário

### Integração com o Backend

O backend está configurado para validar tokens JWT emitidos pelo Keycloak. As configurações podem ser ajustadas no arquivo `appsettings.json` ou através de variáveis de ambiente.

## 📦 Implantação

### Construindo as imagens Docker

Para construir as imagens Docker para produção:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
```

### Implantando em produção

1. Configure as variáveis de ambiente de produção em `.env.production`
2. Execute o comando de implantação:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

## 🐛 Solução de Problemas

### Problemas comuns

1. **Erro de permissão nos scripts**
   ```bash
   chmod +x *.sh
   chmod +x scripts/*.sh
   ```

2. **Problemas com certificados SSL**
   - Certifique-se de que os certificados foram gerados corretamente
   - Adicione os certificados às autoridades de certificação confiáveis do seu sistema operacional

3. **Containers não iniciam**
   ```bash
   docker-compose logs <nome_do_serviço>
   ```

4. **Problemas de rede**
   - Verifique se as portas necessárias estão disponíveis
   - Verifique se os nomes de domínio estão corretamente mapeados no arquivo `/etc/hosts`

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Faça commit das suas alterações (`git commit -m 'Add some AmazingFeature'`)
4. Faça push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🙏 Agradecimentos

- Equipe Hypesoft pelo desafio
- Comunidade de código aberto pelas ferramentas incríveis

---

<p align="center">Desenvolvido com ❤️ para o Desafio Hypesoft</p>
