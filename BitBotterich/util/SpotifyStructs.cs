using System.Text.Json.Serialization;

namespace BitBotterich.util
{
    public class Token
    {
        public string? access_token { get; set; }

        public string? token_type { get; set; }

        public int expires_in { get; set; }
    }

    public class SpotifySearch
    {
        public class ExternalUrls
        {
            public string? spotify { get; set; }
        }

        public class Image
        {
            public int width { get; set; }
            public string? url { get; set; }
            public int height { get; set; }
        }

        public class Followers
        {
            public object? href { get; set; }
            public int total { get; set; }
        }

        public class Artist
        {
            public ExternalUrls? external_urls { get; set; }
            public Followers? followers { get; set; }
            public List<string>? genres { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public List<Image>? images { get; set; }
            public string? name { get; set; }
            public int popularity { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
        }

        public class ArtistCollection
        {
            public string? href { get; set; }
            public List<Artist>? items { get; set; }
            public int limit { get; set; }
            public string? next { get; set; }
            public int offset { get; set; }
            public string? previous { get; set; }
            public int total { get; set; }
        }
        
        public class ArtistSearchResult
        {
            public ArtistCollection? artists { get; set; }
        }

        public class ArtistShort
        {
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public string? name { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
        }

        public class Album
        {
            public string? album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string>? available_markets { get; set; }
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public List<Image>? images { get; set; }
            public string? name { get; set; }
            public string? release_date { get; set; }
            public string? release_date_precision { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
            public List<ArtistShort>? artists { get; set; }
            public string? album_group { get; set; }
        }

        public class AlbumCollection
        {
            public string? href { get; set; }
            public List<Album>? items { get; set; }
            public int limit { get; set; }
            public string? next { get; set; }
            public int offset { get; set; }
            public string? previous { get; set; }
            public int total { get; set; }
        }

        public class AlbumResult
        {
            public AlbumCollection? albums { get; set; }
        }

        public class SimpleTrack
        {
            public List<ArtistShort>? artists { get; set; }
            public List<string>? available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            [JsonPropertyName("explicit")]
            public bool isExplicit { get; set; }
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public string? name { get; set; }
            public string? preview_url { get; set; }
            public int track_number { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
            public bool is_local { get; set; }
        }

        public class Track
        {
            public string? href { get; set; }
            public int limit { get; set; }
            public string? next { get; set; }
            public int offset { get; set; }
            public string? previous { get; set; }
            public int total { get; set; }
            public List<SimpleTrack>? items { get; set; }
        }

        public class Copyright
        {
            public string? text { get; set; }
            public string? type { get; set; }
        }

        public class ExternalIds
        {
            public string? upc { get; set; }
        }

        public class AlbumFull
        {
            public string? album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string>? available_markets { get; set; }
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public List<Image>? images { get; set; }
            public string? name { get; set; }
            public string? release_date { get; set; }
            public string? release_date_precision { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
            public List<ArtistShort>? artists { get; set; }
            public Track? tracks { get; set; }
            public List<Copyright>? copyrights { get; set; }
            public ExternalIds? external_ids { get; set; }
            public List<string>? genres { get; set; }
            public string? label { get; set; }
            public int popularity { get; set; }
        }

        public class AlbumTrack
        {
            public string? album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string>? available_markets { get; set; }
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public List<Image>? images { get; set; }
            public string? name { get; set; }
            public string? release_date { get; set; }
            public string? release_date_precision { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
            public List<ArtistShort>? artists { get; set; }
            public bool is_playable { get; set; }
        }

        public class TrackFull
        {
            public AlbumTrack? album { get; set; }
            public List<ArtistShort>? artists { get; set; }
            public List<string>? available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            [JsonPropertyName("explicit")]
            public bool is_explicit { get; set; }
            public ExternalIds? external_ids { get; set; }
            public ExternalUrls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public bool is_playable { get; set; }
            public string? name { get; set; }
            public int popularity { get; set; }
            public string? preview_url { get; set; }
            public int track_number { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
            public bool is_local { get; set; }
        }
    }
}
