using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Dialogs;

/// <summary>
/// 通用「权限不足」对话框：展示友好消息与缺失的角色/权限。由 <see cref="Extensions.ApiErrorPresenter"/> 在收到 403 时弹出。
/// </summary>
public partial class AccessDeniedDialog
{
    /// <summary>友好提示消息（通常来自服务器 ProblemDetails 的 detail/message）。</summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>缺失的角色短名列表（可空）。</summary>
    [Parameter]
    public List<string>? MissingRoles { get; set; }

    /// <summary>缺失的权限名列表（可空）。</summary>
    [Parameter]
    public List<string>? MissingPermissions { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private void Close() => MudDialog.Close();
}
