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


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_kimiJsInterop is not null)
        {
            await _kimiJsInterop.SetNotScrollMaxHeightByClass("mud-tabs-panels", 10);
            await _kimiJsInterop.SetPageTitle(typeof(THomePage).Name);
        }
        if (_stateHasChanged)
        {
            _stateHasChanged = false;
            StateHasChanged();
        }
    }

    public void AddNewTab(Type componentType, Dictionary<string, object> parameters, string tabTitle, Func<Task<bool>> OnClose)
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

    public async Task CloseTab(int tabId)
    {
        var tab = UserTabs.FirstOrDefault(t => t.Id == tabId);
        if (tab != null)
        {
            if (tab.OnClose is not null)
            {
                var cfClose = await tab.OnClose.Invoke();
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
        builder.AddAttribute(1, nameof(ITabHomePage.AddNewTabCallback), EventCallback.Factory.Create<(Type, Dictionary<string, object>, string, Func<Task<bool>>)>
            (this, AddNewTabFromChild));
        builder.CloseComponent();
    };

    private void AddNewTabFromChild((Type componentType, Dictionary<string, object> parameters, string title, Func<Task<bool>> onClose) args)
    {
        AddNewTab(args.componentType, args.parameters, args.title, args.onClose);
    }

    async Task CloseTabCallback(MudTabPanel panel) => await CloseTab((int)panel.ID!);

    private async Task OnBeforeInternalNavigation(LocationChangingContext locationChangingContext)
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
        public Func<Task<bool>>? OnClose { get; set; }
    }
}