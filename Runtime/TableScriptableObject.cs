using System;
using System.Collections.Generic;
using UnityEngine;

namespace RicKit.Table
{
    public class TableScriptableObject : ScriptableObject
    {
        public string tableName;
        public List<Table.ColumnInfo> columnInfos;
        public List<Row> rows;
        
        [Serializable]
        public struct Row
        {
            public string[] values;
        }
    }
}