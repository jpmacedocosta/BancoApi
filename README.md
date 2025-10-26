# Banco API - Sistema Caixa de Banco

Uma API RESTful para sistema bancário desenvolvida em .NET 8 com Clean Architecture, utilizando PostgreSQL como banco de dados e testes automatizados.

## 🏗️ Arquitetura

O projeto segue os princípios da Clean Architecture:

- **Domain**: Entidades e interfaces de negócio
- **Application**: Serviços, DTOs e regras de aplicação
- **Infrastructure**: Repositórios, DbContext e acesso a dados
- **Controllers**: Endpoints da API
- **Tests**: Testes Automatizados

## 🛠️ Tecnologias Utilizadas

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **Swagger/OpenAPI**
- **xUnit** e **Moq**

## 🗄️ Configuração do Banco de Dados

### 1. Criar banco PostgreSQL
```sql
CREATE DATABASE banco;
CREATE USER bancoapi WITH PASSWORD 'senha123';
GRANT ALL PRIVILEGES ON DATABASE banco TO bancoapi;
```

### 2. Configurar Connection String
Edite os arquivos de configuração:

**`appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=banco;Username=bancoapi;Password=senha123;"
  }
}
```

## 🚀 Como Executar o Projeto

### 1. Clonar o repositório
```bash
git clone <url-do-repositorio>
cd BancoApi
```

### 2. Restaurar dependências
```bash
cd BancoApi
dotnet restore
```

### 3. Aplicar migrations (primeira execução)
```bash
dotnet ef database update
```

### 4. Executar a aplicação
```bash
dotnet run
```

### 5. Acessar o Swagger
Abra o navegador em: `http://localhost:5273/swagger`

## 📊 Estrutura do Banco de Dados

### Tabela: Contas
| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int | Chave primária |
| numero | varchar(20) | Número da conta (único) |
| nome | varchar(100) | Nome do titular |
| documento | varchar(14) | CPF/CNPJ (sem formatação) |
| saldo | decimal(18,2) | Saldo atual |
| data_criacao | timestamp | Data de criação |
| status | int | Status da conta (0=Inativa, 1=Ativa) |
| data_alteracao | timestamp | Data da última alteração |
| usuario_alteracao | text | IP/usuário que fez a alteração |

### Tabela: Transferencias
| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int | Chave primária |
| conta_origem_id | int | ID da conta origem |
| conta_destino_id | int | ID da conta destino |
| valor | decimal(18,2) | Valor da transferência |
| data_transferencia | timestamp | Data da transferência |

## 🔗 Endpoints da API

### Contas
- `GET /api/conta/{id}` - Buscar conta por ID
- `GET /api/conta/termo={termo}` - Buscar conta por nome ou documento
- `POST /api/conta` - Criar nova conta
- `PATCH /api/conta/{documento}/inativar` - Inativar conta

### Transferências
- `GET /api/transferencia/{id}` - Buscar transferência por ID
- `GET /api/transferencia/conta/{documento}` - Buscar transferências por documento da conta
- `POST /api/transferencia` - Criar nova transferência

## 🧪 Testes Automatizados

O projeto possui **43 testes automatizados** implementados com:

- **xUnit** - Framework de testes
- **Moq** - Framework de mock para simulação de dependências
- **FluentAssertions** - Asserções mais legíveis

### Executar Testes
```bash
# Executar todos os testes
dotnet test

# Executar com verbosidade
dotnet test --verbosity normal
```

### Cobertura
- ✅ **43 testes** (100% passando)
- ✅ **18 testes** para `ContaService`
- ✅ **9 testes** para `TransferenciaService`  
- ✅ **18 testes** para Controllers
