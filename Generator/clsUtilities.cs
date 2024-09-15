using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    public static class clsUtilities
    {
        public static string GetTableSingularName(string tableName)
        {
            switch (tableName)
            {
                case "People":
                    return "Person";

                case "Countries":
                    return "Country";

                default:
                    return tableName.Substring(0, tableName.Length - 1);
            }
        }
    
        public static string GetColumnDataType(object dataType, bool IsNullable = false)
        {
            switch (dataType.ToString().ToLower())
            {
                case "nvarchar":
                case "varchar":
                    return "string";

                case "char":
                    return IsNullable ? "char?" : "char";

                case "int":
                    return IsNullable ? "int?" : "int";

                case "tinyint":
                    return IsNullable ? "byte?" : "byte";

                case "smallint":
                    return IsNullable ? "short?" : "short";

                case "decimal":
                case "money":
                    return IsNullable ? "double?" : "double";

                case "smallmoney":
                    return IsNullable ? "float?" : "float";

                case "datetime":
                case "smalldatetime":
                case "date":
                    return IsNullable ? "DateTime?" : "DateTime";

                case "bit":
                    return IsNullable ? "bool?" : "bool";


                default:
                    return "Unknown";
            }
        }


        public static void WirteToFile(string filePath, string value)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(value);
            }
        }


    }
}
