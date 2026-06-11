using Kimi.MudBlazorExtentions.Dialogs;
using Kimi.MudBlazorExtentions.Layouts;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;
using System.Text.Json;

namespace Kimi.MudBlazorExtentions.Extensions;

/// <summary>
/// 集中式 API/异常 → UI 提示分流器。按钮族（ErrorCatchOnClickHandler）与应用 ErrorBoundary 都调它，保证一致体验。
/// </summary>
/// <remarks>
/// 分流（按 <see cref="HttpRequestException.StatusCode"/>）：
/// <list type="bullet">
///   <item>ReturnException → Info Snackbar（业务提示）</item>
///   <item>403 → AccessDeniedDialog（缺失角色/权限）</item>
///   <item>401 → Warning Snackbar + 跳登录</item>
///   <item>无状态码（网络失败）→ Error Snackbar</item>
///   <item>其它（含 5xx）/ 非 Http 异常 → MyErrorContent 对话框（默认主消息、详情看完整 InnerException）</item>
/// </list>
/// </remarks>
public static class ApiErrorPresenter
{
    public static async Task PresentAsync(
        Exception exception,
        ISnackbar? snackbar,
        IDialogService? dialogService,
        NavigationManager? navigation,
        string loginPath = "authentication/login")
    {
        switch (exception)
        {
            case ReturnException when snackbar is not null:
                snackbar.Info(exception.Message);
                return;

            case HttpRequestException http:
                await PresentHttpAsync(http, snackbar, dialogService, navigation, loginPath);
                return;

            default:
                await ShowErrorDialogAsync(exception, dialogService);
                return;
        }
    }

    private static async Task PresentHttpAsync(
        HttpRequestException http,
        ISnackbar? snackbar,
        IDialogService? dialogService,
        NavigationManager? navigation,
        string loginPath)
    {
        var payload = TryParse(http.Message);

        switch (http.StatusCode)
        {
            case HttpStatusCode.Forbidden:
                await ShowAccessDeniedAsync(payload, snackbar, dialogService);
                return;

            case HttpStatusCode.Unauthorized:
                snackbar?.Warning(payload?.Message ?? "登录已失效，请重新登录。");
                if (navigation is not null)
                {
                    var returnUrl = Uri.EscapeDataString(navigation.ToBaseRelativePath(navigation.Uri));
                    navigation.NavigateTo($"{loginPath}?returnUrl={returnUrl}", forceLoad: true);
                }
                return;

            case null: // 无响应：网络/连接失败，无服务器 inner 可看
                snackbar?.Error("网络连接失败，请检查网络后重试。");
                return;

            default: // 5xx 及其它服务器错误：用默认简洁+详情可展开的对话框，便于看 InnerException
                await ShowErrorDialogAsync(http, dialogService);
                return;
        }
    }

    private static async Task ShowAccessDeniedAsync(ApiErrorPayload? payload, ISnackbar? snackbar, IDialogService? dialogService)
    {
        var message = payload?.Message ?? "您没有执行此操作的权限。";
        if (dialogService is null)
        {
            snackbar?.Error(message);
            return;
        }

        var parameters = new DialogParameters<AccessDeniedDialog>
        {
            { x => x.Message, message },
            { x => x.MissingRoles, payload?.MissingRoles },
            { x => x.MissingPermissions, payload?.MissingPermissions },
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        await (await dialogService.ShowAsync<AccessDeniedDialog>("", parameters, options)).Result;
    }

    private static async Task ShowErrorDialogAsync(Exception exception, IDialogService? dialogService)
    {
        if (dialogService is null) return;
        var parameters = new DialogParameters<MyErrorContent>
        {
            { x => x.CurrentException, exception },
            { x => x.IfDialog, true },
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        await (await dialogService.ShowAsync<MyErrorContent>("", parameters, options)).Result;
    }

    private sealed record ApiErrorPayload(string? Message, List<string>? MissingRoles, List<string>? MissingPermissions);

    private static ApiErrorPayload? TryParse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            var message = GetString(root, "detail") ?? GetString(root, "message") ?? GetString(root, "title");
            return new ApiErrorPayload(message, GetStringList(root, "missingRoles"), GetStringList(root, "missingPermissions"));
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static List<string>? GetStringList(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Array) return null;
        var list = new List<string>();
        foreach (var item in v.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                list.Add(item.GetString()!);
            }
        }
        return list.Count > 0 ? list : null;
    }
}
