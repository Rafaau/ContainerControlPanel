using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ContainerControlPanel.Web.Components.Utils;

public partial class InfiniteScrolling : IAsyncDisposable
{
    private ElementReference _lastItemIndicator;
    private DotNetObjectReference<InfiniteScrolling> _currentComponentReference;
    private IJSObjectReference _module;
    private IJSObjectReference _instance;
    private bool _loading = false;
    private bool _enumerationCompleted = false;
    private CancellationTokenSource _loadItemsCts;
    [Inject] IJSRuntime _jsRuntime { get; set; } = default!;

    [Parameter]
    public Func<Task> OnScrollToEnd { get; set; }

    [JSInvokable]
    public async Task LoadMoreItems()
    {
        if (_loading)
            return;

        _loading = true;
        try
        {
            OnScrollToEnd?.Invoke();

            StateHasChanged(); // Allow the UI to display the loading indicator
        }
        finally
        {
            _loading = false;
        }

        StateHasChanged(); // Display the new items and hide the loading indicator
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Initialize the IntersectionObserver
        if (firstRender)
        {
            _currentComponentReference = DotNetObjectReference.Create(this);
            _instance = await _jsRuntime.InvokeAsync<IJSObjectReference>("initialize", _lastItemIndicator, _currentComponentReference);
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Cancel the current load items operation
        if (_loadItemsCts != null)
        {
            _loadItemsCts.Dispose();
            _loadItemsCts = null;
        }

        // Stop the IntersectionObserver
        if (_instance != null)
        {
            await _instance.InvokeVoidAsync("dispose");
            await _instance.DisposeAsync();
            _instance = null;
        }

        if (_module != null)
        {
            await _module.DisposeAsync();
        }

        _currentComponentReference?.Dispose();
    }
}