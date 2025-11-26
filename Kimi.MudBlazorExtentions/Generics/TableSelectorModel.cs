// ***********************************************************************
// Author           : kama zheng
// Created          : 04/16/2025
// ***********************************************************************

using System.Text.RegularExpressions;

namespace Kimi.MudBlazorExtentions.Generics;

public class TableSelectorModel
{
    public List<TableDefinition> TableList { get; set; }
    public string SelectedTableName { get; set; }
    public Type SelectedTableType { get; set; }

    public int PageSize { get; set; } = 1000;
    public int PageIndex { get; set; } = 1;
    public string? Filter { get; set; }
    public string? OrderBy { get; set; }

    public TableSelectorModel(List<TableDefinition> _tableList)
    {
        TableList = _tableList;
        SelectedTableName = TableList[0].TableName;
        SelectedTableType = TableList[0].TableClassType;
    }

    public record TableDefinition(string TableName,
    string TableDisplayName,
    Type TableClassType,
    bool AllowCreateNew = true,
    bool AllowExport = true,
    bool AllowImport = true
    )
    {
        public static TableDefinition From<T>(
            bool allowCreateNew = true,
            bool allowExport = true,
            bool allowImport = true
        ) where T : class
        {
            var name = typeof(T).Name;
            return new TableDefinition(
                name,
                SplitAndCapitalize(name),
                typeof(T),
                allowCreateNew,
                allowExport,
                allowImport
            );
        }
        private static string SplitAndCapitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            // 使用正则在大写字母前插入空格（但不在开头插入）
            string result = Regex.Replace(input, "(?<!^)([A-Z])", " $1");

            // 将每个单词首字母大写，其余保持原样
            result = Regex.Replace(result, @"\b[a-z]", m => m.Value.ToUpper());
            return result.Trim();
        }
    }
}



