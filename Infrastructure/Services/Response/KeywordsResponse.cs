using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class KeywordsResponse
    {
        [JsonPropertyName("sentence")]
        public string sentence { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> keywords { get; set; }
    }
}
