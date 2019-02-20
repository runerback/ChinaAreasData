using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinaAreas
{
    sealed class Area
    {
        public Area(string name)
        {
            this.name = name;
        }

        private readonly string name;
        [JsonProperty("name", Order = 1)]
        public string Name => name;

        [JsonProperty("id", Order = 0)]
        public string ID { get; set; }
        [JsonProperty("type", Order = 2)]
        public string Type { get; set; }

        private readonly List<Area> subAreas = new List<Area>();
        [JsonProperty("sub", Order = 3)]
        public IEnumerable<Area> SubAreas => subAreas;

        public void AddRange(IEnumerable<Area> source)
        {
            subAreas.AddRange(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public bool ShouldSerializeSubAreas()
        {
            return subAreas.Count > 0;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override string ToString()
        {
            return $"{ID} - {name} ({Type})";
        }
    }
}
