namespace ClawFlgma.Shared;

/// <summary>
/// 统一API响应结构
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 响应码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 请求追踪ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Success(T? data = default, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Msg = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建成功响应（自定义消息）
    /// </summary>
    public static ApiResponse<T> Success(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Msg = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message, int code = 400)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Msg = message,
            Data = default
        };
    }

    /// <summary>
    /// 创建未授权响应
    /// </summary>
    public static ApiResponse<T> Unauthorized(string message = "未授权访问")
    {
        return new ApiResponse<T>
        {
            Code = 401,
            Msg = message,
            Data = default
        };
    }

    /// <summary>
    /// 创建禁止访问响应
    /// </summary>
    public static ApiResponse<T> Forbidden(string message = "禁止访问")
    {
        return new ApiResponse<T>
        {
            Code = 403,
            Msg = message,
            Data = default
        };
    }

    /// <summary>
    /// 创建未找到响应
    /// </summary>
    public static ApiResponse<T> NotFound(string message = "资源未找到")
    {
        return new ApiResponse<T>
        {
            Code = 404,
            Msg = message,
            Data = default
        };
    }

    /// <summary>
    /// 创建服务器错误响应
    /// </summary>
    public static ApiResponse<T> Error(string message = "服务器内部错误", int code = 500)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Msg = message,
            Data = default
        };
    }
}

/// <summary>
/// 无数据的统一API响应结构
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static new ApiResponse Success(string message = "操作成功")
    {
        return new ApiResponse
        {
            Code = 200,
            Msg = message
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static new ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse
        {
            Code = code,
            Msg = message
        };
    }

    /// <summary>
    /// 创建未授权响应
    /// </summary>
    public static new ApiResponse Unauthorized(string message = "未授权访问")
    {
        return new ApiResponse
        {
            Code = 401,
            Msg = message
        };
    }

    /// <summary>
    /// 创建禁止访问响应
    /// </summary>
    public static new ApiResponse Forbidden(string message = "禁止访问")
    {
        return new ApiResponse
        {
            Code = 403,
            Msg = message
        };
    }

    /// <summary>
    /// 创建未找到响应
    /// </summary>
    public static new ApiResponse NotFound(string message = "资源未找到")
    {
        return new ApiResponse
        {
            Code = 404,
            Msg = message
        };
    }

    /// <summary>
    /// 创建服务器错误响应
    /// </summary>
    public static new ApiResponse Error(string message = "服务器内部错误", int code = 500)
    {
        return new ApiResponse
        {
            Code = code,
            Msg = message
        };
    }
}
