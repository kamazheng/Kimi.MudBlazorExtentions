## MudBlazor Extensions

This project is a collection of extensions for MudBlazor components. The extensions are designed to make it easier to use MudBlazor components in your projects.

## Installation

Install Package
```
dotnet add package Kimi.MudBlazorExtensions
```

## Components

- DialogBox

```C#

//Suport Nested Class Type
await _dialogService.InputBoxAsync<GetIssuesResponse?>("Input Box", "Please input your GetIssuesResponse");

//Support Tuple Type
await _dialogService.InputBoxAsync<(string Name, string Password, bool IsOk)>("Input Box", "Please input your credential", labels: ["Name", "Password", "Is Ok"]);

//Support Primitive Type
await _dialogService.InputBoxAsync<bool?>("Input Box", "Please input your yes/no");
await _dialogService.InputBoxAsync<DateOnly?>("Input Box", "Please input your Date");
await _dialogService.InputBoxAsync<string?>("Input Box", "Please input your String");
await _dialogService.InputBoxAsync<decimal?>("Input Box", "Please input your Decimal");
await _dialogService.InputBoxAsync<DateTime?>("Input Box", "Please input your DateTime");
await _dialogService.InputBoxAsync<TimeOnly?>("Input Box", "Please input your Time");

```
- SelectDialog

``` C#
var multiSelect = await _dialogService.MultiSelectBoxAsync<int>("MultiSelect Box", new List<SelectDialogItem<int>> {
    new SelectDialogItem<int>{ Text = "Hello1", Value=1 },
    new SelectDialogItem<int>{ Text = "Hello2", Value=2 },
    new SelectDialogItem<int>{ Text = "Hello3", Value=3 },
}, MudBlazor.Color.Default);
var singleSelect = await _dialogService.SelectBoxAsync<int>("Select Box", new List<SelectDialogItem<int>> {
    new SelectDialogItem<int>{ Text = "Hello1", Value=1 },
    new SelectDialogItem<int>{ Text = "Hello2", Value=2 },
    new SelectDialogItem<int>{ Text = "Hello3", Value=3 },
}, MudBlazor.Color.Default);
```