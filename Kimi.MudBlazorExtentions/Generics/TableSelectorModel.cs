// ***********************************************************************
// Author           : kama zheng
// Created          : 04/16/2025
// ***********************************************************************

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
    );
}
