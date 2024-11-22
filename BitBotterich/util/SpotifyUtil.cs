using Newtonsoft.Json;
using RestSharp;
using System.Text;
using static BitBotterich.util.SpotifySearch;

namespace BitBotterich.util
{
    class SpotifyUtil
    {
        public static Token? Token { get; private set; }
        public static string? ClientID { get; set; }
        public static string? ClientSecret { get; set; }

        public static async Task GetTokenAsync()
        {
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(ClientID + ":" + ClientSecret));

            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
            HttpContent content = new FormUrlEncodedContent(args);

            HttpResponseMessage resp = await client.PostAsync("https://accounts.spotify.com/api/token", content);
            string msg = await resp.Content.ReadAsStringAsync();

            Token = JsonConvert.DeserializeObject<Token>(msg);
        }

        public static async Task<ArtistSearchResult?> SearchArtist(string searchWord, int limit = 20, int offset = 0, int retries = 0)
        {
            if (Token is null)
            {
                Console.WriteLine("Error: Spotify Token is null!");
                return null;
            }

            RestClient client = new RestClient("https://api.spotify.com/v1/search");
            client.AddDefaultHeader("Authorization", $"Bearer {Token.access_token}");

            RestRequest request = new RestRequest($"?q={searchWord}&type=artist&limit={limit}&offset={offset}", Method.Get);
            RestResponse response = client.Execute(request);

            if (response.Content is null)
            {
                Console.WriteLine("Error: Tenor returned nothing!");
                return null;
            }

            if (response.IsSuccessful)
            {
                ArtistSearchResult? result = JsonConvert.DeserializeObject<ArtistSearchResult>(response.Content);
                return result;
            }
            else
            {
                if (retries > 0)
                    return null;

                Console.WriteLine("Got new Token");

                await GetTokenAsync();
                return await SearchArtist(searchWord, limit, offset, ++retries);
            }
        }

        public static Artist? GetArtist(string id)
        {
            if (Token is null)
            {
                Console.WriteLine("Error: Spotify Token is null!");
                return null;
            }

            RestClient client = new RestClient("https://api.spotify.com/v1/artists");
            client.AddDefaultHeader("Authorization", $"Bearer {Token.access_token}");

            RestRequest request = new RestRequest($"{id}", Method.Get);
            RestResponse response = client.Execute(request);

            if (response.Content is null)
            {
                Console.WriteLine("Error: Tenor returned nothing!");
                return null;
            }

            if (response.IsSuccessful)
            {
                Artist? result = JsonConvert.DeserializeObject<Artist>(response.Content);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static AlbumCollection? GetArtistsAlbums(string artistId, string? filter, int limit, int offset)
        {
            if (Token is null)
            {
                Console.WriteLine("Error: Spotify Token is null!");
                return null;
            }

            RestClient client = new RestClient("https://api.spotify.com/v1/artists");
            client.AddDefaultHeader("Authorization", $"Bearer {Token.access_token}");

            string resource = $"{artistId}/albums?limit={limit}&offset={offset}";

            if (!String.IsNullOrEmpty(filter))
                resource += $"&include_groups={filter}";

            RestRequest request = new RestRequest(resource, Method.Get);
            RestResponse response = client.Execute(request);

            if (response.Content is null)
            {
                Console.WriteLine("Error: Tenor returned nothing!");
                return null;
            }

            if (response.IsSuccessful)
            {
                AlbumCollection? result = JsonConvert.DeserializeObject<AlbumCollection>(response.Content);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static AlbumFull? GetAlbum(string albumId)
        {
            if (Token is null)
            {
                Console.WriteLine("Error: Spotify Token is null!");
                return null;
            }

            RestClient client = new RestClient("https://api.spotify.com/v1/albums");
            client.AddDefaultHeader("Authorization", $"Bearer {Token.access_token}");

            RestRequest request = new RestRequest($"{albumId}", Method.Get);
            RestResponse response = client.Execute(request);

            if (response.Content is null)
            {
                Console.WriteLine("Error: Tenor returned nothing!");
                return null;
            }

            if (response.IsSuccessful)
            {
                AlbumFull? result = JsonConvert.DeserializeObject<AlbumFull>(response.Content);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static TrackFull? GetTrack(string trackId)
        {
            if (Token is null)
            {
                Console.WriteLine("Error: Spotify Token is null!");
                return null;
            }

            RestClient client = new RestClient("https://api.spotify.com/v1/tracks");
            client.AddDefaultHeader("Authorization", $"Bearer {Token.access_token}");

            RestRequest request = new RestRequest($"{trackId}", Method.Get);
            RestResponse response = client.Execute(request);

            if (response.Content is null)
            {
                Console.WriteLine("Error: Tenor returned nothing!");
                return null;
            }

            if (response.IsSuccessful)
            {
                TrackFull? result = JsonConvert.DeserializeObject<TrackFull>(response.Content);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
