using CodeGenerator_BusinessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.GenerateDataAccess
{
    public class clsGenerateDataAccessLayer
    {
        public enum enMethodType { GetByID = 0, AddNew =1, Update =2}
        
        public static bool GenerateAllDataAccessClasses(string folderPath, string appName, DataTable databaseTables)
        {
            string filePath = "";
            try
            {
                filePath = folderPath.TrimEnd('\'') + $"\\clsDataAccessSettings.cs";
                clsUtilities.WirteToFile(filePath, GenerateDataAccessSettingsClass(appName));

                filePath = folderPath.TrimEnd('\'') + $"\\clsDataAccessUtilities.cs";
                clsUtilities.WirteToFile(filePath, GenerateDataAccessUtilitiesClass(appName));

                foreach (DataRow row in databaseTables.Rows)
                {
                    string tableName = row["TablesName"].ToString();
                    DataTable tableColumns = clsCodeGenerator.GetColumnsOfTableList(appName, tableName);
                    string TableSingularName = clsUtilities.GetTableSingularName(tableName);

                    filePath = folderPath.TrimEnd('\'') + $"\\cls{TableSingularName}Data.cs";
                    clsUtilities.WirteToFile(filePath, GenerateDataAccessLayerClass(appName, tableName, tableColumns));
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GenerateDataAccessLayerClass(string appName, string tableName, DataTable tableColumns)
        {
            StringBuilder generatedCode = new StringBuilder();
            string TableSingularName = clsUtilities.GetTableSingularName(tableName);

            generatedCode.Append("using System;\n");
            generatedCode.Append("using System.Data;\n");
            generatedCode.Append("using System.Data.SqlClient;\n\n");
            generatedCode.Append($"namespace {appName}_DataAccess\n{{\n");

            generatedCode.Append($"    public class cls{TableSingularName}Data\n    {{\r\n\r\n        ");
            generatedCode.Append(_GenerateGetAllMethod(tableName));
            generatedCode.Append("\r\n\r\n        ");
            generatedCode.Append(_GenerateGetByIDMethod(tableName, TableSingularName, tableColumns));
            generatedCode.Append("\r\n\r\n        ");
            generatedCode.Append(_GenerateAddNewMethod(tableName, TableSingularName, tableColumns));
            generatedCode.Append("\r\n\r\n        ");
            generatedCode.Append(_GenerateUpdateMethod(tableName, TableSingularName, tableColumns));
            generatedCode.Append("\r\n\r\n        ");
            generatedCode.Append(_GenerateDeleteMethod(tableName, TableSingularName, tableColumns));
            generatedCode.Append("\r\n\r\n        ");
            generatedCode.Append(_GenerateDoesExistMethod(tableName, TableSingularName, tableColumns));
            generatedCode.Append("\r\n        ");
            generatedCode.Append("\n    }\n}");

            return generatedCode.ToString();
        }
        public static string GenerateDataAccessSettingsClass(string appName)
        {
            return $"using System;\r\nusing System.Diagnostics;\r\nusing System.Configuration;\r\n\r\nnamespace {appName}_DataAccess\r\n{{\r\n    public class clsDataAccessSettings\r\n    {{\r\n        public static string ConnectionString = ConfigurationManager.ConnectionStrings[\"ConnectionString\"].ConnectionString;\r\n    }}\r\n}}";
        }
        public static string GenerateDataAccessUtilitiesClass(string appName)
        {
            return $"using System;\r\nusing System.Diagnostics;\r\nusing System.Reflection;\r\n\r\nnamespace {appName}_DataAccess\r\n{{\r\n    public static class clsDataAccessUtilities\r\n    {{\r\n        public static void LogError(Exception ex)\r\n        {{\r\n            string sourceName = Assembly.GetEntryAssembly().GetName().Name;\r\n\r\n            if (!EventLog.SourceExists(sourceName))\r\n            {{\r\n                EventLog.CreateEventSource(sourceName, \"Application\");\r\n            }}\r\n\r\n            string errorMessage = $\"Exception occurred in: {{ex.Source}}\\n\" +\r\n                $\"Exception Message: {{ex.Message}}\\n\" +\r\n                $\"Exception Type: {{ex.GetType().Name}}\\n\" +\r\n                $\"Stack Trace: {{ex.StackTrace}}\\n\" +\r\n                $\"Error Location: {{ex.TargetSite.Name}}\\n\";\r\n\r\n            EventLog.WriteEntry(sourceName, errorMessage, EventLogEntryType.Error);\r\n        }}\r\n\r\n    }}\r\n}}";
        }

        static StringBuilder _GenerateGetAllMethod(string tableName)
        {
            StringBuilder generatedGetAllCode = new StringBuilder();

            generatedGetAllCode.Append($"public static DataTable GetAll{tableName}()\r\n        {{\r\n        DataTable dt = new DataTable();\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_GetAll{tableName}\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n\r\n                    using (SqlDataReader reader = command.ExecuteReader())\r\n                    {{\r\n                        if (reader.HasRows)\r\n                        {{\r\n                            dt.Load(reader);\r\n                        }}\r\n                    }}\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n\r\n        return dt;\r\n    }}");
            return generatedGetAllCode;
        }
        static StringBuilder _GenerateGetByIDMethod(string tableName, string tableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedGetByIdCode = new StringBuilder();
            string columnName = tableColumns.Rows[0]["Column Name"].ToString();
            string parametersString = _GetParametersString(tableColumns, enMethodType.GetByID);

            generatedGetByIdCode.Append($"public static bool Get{tableSingularName}InfoByID({parametersString})\r\n        {{\r\n        bool isFound = false;\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_Get{tableSingularName}InfoByID\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n                    command.Parameters.AddWithValue(\"@{columnName}\", (object){columnName} ?? DBNull.Value);\r\n\r\n                    using (SqlDataReader reader = command.ExecuteReader())\r\n                    {{\r\n                        if (reader.Read())\r\n                        {{\r\n                            // The record was found successfully !\r\n                            isFound = true;\r\n\r\n                            {_FetchedDataString(tableColumns)}\r\n\r\n                        }}\r\n                        else\r\n                        {{\r\n                            // The record wasn't found !\r\n                            isFound = false;\r\n                        }}\r\n                    }}\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        return isFound;\r\n    }}");
            return generatedGetByIdCode;
        }
        static StringBuilder _GenerateAddNewMethod(string tableName, string tableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedAddNewCode = new StringBuilder();

            string dataType = clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"]);
            string parametersString = _GetParametersString(tableColumns, enMethodType.AddNew);
            string columnName = tableColumns.Rows[0]["Column Name"].ToString();

            generatedAddNewCode.Append($"public static {dataType}? AddNew{tableSingularName}({parametersString})\r\n    {{\r\n        {dataType}? {columnName} = null;\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_AddNew{tableSingularName}\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n                    {_GetCommandParameters(tableColumns)}\r\n\r\n                    SqlParameter output{columnName}Parameter = new SqlParameter(\"@{columnName}\", SqlDbType.Int)\r\n                    {{\r\n                        Direction = ParameterDirection.Output\r\n                    }};\r\n\r\n                    command.Parameters.Add(output{columnName}Parameter);\r\n                    command.ExecuteNonQuery();\r\n\r\n                    {columnName} = ({dataType})output{columnName}Parameter.Value;\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n\r\n        return {columnName};\r\n    }}");
            return generatedAddNewCode;
        }
        static StringBuilder _GenerateUpdateMethod(string tableName, string tableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedUpdateCode = new StringBuilder();
            string parametares = _GetParametersString(tableColumns, enMethodType.Update);

            generatedUpdateCode.Append($"public static bool Update{tableSingularName}Info({parametares})\r\n    {{\r\n        int rowsAffected = 0;\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_Update{tableSingularName}Info\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n                    {_GetCommandParameters(tableColumns)}\r\n                    rowsAffected = command.ExecuteNonQuery();\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n\r\n        return rowsAffected != 0;\r\n    }}");
            return generatedUpdateCode;
        }
        static StringBuilder _GenerateDeleteMethod(string tableName, string tableSingularName, DataTable tableColumns)
        {
            StringBuilder generatedDeleteMethodCode = new StringBuilder();
            string dataType = clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"], true);
            string columnName = tableColumns.Rows[0]["Column Name"].ToString();

            generatedDeleteMethodCode.Append($"public static bool Delete{tableSingularName}({dataType} {columnName})\r\n    {{\r\n        int rowsAffected = 0;\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_Delete{tableSingularName}\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n                    command.Parameters.AddWithValue(\"@{columnName}\", (object){columnName} ?? DBNull.Value);\r\n\r\n                    rowsAffected = command.ExecuteNonQuery();\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n\r\n        return rowsAffected != 0;\r\n    }}");
            return generatedDeleteMethodCode;
        }
        static StringBuilder _GenerateDoesExistMethod(string tableName, string tableSingularName, DataTable tableColumns)
        {
            StringBuilder generateDoesExistMethodCode = new StringBuilder();
            string dataType = clsUtilities.GetColumnDataType(tableColumns.Rows[0]["Data Type"], true);
            string columnName = tableColumns.Rows[0]["Column Name"].ToString();

            generateDoesExistMethodCode.Append($"public static bool Does{tableSingularName}Exist({dataType} {columnName})\r\n    {{\r\n        bool isFound = false;\r\n\r\n        try\r\n        {{\r\n            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\r\n            {{\r\n                connection.Open();\r\n\r\n                using (SqlCommand command = new SqlCommand(\"SP_{tableName}_CheckIf{tableSingularName}Exists\", connection))\r\n                {{\r\n                    command.CommandType = CommandType.StoredProcedure;\r\n                    command.Parameters.AddWithValue(\"@{columnName}\", (object){columnName} ?? DBNull.Value);\r\n\r\n                    SqlParameter returnValue = new SqlParameter\r\n                    {{\r\n                        Direction = ParameterDirection.ReturnValue\r\n                    }};\r\n\r\n                    command.Parameters.Add(returnValue);\r\n                    command.ExecuteScalar();\r\n\r\n                    isFound = (int)returnValue.Value == 1;\r\n                }}\r\n            }}\r\n        }}\r\n        catch (SqlException ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n        catch (Exception ex)\r\n        {{\r\n            clsDataAccessUtilities.LogError(ex);\r\n        }}\r\n\r\n        return isFound;\r\n    }}");
            return generateDoesExistMethodCode;
        }
        
        static string _GetParametersString(DataTable tableColumns, enMethodType methodType)
        {
            StringBuilder parametersString = new StringBuilder();

            switch (methodType)
            {
                case enMethodType.AddNew:
                    foreach (DataRow row in tableColumns.Rows)
                    {
                        // Skip the first row if the wnMethodType is AddNew
                        if (row == tableColumns.Rows[0])
                            continue;

                        bool isNullable = (string)row["Is Nullable"] == "YES";
                        string dataType = clsUtilities.GetColumnDataType(row["Data Type"].ToString(), isNullable);
                        string columnName = row["Column Name"].ToString();

                        parametersString.Append($"{dataType} {columnName}, ");
                    }
                    return parametersString.ToString().TrimEnd(',', ' ');



                case enMethodType.Update:
                    foreach (DataRow row in tableColumns.Rows)
                    {
                        bool isNullable = (string)row["Is Nullable"] == "YES";
                        string dataType = clsUtilities.GetColumnDataType(row["Data Type"].ToString(), isNullable);
                        string columnName = row["Column Name"].ToString();

                        if (row == tableColumns.Rows[0])
                            parametersString.Append($"{dataType}? {columnName}");
                        else
                            parametersString.Append($", {dataType} {columnName}");
                    }
                    return parametersString.ToString();



                default:
                    foreach (DataRow row in tableColumns.Rows)
                    {
                        // Skip the first row if the wnMethodType is AddNew

                        bool isNullable = (string)row["Is Nullable"] == "YES";
                        string dataType = clsUtilities.GetColumnDataType(row["Data Type"].ToString(), isNullable);
                        string columnName = row["Column Name"].ToString();

                        if (row == tableColumns.Rows[0])
                            parametersString.Append($"{dataType}? {columnName}");
                        else
                            parametersString.Append($", ref {dataType} {columnName}");
                    }
                    return parametersString.ToString();
            }
        }
        static StringBuilder _FetchedDataString(DataTable tableColumns)
        {
            StringBuilder generatedFetchData = new StringBuilder();

            foreach(DataRow row in tableColumns.Rows)
            {
                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string columnName = row["Column Name"].ToString();
                string dataType = clsUtilities.GetColumnDataType(row["Data Type"]);

                if (IsNullable)
                    generatedFetchData.Append($"{columnName} = (reader[\"{columnName}\"] != DBNull.Value) ? ({dataType})reader[\"{columnName}\"] : default;\r\n                            ");
                else
                    generatedFetchData.Append($"{columnName} = ({dataType})reader[\"{columnName}\"];\r\n                            ");
            }

            return generatedFetchData;
        }
        static StringBuilder _GetCommandParameters(DataTable tableColumns)
        {
            StringBuilder generatedCommandParameters = new StringBuilder();

            generatedCommandParameters.Append("\r\n                        ");

            foreach(DataRow row in tableColumns.Rows)
            {
                if (row == tableColumns.Rows[0])
                    continue;

                bool IsNullable = row["Is Nullable"].ToString() == "YES";
                string columnName = row["Column Name"].ToString();

                if (IsNullable)
                    generatedCommandParameters.Append($"command.Parameters.AddWithValue(\"@{columnName}\", (object){columnName} ?? DBNull.Value);\r\n                            ");
                else
                    generatedCommandParameters.Append($"command.Parameters.AddWithValue(\"@{columnName}\", {columnName});\r\n                            ");

            }


            return generatedCommandParameters;
        }

    }
}
