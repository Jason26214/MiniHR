# 📘 Mini HR API - 技术设计与实施计划 (v3.3)

https://github.com/Jason26214/MiniHR.git

项目代号: "Phoenix"

版本: 3.3

日期: 2025-11-15

## 第一部分：技术设计文档 (TDD - Technical Design Document)

### 1.0 项目概述 (Project Overview)

#### 1.1 目标

本项目旨在构建一个简化版、企业级的人力资源管理 (HR) 后端系统。其架构设计将作为未来 "HR AI Agent" 项目的坚实基础，优先确保系统的模块化、可测试性与可扩展性。

#### 1.2 核心功能

1. **认证授权**: 提供基于 JWT 的用户注册与登录，并实现基于策略 (Policy) 的权限控制。
2. **员工管理**: 实现员工档案的增删改查 (CRUD)，并确保薪资等敏感数据的访问安全。
3. **简历处理**: 支持简历文件 (如 `.pdf`, `.docx`) 的上传、安全存储及后续解析数据的管理。

### 2.0 系统架构 (System Architecture)

#### 2.1 架构选型

采用 **Clean Architecture (整洁架构)** 的经典四层变体，强制实现“关注点分离” (Separation of Concerns)。

#### 2.2 依赖关系规则

依赖箭头指向被依赖的层。**外层依赖内层，内层（尤其是 Domain）对所有外层保持“无知”**。

```
[层级]            [项目名 (Project)]         [职责 (Responsibilities)]
-------------------------------------------------------------------------------------------------
  API /           MiniHR.WebAPI              - Controllers (控制器), Middleware (中间件), Filters (过滤器)
Presentation                               - DTOs (数据传输对象), API Model Validation (模型验证)
 (展示层)                                  - Program.cs (DI容器配置, 管道配置)
    |
    v
Application       MiniHR.Application         - Application Services (应用服务, 业务逻辑)
 (应用层)                                  - AutoMapper Profiles (对象映射配置)
                                           - FluentValidation (业务验证规则)
    |
    v
Domain            MiniHR.Domain              - Entities (核心实体, e.g., Employee)
 (领域层)                                  - Repository Interfaces (仓储接口, e.g., IEmployeeRepository)
                                           - Domain Events (领域事件 - 可选)
                                           <-- [ 核心: 不依赖任何其他层 ] -->
    ^
    | (实现)
Infrastructure    MiniHR.Infrastructure      - EF Core DbContext (数据库上下文)
 (基础设施层)                                - Repository Implementations (仓储实现)
                                           - FileService (文件存储), JwtService (Token生成)
-------------------------------------------------------------------------------------------------
```

### 3.0 横切关注点 (Cross-Cutting Concerns)

通过 ASP.NET Core **管道 (Pipeline)** 统一处理的全局功能。

1. **CORS (Cross-Origin Resource Sharing)**:
   - **实现**: `app.UseCors()` (Middleware)。
   - **目标**: 配置策略以允许指定的外部源（例如 `http://localhost:3000`）访问 API，是 Web API 与前端（React/Vue/Vite）集成的必备配置。
2. **统一响应模型 (Unified Response Model)**:
   - **实现**: 定义 `ApiResponse<T>` 类作为所有响应的“数据信封”。
   - **目标**: 无论成功或失败，前端始终能解析同一个 JSON 结构，增强 API 的健壮性。
3. **全局异常处理 (Global Exception Handling)**:
   - **实现**: 自定义 `ExceptionMiddleware` 或使用 .NET 8 `IExceptionHandler` 接口。
   - **目标**: 捕获所有未处理异常，将其格式化为 `ApiResponse` (e.g., `{ "success": false, "error": ... }`)，绝不暴露堆栈信息。
4. **成功响应包装 (Success Response Wrapping)**:
   - **实现**: 自定义 `ResultFilter` (IResultFilter / IActionFilter)。
   - **目标**: 自动将控制器返回的 `IActionResult` (如 `Ok(data)`) 包装成 `ApiResponse` (e.g., `{ "success": true, "data": ... }`)。
