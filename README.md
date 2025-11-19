# 📘 Mini HR API - 技术设计与实施计划 (v4.4)

**版本说明**: **v4.4 (澳洲就业 - 终极修正版)**。此版本基于 v4.2，严格保留了所有关于 **Autofac (策略性保留)**、**Serilog (可观测性)** 和 **Policy-Based Auth** 的架构决策，仅补充了 Auth 中间件顺序、Swagger 配置以及细化了测试实施步骤。

## 第一部分：技术设计文档 (TDD - Technical Design Document)

### 1.0 项目概述 (Project Overview)

#### 1.1 目标

本项目旨在构建一个简化版、企业级的人力资源管理 (HR) 后端系统。其架构设计将作为未来 "HR AI Agent" 项目的坚实基础，优先确保系统的模块化、可测试性、合规性（RESTful 标准）及**生产环境可观测性 (Observability)**。

#### 1.2 核心功能

1. **认证授权**: 基于 JWT 的用户注册与登录，重点实现复杂的**基于策略 (Policy-Based) 的权限控制**。
2. **员工管理**: 实现员工档案的 CRUD 操作，支持大数据的分页查询。
3. **简历处理**: 支持简历文件 (如 `.pdf`, `.docx`) 的上传、安全存储及后续解析数据的管理。

### 2.0 系统架构 (System Architecture)

#### 2.1 架构选型

采用 **Clean Architecture (整洁架构)** 的经典四层变体，强制实现“关注点分离” (Separation of Concerns)。

#### 2.2 依赖关系规则

依赖箭头指向被依赖的层。**外层依赖内层，内层（尤其是 Domain）对所有外层保持“无知”**。

```
[层级]            [项目名 (Project)]         [职责 (Responsibilities)]
-------------------------------------------------------------------------------------------------
  API /           MiniHR.WebAPI              - Controllers, Minimal APIs
Presentation                               - DTOs, Global Exception Handling (IExceptionHandler)
 (展示层)                                  - Program.cs (DI, Middleware, Serilog 配置)
                                           - **Standard HTTP Responses (RESTful)**
    |
    v
Application       MiniHR.Application         - Services (业务逻辑, 分页逻辑)
 (应用层)                                  - AutoMapper Profiles
                                           - FluentValidation
    |
    v
Domain            MiniHR.Domain              - Entities (核心实体, e.g., Employee)
 (领域层)                                  - Repository Interfaces (含分页定义)
                                           <-- [ 核心: 不依赖任何其他层 ] -->
    ^
    | (实现)
Infrastructure    MiniHR.Infrastructure      - EF Core DbContext (数据库上下文)
 (基础设施层)                                - Repository Implementations (EF Core 查询)
                                           - External Services (File, Jwt)
-------------------------------------------------------------------------------------------------
```

### 3.0 横切关注点 (Cross-Cutting Concerns) - (核心架构决策)

#### 3.1 依赖注入 (Dependency Injection) - (保留技术栈)

- **实现**: **Autofac**。
- **状态**: 已集成并运行良好。
- **理由**: 虽然 .NET 8 原生 DI 已足够强大，但我们保留 Autofac 以展示对遗留系统维护能力和高级 IOC 容器（如模块化注册）的掌控。这在澳洲企业级面试中是加分项。

#### 3.2 HTTP 响应与错误处理 (RESTful + ProblemDetails)

- **成功 (Success)**:
  - `200 OK`: 请求成功，Body 直接返回数据对象 (JSON)。
  - `201 Created`: 资源创建成功，Header 包含 `Location`，Body 为新资源数据。
  - `204 No Content`: 请求成功（如删除/修改），无 Body。
- **错误 (Error)**:
  - 使用 **RFC 7807 ProblemDetails** 标准格式。
  - 所有错误（4xx, 500）均返回包含 `type`, `title`, `status`, `detail`, `traceId` 等标准字段的 JSON。
- **异常 (Exception)**:
  - 全局异常处理器实现 `IExceptionHandler` 接口 (`GlobalExceptionHandler`)，捕获未处理异常并转换为 500 `ProblemDetails`（生产环境隐藏堆栈）。

#### 3.3 结构化日志 (Structured Logging) - **(新增)**

- **实现**: **Serilog**。
- **配置**:
  - 替代 .NET 默认 Logger。
  - 输出目标 (Sinks): Console (开发环境), File (生产环境, 按天滚动)。
  - **Enrichers**: 自动附加 `MachineName`, `ThreadId`, `HttpRequestId` 等上下文信息，便于排查问题。

