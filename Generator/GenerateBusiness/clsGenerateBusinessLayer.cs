using CodeGenerator_BusinessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.GenerateBusiness
{
    public class clsGenerateBusinessLayer
    {
        public static bool GenerateAllBusinessClasses(string folderPath, string appName, DataTable databaseTables)
        {
            string filePath = "";
            try
            {
                foreach (DataRow row in databaseTables.Rows)
                {
                    string tableName = row["TablesName"].ToString();
                    DataTable tableColumns = clsCodeGenerator.GetColumnsOfTableList(appName, tableName);
                    string TableSingularName = clsUtilities.GetTableSingularName(tableName);

                    filePath = folderPath.TrimEnd('\'') + $"\\cls{TableSingularName}.cs";
                    clsUtilities.WirteToFile(filePath, GenerateBusinessLayerClass(appName, tableName, tableColumns));
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GenerateBusinessLayerClass(string appName, string tableName, DataTable tableColumns)
        {
            StringBuilder generatedCode = new StringBuilder();
            string TableSingularName = clsUtilities.GetTableSingularName(tableName);

            generatedCode.Append("using System;\r\n");
            generatedCode.Append("using System.Data;\r\n");
            generatedCode.Append("using System.Text;\r\n");
            generatedCode.Append($"using {appName}_DataAccess;\r\n\r\n");
            generatedCode.Append($"namespace {appName}_Buisness\n{{\n    ");
            generatedCode.Append($"public class cls{TableSingularName}\n    {{\n");
            generatedCode.Append($"\r\n        private enum enMode {{ AddNew = 0, Update = 1 }};");
            generatedCode.Append($"\r\n        private enMode _mode = enMode.AddNew;\r\n\r\n");
            generatedCode.Append(_GenerateClassFields(tableColumns) + "\r\n");
            generatedCode.Append(_GenerateParameterlessConstructor(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateParameterizedConstructor(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateFindMethod(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenetateDoesExistMethod(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateAddNewMethod(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateUpdateMethod(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateSaveMethod(TableSingularName) + "\r\n\r\n");
            generatedCode.Append(_GenerateDeleteMethod(TableSingularName, tableColumns) + "\r\n\r\n");
            generatedCode.Append(_GenerateGetAllMethod(TableSingularName, tableName) + "\r\n\r\n");
            generatedCode.Append("\r\n        ");
            generatedCode.Append("\n    }\n}");

            return generatedCode.ToString();
        }

        static StringBuilder _GenerateClassFields(DataTable tableColumns)
        {
            StringBuilder generatedFields = new StringBuilder();

            foreach(DataRow row in tableColumns.Rows)
            {
                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string columnName = row["Column Name"].ToString();

                if(row == tableColumns.Rows[0])
                {
                    generatedFields.Append($"        public {clsUtilities.GetColumnDataType(row["Data Type"], true)} {columnName} {{ get; private set; }}\r\n");
                    continue;
                }
                generatedFields.Append($"        public {clsUtilities.GetColumnDataType(row["Data Type"], IsNullable)} {columnName} {{ get; set; }}\r\n");
            }

            return generatedFields;
        }
        static StringBuilder _GenerateParameters(DataTable tableColumns)
        {
            StringBuilder generatedParameters = new StringBuilder();

            foreach(DataRow row in tableColumns.Rows)
            {
                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string columnName = row["Column Name"].ToString();

                if (row == tableColumns.Rows[0])
                    generatedParameters.Append($"{clsUtilities.GetColumnDataType(row["Data Type"], true)} {columnName}");
                else
                    generatedParameters.Append($", {clsUtilities.GetColumnDataType(row["Data Type"], IsNullable)} {columnName}");
            }

            return generatedParameters;
        }
        static StringBuilder _GenerateParameterlessConstructor(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedConstructor = new StringBuilder();

            generatedConstructor.Append($"        public cls{TableSingularName}()\r\n        {{\r\n");
            foreach (DataRow row in tableColumns.Rows)//generate fields
            {
                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string columnName = row["Column Name"].ToString();

                if (IsNullable || row == tableColumns.Rows[0])
                    generatedConstructor.Append($"            {columnName} = null;\r\n");
                else
                    generatedConstructor.Append($"            {columnName} = default;\r\n");
            }
            generatedConstructor.Append($"\r\n            _mode = enMode.AddNew;");
            generatedConstructor.Append("\r\n        }");
            
            return generatedConstructor;
        }
        static StringBuilder _GenerateParameterizedConstructor(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedConstructor = new StringBuilder();

            generatedConstructor.Append($"        private cls{TableSingularName}({_GenerateParameters(tableColumns)})\r\n        {{\r\n");
            foreach(DataRow row in tableColumns.Rows)
            {
                string columnName = row["Column Name"].ToString();
                generatedConstructor.Append($"            this.{columnName} = {columnName};\r\n");
            }
            generatedConstructor.Append($"\r\n            _mode = enMode.Update;");
            generatedConstructor.Append("\r\n        }");

            return generatedConstructor;
        }
        
        static StringBuilder _GenerateFindMethod(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedMethod = new StringBuilder();
            StringBuilder generatedVariables = new StringBuilder();
            StringBuilder generatedDataAccessFuncParameters = new StringBuilder();
            StringBuilder generatedConstructorParameters = new StringBuilder();

            foreach(DataRow row in tableColumns.Rows)
            {
                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string dataType = clsUtilities.GetColumnDataType(row["Data Type"], IsNullable);
                string columnName = row["Column Name"].ToString();

                if(row == tableColumns.Rows[0])
                {
                    generatedDataAccessFuncParameters.Append($"{columnName}");
                    generatedConstructorParameters.Append($"{columnName}");

                }
                else
                {
                    generatedVariables.Append($"            {dataType} {columnName} = default;\r\n");
                    generatedDataAccessFuncParameters.Append($", ref {columnName}");
                    generatedConstructorParameters.Append($", {columnName}");

                }
            }
            generatedMethod.Append($"        public static cls{TableSingularName} Find({clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"], tableColumns.Rows[0]["Is Nullable"].ToString() == "YES")}? {tableColumns.Rows[0]["Column Name"]})\r\n        {{\r\n{generatedVariables}\r\n            bool isFound = cls{TableSingularName}Data.Get{TableSingularName}InfoByID({generatedDataAccessFuncParameters});\r\n\r\n            if (isFound)\r\n                return new cls{TableSingularName}({generatedConstructorParameters});\r\n            else\r\n                return null;\r\n        }}");

            return generatedMethod;
        }
        static StringBuilder _GenetateDoesExistMethod(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedMethod = new StringBuilder();
            string columnName = tableColumns.Rows[0]["Column Name"].ToString();
            string dataType = clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"], true);

            generatedMethod.Append($"        public static bool Does{TableSingularName}Exist({dataType} {columnName})\r\n        {{\r\n            return cls{TableSingularName}Data.Does{TableSingularName}Exist({columnName});\r\n        }}");

            return generatedMethod;
        }
        static StringBuilder _GenerateAddNewMethod(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generateDAFuncParameters = new StringBuilder();
            foreach(DataRow row in tableColumns.Rows)
            {
                if (row == tableColumns.Rows[0])
                    continue;

                generateDAFuncParameters.Append($"{row["Column Name"]}, ");
            }
            string parameters = generateDAFuncParameters.ToString().TrimEnd(',', ' ');

            StringBuilder generatedMethod = new StringBuilder();
            generatedMethod.Append($"        private bool _AddNew{TableSingularName}()\r\n        {{\r\n            {tableColumns.Rows[0]["Column Name"]} = cls{TableSingularName}Data.AddNew{TableSingularName}({parameters});\r\n            return {tableColumns.Rows[0]["Column Name"]}.HasValue;\r\n        }}");
            return generatedMethod;
        }
        static StringBuilder _GenerateUpdateMethod(string TableSingularName, DataTable tableColumns)
        {
            string parameters = "";
            foreach(DataRow row in tableColumns.Rows)
            {
                parameters += $"{row["Column Name"]}, ";
            }

            StringBuilder generatedMethod = new StringBuilder();
            generatedMethod.Append($"        private bool _Update{TableSingularName}()\r\n        {{\r\n            return cls{TableSingularName}Data.Update{TableSingularName}Info({parameters.TrimEnd(',', ' ')});\r\n        }}");
            return generatedMethod;
        }
        static StringBuilder _GenerateSaveMethod(string TableSingularName)
        {
            StringBuilder generatedMethod = new StringBuilder();
            generatedMethod.Append($"        public bool Save()\r\n        {{\r\n            switch (_mode)\r\n            {{\r\n                case enMode.AddNew:\r\n                    if (_AddNew{TableSingularName}())\r\n                    {{\r\n                        _mode = enMode.Update;\r\n                        return true;\r\n                    }}\r\n                    else\r\n                        return false;\r\n\r\n                case enMode.Update:\r\n                    return _Update{TableSingularName}();\r\n\r\n            }}\r\n            return false;\r\n        }}");
            return generatedMethod;
        }
        static StringBuilder _GenerateDeleteMethod(string TableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedMethod = new StringBuilder();
            generatedMethod.Append($"        public static bool Delete{TableSingularName}({clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"],true)} {tableColumns.Rows[0]["Column Name"]})\r\n        {{\r\n            return cls{TableSingularName}Data.Delete{TableSingularName}({tableColumns.Rows[0]["Column Name"]});\r\n        }}");
            return generatedMethod;
        }
        static StringBuilder _GenerateGetAllMethod(string TableSingularName, string tableName)
        {
            StringBuilder generatedMethod = new StringBuilder();
            generatedMethod.Append($"        public static DataTable GetAll{tableName}()\r\n        {{\r\n            return cls{TableSingularName}Data.GetAll{tableName}();\r\n        }}");
            return generatedMethod;
        }


    }
}
