using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using 实现运行效果;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary; // 需要添加 COM 引用：Windows Script Host Object Model

class Program
{
    static void Main(string[] args)
    {
        string customParameter = ""; // 预留的自定义参数
        // 循环接受输入并处理
        while (true)
        {
            Console.WriteLine("请输入命令（输入'exit'退出）：");
            string input = Console.ReadLine();

            if (input.ToLower() == "exit")
            {
                break;
            }

            ProcessInputCommand(input, customParameter);
        }
    }

    static void ProcessInputCommand(string input, string customParameter)
    {
        // 检查并提前处理 -keep 和 -hide 参数
        bool hideWindow = input.Contains("-hide") || input.Contains("-隐藏");
        bool keepWindowOpen = input.Contains("-keep") || input.Contains("-保持");

        // 如果存在这些参数，先移除它们
        if (hideWindow || keepWindowOpen)
        {
            input = input.Replace("-hide", "").Replace("-隐藏", "")
                         .Replace("-keep", "").Replace("-保持", "").Trim();
        }

        var commands = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var command in commands)
        {
            HandleCommand(command, customParameter, hideWindow, keepWindowOpen);
        }
    }

    static void HandleCommand(string command, string customParameter, bool hideWindow, bool keepWindowOpen)
    {
        var parts = SplitCommandLine(command);
        if (parts.Count == 0)
        {
            return;
        }

        var commandName = parts[0];
        var arguments = string.Join(" ", parts.Skip(1));
        var envFiles = GetExecutableFilesFromEnvironmentVariables();

        var fullPath = envFiles.FirstOrDefault(file => Path.GetFileNameWithoutExtension(file).Equals(commandName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(fullPath))
        {
            // 检查是否是快捷方式，并获取目标路径
            if (Path.GetExtension(fullPath).ToLower() == ".lnk")
            {
                fullPath = GetShortcutTarget(fullPath);
            }

            if (!string.IsNullOrEmpty(fullPath))
            {
                // 添加引号以处理路径中的空格
                fullPath = "\"" + fullPath + "\"";

                if (hideWindow || keepWindowOpen)
                {
                    ExecuteCommandOrApplication(fullPath + " " + arguments, envFiles, customParameter, hideWindow, keepWindowOpen);
                }
                else
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = fullPath,
                        Arguments = arguments,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                }
            }
        }
        else if (System.IO.File.Exists(commandName) || Directory.Exists(commandName))
        {
            Process.Start(commandName, arguments);
        }
        else if (IsWebUrl(command))
        {
            ExplorerHelper.OpenItem(command);
        }
        else
        {
            ExecuteCommandOrApplication(command, envFiles, customParameter, hideWindow, keepWindowOpen);
        }
    }

    static string GetShortcutTarget(string shortcutPath)
    {
        if (System.IO.File.Exists(shortcutPath))
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            return shortcut.TargetPath;
        }
        return null;
    }




    static List<string> SplitCommandLine(string commandLine)
    {
        var parts = new List<string>();
        var currentPart = "";
        var inQuotes = false;

        foreach (var c in commandLine)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (!string.IsNullOrEmpty(currentPart))
                {
                    parts.Add(currentPart);
                    currentPart = "";
                }
            }
            else
            {
                currentPart += c;
            }
        }

        if (!string.IsNullOrEmpty(currentPart))
        {
            parts.Add(currentPart);
        }

        return parts.Select(p => TrimMatchingQuotes(p)).ToList();
    }

    static string TrimMatchingQuotes(string input)
    {
        if ((input.Length >= 2) &&
            (input[0] == '\"') && (input[input.Length - 1] == '\"'))
        {
            return input.Substring(1, input.Length - 2);
        }
        return input;
    }

    static List<string> GetExecutableFiles()
    {
        var files = new List<string>();
        // 添加环境变量的路径下的文件
        // 添加程序运行目录下的文件
        // 请根据您的需求和环境添加相应的代码来填充这个列表
        return files;
    }

    static bool IsLocalPath(string path)
    {
        return Path.IsPathRooted(path) && !Path.GetPathRoot(path).StartsWith("\\\\");
    }

    static bool IsNetworkPath(string path)
    {
        return path.StartsWith("\\\\");
    }

    static bool IsWebUrl(string url)
    {
        // 首先检查是否为标准的HTTP或HTTPS URL
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 检查是否含有反斜杠，如果有，则不是网址
        if (url.Contains('\\'))
        {
            return false;
        }

        // 没有 HTTP/HTTPS 前缀的情况
        string domainPart = url.Contains('/') ? url.Split('/')[0] : url;
        int dotCount = domainPart.Count(c => c == '.');

        // 如果有两个或者更多的点，则认为是网址
        return dotCount >= 2;
    }



    static void ExecuteCommandOrApplication(string command, List<string> files, string customParameter, bool hideWindow, bool keepWindowOpen)
    {
        try
        {
            var parts = SplitCommandLine(command);
            var commandName = parts[0];
            var arguments = string.Join(" ", parts.Skip(1)).Trim();

            var processStartInfo = new ProcessStartInfo("cmd")
            {
                UseShellExecute = false,
                CreateNoWindow = hideWindow,
                RedirectStandardOutput = hideWindow,
                RedirectStandardError = hideWindow
            };

            string cmdArguments = $"/c {commandName} {arguments} {customParameter}".Trim();
            if (keepWindowOpen)
            {
                cmdArguments += " && pause";
            }

            processStartInfo.Arguments = cmdArguments;

            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("执行命令时出错: " + ex.Message);
            // 这里可以添加更多错误处理逻辑
        }
    }


    static List<string> GetExecutableFilesFromEnvironmentVariables()
    {
        var paths = new List<string>();
        var systemPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
        var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);

        // 添加系统环境变量路径中的文件
        paths.AddRange(GetFilesFromPath(systemPath));
        // 添加用户环境变量路径中的文件
        paths.AddRange(GetFilesFromPath(userPath));

        return paths.Distinct().ToList();
    }

    static IEnumerable<string> GetFilesFromPath(string path)
    {
        var files = new List<string>();
        if (!string.IsNullOrEmpty(path))
        {
            var directories = path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dir in directories)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        // 获取可执行文件和快捷方式
                        files.AddRange(Directory.EnumerateFiles(dir, "*.exe"));
                        files.AddRange(Directory.EnumerateFiles(dir, "*.lnk"));
                    }
                }
                catch (Exception ex)
                {
                    // 异常处理
                }
            }
        }
        return files;
    }



}
