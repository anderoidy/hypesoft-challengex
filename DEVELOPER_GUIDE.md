
# Hypesoft Challenge X – Instruções para Execução

Este projeto foi desenvolvido como parte de um desafio técnico para vaga de desenvolvedor.
Ele é composto por **backend (.NET 7 + MongoDB)** e **frontend (Next.js 14 + TypeScript)**.

---

## 📌 Pré-requisitos

Antes de começar, instale:

* **Backend**

  * [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
  * [MongoDB 6+](https://www.mongodb.com/try/download/community)
  * [Git](https://git-scm.com/)

* **Frontend**

  * [Node.js 18+](https://nodejs.org/en/)
  * npm 9+ ou yarn 1.22+

---

## 🚀 Passo a Passo para Rodar o Projeto

### 1️⃣ Clonar o Repositório

```bash
git clone https://github.com/anderoidy/hypesoft-challengex.git
cd hypesoft-challengex
```

---

### 2️⃣ Configurar e Rodar o Backend

> ⚠ Observação: O backend ainda possui erros de build e não está totalmente funcional.
> Mesmo assim, seguem os passos de execução previstos.

1. Vá até a pasta da API:

   ```bash
   cd backend/src/Hypesoft.API
   ```

2. Crie o arquivo `appsettings.Development.json` com o conteúdo:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "Microsoft": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     },
     "MongoDBSettings": {
       "DatabaseName": "HypesoftDB_Dev",
       "ConnectionString": "mongodb://localhost:27017"
     },
     "JwtSettings": {
       "Secret": "CHAVE_SECRETA_AQUI_COM_PELO_MENOS_32_CARACTERES",
       "Issuer": "Hypesoft.API",
       "Audience": "Hypesoft.Clients",
       "ExpirationInMinutes": 1440
     },
     "DetailedErrors": true,
     "UseInMemoryDatabase": true
   }
   ```

3. Para iniciar (quando corrigido):

   ```bash
   dotnet run
   ```

A API estará disponível em:

* [http://localhost:5000](http://localhost:5000)
* [https://localhost:5001](https://localhost:5001)

Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

### 3️⃣ Configurar e Rodar o Frontend

1. Abra outro terminal e vá para a pasta do frontend:

   ```bash
   cd frontend
   ```

2. Instale as dependências:

   ```bash
   npm install
   # ou
   yarn
   ```

3. Copie o arquivo `.env.example` para `.env.local`:

   ```bash
   cp .env.example .env.local
   ```

   No `.env.local`, configure:

   ```
   NEXT_PUBLIC_API_URL=http://localhost:5000
   ```

4. Inicie o servidor de desenvolvimento:

   ```bash
   npm run dev
   # ou
   yarn dev
   ```

O frontend estará disponível em:
[http://localhost:3000](http://localhost:3000)

---

## 📂 Estrutura do Projeto

```
hypesoft-challengex/
├── backend/
│   ├── src/Hypesoft.API/         # API .NET
│   ├── Hypesoft.Application/     # Camada de aplicação
│   ├── Hypesoft.Domain/          # Modelos de domínio
│   └── Hypesoft.Infrastructure/  # Infraestrutura e repositórios
├── frontend/                     # Frontend Next.js
│   ├── src/
│   ├── components/
│   ├── features/
│   └── styles/
└── README.md                     # Este arquivo
```

---

## 📌 Status do Projeto

* ✅ Estrutura do frontend e telas principais carregando localmente.
* ✅ Configuração parcial de rotas e componentes no frontend.
* ❌ CRUD não implementado.
* ❌ Backend com erros de build e conexão ainda não finalizada.

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---


