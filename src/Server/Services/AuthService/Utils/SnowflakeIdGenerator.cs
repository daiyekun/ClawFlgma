namespace ClawFlgma.AuthService.Utils;

/// <summary>
/// 雪花算法ID生成器
/// 64位ID结构：符号位(1) + 时间戳(41) + 数据中心ID(5) + 机器ID(5) + 序列号(12)
/// </summary>
public class SnowflakeIdGenerator
{
    // 起始时间戳 (2024-01-01 00:00:00 UTC)
    private const long Epoch = 1704067200000L;

    // 各部分位数
    private const int DatacenterIdBits = 5;
    private const int WorkerIdBits = 5;
    private const int SequenceBits = 12;

    // 各部分最大值
    private const int MaxDatacenterId = -1 ^ (-1 << DatacenterIdBits);
    private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);
    private const int MaxSequence = -1 ^ (-1 << SequenceBits);

    // 各部分偏移
    private const int WorkerIdShift = SequenceBits;
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    private const int TimestampShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

    private readonly long _datacenterId;
    private readonly long _workerId;
    private long _sequence = 0L;
    private long _lastTimestamp = -1L;
    private readonly object _lock = new object();

    public SnowflakeIdGenerator(long datacenterId, long workerId)
    {
        if (datacenterId > MaxDatacenterId || datacenterId < 0)
        {
            throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDatacenterId}");
        }
        if (workerId > MaxWorkerId || workerId < 0)
        {
            throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");
        }

        _datacenterId = datacenterId;
        _workerId = workerId;
    }

    public long NextId()
    {
        lock (_lock)
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException($"Clock moved backwards. Refusing to generate ID for {_lastTimestamp - timestamp} milliseconds");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                {
                    timestamp = WaitForNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0L;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - Epoch) << TimestampShift)
                | (_datacenterId << DatacenterIdShift)
                | (_workerId << WorkerIdShift)
                | _sequence;
        }
    }

    private long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private long WaitForNextMillis(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
        {
            timestamp = GetCurrentTimestamp();
        }
        return timestamp;
    }

    /// <summary>
    /// 从雪花ID中提取时间戳
    /// </summary>
    public static DateTime GetDateTimeFromId(long id)
    {
        var timestamp = (id >> TimestampShift) + Epoch;
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }

    /// <summary>
    /// 检查ID是否为有效的雪花ID
    /// </summary>
    public static bool IsValidSnowflakeId(long id)
    {
        return id > 0 && GetDateTimeFromId(id) > DateTime.MinValue;
    }
}
