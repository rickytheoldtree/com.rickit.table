using System;
using System.Collections.Generic;
using System.Linq;

namespace RicKit.Table.Extensions
{
    public static class CsvExtensions
    {
        public static string ToCsv(this Table table)
        {
            var lines = new List<string>
            {
                // 添加表名
                "#" + table.tableName
            };

            // 添加列头和列类型
            var headerLine = "#," + string.Join(",", table.columnInfos.Values.Select(c => c.name));
            var typeLine = "#," + string.Join(",", table.columnInfos.Values.Select(c => c.type));
            lines.Add(headerLine);
            lines.Add(typeLine);

            // 添加数据行
            foreach (var row in table.rowsData)
            {
                var rowData = table.columnInfos.Keys.Select(index =>
                    row.ContainsKey(table.columnInfos[index].name) ? row[table.columnInfos[index].name].ToString() : "");
                lines.Add($",{string.Join(",", rowData)}");
            }

            return string.Join("\n", lines);
        }

        public static Table FromCsv(string csvText)
        {
            var table = new Table();
            var lines = csvText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var tableNameFound = false;
            var headersFound = false;
            var typesFound = false;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var columns = lines[lineIndex].Split(',');

                if (lineIndex == 0 && columns[0].StartsWith("#"))
                {
                    table.tableName = columns[0].TrimStart('#').Trim();
                    tableNameFound = true;
                }
                else if (tableNameFound && !headersFound)
                {
                    for (var i = 0; i < columns.Length; i++)
                    {
                        var str = columns[i].Trim();
                        if (str.StartsWith("#"))
                        {
                            columns[i] = str.TrimStart('#').Trim();
                        }

                        if (!string.IsNullOrWhiteSpace(columns[i]))
                        {
                            table.columnInfos[i] = new Table.ColumnInfo { name = columns[i] };
                        }
                    }

                    headersFound = true;
                }
                else if (headersFound && !typesFound)
                {
                    for (var i = 0; i < columns.Length; i++)
                    {
                        if (table.columnInfos.ContainsKey(i))
                        {
                            var columnInfo = table.columnInfos[i];
                            columnInfo.type = Table.ConvertToColumnType(columns[i].Trim());
                            table.columnInfos[i] = columnInfo;
                        }
                    }

                    typesFound = true;
                }
                else if (typesFound)
                {
                    var row = table.ParseRow(columns);
                    if (row != null)
                    {
                        table.rowsData.Add(row);
                    }
                }
            }
            return table;
        }
    }
}