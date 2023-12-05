using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SharpShell.SharpContextMenu;
using SharpShell.Attributes;
using Newtonsoft.Json;
using System.Drawing;
using SharpShell.Interop;
using System.Drawing.Imaging;
using System.Threading;
using System.Net;



namespace MenuFunctions
{
    [ComVisible(true)]
    //如果按文件类型，按以下设置
    //[COMServerAssociation(AssociationType.ClassOfExtension, ".xlsx", ".xls")]

    //设置对全部文件和目录可用
    //[COMServerAssociation(AssociationType.AllFiles), COMServerAssociation(AssociationType.Directory)]


    [COMServerAssociation(AssociationType.AllFiles), COMServerAssociation(AssociationType.Directory), COMServerAssociation(AssociationType.DirectoryBackground)]



    public class ArrContextMenu : SharpContextMenu
    {
        private string currentFolderPath;
        /// <summary>
        /// 判断菜单是否需要被激活显示
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowMenu()
        {
            // 判断是否在文件夹背景上
            bool isFolderBackground = SelectedItemPaths.Count() == 0;

            // 判断是否在文件上
            bool isFile = SelectedItemPaths.Any(p => File.Exists(p));
            // 判断是否在文件夹上
            bool isFolder = SelectedItemPaths.Any(p => Directory.Exists(p));




            // 获取当前文件夹路径
            currentFolderPath = isFolderBackground ? ShellHelper.GetForegroundExplorerPath() : null;


            foreach (var itemConfig in LoadMenuItemsFromConfig())
            {
                // 检查是否应该在文件夹背景上显示
                if (isFolderBackground && itemConfig.ShowOnFolderBackground && currentFolderPath != null)
                {
                    return true;
                }
                // 检查是否应该在文件上显示
                if (isFile && itemConfig.ShowOnFiles)
                {
                    // 检查文件类型是否符合配置
                    if (itemConfig.FileTypes == null || itemConfig.FileTypes.Count == 0 || SelectedItemPaths.Any(path => itemConfig.FileTypes.Any(ft => path.EndsWith(ft, StringComparison.OrdinalIgnoreCase))))
                    {
                        return true;
                    }
                }

                // 检查是否应该在文件夹上显示
                if (isFolder && itemConfig.ShowOnFolder)
                {
                    return true;
                }

            }
            return false;
        }




        /// <summary>
        /// 创建一个菜单，包含菜单项，设置ICON
        /// </summary>
        /// <returns></returns>
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var rootItem = new ToolStripMenuItem("功能");

            bool isFolderBackground = SelectedItemPaths.Count() == 0;
            bool isFile = SelectedItemPaths.Any(p => File.Exists(p));

            bool isFolder = SelectedItemPaths.Any(p => Directory.Exists(p));

            var menuItems = LoadMenuItemsFromConfig();

            foreach (var itemConfig in menuItems)
            {
                // 根据 ShowOnFolderBackground 和 ShowOnFiles 决定是否添加菜单项
                if ((isFolderBackground && itemConfig.ShowOnFolderBackground) || (isFile && itemConfig.ShowOnFiles) || (isFolder && itemConfig.ShowOnFolder))
                {
                    // 检查文件类型是否符合配置
                    if (itemConfig.FileTypes == null || itemConfig.FileTypes.Count == 0 || SelectedItemPaths.Any(path => itemConfig.FileTypes.Any(ft => path.EndsWith(ft, StringComparison.OrdinalIgnoreCase))))
                    {
                        if (itemConfig.ShowInRootMenu)
                        {
                            AddMenuItem(menu.Items, itemConfig, currentFolderPath);
                        }
                        else
                        {
                            AddMenuItem(rootItem.DropDownItems, itemConfig, currentFolderPath);
                        }
                    }
                }
            }

            if (rootItem.DropDownItems.Count > 0)
            {
                menu.Items.Add(rootItem);
            }

            return menu;
        }





