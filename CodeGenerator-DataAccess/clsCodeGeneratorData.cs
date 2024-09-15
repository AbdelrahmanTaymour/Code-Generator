using System;
using System.Data;
using System.Data.SqlClient;

namespace CodeGenerator_DataAccess
{
    public class clsCodeGeneratorData
    {

        private static DataTable ExecuteQuery(string connectionString, string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using(SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                                dt.Load(reader);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                clsDataAccessUtilities.LogError(ex);
            }
            catch (Exception ex)
            {
                clsDataAccessUtilities.LogError(ex);
            }

            return dt;
        }
        public static DataTable GetAllDatabases()
        {
            string query = @"SELECT Name AS Databases FROM sys.databases
                                WHERE database_id > 4
                                ORDER BY create_date DESC;";

            return ExecuteQuery(clsDataAccessSettings.ConnectionString(), query);
        }
        public static DataTable GetAllTablesOfDatabase(string DatabaseName)
        {
            string query = @"SELECT Name AS TablesName FROM sys.tables
                                    WHERE Name <> 'sysdiagrams';";

            return ExecuteQuery(clsDataAccessSettings.ConnectionString(DatabaseName), query);
        }
        public static DataTable GetAllColumnsOfTable(string DatabaseName, string TableName)
        {
            string query = @"SELECT COLUMN_NAME AS 'Column Name', DATA_TYPE AS 'Data Type', IS_NULLABLE AS 'Is Nullable'
                                    FROM INFORMATION_SCHEMA.COLUMNS
                                    WHERE TABLE_NAME = @TableName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TableName", TableName)
            };

            return ExecuteQuery(clsDataAccessSettings.ConnectionString(DatabaseName), query, parameters);
        }
        public static DataTable GetAllColumnsOfTableWithPrecision(string databaseName, string TableName)
        {
            string query = @"SELECT 
                                   COLUMN_NAME as 'Column Name',
	                               CASE 
	                                   WHEN CHARACTER_MAXIMUM_LENGTH IS NULL THEN DATA_TYPE
	                                   WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN DATA_TYPE +'('+ CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR) + ')'
                                   END AS 'Data Type'
                              FROM INFORMATION_SCHEMA.COLUMNS
                              WHERE TABLE_NAME = @TableName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TableName", TableName)
            };

            return ExecuteQuery(clsDataAccessSettings.ConnectionString(databaseName), query, parameters);
        }
        public static bool ExecuteStoredProcedure(string databaseName, string storedProcedures)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString(databaseName)))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        string[] batches = storedProcedures.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string batch in batches)
                        {
                            // Trim whitespace and skip empty batches
                            string trimmedBatch = batch.Trim();
                            if (string.IsNullOrWhiteSpace(trimmedBatch))
                                continue;

                            using (SqlCommand command = new SqlCommand(trimmedBatch, connection, transaction))
                            {
                                command.CommandType = CommandType.Text; // Use CommandType.Text for raw SQL commands
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                clsDataAccessUtilities.LogError(ex);
            }
            catch (Exception ex)
            {
                clsDataAccessUtilities.LogError(ex);
            }

            return false;
        }

    }
}
