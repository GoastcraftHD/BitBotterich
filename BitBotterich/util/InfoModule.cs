using Discord.Interactions;

namespace BitBotterich
{
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;

        public InfoModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("say", "Repeat the input")]
        public async Task Say(string message)
            => await RespondAsync(message);

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);
    }
}
