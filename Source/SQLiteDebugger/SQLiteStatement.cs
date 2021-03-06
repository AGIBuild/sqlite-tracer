﻿namespace SQLiteDebugger
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class SQLiteStatement : IDisposable
    {
        private IntPtr stmt;

        internal SQLiteStatement(IntPtr stmt)
        {
            this.stmt = stmt;
        }

        ~SQLiteStatement()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Disposing of object")]
        protected virtual void Dispose(bool disposing)
        {
            UnsafeNativeMethods.sqlite3_finalize(this.stmt);
        }

        public void Reset()
        {
            var rc = UnsafeNativeMethods.sqlite3_reset(this.stmt);
            if (rc != UnsafeNativeMethods.SQLITE_OK)
            {
                throw new InvalidOperationException("SQLite could not reset the statement");
            }
        }

        public void Bind(int index, int number)
        {
            var rc = UnsafeNativeMethods.sqlite3_bind_int(this.stmt, index, number);
            if (rc != UnsafeNativeMethods.SQLITE_OK)
            {
                throw new InvalidOperationException("SQLite could not bind the parameter");
            }
        }

        public void Bind(int index, string text)
        {
            if (text == null)
            {
                this.Bind(index);
                return;
            }

            var rc = UnsafeNativeMethods.sqlite3_bind_text(this.stmt, index, text, -1, new IntPtr(-1));
            if (rc != UnsafeNativeMethods.SQLITE_OK)
            {
                throw new InvalidOperationException("SQLite could not bind the parameter");
            }
        }

        public void Bind(int index)
        {
            var rc = UnsafeNativeMethods.sqlite3_bind_null(this.stmt, index); 
            if (rc != UnsafeNativeMethods.SQLITE_OK)
            {
                throw new InvalidOperationException("SQLite could not bind the parameter");
            }
        }

        public int ColumnInt(int index)
        {
            return UnsafeNativeMethods.sqlite3_column_int(this.stmt, index);
        }

        public string ColumnText(int index)
        {
            var text = UnsafeNativeMethods.sqlite3_column_text(this.stmt, index);
            return StatementInterceptor.UTF8ToString(text);
        }

        public bool Step()
        {
            var rc = UnsafeNativeMethods.sqlite3_step(this.stmt);
            switch (rc)
            {
            case UnsafeNativeMethods.SQLITE_ROW:
                return true;
            case UnsafeNativeMethods.SQLITE_DONE:
                return false;
            default:
                throw new InvalidOperationException("SQLite could not run the query");
            }
        }
    }
}