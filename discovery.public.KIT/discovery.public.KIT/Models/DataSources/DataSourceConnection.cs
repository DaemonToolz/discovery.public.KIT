using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace discovery.KIT.Models.DataSources
{
    /// <summary>
    /// 
    /// </summary>
    public enum DataSourceType
    {
        Oracle,
        SQLServer
    }
    /// <summary>
    /// 
    /// </summary>
    public enum AuthenticationType
    {
        UserPassword,
        Integrated
    }

    /// <summary>
    /// 
    /// </summary>
    public struct OracleData
    {
        public string SID;

    }

    /// <summary>
    /// 
    /// </summary>
    public class Authentication
    {
        public string Username;
        public SecureString Password;
    }


    /// <summary>
    /// 
    /// </summary>
    public class DataSourceConnection
    {
        public Guid ID;
        public string Alias;
        public string Server;
        public int Port;
        public Authentication AuthenticationData;
        public AuthenticationType Authentication;
        public DataSourceType Type;

        public OracleData OracleContent;
        public bool OfflineMode = false;
    }
}
