using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discovery.KIT.Models.DataSources;
using discovery.KIT.ORACLE;
using discovery.KIT.ORACLE.Internal;

namespace discovery.KIT.Internal
{
    public static class ActiveConnectionHandler
    {
        private static OracleConnector OC;

        public static void Connect(ref DataSourceConnection dsc)
        {
            OC?.Disconnect();
            OC = new OracleConnector(new OracleAuthData()
            {
                Host = dsc.Server,
                Port = dsc.Port,
                Password = dsc.AuthenticationData.Password,
                Username = dsc.AuthenticationData.Username,
                SID = dsc.OracleContent.SID
            });
            OC.ConnectoToUri();
        }

        public static void Disconnect()
        {
            OC?.Dispose();
            OC = null;
        }

        public static Task<List<dynamic>> ExecuteRequest(string table, string condition = "", string orderBy = "", int limit = 500)
        {
            return Task.Run(() => OC?.ExecuteRequest(table, condition, orderBy, limit));
        }

        public static Task<List<string>> DiscoverServer()
        {
            return Task.Run(() => OC?.DiscoverServer());
        }

        public static List<string> Headers => OC?.Headers;
        public static List<string> Tables  => OC?.Tables;
        public static List<dynamic> ExistingData => OC?.DataSource;

    }
}
