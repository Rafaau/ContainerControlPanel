namespace ContainerControlPanel.Web.Components.Utils;

public sealed class InfiniteScrollingItemsProviderRequest
{
    public int StartIndex { get; }
    public CancellationToken CancellationToken { get; }

    public InfiniteScrollingItemsProviderRequest(int startIndex, CancellationToken cancellationToken)
    {
        StartIndex = startIndex;
        CancellationToken = cancellationToken;
    }
}

public delegate Task<IEnumerable<object>> InfiniteScrollingItemsProviderDelegate(InfiniteScrollingItemsProviderRequest request);