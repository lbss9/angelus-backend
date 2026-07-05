# Frontend Spec — Angelus (Web MMO 3D de Anjinhos)

## Ferramentas de IA disponíveis

- **Claude Fable 5** (este chat) — implementação principal do frontend
- **Claude Sonnet** (outro chat) — implementação do backend
- **Codex** (plugin instalado via `/plugin install codex@openai-codex`) — revisor auxiliar

### Como usar o Codex CLI para auxiliar o desenvolvimento

O Codex CLI (`codex`) está instalado globalmente. Use-o direto no terminal para revisões e investigações.

**Pré-requisito:** o projeto precisa ser um repositório git.
```bash
git init
git add .
git commit -m "initial"
```

**Revisar mudanças não commitadas:**
```bash
codex review --uncommitted
```

**Pedir ao Codex para investigar um problema:**
```bash
codex exec "por que a câmera não está seguindo o personagem corretamente?"
```

**Através do plugin no Claude Code (slash commands):**
```
/codex:review
/codex:adversarial-review questiona a arquitetura do loop de jogo
/codex:rescue investiga por que o GLTFLoader não está carregando o modelo
```

---

## Tema do Jogo

**Angelus** é um MMO fofo com anjinhos Chibi/Kawaii. O jogador escolhe um dos **3 anjinhos** ao criar seu personagem — cada um tem modelo 3D, personalidade e visual únicos. A ambientação é **pastel suave**: nuvens, luz dourada, céu azul claro.

### Os 3 Anjinhos

| ID | Nome | Visual | Personalidade |
|----|------|--------|---------------|
| `sol` | **Sol** | Dourado, asas grandes, cabelo loiro, roupa amarela | Alegre e energético |
| `lua` | **Lua** | Prateado, asas delicadas, cabelo azul claro, roupa lilás | Calmo e misterioso |
| `rosa` | **Rosa** | Rosado, asas com corações, cabelo rosa, roupa branca | Gentil e carinhoso |

---

## Stack

- **Linguagem:** JavaScript puro (ES Modules, sem framework)
- **3D Engine:** Three.js (via CDN ou npm)
- **Real-time:** SignalR JS Client (`@microsoft/signalr` via CDN)
- **Auth:** JWT armazenado no `localStorage`
- **Estilo:** CSS puro, tema **pastel suave** — nuvens, rosa, lilás, dourado suave

---

## Estrutura de Arquivos

```
frontend/
├── index.html              ← login/registro
├── lobby.html              ← seleção/criação de personagem
├── game.html               ← mundo 3D + chat
├── css/
│   └── style.css
├── assets/
│   ├── models/
│   │   └── character.glb   ← modelo 3D do personagem (Mixamo/Kenney)
│   ├── textures/
│   │   ├── ground.jpg      ← textura do chão
│   │   └── skybox/         ← HDR ou cubemap do céu
│   └── audio/              ← sons ambiente (opcional)
└── js/
    ├── api.js              ← chamadas HTTP para a REST API
    ├── auth.js             ← login, registro, JWT
    ├── lobby.js            ← CRUD de personagem
    ├── game.js             ← entry point do jogo 3D
    ├── world.js            ← Three.js scene, luzes, chão, skybox
    ├── player.js           ← personagem local: input, animação, movimento
    ├── remotePlayer.js     ← outros jogadores: interpolação de posição
    ├── camera.js           ← câmera third-person com mouse
    ├── network.js          ← SignalR: connect, eventos, emit
    └── chat.js             ← UI do chat (overlay sobre o canvas)
```

---

## Telas e Fluxo

```
[index.html]   →  login/registro
      ↓ JWT salvo no localStorage
[lobby.html]   →  ver/criar/deletar personagem → "Entrar no Mundo"
      ↓ characterId salvo no localStorage
[game.html]    →  cena 3D Three.js fullscreen + HUD + chat
```

---

## index.html — Auth

**Layout:**
- Fundo fullscreen com imagem/gradiente de paisagem medieval escura
- Painel central com logo do jogo "Angelus" (fonte Cinzel dourada)
- Tabs: "Entrar" | "Registrar"
- Campos: Email + Senha
- Botão de submit com efeito hover dourado
- Mensagem de erro inline

