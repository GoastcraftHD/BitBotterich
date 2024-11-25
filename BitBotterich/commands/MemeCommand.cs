using BitBotterich.util;
using Discord.Interactions;
using Newtonsoft.Json;
using RestSharp;

namespace BitBotterich.commands
{
    public class MemeCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private readonly InteractionHandler _handler;

        public MemeCommand(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("meme", "Schickt ein zufälliges meme gif")]
        public async Task Meme()
        {
            string? tenorAPIKey = Program.Config!["TENOR_API_KEY"];
            string? tenorAPPName = Program.Config["TENOR_APP_NAME"];

            if (string.IsNullOrEmpty(tenorAPIKey) || string.IsNullOrEmpty(tenorAPPName))
            {
                await ReplyAsync("Error: Could not retrieve Tenor credentials!");
                return;
            }

            RestClient client = new RestClient("https://tenor.googleapis.com/v2/search");
            RestRequest request = new RestRequest($"?q=meme&key={tenorAPIKey}&client_key={tenorAPPName}&random=true&media_filter=gif&limit=1");
            RestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                await RespondAsync("Error: Can't connect to Tenor server!");
                return;
            }

            if (response.Content is null)
            {
                await RespondAsync("Error: Tenor returned nothing!");
                return;
            }

            TenorStructs.Result result = JsonConvert.DeserializeObject<TenorStructs.Result>(response.Content);

            await RespondAsync(result.results[0].media_formats.gif.url);
        }
    }
}
