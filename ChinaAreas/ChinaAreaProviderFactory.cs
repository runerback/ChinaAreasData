using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinaAreas
{
    public sealed class ChinaAreaProviderFactory
    {
        public IChinaAreaProvider Provider => new LatestChinaAreaProvider();
    }
}
