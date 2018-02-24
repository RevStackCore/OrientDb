

namespace RevStackCore.OrientDb
{
    public interface IOrientEdgeEntity<TIn, TOut, TKey> : IOrientEntity<TKey>
    where TIn : class, IOrientEntity<TKey>
    where TOut : class, IOrientEntity<TKey>
    {
        TIn In { get; set; }
        TOut Out { get; set; }
    }
}
