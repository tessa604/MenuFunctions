using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;




    namespace 测试
{
    public class ExplorerHelper
    {

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [DllImport("shell32.dll")]
        static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [DllImport("shell32.dll")]
        static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, out IntPtr pidl, uint sfgaoIn, out uint psfgaoOut);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;

        // 打开文件夹并选择文件
        public static void OpenFolderAndSelectItem(string filePath)
        {
            IntPtr pidl = IntPtr.Zero;
            SHParseDisplayName(filePath, IntPtr.Zero, out pidl, 0, out _);
            if (pidl != IntPtr.Zero)
            {
                SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
                Marshal.FreeCoTaskMem(pidl);
            }
        }

        // 打开文件夹
        public static void OpenFolder(string folderPath)
        {
            SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO
            {
                cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                lpVerb = "open",
                lpFile = folderPath,
                nShow = SW_SHOW
            };
            ShellExecuteEx(ref sei);
        }

        // 打开项目，可以是文件夹、可执行文件、URL或其他文件类型
        public static void OpenItem(string path)
        {


            // 只取第一行作为路径
            path = path.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("提供的路径无效或为空。");
            }

            // 验证路径是否合法
            string extension = string.Empty;
            try
            {
                extension = Path.GetExtension(path);
            }
            catch (ArgumentException ex)
            {
                // 如果获取扩展名时出现异常，打印出错的路径并继续执行
                Console.WriteLine($"获取扩展名时出错。传入的路径为: {path}。错误信息: {ex.Message}");
                return; // 由于无法获取扩展名，此处选择退出方法
            }



            // 如果没有扩展名，我们假定它是一个文件夹
            if (string.IsNullOrEmpty(extension))
            {
                OpenFolder(path);
            }
            else if (File.Exists(path)) // 如果路径指向一个文件
            {
                extension = extension.ToLowerInvariant();

                switch (extension)
                {
                    case ".exe":
                        // 如果是可执行文件，启动该应用程序
                        Process.Start(path);
                        break;
                    case ".url":
                        // 如果是URL文件，用默认浏览器打开它
                        Process.Start(path);
                        break;
                    default:
                        // 对于其他文件类型，使用默认程序打开
                        // 对于其他文件类型，使用默认程序打开
                        var sei = new SHELLEXECUTEINFO
                        {
                            cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                            lpVerb = "open",
                            lpFile = path,
                            nShow = SW_SHOW
                        };
                        ShellExecuteEx(ref sei); // 注意这里使用了 ref 关键字

                        break;
                }
            }
            else
            {
                // 如果文件不存在或不被识别，显示“打开方式”对话框
                // 对于其他文件类型，使用默认程序打开
                var sei = new SHELLEXECUTEINFO
                {
                    cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                    lpVerb = "open",
                    lpFile = path,
                    nShow = SW_SHOW
                };
                ShellExecuteEx(ref sei); // 注意这里使用了 ref 关键字

            }
        }
    }
}