#### 3.4 分页策略 (Pagination Strategy) - **(新增)**

- **目标**: 防止大数据量查询导致的性能问题 (OOM)。
- **实现**:
  - 所有返回集合的 API (如 `GetAll`) **必须**支持分页。
  - **请求参数**: `pageNumber` (默认 1), `pageSize` (默认 10, 最大 50)。
  - **响应结构**: 返回 `PagedResult<T>`，包含 `items` 和 `metadata` (totalCount, totalPages 等)。

#### 3.5 Authentication & Authorization (重点)

- **Authentication (认证)**: 使用 JWT (JSON Web Tokens)。
- **Authorization (授权)**: 使用 **Policy-Based Authorization** (基于策略)。
  - **Policy 示例**:
    - `"AdminOnly"`: RequireRole("Admin")
    - `"EmployeeRead"`: RequireClaim("Permission", "Employee.Read")
  - 这种方式比简单的 Role-Based 更灵活，是澳洲中大型项目的标配。

### 4.0 数据库设计 (Database Design - PostgreSQL)

采用 **EF Core Code-First** + **PostgreSQL** (澳洲初创和中型企业首选)。

- **Users**: `Id`, `Username`, `PasswordHash`, `Role`
- **Employees**: `Id`, `FirstName`, `LastName`, `Email`, `Position`, `Salary`, `HireDate`, `IsDeleted`
- **Resumes**: `Id`, `EmployeeId`, `OriginalFileName`, `StoredFilePath`, `ParsedContent`, `UploadedAt`

### 5.0 API 端点定义 (Endpoint Definitions)

- `POST /api/auth/register` -> `200 OK`
- `POST /api/auth/login` -> `200 OK` (返回 `{ "token": "..." }`)
- `GET /api/employees?pageNumber=1&pageSize=10` -> `200 OK` (返回 `PagedResult<EmployeeDto>`)
- `GET /api/employees/{id}` -> `200 OK` 或 `404 Not Found`
- `POST /api/employees` -> `201 Created` (需 "AdminOnly" Policy)
- `PUT /api/employees/{id}` -> `204 No Content` (需 "AdminOnly" Policy)
- `DELETE /api/employees/{id}` -> `204 No Content` (需 "AdminOnly" Policy)
- `POST /api/employees/{id}/resumes` -> `200 OK` (返回 `ResumeDto`)

### 6.0 关键技术栈 (Tech Stack Checklist)

- [ ] **Framework**: .NET 8
- **Core Architecture**:
  - [x] **DI Container**: **Autofac** (已就绪)
  - [ ] **Logging**: **Serilog** (结构化日志)
  - [ ] **Error Handling**: **IExceptionHandler + ProblemDetails (RFC 7807)**
- **Security**:
  - [ ] **Auth**: JWT (Bearer Token)
  - [ ] **Authorization**: **Policy-Based**
  - [ ] **Encryption**: BCrypt.Net-Next
- **Data & Logic**:
  - [ ] **Web API**: ASP.NET Core Controllers (RESTful)
  - [x] **Database**: PostgreSQL 16
  - [x] **ORM**: Entity Framework Core (EF Core) 8
  - [ ] **Mapping**: AutoMapper
  - [ ] **Validation**: FluentValidation
- **Testing (QA)**:
  - [ ] **Unit Tests**: xUnit, Moq, FluentAssertions
  - [ ] **Integration Tests**: WebApplicationFactory, Testcontainers

## 第二部分：项目实施计划 (Project Implementation Plan)

### 🏁 阶段一：项目骨架 (Project Skeleton) - [已完成]

**目标**: 搭建符合 Clean Architecture 的项目结构，配置核心 DI 与中间件管道。

- **[x] 任务 1.1 ~ 1.5**: (环境, Solution, 引用, Autofac, CORS) - **已就绪**

### 💾 阶段二：数据持久化 (Data Persistence) - [已完成]

**目标**: 掌握 EF Core Code First 完整生命周期。

- **[x] 任务 2.1 ~ 2.3**: (实体定义, DbContext, Migration, Database Update) - **已就绪**

### 🔄 阶段三：RESTful 重构与基础设施 (Standardization & Infra) - [当前阶段]

**目标**: 引入生产级日志，移除自定义封装，回归标准 HTTP 响应 (ProblemDetails)。

- **[ ] 任务 3.1 (日志系统)**:
  - **安装包**: 在 `WebAPI` 安装 `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`。
  - **配置**: 在 `Program.cs` 中配置 Serilog，接管 .NET 内置日志。
  - *验收*: 控制台输出带有颜色的结构化日志。
