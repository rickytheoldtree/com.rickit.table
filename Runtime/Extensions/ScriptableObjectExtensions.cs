using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RicKit.Table.Extensions
{
    public static class ScriptableObjectExtensions
    {
        public static TableScriptableObject ToTableSO(this Table table)
        {
            var tableSO = ScriptableObject.CreateInstance<TableScriptableObject>();
            tableSO.tableName = table.tableName;
            tableSO.columnInfos = table.columnInfos.Values.ToList();
            tableSO.rows = table.rowsData.Select(row => new TableScriptableObject.Row { values = table.columnInfos.Values.Select(c => row.TryGetValue(c.name, out var o) 
                ? o.ToString().Trim() : "").ToArray() }).ToList();
            return tableSO;
        }
        public static Table FromTableSO(TableScriptableObject tableScriptableObject)
        {
            var table = new Table
            {
                tableName = tableScriptableObject.tableName
            };
            for (var i = 0; i < tableScriptableObject.columnInfos.Count; i++)
            {
                var columnInfo = tableScriptableObject.columnInfos[i];
                table.columnInfos[i] = columnInfo;
            }
            foreach (var row in tableScriptableObject.rows)
            {
                var rowData = new Dictionary<string, object>();
                for (var i = 0; i < row.values.Length; i++)
                {
                    rowData[table.columnInfos[i].name] = Table.ConvertToType(row.values[i], table.columnInfos[i].type);
                }
                table.rowsData.Add(rowData);
            }
            return table;
        }
    }
}