5. **Authentication (认证)**:
   - **实现**: `app.UseAuthentication()` (Middleware)。配置 `JwtBearer` 方案，验证 HTTP Header 中的 Token 并构建用户身份。
6. **Authorization (授权)**:
   - **实现**: `app.UseAuthorization()` (Middleware)。检查 `[Authorize]` 属性及已定义的 `Policy` 策略。
7. **Logging (日志)**:
   - **实现**: .NET 内置日志框架或集成 Serilog 中间件。

### 4.0 数据库设计 (Database Design - PostgreSQL)

采用 **EF Core Code-First** 模式。C# 实体类是唯一的数据结构真相来源。

#### 4.1 Table: `Users` (系统用户)

- `Id` (Guid, PK)
- `Username` (string, Unique Index)
- `PasswordHash` (string)
- `Role` (string) - (e.g., "Admin", "User")

#### 4.2 Table: `Employees` (员工档案)

- `Id` (Guid, PK)
- `FirstName` (string)
- `LastName` (string)
- `Email` (string, Unique Index)
- `Position` (string)
- `Salary` (**decimal(18, 2)**) - 高精度十进制存储薪资。
- `HireDate` (**DateTimeOffset**) - 带时区的时间戳。
- `IsDeleted` (bool) - 用于实现软删除 (Soft Delete)。

#### 4.3 Table: `Resumes` (简历)

- `Id` (Guid, PK)
- `EmployeeId` (Guid, FK -> Employees.Id)
- `OriginalFileName` (string)
- `StoredFilePath` (string) - 存储在物理磁盘或 Blob 存储上的路径。
- `ParsedContent` (string, Nullable) - 存储 AI 解析后的文本数据。
- `UploadedAt` (DateTimeOffset)

### 5.0 API 端点定义 (Endpoint Definitions)

- `POST /api/auth/register` (Public)
- `POST /api/auth/login` (Public)
- `GET /api/employees` (Auth: Admin)
- `GET /api/employees/{id}` (Auth: Admin or Self)
- `POST /api/employees` (Auth: Admin)
- `PUT /api/employees/{id}` (Auth: Admin)
- `DELETE /api/employees/{id}` (Auth: Admin)
- `POST /api/employees/{id}/resumes` (Auth: Admin or Self) - 上传简历
- `GET /api/employees/{id}/resumes` (Auth: Admin or Self) - 查看简历信息

### 6.0 关键技术栈 (Tech Stack Checklist)

- [ ] **Framework**: .NET 8
- [ ] **Web API**: ASP.NET Core Controllers
- [ ] **API Filters**: ASP.NET Core Filters (用于响应包装)
- [ ] **Database**: PostgreSQL 16
- [ ] **ORM**: Entity Framework Core (EF Core) 8
- [ ] **Dependency Injection**: **Autofac** (用于高级模块化注册) + .NET 内置 DI
- [ ] **Authentication**: JWT (Bearer Token)
- [ ] **Authorization**: Policy-Based (基于策略)
- [ ] **Mapping**: **AutoMapper**
- [ ] **Unit Testing**: **xUnit** (测试框架) + **Moq** (模拟框架)
- [ ] **Integration Testing**: **WebApplicationFactory**
- [ ] **Containerization**: **Docker** (用于开发环境数据库)

## 第二部分：项目实施计划 (Project Implementation Plan)

### 🏁 阶段一：项目骨架 (Project Skeleton)

**目标**: 搭建符合 Clean Architecture 的项目结构，配置核心 DI 与中间件管道。

- **[ ] 任务 1.1 (环境搭建)**:
  - 安装 .NET 8 SDK (Win11)。
  - 安装 Docker Desktop 并运行 PostgreSQL 16 容器。
  - *验收标准*: 数据库客户端 (DBeaver/pgAdmin) 可成功连接 `localhost:5432`。
- **[ ] 任务 1.2 (创建解决方案)**:
  - 创建 `MiniHR` 空白解决方案 (`.sln`)。
  - 在解决方案中添加 `MiniHR.Domain`, `MiniHR.Application`, `MiniHR.Infrastructure`, `MiniHR.WebAPI` 四个项目。
  - *验收标准*: 解决方案编译通过，项目类型正确 (Class Library / Web API)。