**Comportamento:**
- POST `http://localhost:8080/api/auth/login` ou `/register`
- Salvar `token` e `userId` no `localStorage`
- Redirecionar para `lobby.html`
- Se já tiver token válido, ir direto para lobby

---

## lobby.html — Escolha do Anjinho

**Layout:**
- Background: céu com nuvens fofas em tons pastel
- Header com email do usuário + botão Logout
- Se não tiver personagem: tela de criação
  - Título: "Escolha seu Anjinho"
  - 3 cards lado a lado, um para cada anjinho:
    - Preview 3D ou ilustração do anjinho
    - Nome do anjinho (Sol / Lua / Rosa)
    - Descrição curta da personalidade
    - Card selecionável com borda brilhante ao clicar
  - Campo: Nome do personagem (max 20 chars)
  - Botão "Começar Aventura!"
- Se tiver personagem: card com anjinho + nome + botão "Entrar no Mundo" + "Deletar"

**Comportamento:**
- GET `/api/characters` ao carregar
- POST `/api/characters` com `{ name, angelType }` para criar
- DELETE `/api/characters/{id}` para deletar
- "Entrar no Mundo" → salvar `characterId` e `angelType` no localStorage → ir para `game.html`

---

## game.html — Mundo 3D

### Layout Geral
```
┌─────────────────────────────────────────────┐
│  THREE.JS CANVAS (fullscreen)               │
│                                             │
│  [HUD top-left]  Nome do personagem         │
│                                             │
│         [personagens 3D no mundo]           │
│                                             │
│  ┌──────────────────────────────────────┐   │
│  │ CHAT (overlay bottom-left, ~300px)   │   │
│  │ [Jogador]: mensagem                  │   │
│  │ [campo de texto]       [Enviar]      │   │
│  └──────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

---

## world.js — Cena Three.js

### Configuração inicial
```js
const scene = new THREE.Scene()
const renderer = new THREE.WebGLRenderer({ antialias: true })
renderer.setSize(window.innerWidth, window.innerHeight)
renderer.shadowMap.enabled = true
```

### Iluminação
- `AmbientLight` cor `#fff5e0` (luz quente celestial), intensidade 0.8
- `DirectionalLight` simulando sol suave, com sombra habilitada, cor `#fffbe6`
- `HemisphereLight` gradiente sky `#87ceeb` (azul céu) / ground `#c8f0c8` (verde pastel)

### Chão
- `PlaneGeometry` de 200x200 unidades
- Textura de grama suave ou nuvens (tons pastel)
- Recebe sombras (`receiveShadow = true`)
- Alternativa encantadora: plataforma de nuvem (`#ffffff` com opacidade e bump map)

