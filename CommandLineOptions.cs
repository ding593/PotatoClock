namespace PotatoClock;

/// <summary>命令行参数（主要供开发与自动化验证使用）。</summary>
internal sealed class CommandLineOptions
{
    /// <summary><c>--test-seconds</c> 的值。</summary>
    public int? TestSeconds { get; private set; }

    /// <summary><c>--data-dir</c> 的值。</summary>
    public string? DataDirectory { get; private set; }

    /// <summary><c>--log</c> 的值。</summary>
    public string? LogPath { get; private set; }

    /// <summary><c>--language</c> 的值（zh / en）。</summary>
    public string? LanguageOverride { get; private set; }

    /// <summary>是否请求显示帮助（或参数不认识）。</summary>
    public bool ShowHelp { get; private set; }

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (string.IsNullOrWhiteSpace(argument))
            {
                continue;
            }

            var key = argument;
            string? value = null;
            var separator = argument.IndexOf('=');
            if (separator > 0)
            {
                key = argument[..separator];
                value = argument[(separator + 1)..];
            }

            switch (key.ToLowerInvariant())
            {
                case "--test-seconds":
                case "--fast":
                    value ??= TakeNext(args, ref index);
                    if (int.TryParse(value, out var seconds) && seconds is > 0 and <= 3600)
                    {
                        options.TestSeconds = seconds;
                    }
                    else
                    {
                        options.ShowHelp = true;
                    }

                    break;

                case "--data-dir":
                case "--datadir":
                    value ??= TakeNext(args, ref index);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        options.ShowHelp = true;
                    }
                    else
                    {
                        options.DataDirectory = value;
                    }

                    break;

                case "--log":
                    value ??= TakeNext(args, ref index);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        options.ShowHelp = true;
                    }
                    else
                    {
                        options.LogPath = value;
                    }

                    break;

                case "--language":
                case "--lang":
                    value ??= TakeNext(args, ref index);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        options.ShowHelp = true;
                    }
                    else
                    {
                        options.LanguageOverride = value;
                    }

                    break;

                case "--help":
                case "-h":
                case "/?":
                    options.ShowHelp = true;
                    break;

                default:
                    options.ShowHelp = true;
                    break;
            }
        }

        return options;
    }

    private static string? TakeNext(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            return null;
        }

        index++;
        return args[index];
    }
}
