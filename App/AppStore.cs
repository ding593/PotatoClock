using System.Text.Json;

namespace PotatoClock.App;

/// <summary>
/// 设置与任务的本地存储，默认放在 exe 所在目录（安装目录），不写 C 盘用户目录。
/// 读取失败一律回落默认值，写入失败通过 <see cref="LastError"/> 暴露，绝不抛到调用方。
/// </summary>
public sealed class AppStore
{
    private const int MaxTaskTitleLength = 200;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;
    private readonly string _tasksPath;

    public AppStore(string? dataDirectory = null)
    {
        DataDirectory = string.IsNullOrWhiteSpace(dataDirectory)
            ? AppContext.BaseDirectory
            : dataDirectory!;

        _settingsPath = Path.Combine(DataDirectory, "settings.json");
        _tasksPath = Path.Combine(DataDirectory, "tasks.json");
        IsWritable = ProbeWritable();
    }

    /// <summary>数据目录（默认 exe 所在目录）。</summary>
    public string DataDirectory { get; }

    public string SettingsPath => _settingsPath;

    public string TasksPath => _tasksPath;

    /// <summary>目录是否可写；不可写时界面会给出红字提示。</summary>
    public bool IsWritable { get; }

    /// <summary>最近一次读写失败的描述。</summary>
    public string? LastError { get; private set; }

    public AppSettings LoadSettings()
    {
        var settings = TryRead<AppSettings>(_settingsPath) ?? AppSettings.CreateDefault();
        settings.ClampAndValidate();
        return settings;
    }

    public bool SaveSettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.ClampAndValidate();
        return TryWrite(_settingsPath, settings);
    }

    public List<TaskItem> LoadTasks()
    {
        var tasks = TryRead<List<TaskItem>>(_tasksPath) ?? new List<TaskItem>();
        var result = new List<TaskItem>(tasks.Count);

        foreach (var task in tasks)
        {
            if (task is null)
            {
                continue;
            }

            task.Title = NormalizeTitle(task.Title);
            if (task.Title.Length == 0)
            {
                continue;
            }

            if (task.Id == Guid.Empty)
            {
                task.Id = Guid.NewGuid();
            }

            result.Add(task);
        }

        return result;
    }

    public bool SaveTasks(IEnumerable<TaskItem> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var snapshot = tasks
            .Where(task => task is not null)
            .Select(task => new TaskItem
            {
                Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id,
                Title = NormalizeTitle(task.Title),
                IsDone = task.IsDone,
                CreatedUtc = task.CreatedUtc,
                CompletedUtc = task.CompletedUtc
            })
            .Where(task => task.Title.Length > 0)
            .ToList();

        return TryWrite(_tasksPath, snapshot);
    }

    /// <summary>只做去空白与截断，不注入任何语言相关占位文案。</summary>
    private static string NormalizeTitle(string? title)
    {
        var value = (title ?? string.Empty).Trim();
        return value.Length > MaxTaskTitleLength ? value[..MaxTaskTitleLength] : value;
    }

    private bool ProbeWritable()
    {
        var probePath = Path.Combine(DataDirectory, ".write-probe.tmp");
        try
        {
            Directory.CreateDirectory(DataDirectory);
            File.WriteAllText(probePath, "ok");
            File.Delete(probePath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            LastError = $"data folder not writable: {DataDirectory} ({ex.Message})";
            return false;
        }
    }

    private T? TryRead<T>(string path)
        where T : class
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(text, JsonOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            LastError = $"read {Path.GetFileName(path)} failed, using defaults: {ex.Message}";
            return null;
        }
    }

    private bool TryWrite<T>(string path, T value)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, path, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            LastError = $"write {Path.GetFileName(path)} failed: {ex.Message}";
            return false;
        }
    }
}