### Skybox
- Céu azul claro com nuvens fofas
- Usar HDRI de céu diurno ensolarado
- Fonte: [Poly Haven](https://polyhaven.com/hdris) — buscar "overcast" ou "sky" claro
- Ou: `scene.background = new THREE.Color('#87ceeb')` simples e eficaz

### Loop de render
```js
function animate() {
  requestAnimationFrame(animate)
  const delta = clock.getDelta()
  // atualizar player, câmera, remotePlayers, animações
  renderer.render(scene, camera)
}
```

---

## camera.js — Câmera Third-Person

### Comportamento (padrão MMORPG)
- Posição: `offset = (0, 3, 6)` atrás/acima do personagem
- Segue o personagem com lerp suave (`lerpFactor = 0.1`)
- **Rotação horizontal:** arrastar mouse (botão direito ou esquerdo)
- **Zoom:** scroll do mouse (aproximar/afastar)
- **Clampar pitch:** entre -20° e 60° (não vira de cabeça pra baixo)

### Implementação base
```js
class ThirdPersonCamera {
  constructor(camera, target) {
    this.camera = camera
    this.target = target        // mesh do jogador
    this.theta = 0              // rotação horizontal
    this.phi = 20               // rotação vertical (graus)
    this.radius = 6             // distância do personagem
  }

  update(delta) {
    const targetPos = this.target.position.clone()
    const x = targetPos.x + this.radius * Math.sin(this.theta) * Math.cos(this.phi)
    const y = targetPos.y + this.radius * Math.sin(this.phi)
    const z = targetPos.z + this.radius * Math.cos(this.theta) * Math.cos(this.phi)
    this.camera.position.lerp(new THREE.Vector3(x, y, z), 0.1)
    this.camera.lookAt(targetPos)
  }
}
```

---

## player.js — Personagem Local

### Carregamento dos modelos

Há **3 modelos distintos**, um por anjinho:
```
assets/models/angel-sol.glb    ← anjinho dourado
assets/models/angel-lua.glb    ← anjinho prateado/lilás
assets/models/angel-rosa.glb   ← anjinho rosa
```

Carregar o modelo conforme o `angelType` do personagem:
```js
const modelMap = {
  sol:  'assets/models/angel-sol.glb',
  lua:  'assets/models/angel-lua.glb',
  rosa: 'assets/models/angel-rosa.glb',
}
const loader = new GLTFLoader()
loader.load(modelMap[angelType], gltf => { scene.add(gltf.scene) })
```

**Como obter os modelos:**
- [Mixamo](https://www.mixamo.com) — procurar personagens fofos/chibi com asas
- [Sketchfab](https://sketchfab.com) — buscar "chibi angel" filtrando por Free + Downloadable
- [Quaternius](https://quaternius.com) — character packs low-poly fofos CC0
- Cada modelo deve ter animações: `idle` (asas batendo levemente) e `walk`
- Tamanho recomendado: máx 10k triângulos por anjinho

### Animações
```js
const mixer = new THREE.AnimationMixer(model)
const actions = {
  idle: mixer.clipAction(clips.find(c => c.name === 'idle')),
  walk: mixer.clipAction(clips.find(c => c.name === 'walk')),
}
actions.idle.play()

// Trocar animação suavemente
function setAction(name) {
  const next = actions[name]
  if (current !== next) {
    current.fadeOut(0.2)
    next.reset().fadeIn(0.2).play()
    current = next
  }
}
```

### Movimentação (WASD)
- Direção do movimento relativa à câmera (não ao mundo)
- Velocidade: 5 unidades/segundo
- Rotacionar o modelo na direção do movimento com `quaternion.slerp`
- A cada 50ms (20x/seg): emitir `Move` via SignalR com posição `{x, y, z}` e rotação `{y}`
- Se movendo → animação `walk`, parado → `idle`

### Controles de input
```js
const keys = {}
window.addEventListener('keydown', e => keys[e.code] = true)
window.addEventListener('keyup',   e => keys[e.code] = false)

// No loop:
const dir = new THREE.Vector3()
if (keys['KeyW'] || keys['ArrowUp'])    dir.z -= 1
if (keys['KeyS'] || keys['ArrowDown'])  dir.z += 1
if (keys['KeyA'] || keys['ArrowLeft'])  dir.x -= 1
if (keys['KeyD'] || keys['ArrowRight']) dir.x += 1
```

---

## remotePlayer.js — Outros Jogadores

- Mesmo modelo GLB do jogador local, com cor diferente (tint via material)
- Guardar `Map<characterId, { mesh, targetPos, targetRot }>`
- **Interpolação de posição:** `mesh.position.lerp(targetPos, 0.2)` por frame
- Ao receber `PlayerMoved`: atualizar `targetPos` e `targetRot`
- Ao receber `PlayerJoined`: instanciar novo mesh na cena
- Ao receber `PlayerLeft`: remover mesh da cena
- Nome do personagem flutuando acima: usar `CSS2DRenderer` ou `Sprite` com canvas texture

---

## network.js — SignalR

```js
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`http://localhost:8080/gamehub?access_token=${token}`)
  .withAutomaticReconnect()
  .build()

await connection.start()
await connection.invoke('JoinWorld', characterId)

connection.on('WorldState',   ({ players }) => { /* inicializar todos */ })
connection.on('PlayerJoined', player => { /* adicionar ao mapa */ })
connection.on('PlayerMoved',  ({ id, x, y, z }) => { /* atualizar targetPos */ })
connection.on('PlayerLeft',   ({ id }) => { /* remover da cena */ })
connection.on('ChatMessage',  msg => chat.addMessage(msg))
```

**Throttle de movimento:** usar `setInterval(sendPosition, 50)` — não enviar no requestAnimationFrame

---

## chat.js — Chat (Overlay HTML)

- Div overlay posicionado bottom-left sobre o canvas (z-index alto)
- Altura fixa 220px, largura 340px, fundo semitransparente
- Lista de mensagens com scroll automático para o fim
- Formato: `[HH:MM] NomePersonagem: mensagem`
- Enter ou botão Enviar → `connection.invoke('SendChat', message)`
- Máximo 200 caracteres
- Clicar no chat não move o personagem (stopPropagation no keydown)

---

## Assets — Onde Buscar

### Modelos 3D de personagem
| Fonte | O que pegar | Licença |
|-------|-------------|---------|
| [Mixamo](https://www.mixamo.com) | Personagem + animações idle/walk/run | Grátis (Adobe) |
| [Kenney Character Pack](https://kenney.nl/assets/character-pack) | Personagens low-poly estilizados | CC0 |
| [Quaternius](https://quaternius.com) | Packs completos de personagens | CC0 |
| [Sketchfab](https://sketchfab.com) | Filtrar por "Free" + "Downloadable" | Varia |

### Texturas e ambiente
| Fonte | O que pegar | Licença |
|-------|-------------|---------|
| [Poly Haven](https://polyhaven.com) | HDRIs para skybox, texturas de chão | CC0 |
| [Kenney](https://kenney.nl/assets) | Tiles de ambiente | CC0 |
| [OpenGameArt](https://opengameart.org) | Texturas variadas | CC0/CC BY |

### Workflow recomendado para Mixamo
1. Criar conta grátis em mixamo.com
2. Escolher personagem → Download como **FBX for Unity**
3. Voltar ao personagem → aba **Animations** → escolher `idle`, `walk`
4. Para cada animação: Download → **FBX Binary** → sem skin (In Place)
5. Abrir Blender → importar FBX do personagem + FBXs de animação
6. Combinar animações no mesmo arquivo
7. Exportar como **GLB** (File → Export → glTF 2.0 → formato GLB)

### Formato obrigatório
Usar sempre `.glb` — inclui modelo + texturas + animações em um único arquivo.

---

## Identidade Visual (Pastel Suave — Fofo e Celestial)

- **Fundo:** `#e8f4fd` (azul céu clarinho)
- **Superfícies UI:** `#ffffff` com opacidade 0.85 (nuvem)
- **Primário/Sol:** `#f9d976` (amarelo dourado suave)
- **Secundário/Lua:** `#c3aed6` (lilás suave)
- **Terciário/Rosa:** `#f8b4c8` (rosa bebê)
- **Texto:** `#5a4a6a` (roxo escuro suave — legível sem ser agressivo)
- **Erro:** `#e88080` (vermelho suave)
- **Fonte títulos:** `Fredoka One` (Google Fonts) — redonda e fofa
- **Fonte corpo:** `Nunito` (Google Fonts) — arredondada e agradável
- Bordas com `border-radius` generoso (16px+) e sombras suaves
- Botões com cantos arredondados e efeito de brilho ao hover
- Decorações: estrelinhas ✦, corações ♡, nuvens ☁ como elementos CSS

---

## Conexão com Backend

| O que | Valor |
|-------|-------|
| URL da API | `http://localhost:8080` |
| SignalR Hub | `http://localhost:8080/gamehub` |
| Token storage | `localStorage["token"]` |
| Character ID | `localStorage["characterId"]` |

> O backend roda via Docker com `docker compose up` no diretório `Angelus.Api/`

---

## Performance

- Budget total da cena: máx **500k triângulos**
- Personagem principal: máx **15k triângulos**
- Outros jogadores: mesma mesh instanciada (InstancedMesh se >10 players)
- Texturas: máx 1024x1024 por textura
- Usar `renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))`

---

## Estados de Erro a Tratar

- Token expirado → redirecionar para `index.html`
- SignalR desconectado → banner "Reconectando..." no topo da tela
- Falha ao carregar GLB → mostrar personagem placeholder (cubo colorido)
- Nome de personagem já existe → mensagem inline no lobby
