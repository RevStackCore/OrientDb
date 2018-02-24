using System.Linq;
using System.Threading.Tasks;
using RevStackCore.OrientDb.Client;
using RevStackCore.OrientDb.Query;

namespace RevStackCore.OrientDb
{
    public class OrientDbQueryRepository : IOrientDbQueryRepository
    {
        private readonly OrientDbDatabase _database;
        private readonly HttpOrientDbGraphQueryProvider _queryProvider;

        public OrientDbQueryRepository(OrientDbContext context)
        {
            _database = context.Database;
            _queryProvider = new HttpOrientDbGraphQueryProvider(context.Connection);
        }

        public IQueryable<TModel> Find<TModel>(string query)
        {
            _queryProvider.QueryText = query;
            return new GraphQuery<TModel>(_queryProvider);
        }

        public Task<IQueryable<TModel>> FindAsync<TModel>(string query)
        {
            return Task.FromResult(Find<TModel>(query));
        }
    }
}
