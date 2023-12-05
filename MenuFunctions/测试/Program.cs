using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 测试
{
    internal class Program
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


                try
                {
                    Process.Start(input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("直接执行命令时发生错误: " + ex.Message);
                    Console.WriteLine("尝试通过cmd.exe执行...");

                    RunProgram.ProcessInputCommand(input, customParameter);
                }
            }
        }
    }
}
