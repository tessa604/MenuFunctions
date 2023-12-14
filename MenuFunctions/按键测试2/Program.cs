using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;
using WindowsInput;

using System.Diagnostics;

using System.Threading;

using System.Text;



public class PushKey
{
    static void Main()
    {

        //string handle = ProgramRunner.StartProcessAndGetHandle("notepad.exe");
        string handle = "4138042";
        Console.WriteLine(handle);
        //Thread.Sleep(2000); // 稍微延迟，以模拟正常的按键速度
        WindowMessageSender.SendStringToWindow(handle, "直接变");
       
        //WindowMessageSender_ADD.SendStringToWindow(handle, "nihao111你好");
        //SendKeys.SendWait("abc");



    }
}


public class ProgramRunner
{
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public static string StartProcessAndGetHandle(string filePath)
    {
        Process process = new Process();
        process.StartInfo.FileName = filePath;
        process.Start();

        process.WaitForInputIdle(); // 等待程序准备好接收用户输入
        IntPtr handle = process.MainWindowHandle;

        // 如果 MainWindowHandle 不可用，可以尝试使用 FindWindow 方法
        if (handle == IntPtr.Zero)
        {
            handle = FindWindow(null, process.MainWindowTitle);
        }

        return handle.ToString();
    }
}

public class WindowMessageSender
{

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

    private const uint WM_SETTEXT = 0x000C;

    public static void SendStringToWindow(string windowHandle, string message)
    {
        IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
        SendMessageW(hWnd, WM_SETTEXT, IntPtr.Zero, message);
    }

}

public class WindowMessageSender_ADD
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


    private const uint WM_CHAR = 0x0102;

    public static void SendStringToWindow(string windowHandle, string message)
    {
        IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
        foreach (char c in message)
        {
            SendMessageW(hWnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
            Thread.Sleep(0); // 可以调整这个延迟
        }
    }
}



public class ScintillaHelper
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

    private const int SCI_SETTEXT = 2181;

    public static void SendStringToScintilla(string windowHandle, string message)
    {
        IntPtr hWnd = new IntPtr(Convert.ToInt32(windowHandle));
        SendMessageW(hWnd, SCI_SETTEXT, IntPtr.Zero, message);
    }
}