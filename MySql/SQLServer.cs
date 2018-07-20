using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinitum.Databases {
    public sealed class SQLServer : Database {
        private SqlConnection connection;

        public SQLServer(string server, string database, string user_id, string password) {
            this.server = server;
            this.database = database;
            this.uid = user_id;
            this.password = password;

            lastQuery = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            try {
                connection = new SqlConnection(lastQuery);
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Connection String";
            }
        }

        public SQLServer(string connection_string) {
            try {
                lastQuery = connection_string;

                string[] data = lastQuery.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                int data_length = data.Length;
                if(data_length % 2 == 0) {
                    for(int i = 0;i < data_length / 2;i = +2) {
                        switch(data[i].ToUpper()) {
                            case "SERVER":
                                this.server = data[i + 1];
                                break;
                            case "DATABASE":
                                this.database = data[i + 1];
                                break;
                            case "UID":
                                this.uid = data[i + 1];
                                break;
                            case "PASSWORD":
                                this.password = data[i + 1];
                                break;
                        }
                    }
                }

                connection = new SqlConnection(connection_string);
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Connection String";
            }
        }

        private bool EstablishConnection() {
            try {
                if(connection.State != System.Data.ConnectionState.Open)
                    connection.Open();
                return true;
            } catch(SqlException ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Establishing Connection";
                return false;
            }
        }
        public bool CloseConnection() {
            try {
                if(connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                return true;
            } catch(SqlException ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Closing Connection";
                return false;
            }
        }

        public override bool Insert(string table, string values, string columns = null) {
            try {
                columns = (columns != null) ? "(" + columns + ")" : null;
                lastQuery = string.Format("INSERT INTO {0} {1} VALUES({2})", table, columns, values);

                if(this.EstablishConnection()) {
                    SqlCommand cmd = new SqlCommand(lastQuery, connection);

                    cmd.ExecuteNonQuery();
                    //insertedId = (int)cmd.LastInsertedId;

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Inserting Data";
                return false;
            }
        }

        public override bool Update(string table, string values, string where = null) {
            try {
                if(!string.IsNullOrWhiteSpace(where))
                    where = "WHERE " + where;

                lastQuery = string.Format("UPDATE {0} SET {1} {2}", table, values, where);

                if(this.EstablishConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = lastQuery;
                    cmd.Connection = connection;

                    cmd.ExecuteNonQuery();

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Updating Data";
                return false;
            }
        }

        public override bool Delete(string table, string where = null) {
            try {
                if(!string.IsNullOrWhiteSpace(where))
                    where = "WHERE " + where;

                lastQuery = string.Format("DELETE FROM {0} WHERE {1}", table, where);

                if(this.EstablishConnection() == true) {
                    SqlCommand cmd = new SqlCommand(lastQuery, connection);

                    cmd.ExecuteNonQuery();

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Deleting Data";
                return false;
            }
        }

        public override List<Dictionary<string, string>> Select(string columns, string table, string where = null) {
            try {
                lastQuery = string.Format("SELECT {0} FROM {1} {2}", columns, table, where);

                int rowCount = this.Count(table, where);
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                Dictionary<string, string> data;

                if(rowCount == -1) {
                    return new List<Dictionary<string, string>>();
                } else {
                    if(this.EstablishConnection()) {
                        SqlCommand cmd = new SqlCommand(lastQuery, connection);
                        SqlDataReader dataReader = cmd.ExecuteReader();

                        while(dataReader.Read()) {
                            data = new Dictionary<string, string>();

                            for(int i = 0;i < dataReader.FieldCount;i++) {
                                data.Add(dataReader.GetName(i), dataReader.GetValue(i).ToString());
                            }

                            list.Add(data);
                        }

                        dataReader.Close();
                        this.CloseConnection();
                        return list;
                    } else {
                        return list;
                    }
                }
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Getting Data";
                return new List<Dictionary<string, string>>();
            }
        }

        public override int Count(string table, string where = null) {
            lastQuery = string.Format("SELECT * FROM {0} {1}", table, where);
            int Count = -1;

            try {
                if(this.EstablishConnection()) {
                    SqlCommand cmd = new SqlCommand(lastQuery, connection);

                    dynamic dym = cmd.ExecuteScalar();
                    if(dym != null)
                        return 1;

                    Count = int.Parse(cmd.ExecuteScalar() + "");

                    this.CloseConnection();
                    return Count;
                } else {
                    return Count;
                }
            } catch(Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Getting Row Count";
                return Count;
            }
        }
    }
}
