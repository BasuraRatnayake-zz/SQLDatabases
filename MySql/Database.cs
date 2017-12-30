using System;
using System.Collections.Generic;

namespace Infinitum.Databases {
    public abstract class Database {
        protected string server;
        protected string database;
        protected string uid;
        protected string password;

        protected int insertedId;

        protected string error_stack;
        protected string error_message;
        protected string occured_at;
        protected string lastQuery;

        public int lastInsertedId() {
            return insertedId;
        }

        public Dictionary<string, string> lastOccurredError() {
            Dictionary<string, string> error = new Dictionary<string, string>();
            error.Add("Message", error_message);
            error.Add("StackTrace", error_stack);
            error.Add("PlaceOfOrigin", occured_at);

            return error;
        }

        private bool EstablishConnection() { throw new NotImplementedException(); }
        private bool CloseConnection() { throw new NotImplementedException(); }

        public abstract bool Insert(string table, string values, string columns = null);
        public abstract bool Update(string table, string values, string where);
        public abstract bool Delete(string table, string where);
        public abstract List<Dictionary<string, string>> Select(string columns, string table, string where = null);
        public abstract int Count(string table, string where = null);
    }
}