using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>The plugin's answer when a request runs once per list item or context value: one record per run.</summary>
internal sealed class BridgeListDto<TValues> where TValues : new()
{
    private List<BridgeRecordDto<TValues>> _items = [];

    public int Count { get; set; }

    /// <summary>The whole list's size, when only a page of it was read (BridgeQuery.Take).</summary>
    public int? Total { get; set; }

    /// <summary>Empty when the answer had no "items" at all: <see cref="ItemsSent"/> tells that from an empty list.</summary>
    public List<BridgeRecordDto<TValues>> Items
    {
        get => _items;
        set
        {
            _items = value;
            ItemsSent = true;
        }
    }

    /// <summary>The answer had "items" (even null or empty). Without it, an empty Items is not an empty list.</summary>
    [JsonIgnore]
    public bool ItemsSent { get; private set; }
}
