using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace JukeBoxTester
{
   public class Song
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "pathname")]
        public string PathString { get; set; }

        [JsonProperty(PropertyName = "songname")]
        public string SongName { get; set; }

        [JsonProperty(PropertyName = "artistname")]
        public string ArtistName { get; set; }

        [JsonProperty(PropertyName = "albumname")]
        public string AlbumName { get; set; }
    }
}