        private void AddMenuItem(ToolStripItemCollection parentMenuItems, MenuItemConfig config, string currentFolderPath)
        {


            if (!config.IsVisible) return; // 如果菜单项设置为不可见，则跳过

            var menuItem = new ToolStripMenuItem(config.Text);

            // 加载图标
            if (!string.IsNullOrEmpty(config.IconPath))
            {
                menuItem.Image = LoadImageFromPath(config.IconPath);

            }


            // 将整个 MenuItemConfig 对象作为 Tag，以便在点击事件中使用
            menuItem.Tag = config;

            // 设置点击事件处理器
            menuItem.Click += (sender, e) =>
            {
                var item = sender as ToolStripMenuItem;
                var itemConfig = item.Tag as MenuItemConfig;
                if (itemConfig != null)
                {
                    Item_Click(sender, e, itemConfig, currentFolderPath);
                }
            };

            // 递归添加子菜单项
            if (config.SubItems != null && config.SubItems.Any())
            {
                // 判断是否在文件夹背景上
                bool isFolderBackground = SelectedItemPaths.Count() == 0 || SelectedItemPaths.All(p => Directory.Exists(p));
                // 判断是否在文件上
                bool isFile = SelectedItemPaths.Any(p => File.Exists(p));
                // 判断是否在文件夹上
                bool isFolder = SelectedItemPaths.Any(p => Directory.Exists(p));

                bool hasVisibleSubItems = false;
                foreach (var subItemConfig in config.SubItems)
                {
                    // 根据 ShowOnFolderBackground 和 ShowOnFiles 决定是否添加子菜单项
                    if ((isFolderBackground && subItemConfig.ShowOnFolderBackground) || (isFile && subItemConfig.ShowOnFiles) || (isFolder && subItemConfig.ShowOnFolder))
                    {
                        AddMenuItem(menuItem.DropDownItems, subItemConfig, currentFolderPath);
                        hasVisibleSubItems = true;
                    }
                }

                // 如果没有可见的子菜单项，则不添加此父菜单项
                if (!hasVisibleSubItems)
                {
                    return;
                }
            }

            parentMenuItems.Add(menuItem);
        }





