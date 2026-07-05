# Backend Spec — Angelus (Web MMO 3D de Anjinhos)

## Ferramentas de IA disponíveis

- **Claude Sonnet** (este chat) — implementação principal do backend
- **Claude Fable** (outro chat) — implementação do frontend
- **Codex** (plugin instalado via `/plugin install codex@openai-codex`) — revisor auxiliar

### Como usar o Codex CLI para auxiliar o desenvolvimento

O Codex CLI (`codex`) está instalado globalmente.

**Pré-requisito:** o projeto precisa ser um repositório git.
```bash
git init && git add . && git commit -m "initial"
```

```bash
codex review --uncommitted           # revisar mudanças antes de commitar
codex review --base main             # comparar com branch principal
codex exec "investigar problema X"   # pedir análise ao Codex
```

Slash commands no Claude Code:
```
/codex:review
/codex:adversarial-review
/codex:rescue investiga por que o SignalR não está autenticando
```

---

## Stack
- **Runtime:** .NET 10 / ASP.NET Core
- **Real-time:** SignalR
- **ORM:** Entity Framework Core
- **Banco:** PostgreSQL
- **Auth:** JWT (Bearer token)
- **Docs:** OpenAPI (`/openapi/v1.json`)
- **Deploy:** Docker + docker-compose

---

## Tema do Jogo

**Angelus** é um MMO fofo de anjinhos Chibi/Kawaii. O jogador escolhe um dos **3 anjinhos disponíveis** ao criar seu personagem. Cada anjinho é um modelo 3D único com aparência distinta.

### Os 3 Anjinhos

| ID | Nome | Descrição visual |
|----|------|-----------------|
| `sol` | **Sol** | Anjinho dourado, asas grandes, cabelo loiro, roupa amarela |
| `lua` | **Lua** | Anjinho prateado, asas delicadas, cabelo azul claro, roupa lilás |
| `rosa` | **Rosa** | Anjinho rosado, asas com corações, cabelo rosa, roupa branca com detalhes rosas |

---

## Modelos de Banco

### User
```
Id          (Guid, PK)
Email       (string, unique)
PasswordHash (string, bcrypt)
CreatedAt   (DateTime)
```

### Character
```
Id           (Guid, PK)
UserId       (Guid, FK → User)
Name         (string, unique, max 20)
AngelType    (string — "sol" | "lua" | "rosa")
CreatedAt    (DateTime)
```

> `Color` foi substituído por `AngelType` — o modelo 3D varia conforme o anjinho escolhido.

---

## Estrutura de Pastas

```
Angelus.Api/
├── Controllers/
│   ├── AuthController.cs
│   └── CharacterController.cs
├── Hubs/
│   └── GameHub.cs
├── Models/
│   ├── User.cs
│   └── Character.cs
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   └── Character/
│       ├── CreateCharacterRequest.cs
│       └── CharacterResponse.cs
├── Services/
│   ├── AuthService.cs
│   └── CharacterService.cs
├── Data/
│   └── AppDbContext.cs
├── Dockerfile
└── Program.cs
```

---

## API REST

### Auth — `POST /api/auth`

| Método | Rota | Body | Resposta |
|--------|------|------|----------|
| POST | `/register` | `{ email, password }` | `{ token, userId }` |
| POST | `/login` | `{ email, password }` | `{ token, userId }` |

- Senha mínimo 6 caracteres, hash BCrypt
- JWT expira em 24h

### Personagem — `/api/characters` (requer JWT)

| Método | Rota | Body | Resposta |
|--------|------|------|----------|
| GET | `/` | — | lista de personagens do usuário |
| POST | `/` | `{ name, angelType }` | personagem criado |
| DELETE | `/{id}` | — | 204 No Content |

- Máximo **1 personagem por conta**
- `angelType` deve ser `"sol"`, `"lua"` ou `"rosa"` — validar no backend
- Nome único globalmente, máx 20 caracteres

---

## SignalR Hub — `/gamehub`

Autenticação: JWT via query string `?access_token=...`

### Eventos que o CLIENT envia ao servidor

| Evento | Payload | Descrição |
|--------|---------|-----------|
| `JoinWorld` | `{ characterId }` | Entrar no mundo com anjinho selecionado |
| `Move` | `{ x, y, z, rotY }` | Nova posição 3D + rotação Y do anjinho |
| `SendChat` | `{ message }` | Enviar mensagem no chat global |

### Eventos que o SERVIDOR envia ao client

| Evento | Payload | Descrição |
|--------|---------|-----------|
| `WorldState` | `{ players: [{ id, name, angelType, x, y, z, rotY }] }` | Estado ao entrar |
| `PlayerJoined` | `{ id, name, angelType, x, y, z, rotY }` | Anjinho entrou |
| `PlayerMoved` | `{ id, x, y, z, rotY }` | Anjinho moveu |
| `PlayerLeft` | `{ id }` | Anjinho saiu |
| `ChatMessage` | `{ characterName, angelType, message, timestamp }` | Mensagem no chat |

### Estado em memória
- `connectionId → PlayerState { characterId, name, angelType, x, y, z, rotY }`
- Posição inicial: `x=0, y=0, z=0` (centro do mundo 3D)

---

## Regras de Negócio

- `angelType` aceito: apenas `"sol"`, `"lua"`, `"rosa"`
- Usuário autenticado só entra com personagem que lhe pertence
- Chat: máximo 200 caracteres
- Coordenadas sem validação de colisão no servidor (client-authoritative)

---

## CORS

Origens liberadas: `http://localhost:5500`, `http://127.0.0.1:5500`, `http://localhost:3000`
SignalR requer `AllowCredentials`.

---

## Variáveis de Ambiente (docker-compose)

```
ConnectionStrings__Default   = Host=db;Port=5432;Database=Angelus;Username=postgres;Password=postgres
Jwt__Secret                  = chave secreta (min 32 chars)
Jwt__Issuer                  = game-server
Jwt__Audience                = game-client
```

---

## Rodar com Docker

```bash
cd Z:\development\Angelus.Api
docker compose up --build
```

- API: `http://localhost:8080`
- Docs: `http://localhost:8080/openapi/v1.json`
- Migrations aplicadas automaticamente ao iniciar
