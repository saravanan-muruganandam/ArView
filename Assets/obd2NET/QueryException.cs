using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace obd2NET
{
    public class QueryException : Exception
    {
        public QueryException(string msg):
            base(msg)
        { }
    }
}
