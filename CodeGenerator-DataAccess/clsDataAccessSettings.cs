using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator_DataAccess
{
    public class clsDataAccessSettings
    {
        public static string ConnectionString(string DatabaseName = "master")
        {
            return $"Data Source=.;Initial Catalog={DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
        }

        

        
        
    }

    
}
