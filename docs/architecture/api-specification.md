# ClawFlgma API 接口规范文档

## 文档信息

- **项目名称**: ClawFlgma - 云原生设计协作平台
- **版本**: v1.0
- **创建日期**: 2026-03-20
- **API 基础路径**: `https://api.clawflgma.com/v1`
- **相关文档**: [微服务架构设计](./microservices-design.md)

---

## 一、API 设计原则

### 1.1 RESTful 设计规范

- **资源导向**: 以资源为中心设计 API
- **HTTP 方法语义**: GET(查询)、POST(创建)、PUT(更新)、DELETE(删除)
- **统一响应格式**: JSON 格式,统一状态码
- **版本控制**: URL 路径版本控制 (`/v1/api/resource`)
- **分页查询**: 统一分页参数和响应格式

### 1.2 认证授权

- **认证方式**: JWT Bearer Token
- **Token 格式**: `Authorization: Bearer {access_token}`
- **Token 有效期**: Access Token 2 小时, Refresh Token 7 天
- **权限控制**: RBAC (Role-Based Access Control)

### 1.3 HTTP 状态码

| 状态码 | 含义 | 使用场景 |
|--------|------|---------|
| 200 OK | 成功 | GET、PUT、PATCH 成功 |
| 201 Created | 已创建 | POST 创建资源成功 |
| 204 No Content | 无内容 | DELETE 成功 |
| 400 Bad Request | 错误请求 | 参数验证失败 |
| 401 Unauthorized | 未认证 | Token 无效或过期 |
| 403 Forbidden | 无权限 | 无访问权限 |
| 404 Not Found | 未找到 | 资源不存在 |
| 409 Conflict | 冲突 | 资源冲突(如重复创建) |
| 422 Unprocessable Entity | 无法处理 | 业务逻辑错误 |
| 429 Too Many Requests | 请求过多 | 触发限流 |
| 500 Internal Server Error | 服务器错误 | 系统异常 |

---

## 二、统一响应格式

### 2.1 成功响应

```json
{
  "success": true,
  "data": {
    // 响应数据
  },
  "timestamp": "2026-03-20T10:30:00Z"
}
```

### 2.2 分页响应

```json
{
  "success": true,
  "data": {
    "items": [],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 100,
      "totalPages": 5
    }
  },
  "timestamp": "2026-03-20T10:30:00Z"
}
```

### 2.3 错误响应

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "参数验证失败",
    "details": [
      {
        "field": "email",
        "message": "邮箱格式不正确"
      }
    ]
  },
  "timestamp": "2026-03-20T10:30:00Z"
}
```

---

## 三、认证服务 API

### 3.1 用户注册

**POST** `/api/auth/register`

#### 请求参数

```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "displayName": "John Doe"
}
```

#### 参数说明

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string | 是 | 邮箱地址 |
| password | string | 是 | 密码(8-32位,包含大小写字母和数字) |
| displayName | string | 是 | 显示名称(2-50字符) |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "displayName": "John Doe",
      "createdAt": "2026-03-20T10:30:00Z"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
      "expiresIn": 7200
    }
  }
}
```

#### 错误码

| 错误码 | 说明 |
|--------|------|
| EMAIL_ALREADY_EXISTS | 邮箱已被注册 |
| INVALID_EMAIL_FORMAT | 邮箱格式不正确 |
| PASSWORD_TOO_WEAK | 密码强度不足 |

---

### 3.2 用户登录

**POST** `/api/auth/login`

#### 请求参数

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "displayName": "John Doe",
      "avatarUrl": "https://cdn.clawflgma.com/avatars/default.png"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
      "expiresIn": 7200
    }
  }
}
```

---

### 3.3 刷新令牌

**POST** `/api/auth/refresh`

#### 请求头

```
Authorization: Bearer {refresh_token}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 7200
  }
}
```

---

### 3.4 登出

**POST** `/api/auth/logout`

#### 请求头

```
Authorization: Bearer {access_token}
```

#### 响应示例

```json
{
  "success": true,
  "data": null
}
```

---

### 3.5 OAuth 第三方登录

**GET** `/api/auth/oauth/{provider}`

#### 路径参数

| 参数 | 说明 |
|------|------|
| provider | OAuth 提供商(github、google、wechat) |

#### 响应

```
HTTP 302 Redirect
Location: https://github.com/login/oauth/authorize?client_id=xxx&...
```

---

### 3.6 OAuth 回调

**GET** `/api/auth/oauth/callback?code={code}&state={state}`

#### 查询参数

| 参数 | 说明 |
|------|------|
| code | OAuth 授权码 |
| state | 防 CSRF 状态码 |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "displayName": "John Doe"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
      "expiresIn": 7200
    }
  }
}
```

