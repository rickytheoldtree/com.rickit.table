using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RicKit.Table
{
    public class Table
    {
        public enum ColumnType
        {
            CustomType,
            Int,
            Float,
            String,
        }
        public interface IFromRow
        {
            bool TryConvert(Dictionary<string, object> row);
        }
        [Serializable]
        public struct ColumnInfo
        {
            public string name;
            public ColumnType type;
        }

        public string tableName = "";
        public readonly Dictionary<int, ColumnInfo> columnInfos = new Dictionary<int, ColumnInfo>();
        public readonly List<Dictionary<string, object>> rowsData = new List<Dictionary<string, object>>();

        public Dictionary<string, object> ParseRow(IReadOnlyList<string> columns)
        {
            var row = new Dictionary<string, object>();

            for (var i = 0; i < columns.Count; i++)
            {
                if (columnInfos.TryGetValue(i, out var columnInfo))
                {
                    row[columnInfo.name] = ConvertToType(columns[i], columnInfo.type);
                }
            }

            return row;
        }

        public static object ConvertToType(string value, ColumnType type)
        {
            switch (type)
            {
                case ColumnType.Int:
                    return int.TryParse(value, out var intValue) ? intValue : 0;
                case ColumnType.Float:
                    return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue) ? floatValue : 0f;
                case ColumnType.String:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static ColumnType ConvertToColumnType(string value)
        {
            switch (value)
            {
                case "Int":
                case "int":
                    return ColumnType.Int;
                case "Float":
                case "float":
                    return ColumnType.Float;
                case "String":
                case "string":
                    return ColumnType.String;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public List<T> ToList<T>() where T : IFromRow, new()
        {
            var languageConfigs = new List<T>();
            foreach (var row in rowsData)
            {
                var config = new T();
                if (config.TryConvert(row))
                {
                    languageConfigs.Add(config);
                }
            }
            return languageConfigs;
        }
    }
    
    public class StringListRow : Table.IFromRow
    {
        public List<string> values;
        public bool TryConvert(Dictionary<string, object> row)
        {
            values = row.Values.Select(v => v.ToString()).ToList();
            return true;
        }
    }
}