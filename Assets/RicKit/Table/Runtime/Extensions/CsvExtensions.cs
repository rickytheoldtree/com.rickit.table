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
                "#" + ConvertToCsvField(table.tableName)
            };

            // 添加列头和列类型
            var headerLine = "#," + string.Join(",", table.columnInfos.Values.Select(c => ConvertToCsvField(c.name)));
            var typeLine = "#," + string.Join(",", table.columnInfos.Values.Select(c => c.type));
            lines.Add(headerLine);
            lines.Add(typeLine);

            // 添加数据行
            foreach (var row in table.rowsData)
            {
                var rowData = table.columnInfos.Keys.Select(index =>
                    row.ContainsKey(table.columnInfos[index].name) ? 
                        ConvertToCsvField(row[table.columnInfos[index].name].ToString()) : "");
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
                var columns = ParseCsvLine(lines[lineIndex]);

                if (lineIndex == 0 && columns[0].StartsWith("#"))
                {
                    table.tableName = columns[0].TrimStart('#').Trim();
                    tableNameFound = true;
                }
                else if (tableNameFound && !headersFound)
                {
                    for (var i = 0; i < columns.Count; i++)
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
                    for (var i = 0; i < columns.Count; i++)
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

        private static string ConvertToCsvField(string field)
        {
            // 检查是否需要将字段用双引号包围：字段中包含逗号、换行符或双引号
            bool requiresQuotes = field.Contains(",") || field.Contains("\n") || field.Contains("\"");

            // 对字段中的双引号进行转义（替换为两个双引号）
            string escapedField = field.Replace("\"", "\"\"");

            // 如果需要，将转义后的字段用双引号包围
            if (requiresQuotes)
            {
                escapedField = $"\"{escapedField}\"";
            }

            return escapedField;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = "";
            var insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (insideQuotes)
                    {
                        if (i < line.Length - 1 && line[i + 1] == '"')
                        {
                            // 双引号转义
                            currentField += '"';
                            i++;
                        }
                        else
                        {
                            // 结束引号
                            insideQuotes = false;
                        }
                    }
                    else
                    {
                        // 开始引号
                        insideQuotes = true;
                    }
                }
                else if (c == ',')
                {
                    if (insideQuotes)
                    {
                        // 逗号在引号内部，是字段的一部分
                        currentField += c;
                    }
                    else
                    {
                        // 逗号在引号外部，表示字段结束
                        fields.Add(currentField);
                        currentField = "";
                    }
                }
                else
                {
                    // 普通字符，添加到当前字段
                    currentField += c;
                }
            }

            // 添加最后一个字段
            fields.Add(currentField);

            return fields;
        }
    }
}