---

## 四、用户服务 API

### 4.1 获取当前用户信息

**GET** `/api/users/me`

#### 请求头

```
Authorization: Bearer {access_token}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "displayName": "John Doe",
    "avatarUrl": "https://cdn.clawflgma.com/avatars/user123.png",
    "bio": "UI Designer",
    "location": "Beijing, China",
    "website": "https://johndoe.com",
    "createdAt": "2026-01-15T08:00:00Z",
    "lastLoginAt": "2026-03-20T10:30:00Z"
  }
}
```

---

### 4.2 更新用户信息

**PUT** `/api/users/me`

#### 请求参数

```json
{
  "displayName": "John Doe Updated",
  "bio": "Senior UI Designer",
  "location": "Shanghai, China",
  "website": "https://johndoe.dev"
}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "displayName": "John Doe Updated",
    "bio": "Senior UI designer",
    "location": "Shanghai, China",
    "website": "https://johndoe.dev",
    "updatedAt": "2026-03-20T10:35:00Z"
  }
}
```

---

### 4.3 上传头像

**POST** `/api/users/me/avatar`

#### 请求格式

```
Content-Type: multipart/form-data
```

#### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| file | file | 是 | 图片文件(JPG/PNG, 最大 5MB) |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "avatarUrl": "https://cdn.clawflgma.com/avatars/user123-new.png"
  }
}
```

---

### 4.4 获取用户团队列表

**GET** `/api/users/me/teams`

#### 响应示例

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "name": "Design Team",
        "description": "UI/UX Design Team",
        "memberCount": 12,
        "role": "admin",
        "createdAt": "2026-01-20T09:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 3,
      "totalPages": 1
    }
  }
}
```

---

## 五、项目服务 API

### 5.1 创建项目

**POST** `/api/projects`

#### 请求参数

```json
{
  "name": "Mobile App Design",
  "description": "iOS and Android app design project",
  "visibility": "team",
  "teamId": "550e8400-e29b-41d4-a716-446655440001"
}
```

#### 参数说明

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 项目名称(1-100字符) |
| description | string | 否 | 项目描述(最多 500 字符) |
| visibility | string | 是 | 可见性(private、team、public) |
| teamId | string | 否 | 所属团队 ID |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Mobile App Design",
    "description": "iOS and Android app design project",
    "visibility": "team",
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "teamId": "550e8400-e29b-41d4-a716-446655440001",
    "memberCount": 1,
    "fileCount": 0,
    "createdAt": "2026-03-20T10:40:00Z",
    "lastModified": "2026-03-20T10:40:00Z"
  }
}
```

---

### 5.2 获取项目列表

**GET** `/api/projects?page=1&pageSize=20&visibility=team`

#### 查询参数

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| page | int | 否 | 1 | 页码 |
| pageSize | int | 否 | 20 | 每页数量(最大 100) |
| visibility | string | 否 | - | 过滤可见性 |
| search | string | 否 | - | 搜索关键词 |
| teamId | string | 否 | - | 过滤团队 |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440002",
        "name": "Mobile App Design",
        "description": "iOS and Android app design project",
        "visibility": "team",
        "owner": {
          "id": "550e8400-e29b-41d4-a716-446655440000",
          "displayName": "John Doe",
          "avatarUrl": "https://cdn.clawflgma.com/avatars/user123.png"
        },
        "team": {
          "id": "550e8400-e29b-41d4-a716-446655440001",
          "name": "Design Team"
        },
        "memberCount": 5,
        "fileCount": 12,
        "thumbnail": "https://cdn.clawflgma.com/thumbnails/project-123.png",
        "createdAt": "2026-03-20T10:40:00Z",
        "lastModified": "2026-03-20T11:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 8,
      "totalPages": 1
    }
  }
}
```

