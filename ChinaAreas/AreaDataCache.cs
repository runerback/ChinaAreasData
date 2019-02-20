using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinaAreas
{
    sealed class AreaCache
    {
        public AreaCache(string name, string url)
        {
            this.name = name;
            this.url = url;
        }

        private readonly string name;
        public string Name => name;

        public string ID { get; set; }
        public string Type { get; set; }

        private readonly string url;
        public string URL => url;

        private readonly List<AreaCache> subAreas = new List<AreaCache>();
        public IEnumerable<AreaCache> SubAreas => subAreas;

        public bool HasSubAreas => subAreas.Count > 0;

        public void AddRange(IEnumerable<AreaCache> source)
        {
            subAreas.AddRange(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public Area ToData()
        {
            var data = new Area(name)
            {
                ID = ID,
                Type = Type
            };
            data.AddRange(subAreas.Select(item => item.ToData()));
            return data;
        }

        public override string ToString()
        {
            return $"{ID} - {name} ({Type})";
        }
    }
}
