using CodeGenerator_BusinessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.GenerateStoredProcedure
{
    public class clsGenerateStoredProcedure
    {

        public static string GenerateStoredProcedure(string appName, string tableName, DataTable tableColumnsPrecision)
        {
            StringBuilder generatedCode = new StringBuilder();
            string TableSingularName = clsUtilities.GetTableSingularName(tableName);

            generatedCode.Append(_GenerateGetAllSP(tableName) + "\r\n");
            generatedCode.Append("--------------------------------------------------------\r\n\r\n");
            generatedCode.Append(_GenerateIfExistSP(TableSingularName, tableName, tableColumnsPrecision) + "\r\n");
            generatedCode.Append("--------------------------------------------------------\r\n\r\n");
            generatedCode.Append(_GenerateDeleteSP(TableSingularName, tableName, tableColumnsPrecision) + "\r\n");
            generatedCode.Append("--------------------------------------------------------\r\n\r\n");
            generatedCode.Append(_GenerateGetInfoByIdSP(TableSingularName, tableName, tableColumnsPrecision) + "\r\n");
            generatedCode.Append("--------------------------------------------------------\r\n\r\n");
            generatedCode.Append(_GenerateAddNewSP(TableSingularName, tableName, tableColumnsPrecision) + "\r\n");
            generatedCode.Append("--------------------------------------------------------\r\n\r\n");
            generatedCode.Append(_GenerateUpdateSP(TableSingularName, tableName, tableColumnsPrecision) + "\r\n");

            return generatedCode.ToString();
        }
        public static string ShowSPForAllTables(string appName, DataTable databaseTables)
        {
            StringBuilder allStoredProcedures = new StringBuilder();

            foreach (DataRow row in databaseTables.Rows)
            {
                string tableName = row["TablesName"].ToString();
                DataTable tableColumnsPrecision = clsCodeGenerator.GetColumnsOfTableWithPrecisionList(appName, tableName);

                allStoredProcedures.Append( GenerateStoredProcedure(appName, tableName, tableColumnsPrecision));
                
            }
            return allStoredProcedures.ToString();
        }
        public static bool GenerateSPForSelectedTable(string appName, string tableName, DataTable tableColumnsPrecision)
        {
            string storedProcedure = GenerateStoredProcedure(appName, tableName, tableColumnsPrecision);
            return clsCodeGenerator.ExecuteStoredProcedure(appName, storedProcedure);
        }
        public static bool GenerateSPForAllTables(string appName, DataTable databaseTables)
        {
            foreach (DataRow row in databaseTables.Rows)
            {
                string tableName = row["TablesName"].ToString();
                DataTable tableColumnsPrecision = clsCodeGenerator.GetColumnsOfTableWithPrecisionList(appName, tableName);
                //string TableSingularName = clsUtilities.GetTableSingularName(tableName);

                string storedProcedure = GenerateStoredProcedure(appName, tableName, tableColumnsPrecision);
                if (!clsCodeGenerator.ExecuteStoredProcedure(appName, storedProcedure))
                    return false;
            }
            return true;
        }



        static StringBuilder _GenerateGetAllSP(string tableName)
        {
            StringBuilder generatedSP = new StringBuilder();
            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_GetAll{tableName}\r\nAS \r\n    BEGIN \r\n        SELECT * FROM {tableName};\r\n    END\r\nGO;");
        }
        static StringBuilder _GenerateIfExistSP(string TableSingularName, string tableName, DataTable tableColumnsPrecision)
        {
            StringBuilder generatedSP = new StringBuilder();
            string columnName = tableColumnsPrecision.Rows[0]["Column Name"].ToString();
            string dataType = tableColumnsPrecision.Rows[0]["Data Type"].ToString().ToUpper();

            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_CheckIf{TableSingularName}Exists\r\n    @{columnName} {dataType}\r\nAS \r\n    BEGIN \r\n        IF EXISTS(SELECT isFound = 1 FROM {tableName} WHERE {columnName} = @{columnName})\r\n            RETURN 1;\r\n        ELSE\r\n            RETURN 0;\r\n    END\r\nGO;");
        }
        static StringBuilder _GenerateDeleteSP(string TableSingularName, string tableName, DataTable tableColumnsPrecision)
        {
            StringBuilder generatedSP = new StringBuilder();
            string columnName = tableColumnsPrecision.Rows[0]["Column Name"].ToString();
            string dataType = tableColumnsPrecision.Rows[0]["Data Type"].ToString().ToUpper();

            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_Delete{TableSingularName}\r\n    @{columnName} {dataType}\r\nAS \r\n    BEGIN                    \r\n        DELETE FROM {tableName} WHERE {columnName} = @{columnName};\r\n    END\r\nGO;");
        }
        static StringBuilder _GenerateGetInfoByIdSP(string TableSingularName, string tableName, DataTable tableColumnsPrecision)
        {
            StringBuilder generatedSP = new StringBuilder();
            string columnName = tableColumnsPrecision.Rows[0]["Column Name"].ToString();
            string dataType = tableColumnsPrecision.Rows[0]["Data Type"].ToString().ToUpper();

            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_Get{TableSingularName}InfoByID\r\n    @{columnName} {dataType}\r\nAS \r\n    BEGIN                    \r\n        SELECT * FROM {tableName} WHERE {columnName} = @{columnName};\r\n    END\r\nGO;");
        }
        static StringBuilder _GenerateAddNewSP(string TableSingularName, string tableName, DataTable tableColumnsPrecision)
        {
            StringBuilder generatedSP = new StringBuilder();
            string columnName = "";
            string dataType = "";

            string parametersSP = "";
            string insertPrams = "";
            string valuePrams = "";

            foreach(DataRow row in tableColumnsPrecision.Rows)
            {
                if (row == tableColumnsPrecision.Rows[0])
                    continue;

                columnName = row["Column Name"].ToString();
                dataType = row["Data Type"].ToString().ToUpper();

                parametersSP += $"@{columnName} {dataType},\r\n\t";
                insertPrams += $"{columnName}, ";
                valuePrams += $"@{columnName}, ";
            }

            columnName = tableColumnsPrecision.Rows[0]["Column Name"].ToString();
            dataType = tableColumnsPrecision.Rows[0]["Data Type"].ToString().ToUpper();

            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_AddNew{TableSingularName}\r\n\t@New{columnName} {dataType} OUTPUT,\r\n\t{parametersSP.TrimEnd(',', '\r', '\n', '\t')}\r\nAS \r\n    BEGIN                    \r\n        INSERT INTO Users ({insertPrams.TrimEnd(',', ' ')})\r\n        VALUES ({valuePrams.TrimEnd(',', ' ')});\r\n\r\n        SET @New{columnName} = SCOPE_IDENTITY();\r\n    END\r\nGO;");
        }
        static StringBuilder _GenerateUpdateSP(string TableSingularName, string tableName, DataTable tableColumnsPrecision)
        {

            StringBuilder generatedSP = new StringBuilder();
            string columnName = "";
            string dataType = "";

            string parametersSP = "";
            string setPrams = "";

            foreach (DataRow row in tableColumnsPrecision.Rows)
            {
                columnName = row["Column Name"].ToString();
                dataType = row["Data Type"].ToString().ToUpper();

                parametersSP += $"@{columnName} {dataType},\r\n\t";

                if (row == tableColumnsPrecision.Rows[0])
                    continue;

                setPrams += $"{columnName} = @{columnName},\r\n\t\t\t";
            }

            columnName = tableColumnsPrecision.Rows[0]["Column Name"].ToString();
            dataType = tableColumnsPrecision.Rows[0]["Data Type"].ToString().ToUpper();

            return generatedSP.Append($"CREATE PROCEDURE SP_{tableName}_Update{TableSingularName}Info\r\n\t{parametersSP.TrimEnd(',', '\r', '\n', '\t')}\r\n\r\nAS \r\n    BEGIN                    \r\n        UPDATE {tableName}\r\n        SET \r\n\t\t\t{setPrams.TrimEnd(',', '\r', '\n', '\t')}\r\n        WHERE {columnName} = @{columnName};\r\n    END\r\nGO;");
        }

    }
}