---

### 5.3 获取项目详情

**GET** `/api/projects/{projectId}`

#### 路径参数

| 参数 | 说明 |
|------|------|
| projectId | 项目 ID |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Mobile App Design",
    "description": "iOS and Android app design project",
    "visibility": "team",
    "owner": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "displayName": "John Doe",
      "email": "user@example.com",
      "avatarUrl": "https://cdn.clawflgma.com/avatars/user123.png"
    },
    "team": {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Design Team",
      "memberCount": 12
    },
    "members": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440003",
        "displayName": "Jane Smith",
        "email": "jane@example.com",
        "role": "editor",
        "joinedAt": "2026-03-18T14:00:00Z"
      }
    ],
    "files": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440010",
        "name": "Homepage.fig",
        "type": "design",
        "thumbnail": "https://cdn.clawflgma.com/thumbnails/file-123.png",
        "lastModified": "2026-03-20T11:00:00Z"
      }
    ],
    "createdAt": "2026-03-20T10:40:00Z",
    "lastModified": "2026-03-20T11:00:00Z"
  }
}
```

---

### 5.4 更新项目

**PUT** `/api/projects/{projectId}`

#### 请求参数

```json
{
  "name": "Mobile App Design v2",
  "description": "Updated description",
  "visibility": "private"
}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Mobile App Design v2",
    "description": "Updated description",
    "visibility": "private",
    "updatedAt": "2026-03-20T10:45:00Z"
  }
}
```

---

### 5.5 删除项目

**DELETE** `/api/projects/{projectId}`

#### 响应示例

```json
{
  "success": true,
  "data": null
}
```

---

### 5.6 添加项目成员

**POST** `/api/projects/{projectId}/members`

#### 请求参数

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440003",
  "role": "editor"
}
```

#### 参数说明

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| userId | string | 是 | 用户 ID |
| role | string | 是 | 角色(admin、editor、viewer) |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "displayName": "Jane Smith",
    "email": "jane@example.com",
    "role": "editor",
    "joinedAt": "2026-03-20T10:50:00Z"
  }
}
```

---

## 六、设计服务 API

### 6.1 创建设计文档

**POST** `/api/designs`

#### 请求参数

```json
{
  "projectId": "550e8400-e29b-41d4-a716-446655440002",
  "name": "Homepage",
  "description": "Main homepage design",
  "width": 1920,
  "height": 1080
}
```

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "projectId": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Homepage",
    "description": "Main homepage design",
    "rootNode": {
      "id": "frame-root",
      "type": "frame",
      "name": "Homepage",
      "transform": {
        "x": 0,
        "y": 0,
        "width": 1920,
        "height": 1080,
        "rotation": 0
      },
      "children": []
    },
    "components": [],
    "styles": {},
    "version": 1,
    "createdAt": "2026-03-20T10:55:00Z",
    "lastModified": "2026-03-20T10:55:00Z"
  }
}
```

---

### 6.2 获取设计文档

**GET** `/api/designs/{designId}?version=5`

#### 路径参数

| 参数 | 说明 |
|------|------|
| designId | 设计文档 ID |

#### 查询参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| version | int | 否 | 指定版本号(默认最新) |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "projectId": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Homepage",
    "description": "Main homepage design",
    "rootNode": {
      "id": "frame-root",
      "type": "frame",
      "name": "Homepage",
      "transform": {
        "x": 0,
        "y": 0,
        "width": 1920,
        "height": 1080,
        "rotation": 0
      },
      "children": [
        {
          "id": "rect-header",
          "type": "rectangle",
          "name": "Header",
          "transform": {
            "x": 0,
            "y": 0,
            "width": 1920,
            "height": 80
          },
          "properties": {
            "fill": "#FFFFFF",
            "stroke": "#E0E0E0",
            "strokeWidth": 1
          }
        }
      ]
    },
    "components": [
      {
        "id": "comp-button",
        "name": "Button",
        "rootNode": {}
      }
    ],
    "styles": {
      "primary-color": {
        "type": "color",
        "value": "#007AFF"
      }
    },
    "version": 5,
    "createdAt": "2026-03-20T10:55:00Z",
    "lastModified": "2026-03-20T11:05:00Z",
    "createdBy": "550e8400-e29b-41d4-a716-446655440000",
    "lastModifiedBy": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

