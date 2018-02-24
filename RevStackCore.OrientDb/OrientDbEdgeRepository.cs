using System;
using System.Collections.Generic;
using System.Text;

namespace RevStackCore.OrientDb
{
    public class OrientDbEdgeRepository<TEdge, TIn, TOut, TKey> : OrientDbRepository<TEdge, TKey>, IOrientDbEdgeRepository<TEdge, TIn, TOut, TKey>
        where TEdge : class, IOrientEdgeEntity<TIn, TOut, TKey>
        where TIn : class, IOrientEntity<TKey>
        where TOut : class, IOrientEntity<TKey>
    {
        private readonly OrientDbContext _context;

        public OrientDbEdgeRepository(OrientDbContext context)
            : base(context)
        {
            _context = context;
        }

        public override TEdge Add(TEdge entity)
        {
            var type = entity.GetType();
            var name = type.Name;

            if (entity.In == null)
            {
                throw new System.Exception("'In' property of type OrientDbEntity is required");
            }

            if (entity.Out == null)
            {
                throw new System.Exception("'Out' property of type OrientDbEntity is required");
            }

            var typeName = Utils.OrientDbUtils.GetEntityIdType(entity);
            entity = Utils.OrientDbUtils.SetEntityIdProperty(entity);

            _context.Database.Execute("CREATE CLASS " + name + " EXTENDS E");

            //https://github.com/orientechnologies/orientdb/issues/5688
            //FIX: alter database custom standardElementConstraints=false
            _context.Database.Execute("alter database custom standardElementConstraints=false");
            _context.Database.Execute("CREATE PROPERTY " + name + ".id " + typeName);
            _context.Database.Execute("CREATE INDEX " + name + ".id UNIQUE");
            //FIX: Older orient instances
            //var q = "CREATE EDGE " + name + " FROM " + inRid + " to " + outRid + " SET id = '" + entity.Id + "'";
            var q = "CREATE EDGE " + name + " FROM " + entity.Out.RId + " to " + entity.In.RId + " content { 'id': '" + entity.Id + "'}";
            _context.Database.Execute(q);

            return base.Update(entity);
        }

        public override TEdge Update(TEdge entity)
        {
            var type = entity.GetType();
            var name = entity.GetType().Name;

            if (entity.In == null)
            {
                throw new System.Exception("'In' property of type OrientDbEntity is required");
            }

            if (entity.Out == null)
            {
                throw new System.Exception("'Out' property of type OrientDbEntity is required");
            }

            var q = "UPDATE EDGE " + name + " SET in = " + entity.In.RId + ", out = " + entity.Out.RId + " WHERE id ='" + entity.Id + "'";
            _context.Database.Execute(q);

            return base.Update(entity);
        }

        public override void Delete(TEdge entity)
        {
            var name = entity.GetType().Name;
            _context.Database.Execute("DELETE EDGE " + name + " where id = '" + entity.Id.ToString() + "'");
        }
    }
}
