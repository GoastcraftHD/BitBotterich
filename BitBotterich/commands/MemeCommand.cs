using BitBotterich.util;
using Discord.Interactions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitBotterich.commands
{
    public class MemeCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private InteractionHandler _handler;

        public MemeCommand(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("meme", "Schickt ein zufälliges meme gif")]
        public async Task Meme()
        {
            RestClient client = new RestClient("https://tenor.googleapis.com/v2/search");
            RestRequest request = new RestRequest($"?q=meme&key={Program.Config["TENOR_API_KEY"]}&client_key={Program.Config["TENOR_APP_NAME"]}&random=true&media_filter=gif&limit=1");
            RestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                await RespondAsync("Error: Can't connect to Tenor server!");
                return;
            }

            TenorStructs.Result result = JsonConvert.DeserializeObject<TenorStructs.Result>(response.Content);

            await RespondAsync(result.results[0].media_formats.gif.url);
        }
    }
}