---

### 6.3 更新设计文档

**PUT** `/api/designs/{designId}`

#### 请求参数

```json
{
  "rootNode": {
    "id": "frame-root",
    "type": "frame",
    "name": "Homepage Updated",
    "transform": {
      "x": 0,
      "y": 0,
      "width": 1920,
      "height": 1200
    },
    "children": []
  },
  "expectedVersion": 5
}
```

#### 参数说明

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| rootNode | object | 是 | 根节点 |
| components | array | 否 | 组件列表 |
| styles | object | 否 | 样式字典 |
| expectedVersion | int | 是 | 乐观锁版本号 |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "version": 6,
    "lastModified": "2026-03-20T11:10:00Z"
  }
}
```

---

### 6.4 获取设计文档历史版本

**GET** `/api/designs/{designId}/versions?page=1&pageSize=20`

#### 响应示例

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "version": 6,
        "description": "Updated homepage height",
        "createdBy": {
          "id": "550e8400-e29b-41d4-a716-446655440000",
          "displayName": "John Doe"
        },
        "createdAt": "2026-03-20T11:10:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 6,
      "totalPages": 1
    }
  }
}
```

---

### 6.5 导出设计资源

**POST** `/api/designs/{designId}/export`

#### 请求参数

```json
{
  "format": "png",
  "scale": 2,
  "nodeIds": ["frame-root", "rect-header"],
  "includeAssets": true
}
```

#### 参数说明

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| format | string | 是 | 导出格式(png、jpg、svg、pdf) |
| scale | int | 否 | 缩放倍数(1、2、3),默认 1 |
| nodeIds | array | 否 | 指定节点 ID(默认全部) |
| includeAssets | boolean | 否 | 是否包含资源文件,默认 false |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "exportId": "export-550e8400-e29b-41d4-a716-446655440020",
    "status": "processing",
    "estimatedTime": 10
  }
}
```

---

### 6.6 获取导出结果

**GET** `/api/designs/exports/{exportId}`

#### 响应示例

```json
{
  "success": true,
  "data": {
    "exportId": "export-550e8400-e29b-41d4-a716-446655440020",
    "status": "completed",
    "files": [
      {
        "nodeName": "Homepage",
        "fileName": "Homepage@2x.png",
        "downloadUrl": "https://cdn.clawflgma.com/exports/export-123/Homepage@2x.png",
        "fileSize": 245678,
        "expiresAt": "2026-03-20T12:10:00Z"
      }
    ]
  }
}
```

---

## 七、协作服务 API (SignalR)

### 7.1 SignalR 连接

**端点**: `wss://api.clawflgma.com/collaboration`

#### 连接参数

```
?access_token={jwt_token}
```

### 7.2 SignalR 事件

#### 客户端 → 服务端

| 事件名 | 参数 | 说明 |
|--------|------|------|
| JoinDocument | documentId: string | 加入文档协作 |
| LeaveDocument | documentId: string | 离开文档协作 |
| SendOperation | documentId: string, operation: object | 发送编辑操作 |
| SendCursor | documentId: string, cursor: object | 发送光标位置 |

#### 服务端 → 客户端

| 事件名 | 参数 | 说明 |
|--------|------|------|
| DocumentSync | document: object | 文档完整同步 |
| OperationReceived | operation: object | 接收远程操作 |
| OperationAcknowledged | operationId: string | 操作确认 |
| UserJoined | user: object | 用户加入 |
| UserLeft | userId: string | 用户离开 |
| CursorMoved | userId: string, cursor: object | 光标移动 |
| OnlineUsers | users: array | 在线用户列表 |

### 7.3 操作数据结构

