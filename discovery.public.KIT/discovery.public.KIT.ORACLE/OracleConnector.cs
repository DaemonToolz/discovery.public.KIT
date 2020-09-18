using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using Windows.ApplicationModel.Chat;
using discovery.KIT.ORACLE.Generics;
using discovery.KIT.ORACLE.Internal;
using discovery.KIT.Security;
using Oracle.ManagedDataAccess.Client;

namespace discovery.KIT.ORACLE
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleConnector  : IDisposable{

        private OracleAuthData _auth;
        private OracleConnection _oracleConnection;
        private List<string> _headers = new List<string>();
        private List<string> _tables = new List<string>();
        private List<dynamic> _dataSource = new List<dynamic>();

        public OracleConnector(OracleAuthData auth)
        {
            Auth = auth;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<dynamic> DataSource
        {
            get => _dataSource;
            private set => _dataSource = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Tables
        {
            get => _tables;
            private set => _tables = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Headers
        {
            get => _headers;
            private set => _headers = value;
        }

        /// <summary>
        /// 
        /// </summary>
        private OracleConnection OracleConnection
        {
            set => _oracleConnection = value;
            get => _oracleConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        private OracleAuthData Auth
        {
            get => _auth;
            set => _auth = value;
        }

        /// <summary>
        /// 
        /// </summary>
        private string ConnectionString =>
            $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={Auth.Host})(PORT={Auth.Port})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={Auth.SID})));User Id={Auth.Username};Password={SecuritySettings.SecureStringToString(Auth.Password)};";

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (OracleConnection == null) return;
            OracleConnection.Close();
            OracleConnection.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ConnectoToUri()
        {
            if (OracleConnection != null) Disconnect();


            try
            {
                OracleConnection = new OracleConnection(ConnectionString);
                OracleConnection.Open();
                return true;
            }
            catch (Exception e)
            {
                OracleConnection = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (OracleConnection != null)
                {
                    OracleConnection.Close();
                    OracleConnection.Dispose();
                }

                OracleConnection = null;
            }
            catch
            {
                OracleConnection = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="limit"></param>
        public List<dynamic> ExecuteRequest(string Table, string condition = "", string orderBy = "", int limit = 500)
        {

            if (string.IsNullOrEmpty(Table) || null == OracleConnection)
            {
                return DataSource = default;
            }

            try
            {
                DiscoverTable(Table);
                if (limit <= 0 || limit > 500)
                {
                    limit = 500;
                }

                condition = string.IsNullOrEmpty(condition)
                    ? $"WHERE ROWNUM < {limit}"
                    : $"WHERE {condition} AND ROWNUM < {limit}";

                if (orderBy != null && orderBy.Trim() != "")
                {
                    condition += $" ORDER BY {orderBy}";
                }

                using var command = new OracleCommand($"SELECT * FROM {Table} {condition}", OracleConnection);
                using var adapter = new OracleDataAdapter(command);
                using var builder = new OracleCommandBuilder(adapter);
                using var dataSet = new DataSet();


                adapter.Fill(dataSet);

                var dict = new Dictionary<string, dynamic>();

                DataSource.Clear();

                var table = dataSet.Tables[0];
                foreach (DataRow data in table.Rows)
                {
                    foreach (DataColumn col in data.Table.Columns)
                    {
                        dict.Add(col.ColumnName, data[col.ColumnName]);
                    }

                    var eo = new ExpandoObject();
                    var eoColl = (ICollection<KeyValuePair<string, object>>) eo;

                    foreach (var kvp in dict)
                    {
                        eoColl.Add(kvp);
                    }

                    DataSource.Add((dynamic) eo);

                    dict.Clear();
                }
            }
            catch
            {
                DataSource = default;
            }

            return DataSource;
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Table"></param>
        private void DiscoverTable(string Table)
        {
            if (string.IsNullOrEmpty(Table) || null == OracleConnection)
            {
                return;
            }

            try
            {
                using var command = OracleConnection?.CreateCommand();
                command.BindByName = true;
                command.CommandText = "SELECT column_name FROM sys.all_tab_columns WHERE TABLE_NAME= :table";
                command.Parameters.Add(new OracleParameter("table", Table));
                using var reader = command.ExecuteReader();
                Headers.Clear();

                while (reader.Read())
                {
                    Headers.Add(reader.GetString(0));
                }
            } catch
            {
                Headers.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> DiscoverServer()
        {
            if (null == OracleConnection)
            {
                return default;
            }

            try
            {
                using var command = OracleConnection?.CreateCommand();
                command.BindByName = true;
                command.CommandText = "SELECT table_name FROM all_tables ORDER BY table_name DESC";
                using var reader = command.ExecuteReader();
                Tables.Clear();

                while (reader.Read())
                {
                    Tables.Add(reader.GetString(0));
                }
            } catch
            {
                Tables.Clear();
            }

            return Tables;
        }

        public void Dispose()
        {
            OracleConnection.Close();
            OracleConnection.Dispose();
        }
    }
}
