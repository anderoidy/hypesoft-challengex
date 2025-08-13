# Hypesoft Challenge - Frontend

Este é o frontend do desafio técnico da Hypesoft, desenvolvido com Next.js 14, TypeScript, TailwindCSS e Shadcn/ui.

## 🚀 Tecnologias

- [Next.js 14](https://nextjs.org/) - Framework React
- [TypeScript](https://www.typescriptlang.org/) - Tipagem estática
- [TailwindCSS](https://tailwindcss.com/) - Estilização
- [Shadcn/ui](https://ui.shadcn.com/) - Componentes UI
- [React Query](https://tanstack.com/query/latest) - Gerenciamento de estado e cache
- [React Hook Form](https://react-hook-form.com/) - Formulários
- [Zod](https://zod.dev/) - Validação de esquemas
- [Recharts](https://recharts.org/) - Gráficos e visualizações
- [Vitest](https://vitest.dev/) - Testes unitários
- [ESLint](https://eslint.org/) - Linter
- [Prettier](https://prettier.io/) - Formatador de código

## 📦 Pré-requisitos

- Node.js 18.17.1 ou superior
- npm 9+ ou yarn 1.22+
- Git

## 🛠️ Configuração do Ambiente

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/seu-usuario/hypesoft-challengex.git
   cd hypesoft-challengex/frontend
   ```

2. **Instalar dependências**
   ```bash
   npm install
   # ou
   yarn
   ```

3. **Configurar variáveis de ambiente**
   ```bash
   cp .env.example .env.local
   ```
   Edite o arquivo `.env.local` com as configurações necessárias.

## 🚀 Executando o Projeto

- **Desenvolvimento**
  ```bash
  npm run dev
  # ou
  yarn dev
  ```
  Acesse [http://localhost:3000](http://localhost:3000) no navegador.

- **Build de produção**
  ```bash
  npm run build
  # ou
  yarn build
  ```

- **Iniciar servidor de produção**
  ```bash
  npm start
  # ou
  yarn start
  ```

## 🧪 Testes

- **Executar todos os testes**
  ```bash
  npm test
  # ou
  yarn test
  ```

- **Executar testes em modo watch**
  ```bash
  npm run test:watch
  # ou
  yarn test:watch
  ```

- **Gerar cobertura de testes**
  ```bash
  npm run test:coverage
  # ou
  yarn test:coverage
  ```

## 🛠️ Ferramentas Adicionais

- **Linter**
  ```bash
  npm run lint
  # ou
  yarn lint
  ```

- **Formatar código**
  ```bash
  npm run format
  # ou
  yarn format
  ```

- **Verificação de tipos**
  ```bash
  npm run typecheck
  # ou
  yarn typecheck
  ```

## 📂 Estrutura de Diretórios

```
src/
├── app/                  # Rotas da aplicação (App Router)
├── components/           # Componentes reutilizáveis
│   ├── ui/               # Componentes UI do Shadcn
│   └── ...
├── features/             # Funcionalidades da aplicação
├── hooks/                # Custom hooks
├── lib/                  # Utilitários e configurações
├── schemas/              # Schemas de validação (Zod)
├── services/             # Serviços de API
├── store/                # Gerenciamento de estado (Zustand/Context)
├── styles/               # Estilos globais
└── utils/                # Funções utilitárias
```

## 🎨 Design System

O projeto utiliza o Shadcn/ui como base para componentes, que são estilizados com TailwindCSS. As cores e temas podem ser personalizados no arquivo `tailwind.config.js`.

### Cores Principais

- **Primária**: `hsl(222.2 47.4% 11.2%)`
- **Secundária**: `hsl(210 40% 96.1%)`
- **Destaque**: `hsl(217.2 91.2% 59.8%)`

### Tipografia

- **Fonte Principal**: Inter
- **Fonte Monoespaçada**: Roboto Mono

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas alterações (`git commit -m 'Add some AmazingFeature'`)
4. Faça o push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

Desenvolvido com 💜 por Anderson Pereira 

Status do Projeto
Este projeto foi desenvolvido como parte de um desafio técnico para uma vaga de desenvolvedor.

Até o momento, consegui implementar:

Estrutura inicial do frontend 

Telas principais carregando corretamente no ambiente local.

Configuração parcial de rotas e componentes no frontend.

O que ficou pendente:

Correção de erros de build no backend (.NET).

Configuração da conexão entre frontend e backend.

Implementação e testes completos das operações de CRUD.

Observações:

O backend ainda não compila devido a erros de configuração e dependências.

O frontend funciona parcialmente, exibindo as telas, mas as funcionalidades de integração com a API ainda não estão operacionais.


