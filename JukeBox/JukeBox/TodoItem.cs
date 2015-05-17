using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace JukeBoxTester
{
    public class TodoItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "clientid")]
        public string clientid { get; set; }

        [JsonProperty(PropertyName = "songid")]
        public string songid { get; set; }

        [JsonProperty(PropertyName = "__createdAt")]
        public DateTime __createdAt { get; set; }
    }
}
