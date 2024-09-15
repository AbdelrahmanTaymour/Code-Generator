using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.GenerateAppConfig
{
    public class clsGenerateAppConfigFile
    {
        public static string GenerateAppConfigClass(string appName)
        {
            StringBuilder generatedCode = new StringBuilder();

            generatedCode.Append($"<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n\t\r\n    <startup> \r\n        <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.7.2\" />\r\n    </startup>\r\n\t\r\n\t<connectionStrings>\r\n\t\t<add name=\"ConnectionString\" connectionString=\"Server=.;Database={appName};Integrated Security=True;\" providerName=\"System.Data.SqlClient\" />\r\n\t</connectionStrings>\r\n\t\r\n</configuration>");

            return generatedCode.ToString();
        }

        public static bool CreateAppConfigFile(string folderPath, string appName)
        {
            string filePath = folderPath.TrimEnd('\'') + $"\\App.config";
            try
            {
                clsUtilities.WirteToFile(filePath, GenerateAppConfigClass(appName));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
