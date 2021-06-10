using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SP.Studio.Data
{
    /// <summary>
    /// Access 的实现类
    /// </summary>
    public class OledbOperation : IDbOperation
    {
        public OledbOperation(DbExecutor db) : base(db) { }

        public override void Insert(string tableName, out object identity, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override bool InsertIdentity(string tableName, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override DataSet GetList(int pageIndex, int pageSize, string tableName, string fields, string condition, string sort, out int recordCount, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override DataSet GetList(string tableName, string fields, string condition, string sort, int top, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
