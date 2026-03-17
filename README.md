# FullStackAPI Guild

API ASP.NET 8 para gerenciamento de uma unica guild de MU Online, com cadastro controlado por `ClaCode`, hierarquia interna e autenticacao com JWT.

## Estado atual

### Backend ja implementado

- [x] Solution `.NET` criada.
- [x] Projeto `ASP.NET Core Web API` em `.NET 8` criado.
- [x] PostgreSQL configurado e conectado.
- [x] EF Core configurado com Npgsql.
- [x] Migration inicial criada e aplicada.
- [x] `AppDbContext` criado.
- [x] Entidades `User`, `ClaCode` e `RoleChangeHistory` criadas.
- [x] Enum `UserRole` criado.
- [x] Seed do usuario `DEV` criado.
- [x] Cadastro com `ClaCode` implementado.
- [x] Login com JWT implementado.
- [x] Geracao de `ClaCode` implementada.
- [x] Swagger com `Authorize` configurado.
- [x] Endpoint `GET /api/users/me` implementado.
- [x] Endpoint `GET /api/users` implementado.
- [x] Endpoint `GET /api/cla-codes` implementado.
- [x] Endpoint `PATCH /api/users/{id}/role` implementado.
- [x] Tratamento global de erros iniciado com middleware.

### Ainda falta no backend

- [ ] Padronizar completamente os controllers para usar o tratamento global de erros.
- [ ] Aplicar a regra final de no maximo `2 DEV`s` e no minimo `1 DEV` ativo.
- [ ] Implementar inativacao de usuarios.
- [ ] Implementar listagem filtrada de usuarios por status e permissao.
- [ ] Melhorar validacoes de entrada.
- [ ] Criar logs melhores.
- [ ] Criar testes automatizados.
- [ ] Refinar autorizacao por policy, se necessario.

## Regras de negocio

- [x] O sistema e exclusivo para uma unica guild.
- [x] Todo novo usuario cadastrado entra com cargo `Member`.
- [x] O cadastro so acontece com `ClaCode` valido.
- [x] O `ClaCode` tem 6 caracteres aleatorios com letras maiusculas e numeros.
- [x] O `ClaCode` tem validade de 15 minutos.
- [x] O `ClaCode` e de uso unico.
- [x] Nao ha revogacao de `ClaCode`.
- [x] Apenas `AssistantMaster`, `GuildMaster` e `DEV` podem gerar `ClaCode`.
- [x] Apenas `GuildMaster` e `DEV` podem promover ou rebaixar usuarios.
- [x] `BattleMaster` e o responsavel por guerra, eventos e embates.
- [x] `GuildMaster` futuramente tambem podera criar alguns eventos.
- [x] O sistema deve manter no minimo `1 DEV` ativo.
- [x] O sistema permite no maximo `2 DEV`s` ativos.
- [x] `DEV` nao pode se auto-rebaixar.
- [x] `GuildMaster` e `DEV` podem inativar usuarios conforme a hierarquia definida.
- [x] Usuario inativo nao pode fazer login.
- [x] `AssistantMaster` pode consultar apenas usuarios ativos.
- [x] `GuildMaster` e `DEV` podem consultar usuarios ativos, inativos e todos.

## Hierarquia

- [x] `Member`: acesso basico.
- [x] `BattleMaster`: responsavel por guerra, embates e eventos de conflito.
- [x] `AssistantMaster`: cria `ClaCode` e cobre escopos inferiores em ausencia do `GuildMaster`.
- [x] `GuildMaster`: cria `ClaCode`, cobre escopos inferiores e promove/rebaixa.
- [x] `DEV`: acesso total para testes, manutencao e implementacao.

## Matriz de hierarquia

