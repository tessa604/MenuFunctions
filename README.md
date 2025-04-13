功能介绍:
可以分别设置显示在文件右键菜单,目录菜单,文件夹背景菜单
可直接运行程序,路径,网址,cmd命令
传递多路径到同一程序
图标可以设置常见图片格式,ico,png,gif,jpg,bmp,exe,msi,msc,com,lnk,网址
自定义传参,比如设置一个参数-k 这个可以分别传给所有的地址, 也可以附加在所有地址的最后面
可以设定按照固定的扩展名来显示菜单
多层级菜单,或者置顶到一级菜单

扩展功能
复制路径到剪贴板功能
打开所在文件夹并且选中文件或目录 (这个本来用explorer select可以实现,但是这个启动新进程会特别慢.所以增加这个功能.)


![示例图片](Resource/menu.png)



第一次运行:注册dll,会将MenuFunctions.dll注册到系统,启用右键菜单.
MenuConfig.json是配置文件,不可删.修改里面配置会实时生效.
建议你电脑装个notepad3.这样如果你配置写错了.他会标记错误.而普通记事本你配置写错了也不知道错哪了
如果要卸载的话,点击卸载dll.
注意这个路径你注册后不可改位置.别到时候给文件夹移动走了.. 那没准右键菜单就直接报错了..可以先卸载,再挪文件夹位置. 然后再注册
注意这个东西有固态盘请一定放固态盘. 包括图标用的图标,要不然每次这个右键都会读硬盘.读配置文件和图片. 你放普通硬盘慢的话,没准会卡菜单.. 当然只是没准. 其实配置文件现在30多kb. 读个30kb我觉得还是轻轻松松..要是30kb读着都慢.那可能是硬盘坏了..
文件夹内任何文件都不可删除.

更新 2023-12-7
1 路径现在改到添加到所有参数的后面,这样很多命令就正常了. 比如python.exe 显示路径.py 选中路径 这种好几个叠起来的就可以正常运行了.
2.图标现在除了常用图片外,改为获取全部格式的ico图标.就比如你见一个文件是json图标挺好看,你可以把这个json文件的路径填进去,然后就变成了json的图标..
3.增加使用cmd运行命令.这样可以直接在ProgramPath输入命令从而用cmd执行
4.cmd命令的一些拓展,可以隐藏cmd窗口启动,或者保持cmd不关闭.方便各种用途.比如你给文件设置隐藏属性,可能就不需要显示cmd窗口. 比如你要ping ip,就需要保持窗口不要一闪而过.
5.增加菜单分割线
6.增加自检弹窗. 这个可以检测实际运行的命令和路径是否符合预期.
7.增加菜单文本动态显示路径功能.就是那个压缩包.你选中文件点压缩包,会直接显示解压到xxx,或者压缩到xxx. 这个,可以把路径,或者文件名等传给菜单

这个配置文件模板 放到了原始Json文件备份里,名字是MenuConfig_new.json,有兴趣可以改名用这个试试
另外查了查GPT说是win11的多标签获取路径好像弄不了,先这样把.测试这个测试的我要吐啊...




=========================================================================
=========================================================================




MenuFunctions，windows右键菜单扩展
可以分别设置显示在文件右键菜单,目录菜单,文件夹背景菜单 可直接运行程序,路径,网址,cmd命令
https://github.com/onlyclxy/MenuFunctions

自定义Windows系统右键菜单工具
https://github.com/bigsinger/CustomContextMenu

生成Windows右键菜单的注册文件
https://github.com/GHJayce/RightClickMenu

一个 windows 桌面右键菜单脚本丰富工具
https://github.com/C4skg/pycontextmenu
nextcloud-uploader
https://github.com/kkzzhizhou/nextcloud-uploader

格式化工具添加 Windows 右键菜单选项的工具
https://github.com/jugolink/AstyleFormatter
AATexDiff
https://github.com/aasdl1/AATexDiff

filebrowser，将filebrowser添加到右键菜单选项中
https://github.com/filebrowser/filebrowser/releases/
https://github.com/filebrowser/filebrowser