- **[ ] 任务 3.2 (错误处理重构)**:
  - **删除**: 移除 `ApiResult.cs` 类和 `ExceptionMiddleware`。
  - **实现**: 创建 `GlobalExceptionHandler` (实现 `IExceptionHandler`)。
  - **配置**: 在 `Program.cs` 注册 `AddProblemDetails()` 和 `UseExceptionHandler()`。
  - *验收*: 抛出异常时，API 返回 RFC 7807 格式的 JSON (`type`, `title`, `status`, `detail`)。
- **[ ] 任务 3.3 (Controller 清洗)**:
  - **重构**: 修改 `EmployeesController`。
  - **动作**:
    - 返回类型改为 `IActionResult` 或 `ActionResult<T>`。
    - 移除所有 `ApiResult` 包装。
    - 成功返回 `Ok(data)`, `CreatedAtAction(...)`。
    - 失败返回 `BadRequest()`, `NotFound()`。

### 🛡️ 阶段四：认证与授权 (Auth & Security) - [核心重点]

**目标**: 实现完整的用户注册登录流程，并配置基于策略的权限控制。

- **[ ] 任务 4.1 (安全基础设施)**:
  - **安装包 (Infra)**: `BCrypt.Net-Next`。
  - **安装包 (WebAPI)**: `Microsoft.AspNetCore.Authentication.JwtBearer` (**关键修复**)。
  - **实现**: 在 `Infrastructure` 层实现 `PasswordHasher` 和 `JwtTokenGenerator` (密钥从配置读取)。
- **[ ] 任务 4.2 (Auth 业务逻辑)**:
  - 在 `Application` 层实现 `AuthService` (Register/Login 逻辑)。
  - 创建 `AuthController`，暴露 `POST /register` 和 `POST /login` 端点。
- **[ ] 任务 4.3 (配置鉴权管道)**:
  - 在 `Program.cs` 配置 `AddAuthentication().AddJwtBearer(...)`。
  - **关键顺序**: 确保 `app.UseAuthentication()` 在 `app.UseAuthorization()` 之前。
- **[ ] 任务 4.4 (配置 Policy 授权)**:
  - 使用 `AddAuthorization` 定义策略 `"AdminOnly"` (RequireRole "Admin")。
  - 在 `EmployeesController` 的写操作 (POST/PUT/DELETE) 上应用 `[Authorize(Policy = "AdminOnly")]`。
- **[ ] 任务 4.5 (Swagger 鉴权支持)**:
  - 配置 `AddSwaggerGen` 以支持 JWT Bearer 输入 (`OpenApiSecurityScheme`)。
  - *验收*: Swagger UI 出现“小锁”图标，输入 Token 后请求自动带上 Header。

### 🚀 阶段五：API 补全与业务逻辑 (Business Logic & Pagination)

**目标**: 按照逻辑顺序完成剩余 API 开发，并加入分页防止 OOM。

- **[ ] 任务 5.1 (分页基础设施)**:
  - 定义 `PagedResult<T>` (包含 Items, PageNumber, TotalPages 等)。
  - 为 `IQueryable` 编写 `ToPagedListAsync` 扩展方法。
- **[ ] 任务 5.2 (完善 Employee API)**:
  - 修改 `GetById` (处理 404) 和 `GetAll` (加入分页参数 `[FromQuery]`).
  - 实现 `Update` (PUT): 完整更新，返回 204 No Content。
  - 实现 `Delete` (DELETE): 软删除，返回 204 No Content。
- **[ ] 任务 5.3 (Resume API 开发)**:
  - 实现 `IFileService` (注意 Docker 卷挂载路径)。
  - 实现 `POST /employees/{id}/resumes`: 接收 `IFormFile`，保存文件，写入数据库。

### ✅ 阶段六：测试与验收 (Quality Assurance)

**目标**: 构建符合澳洲标准的“现代测试金字塔”。

- **[ ] 任务 6.1 (单元测试 Unit Tests)**:
  - **范围**: `Application` 层 (重点测试 `EmployeeService` 的业务逻辑)。
  - **工具**: `xUnit`, `Moq`, `FluentAssertions`。
- **[ ] 任务 6.2 (集成测试 Integration Tests)**:
  - **范围**: `WebAPI` 层 (Controller -> DB)。
  - **工具**: `WebApplicationFactory`, `Testcontainers` (PostgreSQL)。
  - **内容**: 验证 Auth 拦截 (401/403)、错误格式 (ProblemDetails) 和 完整 CRUD 流程。