- **[ ] 任务 1.3 (配置项目引用)**:
  - 严格按照 TDD 2.2 节的依赖图配置项目引用。
  - *验收标准*: `MiniHR.Domain` 项目无任何其他项目引用。
- **[ ] 任务 1.4 (集成 Autofac)**:
  - 在 `MiniHR.WebAPI` 中引入 `Autofac.Extensions.DependencyInjection`。
  - 修改 `Program.cs`，使用 `UseServiceProviderFactory(new AutofacServiceProviderFactory())` 替换默认 DI 容器。
  - *验收标准*: API 项目能正常启动，Autofac 接管 DI。
- **[ ] 任务 1.5 (配置 API 管道 - CORS)**:
  - 在 `MiniHR.WebAPI` 的 `Program.cs` 中配置 CORS 中间件 (`app.UseCors()`)。
  - 定义一个名为 "AllowSpecificOrigin" 的策略，允许来自 `http://localhost:3000` (未来 React/Vue 开发服务器) 的 `GET`, `POST`, `PUT`, `DELETE` 请求，并允许 `Authorization` 和 `Content-Type` 头部。
  - *验收标准*: 浏览器的 `OPTIONS` 预检请求能够成功通过。

### 💾 阶段二：数据持久化 (Data Persistence)

**目标**: 掌握 EF Core Code First 完整生命周期。

- **[ ] 任务 2.1 (定义实体)**:
  - 在 `MiniHR.Domain` 中编写 `User` 和 `Employee` 实体类。
  - *验收标准*: 实体类使用 `Guid` 作主键, `decimal` 和 `DateTimeOffset` 作为标准类型。
- **[ ] 任务 2.2 (配置 DbContext)**:
  - 在 `MiniHR.Infrastructure` 中安装 `Npgsql.EntityFrameworkCore.PostgreSQL`。
  - 编写 `MiniHrDbContext` 类，继承 `DbContext`。
  - 在 `OnModelCreating` 中使用 `Fluent API` 配置 `Salary` 的精度 (`HasPrecision(18, 2)`) 和 `Email` 的唯一索引。
- **[ ] 任务 2.3 (数据库迁移)**:
  - 在 `MiniHR.WebAPI` 的 `Program.cs` 中注册 `DbContext` (需从 `IConfiguration` 读取连接字符串)。
  - 运行 `dotnet ef migrations add InitialCreate --startup-project ../MiniHR.WebAPI`。
  - 运行 `dotnet ef database update --startup-project ../MiniHR.WebAPI`。
  - *验收标准*: 在 Postgres 数据库中看到 `Users` 和 `Employees` 表结构。

### 🔌 阶段三：业务逻辑与 API 管道 (Services & Middleware)

**目标**: 实现业务逻辑与 API 层的解耦，构建健壮的请求处理管道。

- **[ ] 任务 3.1 (Repository 模式)**:
  - 在 `MiniHR.Domain` 定义 `IEmployeeRepository` 接口。
  - 在 `MiniHR.Infrastructure` 实现 `EmployeeRepository`。
  - 使用 Autofac 配置 `EmployeeRepository` 对 `IEmployeeRepository` 的依赖注入。
- **[ ] 任务 3.2 (DTO 与 AutoMapper)**:
  - 在 `MiniHR.WebAPI` (或 `Application`) 中定义 `EmployeeDto` 和 `CreateEmployeeDto`。
  - 在 `MiniHR.Application` 中配置 AutoMapper，实现 `Employee` <=> `DTO` 的映射。
- **[ ] 任务 3.3 (Service 逻辑)**:
  - 在 `MiniHR.Application` 编写 `EmployeeService`，注入 `IEmployeeRepository` 和 `IMapper`。
- **[ ] 任务 3.4 (Controller 实现)**:
  - 在 `MiniHR.WebAPI` 编写 `EmployeesController`，注入 `IEmployeeService`。
  - 实现 `Get` 和 `Post` 端点。
