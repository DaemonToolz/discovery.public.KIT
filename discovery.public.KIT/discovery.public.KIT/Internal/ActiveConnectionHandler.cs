using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discovery.KIT.Models.DataSources;
using discovery.KIT.ORACLE;
using discovery.KIT.ORACLE.Internal;
using discovery.KIT.ORACLE.Models;

namespace discovery.KIT.Internal
{
    public static class ActiveConnectionHandler
    {
        private static OracleConnector OC;

        public static bool Connect(ref DataSourceConnection dsc)
        {
            Disconnect();
            if (dsc.ID == default || string.IsNullOrEmpty(dsc.Alias) || string.IsNullOrEmpty(dsc.Server) || dsc.Port <= 0 || dsc.AuthenticationData?.Password == null || string.IsNullOrEmpty(dsc.AuthenticationData?.Username) )
            {
                return false;
            }

            OC = new OracleConnector(new OracleAuthData()
            {
                Host = dsc.Server,
                Port = dsc.Port,
                Password = dsc.AuthenticationData.Password,
                Username = dsc.AuthenticationData.Username,
                SID = dsc.OracleContent.SID
            });
            return OC.ConnectoToUri();
        }

        public static Task<bool> ConnectAsync(DataSourceConnection dsc)
        {
            return Task.Run(() => Connect(ref dsc));
        }

        public static void Disconnect()
        {
            OC?.Dispose();
            OC = null;
        }

        public static Task<List<dynamic>> ExecuteRequest(string table, int limit = 500)
        {
            return Task.Run(() => 
                OC?.ExecuteRequest(
                    table, 
                    string.Join("AND ", Filters.Select(data => $"{data.Column ?? string.Empty}{data.Filter?.Sign ?? string.Empty}{data.Value ?? string.Empty}").ToList<string>()), 
                    string.Join(", ", OrderBy.Select(data => $"{data.Column ?? string.Empty}{data.Direction?.Sign ?? string.Empty}").ToList<string>()), 
                    limit));
        }

        public static Task<List<string>> DiscoverServer()
        {
            return Task.Run(() => OC?.DiscoverServer());
        }

        public static List<string> Headers => OC?.Headers;
        public static List<string> Tables  => OC?.Tables;
        public static List<dynamic> ExistingData => OC?.DataSource ?? new List<dynamic>();

        public static List<QueryFilter> Filters = new List<QueryFilter>();
        public static List<QueryOrderBy> OrderBy = new List<QueryOrderBy>();

    }
}
