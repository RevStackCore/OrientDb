using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Query
{
    public abstract class ProjectionRow
    {
        public abstract object GetValue(int index);
    }
}
