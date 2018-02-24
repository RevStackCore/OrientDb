using RevStackCore.Pattern;

namespace RevStackCore.OrientDb
{
    public interface IOrientEntity<TKey> : IEntity<TKey>
    {
        TKey RId { get; set; }
    }
}
