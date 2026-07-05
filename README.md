# Angelus — MMORPG Backend

Backend de um MMORPG web 3D com anjinhos fofos. Projeto de portfólio desenvolvido com **.NET 10**, seguindo **Clean Architecture**, **CQRS** e boas práticas de engenharia de software.

![CI](https://github.com/lbss9/angelus-backend/actions/workflows/ci.yml/badge.svg)

---

## Arquitetura

```
Angelus.Api           ← entrada HTTP (Controllers + SignalR Hub)
Angelus.Infrastructure ← detalhes técnicos (EF Core, JWT, PostgreSQL)
Angelus.Application   ← casos de uso, CQRS (Commands + Queries)
Angelus.Domain        ← entidades e contratos (sem dependências externas)
Angelus.Tests         ← testes unitários (xUnit + Moq + FluentAssertions)
```

**Fluxo de uma requisição:**
```
HTTP → Controller → CommandHandler → IRepository → Repository → PostgreSQL
```

Cada camada depende apenas da camada de dentro. O Domain não conhece EF Core. O Application não conhece HTTP.

---

## Stack

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 |
| Banco | PostgreSQL 16 |
| Auth | JWT Bearer |
| Tempo real | SignalR (WebSockets) |
| Testes | xUnit + Moq + FluentAssertions |
| Observabilidade | OpenTelemetry + Prometheus + Grafana |
| Containers | Docker + docker-compose |
| CI/CD | GitHub Actions |

---

## Endpoints

### Auth
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/register` | Cria conta |
| POST | `/api/auth/login` | Autentica e retorna JWT |

### Personagens *(requer JWT)*
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/characters` | Lista personagens do usuário |
| POST | `/api/characters` | Cria personagem (sol / lua / rosa) |
| DELETE | `/api/characters/{id}` | Remove personagem |

### Tempo Real
| Protocolo | Rota | Eventos |
|---|---|---|
| WebSocket | `/gamehub` | `Move`, `Chat`, `PlayerJoined`, `PlayerLeft` |

---

## Rodar localmente

**Pré-requisitos:** Docker

```bash
git clone https://github.com/lbss9/angelus-backend
cd angelus-backend
docker compose up --build
```

API disponível em `http://localhost:8080`  
Documentação OpenAPI em `http://localhost:8080/openapi/v1.json`

---

## Testes

```bash
dotnet test
```

9 testes unitários cobrindo os handlers de Auth e Characters. Sem dependência de banco — usa Moq para simular repositórios.

---

## Padrões aplicados

- **Clean Architecture** — separação total entre regras de negócio e detalhes técnicos
- **CQRS** — Commands (escrita) separados de Queries (leitura)
- **Result pattern** — `Result<T>` para retorno explícito de sucesso/erro sem exceptions
- **Repository pattern** — interfaces no Domain, implementações no Infrastructure
- **Dependency Injection** — `AddInfrastructure()` registra toda a camada de infra
