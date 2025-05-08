using Microsoft.AspNetCore.Components;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class TableSelector
{
    [Parameter, EditorRequired]
    public TableSelectorModel TableSelectorModel { get; set; } = null!;
    [Parameter]
    public EventCallback<TableSelectorModel> TableSelectorModelChanged { get; set; }

    [Parameter]
    public string SelectTableLabel { get; set; } = "Select Table";

    [Parameter]
    public string PageSizeLabel { get; set; } = "Page Size";

    [Parameter]
    public string PageIndexLabel { get; set; } = "Page Index";

    [Parameter]
    public string OrderByLabel { get; set; } = "Order By";

    [Parameter]
    public string FilterLabel { get; set; } = "Filter";

    [Parameter]
    public int? ZIndex { get; set; } = 1000;

    [Parameter]
    public string? AndLabel { get; set; } = "AND";

    [Parameter]
    public string? OrLabel { get; set; } = "OR";

    [Parameter]
    public string? FieldLabel { get; set; } = "Field";
    [Parameter]
    public string? OperatorLabel { get; set; } = "Operator";
    [Parameter]
    public string? ValueLabel { get; set; } = "Value";
    [Parameter]
    public string? AscendLabel { get; set; } = "Ascend";

    [Parameter]
    public string? DescendLabel { get; set; } = "Descend";


    protected override async Task OnInitializedAsync()
    {
        await Task.CompletedTask;
    }

    private async Task SelectedTableChanged(string tableName)
    {
        TableSelectorModel.SelectedTableName = tableName;
        TableSelectorModel.SelectedTableType = TableSelectorModel.TableList.FirstOrDefault(m => m.TableName == tableName)!.TableClassType;
        await TableSelectorModelChanged.InvokeAsync(TableSelectorModel);
        StateHasChanged();
    }

    private async Task PageIndexChanged(int pageIndex)
    {
        TableSelectorModel.PageIndex = pageIndex;
        await TableSelectorModelChanged.InvokeAsync(TableSelectorModel);
    }
    private async Task PageSizeChanged(int pagesize)
    {
        TableSelectorModel.PageSize = pagesize;
        await TableSelectorModelChanged.InvokeAsync(TableSelectorModel);
    }

    private async Task OrderByChanged(string orderBy)
    {
        TableSelectorModel.OrderBy = orderBy;
        await TableSelectorModelChanged.InvokeAsync(TableSelectorModel);
    }

    private async Task FilterChanged(string filter)
    {
        TableSelectorModel.Filter = filter;
        await TableSelectorModelChanged.InvokeAsync(TableSelectorModel);
    }
}