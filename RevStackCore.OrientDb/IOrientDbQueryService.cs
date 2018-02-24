using System.Linq;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb
{
    public interface IOrientDbQueryService
    {
        IQueryable<TModel> Find<TModel>(string query);
        Task<IQueryable<TModel>> FindAsync<TModel>(string query);
    }
}