利用JQuery开发的类windows方式操作文件功能(右键菜单，二级菜单，拖动选中，Ctrl+Shift快捷键选择文件等)
https://github.com/lizhenfeiok/use-files-like-windows?tab=readme-ov-file
MinIO QuickUpload，允许用户通过鼠标右键菜单将文件快速上传到 MinIO 云存储。
https://github.com/htobs/MinIO-QuickUpload
Windows Path Manager
https://github.com/nuomi77/windows-path-manager
文件一键整理与复原工具
https://github.com/Chiyang001/file_sort


Folder-Hider，用bat写的文件夹伪装成回收站或者别的系统文件夹的工具，可添加到右键菜单
https://github.com/Looomo/Folder-Hider

win-script，构建鼠标右键菜单
https://github.com/rj-hwang/win-script

VS项目右键菜单扩展，VSProjectContextMenuExtend
https://github.com/usernamedd/VSProjectContextMenuExtend




=========================================================================
=========================================================================


计算机\HKEY_CLASSES_ROOT\CLSID\{29541F5E-F157-386D-93A0-792253DF0573}

/ve /t REG_SZ /d MenuFunctions.ArrContextMenu /f
 |_ _ _ _"\Implemented Categories"  /ve /t REG_SZ /f
	 |_ _ _ _ "\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}" /ve /t REG_SZ /f

 |_ _ _ _\InprocServer32
	/ve /t REG_SZ /d mscoree.dll /f
	/v Assembly /t REG_SZ /d "MenuFunctions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab16c9fcf51914e7" /f
	/v Class /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f
	/v CodeBase /t REG_SZ /d "file:///D:/360安全浏览器下载/Release/MenuFunctions.DLL" /f
	/v RuntimeVersion /t REG_SZ /d v4.0.30319 /f
	/v ThreadingModel /t REG_SZ /d Both /f

 |_ _ _ "\1.0.0.0"
	/v Assembly /t REG_SZ /d "MenuFunctions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab16c9fcf51914e7" /f
	/v Class /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f
	/v CodeBase /t REG_SZ /d "file:///D:/360安全浏览器下载/Release/MenuFunctions.DLL" /f
	/v RuntimeVersion /t REG_SZ /d "v4.0.30319" /f

 |_ _ _ _\ProgId
	/ve /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f



计算机\HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{29541F5E-F157-386D-93A0-792253DF0573}

 |_ _ _ _\Implemented Categories
	 |_ _ _ _{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}
 |_ _ _ _ \InprocServer32
	/ve /t REG_SZ /d mscoree.dll /f
	/v Assembly /t REG_SZ /d "MenuFunctions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab16c9fcf51914e7" /f
	/v Class /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f
	/v CodeBase /t REG_SZ /d "file:///D:/360安全浏览器下载/Release/MenuFunctions.DLL" /f
	/v RuntimeVersion /t REG_SZ /d "v4.0.30319" /f
	/v ThreadingModel /t REG_SZ /d "Both" /f

             |_ _ _ _\1.0.0.0
	/v Assembly /t REG_SZ /d "MenuFunctions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab16c9fcf51914e7" /f
	/v Class /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f
	/v CodeBase /t REG_SZ /d "file:///D:/360安全浏览器下载/Release/MenuFunctions.DLL" /f
	/v RuntimeVersion /t REG_SZ /d "v4.0.30319" /f

 |_ _ _ _ "\ProgId" /ve /t REG_SZ /d "MenuFunctions.ArrContextMenu" /f



=========================================================================
=========================================================================


注册表编辑器
Registry Workshop
software
RegTest-Microsoft Visual Studio

右键扩展菜单管理器
Visual C++ 6.0
DesSpace
desktopOK
desktops

regworkshop
MSDN Library-visual studio 2008

注册表添加与删除
RegCreateKeyEx Function函数

更改盘符的图标
C++实现添加桌面右键新建菜单
C&C++实现的右键自定义菜单实现







=========================================================================
=========================================================================







