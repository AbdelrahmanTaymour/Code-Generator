using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator_DataAccess
{
    public class clsDataAccessUtilities
    {
        public static void LogError(Exception ex)
        {
            string sourceName = Assembly.GetEntryAssembly().GetName().Name;

            if (!EventLog.SourceExists(sourceName))
            {
                EventLog.CreateEventSource(sourceName, "Application");
            }

            string errorMessage = $"Exception occurred in: {ex.Source}\n" +
                $"Exception Message: {ex.Message}\n" +
                $"Exception Type: {ex.GetType().Name}\n" +
                $"Stack Trace: {ex.StackTrace}\n" +
                $"Error Location: {ex.TargetSite.Name}\n";

            // Write the error to the event log
            EventLog.WriteEntry(sourceName, errorMessage, EventLogEntryType.Error);
        }
    }
}
