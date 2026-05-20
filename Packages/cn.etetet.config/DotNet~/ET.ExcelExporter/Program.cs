using System;
using System.ComponentModel;
using System.IO;
using CommandLine;
using OfficeOpenXml;

namespace ET
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("ETET");
                
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Log.Console(e.ExceptionObject.ToString());
                };
                
                // 命令行参数
                Options options = World.Instance.AddSingleton<Options>();
                options.Console = 1;
                
                World.Instance.AddSingleton<Logger>().Log = new NLogger("ExcelExporter");
                
                
                foreach (Type type in typeof(Program).Assembly.GetTypes())
                {
                    if (!typeof(IExcelHandler).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type == typeof(IExcelHandler))
                    {
                        continue;
                    }
                    
                    IExcelHandler iExcelHandler = Activator.CreateInstance(type) as IExcelHandler;
                    iExcelHandler.Run();
                }
                
                if (!LubanGen.CreateLubanConf("Code"))
                {
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return -1;
            }
            Console.WriteLine("excelexporter ok!");
            return 0;
        }
    }
}