```typescript
interface CollaborationOperation {
  operationId: string;
  type: 'insert' | 'update' | 'delete' | 'move';
  targetNodeId: string;
  payload: any;
  properties?: Record<string, any>;
  userId: string;
  timestamp: number;
  revision: number;
}

interface CursorPosition {
  x: number;
  y: number;
  nodeId?: string;
  selection?: string[];
}
```

### 7.4 示例流程

```typescript
// 客户端连接
const connection = new signalR.HubConnectionBuilder()
  .withUrl('wss://api.clawflgma.com/collaboration', {
    accessTokenFactory: () => 'your-jwt-token'
  })
  .build();

await connection.start();

// 加入文档
await connection.invoke('JoinDocument', '507f1f77bcf86cd799439011');

// 监听事件
connection.on('DocumentSync', (document) => {
  console.log('Document synced:', document);
});

connection.on('OperationReceived', (operation) => {
  console.log('Remote operation:', operation);
  applyOperation(operation);
});

// 发送操作
await connection.invoke('SendOperation', '507f1f77bcf86cd799439011', {
  operationId: 'op-123',
  type: 'update',
  targetNodeId: 'rect-header',
  properties: {
    fill: '#007AFF'
  }
});
```

---

## 八、资源服务 API

### 8.1 上传资源文件

**POST** `/api/assets/upload`

#### 请求格式

```
Content-Type: multipart/form-data
```

#### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| file | file | 是 | 文件(最大 50MB) |
| projectId | string | 是 | 项目 ID |
| type | string | 否 | 资源类型(image、font、icon) |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440030",
    "fileName": "logo.png",
    "fileUrl": "https://cdn.clawflgma.com/assets/project-123/logo.png",
    "fileSize": 102400,
    "mimeType": "image/png",
    "width": 512,
    "height": 512,
    "uploadedAt": "2026-03-20T11:15:00Z"
  }
}
```

---

### 8.2 获取资源列表

**GET** `/api/assets?projectId={projectId}&type=image`

#### 查询参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| projectId | string | 是 | 项目 ID |
| type | string | 否 | 资源类型 |
| search | string | 否 | 搜索文件名 |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440030",
        "fileName": "logo.png",
        "fileUrl": "https://cdn.clawflgma.com/assets/project-123/logo.png",
        "thumbnail": "https://cdn.clawflgma.com/thumbnails/asset-123.png",
        "fileSize": 102400,
        "mimeType": "image/png",
        "uploadedAt": "2026-03-20T11:15:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 15,
      "totalPages": 1
    }
  }
}
```

---

### 8.3 删除资源

**DELETE** `/api/assets/{assetId}`

#### 响应示例

```json
{
  "success": true,
  "data": null
}
```

---

## 九、通知服务 API

### 9.1 获取通知列表

**GET** `/api/notifications?page=1&pageSize=20&unreadOnly=false`

#### 查询参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| page | int | 否 | 页码 |
| pageSize | int | 否 | 每页数量 |
| unreadOnly | boolean | 否 | 仅未读通知 |

