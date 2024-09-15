using CodeGenerator_DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator_BusinessLayer
{
    public class clsCodeGenerator
    {
        public static DataTable GetDatabasesList()
        {
            return clsCodeGeneratorData.GetAllDatabases();
        }

        public static DataTable GetTablesList(string DatabaseName)
        {
            return clsCodeGeneratorData.GetAllTablesOfDatabase(DatabaseName);
        }

        public static DataTable GetColumnsOfTableList(string DatabaseName, string TableName)
        {
            return clsCodeGeneratorData.GetAllColumnsOfTable(DatabaseName, TableName);
        }

        public static DataTable GetColumnsOfTableWithPrecisionList(string DatabaseName, string TableName)
        {
            return clsCodeGeneratorData.GetAllColumnsOfTableWithPrecision(DatabaseName, TableName);
        }

        public static bool ExecuteStoredProcedure(string databaseName, string storedProcedures)
        {
            return clsCodeGeneratorData.ExecuteStoredProcedure(databaseName, storedProcedures);
        }
    }
}
