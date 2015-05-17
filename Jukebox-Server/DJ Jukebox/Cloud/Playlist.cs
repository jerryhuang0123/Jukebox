using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace JukeBoxTester
{
    class Playlist
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "songid")]
        public string songid { get; set; }

        [JsonProperty(PropertyName = "songname")]
        public string SongName { get; set; }

        [JsonProperty(PropertyName = "artistname")]
        public string ArtistName { get; set; }

        [JsonProperty(PropertyName = "albumname")]
        public string AlbumName { get; set; }

        [JsonProperty(PropertyName = "state")]
        public int State { get; set; }
    }
}
