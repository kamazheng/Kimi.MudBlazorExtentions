using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Layouts;

/// <summary>
/// This is a generic tab component that can be used to create a tabbed interface. THomePage is the tab home page. THomePage must implement ITabHomePage, and create a [Parameter] of EventCallback, which will be called by RenderHomePage, pass in AddNewTabFromChild to create the new Tab based on the EventCallback parameters, include new tab Page type and its parameters, etc.
/// </summary>
/// <typeparam name="THomePage">Must implement ITabHomePage to add [Parameter] of EventCallback, which will be called by RenderHomePage, pass in AddNewTabFromChild to create the new Tab based on the EventCallback parameters, include Page type and its parameters, etc.</typeparam>
public partial class KimiTabs<THomePage> where THomePage : ComponentBase, ITabHomePage
{

    [Inject]
    public KimiJsInterop? _kimiJsInterop { get; set; }

    [Inject] IJSRuntime? JsRuntime { get; set; }
    private bool ConfirmExternalNavigation { get; set; } = true;

    [Inject]
    public IDialogService? _dialogService { get; set; }

    bool _stateHasChanged;
    private int activeTabIndex;
    private readonly List<TabItem> UserTabs = [];

    // ✅ 正确：把方法转为委托
    Func<string, bool, Task> closeDelegate => CloseActiveTab;

    public MudDynamicTabs MudTabs = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_kimiJsInterop is not null)
        {
            // 把面板容器高度限到"视口剩余空间"，配合 KimiTabs.razor.css 的 ::deep .mud-tabs-panels{overflow-y:auto}
            // 使页面内容(含表格底部分页器)在内部滚动、不被 .mud-tabs 的 overflow:hidden 裁掉。
            // ⚠️ 第三参 min-height 必须显式传 "0"：该参默认值是 "600px"，旧值 "max(100%, 600px)" 会在
            //    视口高 < 600px 的矮屏/低分屏上把面板强撑到 600px > 视口 → 分页器被顶出视口外永久不可见
            //    (典型"分页器偶尔有偶尔没有"——叠加本测量的时序竞态)。overflow-y:auto 由 CSS 恒定保证后，
            //    即便测量高度有时序偏差也仅是滚动区略有出入，分页器始终可滚到，竞态被降级为无害。
            await _kimiJsInterop.SetNotScrollMaxHeightByClass("mud-tabs-panels", 0, "0");
            await _kimiJsInterop.SetPageTitle(typeof(THomePage).Name);
        }
        if (_stateHasChanged)
        {
            _stateHasChanged = false;
            StateHasChanged();
        }
    }

    public void AddNewTab(Type componentType, Dictionary<string, object> parameters, string tabTitle, Func<string, bool, Task<bool>> OnClose)
    {
        var tabId = CalculateTabId(parameters, componentType);
        var existingTab = UserTabs.FirstOrDefault(t => t.Id == tabId);
        if (existingTab != null)
        {
            activeTabIndex = UserTabs.IndexOf(existingTab) + 1; //Home page is always at index 0
            _stateHasChanged = true;
        }
        else
        {
            var newTab = new TabItem
            {
                Id = tabId,
                Title = tabTitle,
                Parameters = parameters,
                Content = CreateTabContent(parameters, componentType),
                OnClose = OnClose
            };
            UserTabs.Add(newTab);
            activeTabIndex = UserTabs.Count;
        }
    }

    private static RenderFragment CreateTabContent(Dictionary<string, object> parameters, Type componentType) => builder =>
    {
        builder.OpenComponent(0, componentType);
        foreach (var parameter in parameters)
        {
            builder.AddAttribute(1, parameter.Key, parameter.Value);
        }
        builder.CloseComponent();
    };

    private static int CalculateTabId(Dictionary<string, object> parameters, Type componentType)
    {
        var hash = componentType!.FullName!.GetHashCode();
        foreach (var parameter in parameters)
        {
            hash = HashCode.Combine(hash, parameter.Key.GetHashCode(), parameter.Value.GetHashCode());
        }
        return hash;
    }

    public async Task CloseActiveTab(string confirmMsg = "", bool forceClose = false)
    {
        var tab = UserTabs[MudTabs.ActivePanelIndex - 1];
        if (tab.OnClose is not null)
        {
            var cfClose = await tab.OnClose.Invoke(
                string.IsNullOrEmpty(confirmMsg) ? "Are you sure to close this tab?" : confirmMsg, forceClose);
            if (!cfClose) return;
            RemoveTab(tab);
        }
        else
        {
            RemoveTab(tab);
        }
        activeTabIndex = 0;
        StateHasChanged();
    }

    public async Task CloseTab(int tabId, string confirmMsg = "", bool forceClose = false)
    {
        var tab = UserTabs.FirstOrDefault(t => t.Id == tabId);
        if (tab != null)
        {
            if (tab.OnClose is not null)
            {
                var cfClose = await tab.OnClose.Invoke(
                    string.IsNullOrEmpty(confirmMsg) ? "Are you sure to close this tab?" : confirmMsg, forceClose);
                if (cfClose)
                {
                    RemoveTab(tab);
                }
            }
            else
            {
                RemoveTab(tab);
            }
        }
        await Task.CompletedTask;
    }

    private void RemoveTab(TabItem tab)
    {
        UserTabs.Remove(tab);
        _stateHasChanged = true;
    }

    private RenderFragment RenderHomePage() => builder =>
    {
        builder.OpenComponent(0, typeof(THomePage));
        builder.AddAttribute(1, nameof(ITabHomePage.AddNewTabCallback), EventCallback.Factory.Create<(Type, Dictionary<string, object>, string, Func<string, bool, Task<bool>>)>
            (this, AddNewTabFromChild));
        builder.CloseComponent();
    };

    private void AddNewTabFromChild((Type componentType, Dictionary<string, object> parameters, string title, Func<string, bool, Task<bool>> onClose) args)
    {
        AddNewTab(args.componentType, args.parameters, args.title, args.onClose);
    }

    async Task CloseTabCallback(MudTabPanel panel) => await CloseTab((int)panel.ID!);

    protected virtual async Task OnBeforeInternalNavigation(LocationChangingContext locationChangingContext)
    {
        if (JsRuntime is null) return;
        var confirmMsg = "Are you sure you want to navigate away from this page? Your unsaving data will lost";
        bool confirmNavigation;
        if (_dialogService is not null)
        {
            confirmNavigation = await _dialogService.ConfirmV2Async("Confirm", confirmMsg, color: Color.Warning);
        }
        else
        {
            confirmNavigation = await JsRuntime.InvokeAsync<bool>("Confirm", confirmMsg);
        }
        if (!confirmNavigation) locationChangingContext.PreventNavigation();
    }


    private sealed class TabItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
        public RenderFragment? Content { get; set; }
        public bool ShowCloseIcon { get; set; } = true;
        public Func<string, bool, Task<bool>>? OnClose { get; set; }
    }
}