        // 菜单动作
        private void Item_Click(object sender, EventArgs e, MenuItemConfig config, string currentFolderPath)
        {
            // 获取选中的文件或目录的路径
            var selectedPaths = SelectedItemPaths.ToList(); 
           
  

            string args;

            if (config.AppendCommandToEachPath && !string.IsNullOrEmpty(config.Command))
            {
                // 将命令附加到每个路径后面，并将整个组合用引号包围
                // 当OnlyUsingProgram为false时，清空selectedPaths，避免添加菜单路径
                if (!config.OnlyUsingProgram) {
                    args = string.Join(" ", selectedPaths.Select(p => $"\"{p} {config.Command}\""));
                }
                else
                {
                    args = $"{config.Command}";
                }
                   
            }
            else if (config.AppendCommandToEachPath)
            {
                // 如果命令为空，只添加路径
                if (!config.OnlyUsingProgram)
                {
                    args = string.Join(" ", selectedPaths.Select(p => $"\"{p}\""));
                }
                else
                {
                    args ="";
                }
                  
            }
            else
            {
                // 将命令放在所有路径的最后，每个路径保持单独一行
                string selectedPathsArg = string.Join(" ", selectedPaths.Select(p => $"\"{p}\""));

                // 只有当命令非空时才添加
                if (!string.IsNullOrEmpty(config.Command))
                {
                    if (!config.OnlyUsingProgram)
                    {
                        args = $"{selectedPathsArg} \"{config.Command}\"";
                    }
                    else
                    {
                        args = $"{config.Command}";
                    }
                }
                else
                {
                    if (!config.OnlyUsingProgram)
                    {
                        args = selectedPathsArg;
                    }
                    else { 
                        args = ""; 
                    }
                        
                }
            }


            // 如果没有选中任何路径，则使用当前文件夹路径
            if (string.IsNullOrEmpty(args) && !string.IsNullOrEmpty(currentFolderPath))
            {
                if (config.AppendCommandToEachPath)
                {
                    if (!string.IsNullOrEmpty(config.Command))
                    {
                        // 将命令附加到当前文件夹路径后面，并将整个组合用引号包围
                        if (!config.OnlyUsingProgram)
                        {
                            args = $"\"{currentFolderPath} {config.Command}\"";
                        }
                        else
                        {
                            args = $"{config.Command}";
                        }
                    }
                    else
                    {
                        if (!config.OnlyUsingProgram)
                        {
                            // 如果命令为空，只添加当前文件夹路径
                            args = $"\"{currentFolderPath}\"";
                        }
                        else
                        {
                            args = "";

                        }
                      
                      
                    }
                }
                else
                {
                    // 当AppendCommandToEachPath为false时，如果有命令，将命令和路径分开添加
                    if (!string.IsNullOrEmpty(config.Command))
                    {
                        if (!config.OnlyUsingProgram)
                        {
                            args = string.Join(" ", $"\"{currentFolderPath}\"", $"\"{config.Command}\"");
                        }
                        else
                        {
                            args = $"{config.Command}";
                        }
                        
                    }
                    else
                    {
                        if (!config.OnlyUsingProgram)
                        {
                            args = $"\"{currentFolderPath}\"";
                        }
                        else
                        {
                            args = "";
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(args) && !string.IsNullOrEmpty(currentFolderPath) && !config.AppendCommandToEachPath)
            {

                if (!config.OnlyUsingProgram)
                {
                    args = string.Join(" ", $"\"{currentFolderPath}\"", $"\"{config.Command}\"");
                }
                else
                {
                    args = $"{config.Command}";
                }


            }



            // 如果指定了程序路径，则启动该程序并传递参数
            if (!string.IsNullOrEmpty(config.ProgramPath))
            {
                try
                {
                    // 检查ProgramPath是否匹配预设命令
                    switch (config.ProgramPath)
                    {
                        case "copypath":

                            BasicFunctions.CopyToClipboard(args);
                            
                            break;
                        // 这里可以添加更多case来处理其他预设命令
                        case "openfolderandselectitems":
                            //args= BasicFunctions.RemoveQuotes(args);
                            //MessageBox.Show(args);
                            ExplorerHelper.OpenFolderAndSelectItem(args); 
                            break;

                        default:

                            if (string.IsNullOrEmpty(args)) {

                                Process.Start(config.ProgramPath);
                            }
                            else
                            {
                                
                                Process.Start(config.ProgramPath ,args);

                            }
                               
                            

                            break;
                    }



                }
                catch (Exception ex)
                {
                    string errorMessage = $"无法启动程序 '{config.ProgramPath}': {ex.Message}";
                    MessageBox.Show(errorMessage, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log(errorMessage); // 记录日志
                }


            }
            else
            {
                // 执行默认操作
                //string rootPath = getRootPath();
                //string appFile = Path.Combine(rootPath, "DisplayPathTools.exe");
                //if (!File.Exists(appFile))
                //{
                //    string errorMessage = $"找不到默认的显示路径程序路径:\n{appFile}";
                //    MessageBox.Show(errorMessage, "出错了", MessageBoxButtons.OK);
                //    Logger.Log(errorMessage); // 记录日志
                //    return;
                //}

                try
                {

                    if (string.IsNullOrEmpty(args))
                    {

                        Process.Start("DisplayPathTools.exe");
                    }
                    else
                    {

                        Process.Start("DisplayPathTools.exe", args);

                    }



                }
                catch (Exception ex)
                {
                    string errorMessage = $"无法启动默认程序 '{"DisplayPathTools.exe"}': {ex.Message}";
                    MessageBox.Show(errorMessage, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log(errorMessage); // 记录日志
                }
            }
        }


        //获取当前dll所在路径
        private string getRootPath()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assemblyPath);
        }


        public class MenuItemConfig
        {
            public string Text { get; set; } //菜单名字
            public string Command { get; set; } //传递的参数
            public string ProgramPath { get; set; } //要运行的程序
            public List<MenuItemConfig> SubItems { get; set; }
            public List<string> FileTypes { get; set; } // 文件类型
            public bool ShowInRootMenu { get; set; } // 是否在根菜单中直接显示

            public string IconPath { get; set; } //图标路径,支持png,jpg,ico,exe
            public bool IsVisible { get; set; } = true; //是否显示这个菜单,设置false可以隐藏

            public bool ShowOnFiles { get; set; } = true; // 显示在文件上
            public bool AppendCommandToEachPath { get; set; } = true; // 参数跟在每一条路径后面,还是在最后面

            public bool ShowOnFolderBackground { get; set; } = true; // 显示在文件夹背景

            public bool ShowOnFolder { get; set; } = true; // 显示在文件夹上


            public bool OnlyUsingProgram { get; set; } = false; // 默认为 false
            
        }





        private List<MenuItemConfig> LoadMenuItemsFromConfig()
        {
            try
            {
                // 获取程序集所在的目录路径
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var directoryPath = Path.GetDirectoryName(assemblyPath);

                // 构建配置文件的完整路径
                var configFilePath = Path.Combine(directoryPath, "MenuConfig.json");

                // 读取配置文件内容
                var json = File.ReadAllText(configFilePath);

                // 反序列化 JSON 到 MenuItemConfig 列表
                var items = JsonConvert.DeserializeObject<List<MenuItemConfig>>(json);

                // 如果反序列化结果为 null，返回空列表
                return items ?? new List<MenuItemConfig>();
            }
            catch (JsonException jsonEx)
            {
                // 处理 JSON 解析错误
                string errorMessage = $"MenuFunctions右键菜单的JSON 解析错误: {jsonEx.Message}";
                MessageBox.Show(errorMessage, "JSON配置文件错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
                return new List<MenuItemConfig>();
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                // 处理文件未找到错误
                string errorMessage = $"MenuFunctions右键菜单的配置文件未找到: {fileNotFoundEx.Message}";
                MessageBox.Show(errorMessage, "配置文件未找到,配置文件错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
                return new List<MenuItemConfig>();
            }
            catch (Exception ex)
            {
                // 处理其他类型的异常
                string errorMessage = $"MenuFunctions右键菜单的加载配置文件时出错: {ex.Message}";
                MessageBox.Show(errorMessage, "加载配置文件时出错,配置文件错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
                return new List<MenuItemConfig>();
            }
        }


        private Image LoadImageFromPath(string path)
        {
            try
            {


                Image image = null;


                // 设置支持的SSL/TLS协议版本
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;



                // 检测是否为URL并尝试从URL加载图像
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    using (var webClient = new WebClient())
                    {
                        byte[] imageData = webClient.DownloadData(uriResult);
                        using (var stream = new MemoryStream(imageData))
                        {
                            Image tempImage = Image.FromStream(stream);

                            // 克隆图像以提高稳定性
                            image = new Bitmap(tempImage);
                            tempImage.Dispose();
                        }
                    }
                }
                else
                {

                    // 如果路径不包含目录信息，则尝试从系统目录中查找
                    if (!Path.IsPathRooted(path))
                    {
                        string fullPath = FindFileInSystemPaths(path);
                        if (File.Exists(fullPath))
                        {
                            path = fullPath;
                        }

                    }



                    if (!File.Exists(path))
                    {
                        Logger.Log("ico path not exist:" + path);
                        return null;
                    }


                    // 获取文件扩展名
                    string extension = Path.GetExtension(path).ToLowerInvariant();

                    // 如果是图像文件，直接加载
                    if (extension == ".png" || extension == ".bmp" || extension == ".jpg" || extension == ".jpeg" || extension == ".gif")
                    {
                        image = Image.FromFile(path);
                    }
                    // 如果是图标文件，加载图标
                    else if (extension == ".ico")
                    {
                        image = new Icon(path).ToBitmap();
                    }
                    // 如果是可执行文件，提取图标
                    else if (extension == ".exe" || extension == ".com" || extension == ".msc" || extension == ".msi" || extension == ".lnk")
                    {
                        image = ExtractIconFromExe(path);
                    }



                }

                // 调整图像大小为 16x16 像素
                if (image != null && (image.Width != 16 || image.Height != 16))
                {
                    Bitmap resizedImage = ResizeImage(image, 16, 16);
                    image.Dispose(); // 释放原始图像资源
                    return resizedImage;
                }

                return image;
            }
            catch (Exception ex)
            {
                // 处理图像加载失败的情况
                Logger.Log("Error loading image from path '" + path + "': " + ex.Message);
                return null;
            }
        }

        private string FindFileInSystemPaths(string filename)
        {
            // 获取系统环境变量的路径
            var systemPath = Environment.GetEnvironmentVariable("PATH");
            // 分割路径字符串
            var paths = systemPath.Split(';');

            foreach (var path in paths)
            {
                // 构建完整的文件路径
                var fullPath = Path.Combine(path, filename);
                // 检查文件是否存在
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // 如果在环境变量路径中找不到文件，返回 null
            return null;
        }


        private Image ExtractIconFromExe(string filePath)
        {
            try
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    return icon?.ToBitmap();
                }
            }
            catch
            {
                // 处理图标提取失败的情况

                return null;
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



    }



    public static class ShellHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SHGetPathFromIDListW(IntPtr pidl, StringBuilder pszPath);

        [DllImport("shell32.dll")]
        private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

        public static string GetForegroundExplorerPath()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                {
                    Logger.Log("Foreground window handle is zero.");
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // 返回桌面路径
                }

                var shellWindows = new SHDocVw.ShellWindows();
                foreach (SHDocVw.InternetExplorer window in shellWindows)
                {
                    if (hwnd == new IntPtr(window.HWND))
                    {
                        IntPtr pidl;
                        SHParseDisplayName(window.LocationURL, IntPtr.Zero, out pidl, 0, out _);
                        if (pidl != IntPtr.Zero)
                        {
                            StringBuilder path = new StringBuilder(260);
                            if (SHGetPathFromIDListW(pidl, path))
                            {
                                Marshal.FreeCoTaskMem(pidl);
                                //Logger.Log("Foreground Explorer Path: " + path.ToString());
                                return path.ToString();
                            }
                            Marshal.FreeCoTaskMem(pidl);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in GetForegroundExplorerPath: " + ex.Message);
            }

            // 如果无法获取路径，返回桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // Logger.Log("Foreground Explorer Path not found, returning desktop path: " + desktopPath);
            return desktopPath;
        }



    }



    public static class Logger
    {
        private static string logFilePath = "app_log.txt"; // 日志文件的路径

        public static void Log(string message)
        {
            try
            {
                // 将消息和时间戳写入日志文件
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}\n");
            }
            catch (Exception ex)
            {
                // 如果写入日志时出错，可以在这里处理，例如输出到控制台
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }


    }


    public  class BasicFunctions
    {

        public static void CopyToClipboard(string text)
        {
            try
            {
                // 确保我们在STA线程模式下，因为剪贴板操作需要STA模式
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    Thread staThread = new Thread(() => Clipboard.SetText(text));
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start();
                    staThread.Join();
                }
                else
                {
                    Clipboard.SetText(RemoveQuotes(text));
                }
            }
            catch (Exception ex)
            {
                // 处理异常情况，例如剪贴板不可用或其他错误
                Logger.Log("无法将文本复制到剪贴板: " + ex.Message);
            }
        }



        public static string RemoveEmptyLines(string input)
        {
            // 将字符串分割成行
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // 使用LINQ来过滤掉空行
            var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            // 将过滤后的行重新组合成一个字符串
            return string.Join(Environment.NewLine, nonEmptyLines);
        }

        public static string RemoveQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // 移除单引号和双引号
            return input.Replace("'", "").Replace("\"", "");
        }

    }

}

