using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;


namespace 按键测试
{
    public class PushKey
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, string lParam);

        public const UInt32 WM_SETTEXT = 0x000C;

        public static async Task SimulateTextInputOnSpecificChildWindowAsync(string parentWindowHandle, string childClassName, string text)
        {
            IntPtr parentHWnd = new IntPtr(Convert.ToInt32(parentWindowHandle));
            // 查找特定类名的子窗口
            IntPtr childHWnd = FindWindowEx(parentHWnd, IntPtr.Zero, childClassName, null);
            if (childHWnd != IntPtr.Zero)
            {
                SendMessage(childHWnd, WM_SETTEXT, IntPtr.Zero, text);
            }
            await Task.CompletedTask;
        }


        public static void SendTextToWindow(string windowHandle, string text)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));

            SendMessage(hWnd, WM_SETTEXT, IntPtr.Zero, text);

        }

        public static void SimulateTextInputOnWindow(string windowHandle, string className, string text)
        {
            IntPtr parentHWnd = new IntPtr(Convert.ToInt32(windowHandle));
            IntPtr targetHWnd = parentHWnd;

            // 如果提供了类名，则尝试找到特定的子窗口
            if (!string.IsNullOrEmpty(className))
            {
                targetHWnd = FindWindowEx(parentHWnd, IntPtr.Zero, className, null);
                if (targetHWnd == IntPtr.Zero)
                {
                    return; // 如果找不到子窗口，直接返回
                }
            }

            SendMessage(targetHWnd, WM_SETTEXT, IntPtr.Zero, text);
        }

        public static async Task<string> StartProgramAndGetHandleAsync(string programPath)
        {
            Process process = Process.Start(programPath);
            while (process.MainWindowHandle == IntPtr.Zero)
            {
                await Task.Delay(100); // 稍微等待，避免密集轮询
                process.Refresh(); // 刷新进程信息
            }
            return process.MainWindowHandle.ToString();
        }


        public static async Task SimulateTextInputOnWindowAsync(string windowHandle, string text)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            SetForegroundWindow(hWnd);
            SendKeys.SendWait(text);
            await Task.CompletedTask;
        }



        public static async Task SimulateKeyPressOnWindowAsync(string windowHandle, string keys)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            SetForegroundWindow(hWnd);
            SendKeys.SendWait(keys);
            await Task.CompletedTask;
        }

        public static async Task ParseAndExecuteDelayAsync(string delayString)
        {
            if (int.TryParse(delayString, out int delayMilliseconds))
            {
                await Task.Delay(delayMilliseconds);
            }
            // 如果无法解析为数字，则不执行延迟
        }

        static async Task Main()
        {
            // 示例：异步启动记事本并获取其窗口句柄
             //string handle = await StartProgramAndGetHandleAsync("E:\\X52_Project\\X52_SourceBase\\工具\\商需工具\\资源检查工具\\ResourceCheckTool.exe");
            string handle = await StartProgramAndGetHandleAsync("notepad.exe");
            //await ParseAndExecuteDelayAsync("1000");
            WindowInfoHelper.ShowAllChildWindowInfo(handle);
            //WindowHelper.SendTextToWindow(handle, "000", "Edit8", 0);
            //WindowHelper.SendTextToWindow(handle, "1", "Edit", 1);
            //WindowHelper.SendTextToWindow(handle, "2", "Edit", 2);
            //WindowHelper.SendTextToWindow(handle, "3", "Edit", 3);
            //WindowHelper.SendTextToWindow(handle, "4", "Edit", 4);
            //WindowHelper.SendTextToWindow(handle, "5", "Edit", 5);

            //WindowInfoHelper.SendClassAndIndexToAllWindows(handle); //打印全部
           // UIAutomationHelper.SendTextToWindow(handle, "Scintilla,",0, "123");
            //WindowHelper.SendTextToWindow(handle,"Edit", 0,"123");
            //WindowHelper.ClickButton(handle,"Button",0);

            // 示例：在记事本窗口中模拟文本输入
            await SimulateTextInputOnWindowAsync(handle, "你好 hello world");
            // 示例：延迟1000毫秒
            //await ParseAndExecuteDelayAsync("1000");
            // SendTextToWindow(handle, "你好 hello world");
            // await SimulateTextInputOnWindowAsync(handle, "你好 hello world");
            // 示例：延迟1000毫秒
            // await ParseAndExecuteDelayAsync("1000");

            // 示例：在记事本窗口中模拟按键操作
            //await SimulateKeyPressOnWindowAsync(handle, "{ENTER}");

            //await SimulateTextInputOnSpecificChildWindowAsync(handle, "Scintilla", "abc");
            //SimulateTextInputOnWindow(handle, "Edit", "abc");
            // await SimulateKeyPressOnWindowAsync(handle,"{ENTER}");
        }


    }



    public class WindowHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        // 新的 SendMessage 重载，用于 BM_CLICK
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        const uint WM_SETTEXT = 0x000C;
        const uint BM_CLICK = 0x00F5;

        public static void SendTextToWindow(string windowHandle,  string className, int instance, string text)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            int count = 0;

            EnumWindowProc callback = (childHandle, parameter) =>
            {
                StringBuilder classText = new StringBuilder(256);
                GetClassName(childHandle, classText, classText.Capacity);

                if (classText.ToString() == className)
                {
                    if (count == instance)
                    {
                        SendMessage(childHandle, WM_SETTEXT, IntPtr.Zero, text);
                        return false; // Stop enumeration
                    }
                    count++;
                }

                return true; // Continue enumeration
            };

            EnumChildWindows(hWnd, callback, IntPtr.Zero);
        }
        public static void ClickButton(string windowHandle, string className, int instance)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            FindAndOperateWindow(hWnd, className, instance, (childHandle) =>
            {
                SendMessage(childHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);


            });
        }

        private static void FindAndOperateWindow(IntPtr parentHandle, string className, int instance, Action<IntPtr> operation)
        {
            int count = 0;
            EnumWindowProc callback = (childHandle, parameter) =>
            {
                StringBuilder classText = new StringBuilder(256);
                GetClassName(childHandle, classText, classText.Capacity);

                if (classText.ToString() == className)
                {
                    if (count == instance)
                    {
                        operation(childHandle);
                        return false; // Stop enumeration
                    }
                    count++;
                }

                return true; // Continue enumeration
            };

            EnumChildWindows(parentHandle, callback, IntPtr.Zero);
        }
    }


    public class WindowInfoHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public static void ShowAllChildWindowInfo(string windowHandle)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            StringBuilder dialogInfoBuilder = new StringBuilder();
            StringBuilder consoleInfoBuilder = new StringBuilder();
            Dictionary<string, int> classCount = new Dictionary<string, int>();

            EnumChildWindows(hWnd, (childHandle, parameter) =>
            {
                StringBuilder classText = new StringBuilder(256);
                StringBuilder windowText = new StringBuilder(256);

                GetClassName(childHandle, classText, classText.Capacity);
                GetWindowText(childHandle, windowText, windowText.Capacity);

                string className = classText.ToString();
                if (!classCount.ContainsKey(className))
                {
                    classCount[className] = 0;
                }

                dialogInfoBuilder.AppendLine($"{childHandle}, {className}, {windowText}, {classCount[className]}");
                consoleInfoBuilder.AppendLine($"Handle: {childHandle}, Class: {className}, Title: {windowText}, Index: {classCount[className]}");

                classCount[className]++;

                return true; // 继续枚举子窗口
            }, IntPtr.Zero);

            MessageBox.Show(dialogInfoBuilder.ToString(), "Child Window Info");
            Console.WriteLine(consoleInfoBuilder.ToString());
        }

        public static void SendClassAndIndexToAllWindows(string windowHandle)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            Dictionary<string, int> classCount = new Dictionary<string, int>();

            EnumWindowProc callback = (childHandle, parameter) =>
            {
                StringBuilder classText = new StringBuilder(256);
                GetClassName(childHandle, classText, classText.Capacity);
                string className = classText.ToString();

                if (!classCount.ContainsKey(className))
                {
                    classCount[className] = 0;
                }

                string textToSend = $"{className},{classCount[className]}";
                PushKey.SendMessage(childHandle, PushKey.WM_SETTEXT, IntPtr.Zero, textToSend);
                classCount[className]++;

                return true; // Continue enumeration
            };

            EnumChildWindows(hWnd, callback, IntPtr.Zero);
        }
    }
    public class UIAutomationHelper
    {
        public static void SendTextToWindow(string windowHandle, string className, int instance, string text)
        {
            IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
            var rootElement = AutomationElement.FromHandle(hWnd);

            Condition condition = new PropertyCondition(AutomationElement.ClassNameProperty, className);
            var elements = rootElement.FindAll(TreeScope.Descendants, condition);

            if (elements.Count > instance)
            {
                AutomationElement element = elements[instance];
                ValuePattern valuePattern = (ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern);
                valuePattern.SetValue(text);
            }
            else
            {
                Console.WriteLine("Element not found or instance exceeds number of elements found.");
            }
        }
    }


}