- **[ ] 任务 3.5 (统一响应模型 - 定义)**:
  - 在 `MiniHR.WebAPI` 中定义 `ApiResponse<T>` 和 `ApiError` 类，作为 TDD 3.0 节中定义的标准“数据信封”。
- **[ ] 任务 3.6 (自定义中间件 - 全局异常)**:
  - 在 `MiniHR.WebAPI` 中实现 `ExceptionMiddleware` (或 `IExceptionHandler`)。
  - 在 `Program.cs` 的管道**最顶层**注册它。
  - *验收标准*: 故意在 Service 抛出异常，API 响应为 `{ "success": false, "data": null, "error": { "code": 500, "message": ... } }`。
- **[ ] 任务 3.7 (自定义过滤器 - 成功响应)**:
  - 在 `MiniHR.WebAPI` 中实现一个 `SuccessResponseFilter` (继承 `IResultFilter`)。
  - 该过滤器拦截 `OkObjectResult` (即 `Ok(data)`)，并将其包装为 `Ok(new ApiResponse<T> { Success = true, Data = data })`。
  - 在 `Program.cs` 中全局注册此过滤器。
  - *验收标准*: 成功调用 `GET /api/employees` 返回的 JSON 结构符合 `ApiResponse<T>`。

### 🛡️ 阶段四：安全与鉴权 (Security & Authorization)

**目标**: 保护 API 端点，实现基于角色的访问控制。

- **[ ] 任务 4.1 (密码哈希)**:
  - 安装 `BCrypt.Net-Next`。
  - 在 `Infrastructure` 中实现 `AuthService` (含 `Register` 和 `Login` 方法)，确保注册时哈希密码，登录时验证哈希。
- **[ ] 任务 4.2 (JWT 生成与验证)**:
  - 在 `Infrastructure` 中实现 JWT Token 生成逻辑 (需从 `IConfiguration` 读取密钥)。
  - 在 `Program.cs` 中配置 `app.UseAuthentication()` 和 `JwtBearer` 验证。
- **[ ] 任务 4.3 (Policy 策略)**:
  - 在 `Program.cs` 中定义 `Authorization` 策略，例如 `Policy("AdminOnly")`，要求 Role Claim 必须为 "Admin"。
  - *验收标准*: `[Authorize(Policy = "AdminOnly")]` 应用于 `GET /api/employees`，非 Admin Token 访问返回 403 Forbidden。

### 🚀 阶段五：高级功能与质量保证 (Advanced Features & QA)

**目标**: 掌握文件处理和多维度测试，为 AI 项目集成做准备。

- **[ ] 任务 5.1 (文件上传)**:
  - 在 `Infrastructure` 中实现 `FileService` (使用 `System.IO` 将 `IFormFile` 保存到本地磁盘)。
  - 在 `ResumesController` 中实现文件上传端点。
  - *验收标准*: 上传 PDF 成功，`Resumes` 表中记录了正确的文件路径。
- **[ ] 任务 5.2 (单元测试 - xUnit & Moq)**:
  - 创建 `MiniHR.Tests.Unit` (xUnit) 项目。
  - 安装 `Moq`。
  - **编写第一个测试**:
    - *目标*: `EmployeeService.CreateEmployee`。
    - *Arrange*: `Mock<IEmployeeRepository>`，模拟 `GetByEmailAsync` 返回 `null`。
    - *Act*: 调用 `service.CreateEmployee(...)`。
    - *Assert*: 验证 `repository.AddAsync` **被调用了恰好一次** (`Verify(..., Times.Once)`)。
- **[ ] 任务 5.3 (集成测试 - WebApplicationFactory)**:
  - 创建 `MiniHR.Tests.Integration` (xUnit) 项目。
  - 安装 `Microsoft.AspNetCore.Mvc.Testing`。
  - **编写第一个测试**:
    - *目标*: `POST /api/employees` 端点。
    - *Arrange*: 创建 `WebApplicationFactory` 和 `HttpClient`。创建一个 `CreateEmployeeDto`。
    - *Act*: `await client.PostAsJsonAsync("/api/employees", dto)`。
    - *Assert*: 验证 HTTP 响应状态码为 `201 Created`，并从测试数据库中确认该员工已被创建。