#### 响应示例

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440040",
        "type": "design_updated",
        "title": "设计已更新",
        "message": "Homepage 设计已被 Jane 更新",
        "read": false,
        "data": {
          "designId": "507f1f77bcf86cd799439011",
          "designName": "Homepage"
        },
        "createdAt": "2026-03-20T11:10:00Z"
      }
    ],
    "unreadCount": 3,
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "total": 25,
      "totalPages": 2
    }
  }
}
```

---

### 9.2 标记通知为已读

**PUT** `/api/notifications/{notificationId}/read`

#### 响应示例

```json
{
  "success": true,
  "data": null
}
```

---

### 9.3 标记所有通知为已读

**PUT** `/api/notifications/read-all`

#### 响应示例

```json
{
  "success": true,
  "data": {
    "updatedCount": 3
  }
}
```

---

## 十、错误码参考

### 10.1 通用错误码

| 错误码 | 说明 |
|--------|------|
| INTERNAL_ERROR | 内部服务器错误 |
| VALIDATION_ERROR | 参数验证失败 |
| UNAUTHORIZED | 未认证 |
| FORBIDDEN | 无权限 |
| NOT_FOUND | 资源不存在 |
| RATE_LIMIT_EXCEEDED | 请求频率超限 |

### 10.2 认证错误码

| 错误码 | 说明 |
|--------|------|
| EMAIL_ALREADY_EXISTS | 邮箱已被注册 |
| INVALID_CREDENTIALS | 用户名或密码错误 |
| TOKEN_EXPIRED | Token 已过期 |
| TOKEN_INVALID | Token 无效 |
| OAUTH_FAILED | OAuth 认证失败 |

### 10.3 项目错误码

| 错误码 | 说明 |
|--------|------|
| PROJECT_NOT_FOUND | 项目不存在 |
| PROJECT_NAME_DUPLICATE | 项目名称重复 |
| MEMBER_LIMIT_EXCEEDED | 成员数量超限 |

### 10.4 设计错误码

| 错错码 | 说明 |
|--------|------|
| DESIGN_NOT_FOUND | 设计文档不存在 |
| VERSION_CONFLICT | 版本冲突 |
| NODE_NOT_FOUND | 节点不存在 |
| EXPORT_FAILED | 导出失败 |

---

## 十一、API 调用示例

### 11.1 JavaScript/TypeScript

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.clawflgma.com/v1',
  timeout: 30000
});

// 请求拦截器 - 添加 Token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 响应拦截器 - 处理错误
api.interceptors.response.use(
  response => response.data,
  error => {
    if (error.response?.status === 401) {
      // Token 过期,刷新 Token
      return refreshToken().then(() => {
        error.config.headers.Authorization = `Bearer ${newToken}`;
        return api.request(error.config);
      });
    }
    return Promise.reject(error);
  }
);

// API 调用示例
async function getProject(projectId: string) {
  return await api.get(`/api/projects/${projectId}`);
}

async function createDesign(data: CreateDesignRequest) {
  return await api.post('/api/designs', data);
}
```

### 11.2 C# (.NET)

```csharp
using HttpClient client = new HttpClient();
client.BaseAddress = new Uri("https://api.clawflgma.com/v1");
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);

// 获取项目
var response = await client.GetAsync($"/api/projects/{projectId}");
var project = await response.Content.ReadFromJsonAsync<ApiResponse<Project>>();

// 创建设计
var design = new { projectId, name = "Homepage" };
var createResponse = await client.PostAsJsonAsync("/api/designs", design);
var result = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Design>>();
```

---

## 十二、API 性能优化

### 12.1 缓存策略

| 资源类型 | 缓存时间 | 缓存位置 |
|---------|---------|---------|
| 用户信息 | 1 小时 | Redis + 客户端 |
| 项目列表 | 10 分钟 | Redis |
| 设计文档 | 10 分钟 | Redis |
| 静态资源 | 1 年 | CDN |

### 12.2 限流策略

| API 类型 | 限流规则 |
|---------|---------|
| 认证 API | 10 次/分钟 |
| 读取 API | 100 次/分钟 |
| 写入 API | 50 次/分钟 |
| 导出 API | 10 次/小时 |

---

## 十三、API 版本控制

### 13.1 版本策略

- **URL 版本**: `/v1/api/resource`
- **兼容性**: 保持向后兼容,重大变更发布新版本
- **生命周期**: 旧版本维护 12 个月后下线

### 13.2 版本迁移

```json
// 响应头提示版本升级
{
  "X-API-Version": "v1",
  "X-API-Deprecated": "true",
  "X-API-Sunset": "2027-03-20",
  "Link": "</v2/api/projects>; rel=\"successor-version\""
}
```

---

## 十四、总结

本 API 接口规范文档详细定义了 ClawFlgma 系统的:

✅ **设计原则**: RESTful 规范、统一响应格式、JWT 认证

✅ **核心服务 API**: 认证、用户、项目、设计、协作、资源、通知

✅ **实时通信**: SignalR WebSocket 事件定义

✅ **错误处理**: 统一错误码和响应格式

✅ **性能优化**: 缓存策略、限流控制

✅ **版本管理**: URL 版本控制、兼容性策略

下一步将设计部署与运维方案。
