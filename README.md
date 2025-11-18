# 📘 Mini HR API - 技术设计与实施计划 (v3.4)

**版本说明**: v3.4 (导师修正版)。此版本废除了 TDD 3.7 (自动过滤器)，确立了“手动控制响应”和“HTTP 200 策略”的核心原则，旨在构建一个逻辑清晰、高度可控的 API 系统。

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
  API /           MiniHR.WebAPI              - Controllers (控制器), Middleware (中间件)
Presentation                               - DTOs (数据传输对象), API Model Validation (模型验证)
 (展示层)                                  - Program.cs (DI容器配置, 管道配置)
                                           - **Models/ApiResult.cs (统一响应模型)**
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

### 3.0 横切关注点 (Cross-Cutting Concerns) - (核心架构决策)

ASP.NET Core 管道将用于处理**意外**的全局异常和基础设施（如 CORS/Auth），但**业务响应的包装**将**手动**在 Controller 中完成。

**HTTP 响应黄金准则 (200-Only Strategy):**

1. 所有 API 端点在**业务层面**（包括业务失败、参数错误、系统异常）**永远**返回 **HTTP 200 OK**。
2. 客户端**永远**不依赖 HTTP 状态码判断业务结果。
3. 客户端**必须**解析 JSON 响应体中的 `Success` (布尔值) 和 `Code` (数字) 字段。

#### 3.1 统一响应模型 (Unified Response Model) (TDD 3.5)

- **实现**: 定义 `ApiResult<T>` 类作为**所有**响应的“数据信封”。

- **目标**: 无论成功或失败，前端始终能解析同一个 JSON 结构。

- **结构定义** (`MiniHR.WebAPI/Models/ApiResult.cs`):

  ```
  public class ApiResult<T>
  {
      // 业务是否成功
      public bool Success { get; set; }
  
      // 自定义业务状态码 (复用 HTTP 语义: 200, 201, 400, 401, 403, 404, 500)
      public int Code { get; set; }
  
      // 成功时的数据
      public T? Data { get; set; }
  
      // 失败时的错误信息
      public object? Error { get; set; }
  
      // (可选) 调试或提示消息
      public string? Message { get; set; }
  }
  ```

#### 3.2 全局异常处理 (Global Exception Handling) (TDD 3.6)

- **实现**: 自定义 `ExceptionMiddleware`。
- **目标**: 仅用于捕获**未处理的、意外的**异常（例如 `NullReferenceException`、数据库连接超时等）。这是系统的最后一道防线。
- **行为**:
  1. `try...catch` 捕获管道中的异常。
  2. 记录 **Error** 级别日志（包含堆栈信息）。
  3. **手动**构建并返回一个 `ApiResult<object>`：
     - `Success = false`
     - `Code = 500`
     - `Error = "服务器内部错误，请联系管理员"` (隐藏堆栈细节)
  4. **关键**: 将 Response 的 HTTP StatusCode 设为 **200**。

#### 3.3 业务逻辑验证 (Business Logic Validation)

- **实现**: **在 Controller 方法内部**手动实现。
- **目标**: 处理**已知的、预期的**业务分支（例如 参数校验失败、资源未找到、权限不足）。
- **行为**:
  1. Controller 检查 `ModelState.IsValid`。如果不通过，返回 `ApiResult` (`Code=400`, `Success=false`, `Error=校验错误详情`)。
  2. Controller 调用 Service。如果 Service 返回 null (未找到)，返回 `ApiResult` (`Code=404`, `Success=false`)。
  3. **关键**: 这些“错误”在 HTTP 层面依然是 **200 OK**。

#### 3.4 CORS (Cross-Origin Resource Sharing)

- **实现**: `app.UseCors()`。
- **策略**: "AllowSpecificOrigin"。允许前端开发服务器 (`http://localhost:3000`) 访问，并显式允许 `Authorization` 和 `Content-Type` 头部。

#### 3.5 Authentication & Authorization

- **实现**: `app.UseAuthentication()` + `app.UseAuthorization()`。
- **行为**: 401 (未登录) 和 403 (无权限) 通常由框架在中间件层直接返回。
  - *注*: 为了保持 200-Only 策略的绝对统一，高级做法是自定义 `JwtBearerEvents` 来拦截 401/403 并重写为 200 OK 的 `ApiResult`。在本项目初期，我们可以暂且容忍 401/403 作为 HTTP 状态码存在，或者在后续进阶任务中统一处理。

### 4.0 数据库设计 (Database Design - PostgreSQL)

采用 **EF Core Code-First** 模式。

#### 4.1 Table: `Users`

- `Id` (Guid, PK)
- `Username` (string, Unique Index)
- `PasswordHash` (string)
- `Role` (string) - (e.g., "Admin", "User")

#### 4.2 Table: `Employees`

- `Id` (Guid, PK)
- `FirstName` (string)
- `LastName` (string)
- `Email` (string, Unique Index)
- `Position` (string)
- `Salary` (**decimal(18, 2)**)
- `HireDate` (**DateTimeOffset**)
- `IsDeleted` (bool) - 软删除

