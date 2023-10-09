using SmileBotFirstConnection.Models;

namespace SmileBotFirstConnection.MetApi;

public interface IMetApi
{
    Task<CollectionObjects> GetCollectionObjectsAsync();
    Task<CollectionItem> GetCollectionItemAsync(string objectNum);
    Task<CollectionObjects> SearchCollectionAsync(string query);
}
