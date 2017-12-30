using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Infinitum.Databases {
    public sealed class SQLite : Database {
        private SQLiteConnection connection;

        public SQLite(string database, bool first_time = false, string password = null) {
            this.database = database;
            this.password = password;            

            try {
                if (first_time) {
                    lastQuery = "Data Source=" + database + ";Version=3;";
                    connection = new SQLiteConnection(lastQuery);
                    if (!string.IsNullOrWhiteSpace(password)) connection.SetPassword(password.ToString());
                } else {
                    lastQuery = "Data Source="+ database + ";Version=3;";
                    if (!string.IsNullOrWhiteSpace(password)) lastQuery += "Password=" + password + ";";
                    connection = new SQLiteConnection(lastQuery);
                }
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Connection String";
            }
        }

        private bool EstablishConnection() {
            try {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();
                return true;
            } catch (SQLiteException ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Establishing Connection";
                return false;
            }
        }
        private bool CloseConnection() {
            try {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                return true;
            } catch (SQLiteException ex) {
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

                if (this.EstablishConnection()) {
                    SQLiteCommand cmd = new SQLiteCommand(lastQuery, connection);

                    cmd.ExecuteNonQuery();
                    insertedId = (int)cmd.ExecuteScalar();

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Inserting Data";
                return false;
            }
        }

        public override bool Update(string table, string values, string where = null) {
            try {
                if (!string.IsNullOrWhiteSpace(where))
                    where = "WHERE " + where;

                lastQuery = string.Format("UPDATE {0} SET {1} {2}", table, values, where);

                if (this.EstablishConnection()) {
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.CommandText = lastQuery;
                    cmd.Connection = connection;

                    cmd.ExecuteNonQuery();

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Updating Data";
                return false;
            }
        }

        public override bool Delete(string table, string where = null) {
            try {
                if (!string.IsNullOrWhiteSpace(where))
                    where = "WHERE " + where;

                lastQuery = string.Format("DELETE FROM {0} WHERE {1}", table, where);

                if (this.EstablishConnection() == true) {
                    SQLiteCommand cmd = new SQLiteCommand(lastQuery, connection);

                    cmd.ExecuteNonQuery();

                    this.CloseConnection();
                    return true;
                }
                return false;
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Deleting Data";
                return false;
            }
        }

        public override List<Dictionary<string, string>> Select(string columns, string table, string where = null) {
            try {
                if (!string.IsNullOrWhiteSpace(where))
                    where = "WHERE " + where;

                lastQuery = string.Format("SELECT {0} FROM {1} {2}", columns, table, where);

                int rowCount = this.Count(table, where);
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                Dictionary<string, string> data;

                if (rowCount == -1) {
                    return new List<Dictionary<string, string>>();
                } else {
                    if (this.EstablishConnection()) {
                        SQLiteCommand cmd = new SQLiteCommand(lastQuery, connection);
                        SQLiteDataReader dataReader = cmd.ExecuteReader();

                        while (dataReader.Read()) {
                            data = new Dictionary<string, string>();

                            for (int i = 0; i < dataReader.FieldCount; i++) {
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
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Getting Data";
                return new List<Dictionary<string, string>>();
            }
        }

        public override int Count(string table, string where = null) {
            if (!string.IsNullOrWhiteSpace(where))
                where = "WHERE " + where;

            lastQuery = string.Format("SELECT * FROM {0} {1}", table, where);
            int Count = -1;

            try {
                if (this.EstablishConnection()) {
                    SQLiteCommand cmd = new SQLiteCommand(lastQuery, connection);

                    Count = int.Parse(cmd.ExecuteScalar() + "");

                    this.CloseConnection();
                    return Count;
                } else {
                    return Count;
                }
            } catch (Exception ex) {
                error_message = ex.Message;
                error_stack = ex.StackTrace;
                occured_at = "Getting Row Count";
                return Count;
            }
        }
    }
}