#### 4.3 Table: `Resumes`

- `Id` (Guid, PK)
- `EmployeeId` (Guid, FK -> Employees.Id)
- `OriginalFileName` (string)
- `StoredFilePath` (string) - 物理路径
- `ParsedContent` (string, Nullable) - AI 解析文本
- `UploadedAt` (DateTimeOffset)

### 5.0 API 端点定义 (Endpoint Definitions)

所有端点 (除了可能的 401/403) **永远**返回 **HTTP 200 OK**。

- `POST /api/auth/register` -> `ApiResult<object>`
- `POST /api/auth/login` -> `ApiResult<string>` (Data 为 Token)
- `GET /api/employees` -> `ApiResult<IEnumerable<EmployeeDto>>`
- `GET /api/employees/{id}` -> `ApiResult<EmployeeDto>`
- `POST /api/employees` -> `ApiResult<EmployeeDto>` (Code: 201)
- `PUT /api/employees/{id}` -> `ApiResult<EmployeeDto>`
- `DELETE /api/employees/{id}` -> `ApiResult<object>` (Code: 204)
- `POST /api/employees/{id}/resumes` -> `ApiResult<ResumeDto>`

### 6.0 关键技术栈 (Tech Stack Checklist)

- [ ] **Framework**: .NET 8
- [ ] **Web API**: ASP.NET Core Controllers
- [ ] **Database**: PostgreSQL 16
- [ ] **ORM**: Entity Framework Core (EF Core) 8
- [ ] **Dependency Injection**: **Autofac**
- [ ] **Authentication**: JWT (Bearer Token)
- [ ] **Authorization**: Policy-Based
- [ ] **Mapping**: **AutoMapper**
- [ ] **Testing**: **xUnit** + **Moq** + **WebApplicationFactory**

## 第二部分：项目实施计划 (Project Implementation Plan)

### 🏁 阶段一：项目骨架 (Project Skeleton) - [已完成]

**目标**: 搭建符合 Clean Architecture 的项目结构，配置核心 DI 与中间件管道。

- **[x] 任务 1.1 ~ 1.5**: (环境, Solution, 引用, Autofac, CORS) - **已就绪**

### 💾 阶段二：数据持久化 (Data Persistence) - [已完成]

**目标**: 掌握 EF Core Code First 完整生命周期。

- **[ ] 任务 2.1 (定义实体)**:
  - 在 `Domain` 层编写 `User`, `Employee`, `Resume`。
- **[ ] 任务 2.2 (配置 DbContext)**:
  - 在 `Infra` 层编写 `MiniHrDbContext`，配置 Fluent API (精度, 索引)。
- **[ ] 任务 2.3 (数据库迁移)**:
  - 注册 DbContext，执行 Migration，生成数据库。

### 🔌 阶段三：业务逻辑与 API 管道 (Services & Middleware)

**目标**: 实现业务逻辑，构建统一响应体系。

- **[ ] 任务 3.1 (Repository 模式)**:
  - 定义并实现 `IEmployeeRepository`。
- **[ ] 任务 3.2 (DTO 与 AutoMapper)**:
  - 定义 DTOs，配置 AutoMapper 映射。
- **[ ] 任务 3.3 (Service 逻辑)**:
  - 编写 `EmployeeService`，实现核心业务。
- **[ ] 任务 3.4 (统一响应模型 - 定义)**:
  - 在 `MiniHR.WebAPI/Models` 中定义 `ApiResult<T>` 和 `ApiError`。
- **[ ] 任务 3.5 (Controller 实现)**:
  - 编写 `EmployeesController`。
  - **严格遵守**: 所有 Action 必须返回 `ApiResult<T>`。
  - **手动处理**: `ModelState` 检查、`404` 检查、成功响应包装。
- **[ ] 任务 3.6 (自定义中间件 - 全局异常)**:
  - 实现 `ExceptionMiddleware`。
  - 捕获意外异常 -> 记录日志 -> 返回 `200 OK` 的 `ApiResult` (Code 500)。

### 🛡️ 阶段四：安全与鉴权 (Security & Authorization)

**目标**: 保护 API 端点。

- **[ ] 任务 4.1 (密码哈希)**: `BCrypt.Net` 实现。
- **[ ] 任务 4.2 (JWT)**: Token 生成与验证。
- **[ ] 任务 4.3 (Policy)**: 配置 "AdminOnly" 策略。

### 🚀 阶段五：高级功能与质量保证 (QA)

**目标**: 文件处理与测试。

- **[ ] 任务 5.1 (文件上传)**: 实现简历上传与存储。
- **[ ] 任务 5.2 (单元测试)**: xUnit + Moq 测试 Service 逻辑。
- **[ ] 任务 5.3 (集成测试)**: WebApplicationFactory 测试 API 端点 (验证 200-Only 策略是否生效)。