- [x] `Member` nao altera cargo de ninguem.
- [x] `BattleMaster` nao altera cargo de ninguem.
- [x] `AssistantMaster` nao altera cargo de ninguem.
- [x] `GuildMaster` pode alterar `Member`, `BattleMaster` e `AssistantMaster`.
- [x] `GuildMaster` nao pode promover para `GuildMaster`.
- [x] `GuildMaster` nao pode promover para `DEV`.
- [x] `GuildMaster` nao pode alterar outro `GuildMaster`.
- [x] `GuildMaster` nao pode alterar `DEV`.
- [x] `DEV` pode alterar qualquer cargo.
- [x] `DEV` nao pode se auto-rebaixar.
- [x] Nenhuma operacao pode deixar o sistema sem pelo menos `1 DEV` ativo.
- [x] Nenhuma operacao pode criar mais de `2 DEV`s` ativos.

## Matriz de inativacao

- [x] `Member` nao inativa ninguem.
- [x] `BattleMaster` nao inativa ninguem.
- [x] `AssistantMaster` nao inativa ninguem.
- [x] `GuildMaster` pode inativar `Member`, `BattleMaster` e `AssistantMaster`.
- [x] `GuildMaster` nao pode inativar outro `GuildMaster`.
- [x] `GuildMaster` nao pode inativar `DEV`.
- [x] `DEV` pode inativar qualquer usuario.
- [x] `DEV` pode inativar outro `DEV` somente se a operacao mantiver pelo menos `1 DEV` ativo.

## Matriz de consulta de usuarios

- [x] `Member` nao lista usuarios.
- [x] `BattleMaster` nao lista usuarios.
- [x] `AssistantMaster` pode listar apenas usuarios ativos.
- [x] `GuildMaster` pode listar usuarios ativos, inativos e todos.
- [x] `DEV` pode listar usuarios ativos, inativos e todos.
- [x] Quando nenhum filtro for informado, o padrao recomendado e listar apenas usuarios ativos.

## Endpoints implementados

- [x] `POST /api/auth/register`
- [x] `POST /api/auth/login`
- [x] `POST /api/cla-codes`
- [x] `GET /api/cla-codes`
- [x] `GET /api/users/me`
- [x] `GET /api/users`
- [x] `PATCH /api/users/{id}/role`

## Endpoints planejados para a proxima fase

- [ ] `GET /api/users?status=active`
- [ ] `GET /api/users?status=inactive`
- [ ] `GET /api/users?status=all`
- [ ] `PATCH /api/users/{id}/deactivate`

## Estrutura atual do backend

- [x] `Controllers`
- [x] `Data`
- [x] `DTOs`
- [x] `Entities`
- [x] `Enums`
- [x] `Middleware`
- [x] `Services`

## Dependencias principais do backend

- [x] `.NET 8 SDK`
- [x] `Microsoft.EntityFrameworkCore`
- [x] `Microsoft.EntityFrameworkCore.Design`
- [x] `Npgsql.EntityFrameworkCore.PostgreSQL`
- [x] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] `BCrypt.Net-Next`
- [x] `Swashbuckle.AspNetCore`
- [x] `dotnet-ef`

## Banco de dados

### Ja concluido

- [x] PostgreSQL instalado.
- [x] Banco de dados criado.
- [x] Usuario do banco criado.
- [x] Permissoes do usuario do banco ajustadas.
- [x] `ConnectionStrings:DefaultConnection` configurada via `user-secrets`.
- [x] Tabelas iniciais criadas com migration.

### Estrutura atual

- [x] Tabela `Users`
- [x] Tabela `ClaCodes`
- [x] Tabela `RoleChangeHistories`
- [x] Indice unico para `Username`
- [x] Indice unico para `Email`
- [x] Relacionamentos entre criador/usuario do `ClaCode`

## O que falta para fechar o MVP backend

- [ ] Finalizar a limpeza dos controllers para depender mais do middleware global.
- [ ] Aplicar a regra de no minimo `1 DEV` ativo e no maximo `2 DEV`s` ativos.
- [ ] Implementar inativacao de usuarios com as regras de hierarquia.
- [ ] Implementar filtro de listagem de usuarios por status respeitando o perfil do solicitante.
- [ ] Melhorar mensagens e padrao de erros.
- [ ] Adicionar validacoes de entrada mais fortes.
- [ ] Criar testes unitarios para:
- [ ] cadastro com `ClaCode`
- [ ] login
- [ ] promocao/rebaixamento
- [ ] bloqueio de terceiro `DEV`
- [ ] bloqueio de remocao do ultimo `DEV`
- [ ] inativacao por `GuildMaster`
- [ ] inativacao de `DEV` por `DEV`
- [ ] listagem de ativos para `AssistantMaster`
- [ ] bloqueio de listagem de inativos para `AssistantMaster`
- [ ] Criar testes de integracao dos fluxos principais.

## Frontend

### Ainda nao iniciado

- [ ] Escolher stack final do frontend.
- [ ] Recomendado: `React + Vite + TypeScript`.
- [ ] Criar tela de login.
- [ ] Criar tela de cadastro com `ClaCode`.
- [ ] Criar tela de perfil.
- [ ] Criar tela de listagem de usuarios.
- [ ] Criar tela de alteracao de cargo.
- [ ] Criar tela de geracao e listagem de `ClaCodes`.
- [ ] Armazenar e enviar JWT nas requisicoes.

## Proximos passos recomendados

1. Finalizar o tratamento global de erros nos controllers.
2. Aplicar as regras finais de `DEV` e inativacao.
3. Implementar a listagem de usuarios com filtro por status.
4. Melhorar validacoes de entrada.
5. Adicionar logs melhores.
6. Criar testes automatizados.
7. Iniciar o frontend.

## Referencias oficiais

- ASP.NET Core Web API: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- EF Core CLI: https://learn.microsoft.com/en-us/ef/core/cli/dotnet
- EF Core Migrations: https://learn.microsoft.com/pt-br/ef/core/managing-schemas/migrations/
- JWT Bearer no ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication
- Npgsql EF Core Provider: https://www.npgsql.org/efcore/
- Npgsql Basic Usage: https://www.npgsql.org/doc/basic-usage.html
- Npgsql Connection Strings: https://www.npgsql.org/doc/connection-string-parameters
