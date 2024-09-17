using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using static BitBotterich.util.QuoteHelper;

namespace BitBotterich.commands
{
    public class QuoteCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private InteractionHandler _handler;

        public QuoteCommand(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("zitat", "Erstellt einen neuen Zitat Zähler.")]
        public async Task CreateQuote(string zitat, string name)
        {
            Guid quoteId = Guid.NewGuid();

            Quote quote = new Quote();
            quote.Text = zitat;
            quote.Name = name;
            quote.Count = 1;

            QuoteCounter.Add(quoteId.ToString(), quote);

            ComponentBuilder builder = new ComponentBuilder()
                .WithButton(" ", "addQuoteCounter:" + quoteId, ButtonStyle.Primary, Emoji.Parse(":heavy_plus_sign:"))
                .WithButton(" ", "subtractQuoteCounter:" + quoteId, ButtonStyle.Danger, Emoji.Parse(":heavy_minus_sign:"));

            await RespondAsync(CreateQuoteText(quote), components: builder.Build());

            SaveQuotes();
        }

        [ComponentInteraction("addQuoteCounter:*", true)]
        public async Task AddQuote(string quoteId)
        {
            Quote quote;

            if (!QuoteCounter.TryGetValue(quoteId, out quote))
            {
                await ReplyAsync($"Error! Couldn't retrive Guid '{quoteId}' from Quote Dictonary!");
                return;
            }

            quote.Count++;
            QuoteCounter[quoteId] = quote;

            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;
            await originalMsg.UpdateAsync(x =>
            {
                x.Content = CreateQuoteText(quote);
            });
            
            SaveQuotes();
        }

        [ComponentInteraction("subtractQuoteCounter:*", true)]
        public async Task SubtractQuote(string quoteId)
        {
            Quote quote;

            if (!QuoteCounter.TryGetValue(quoteId, out quote))
            {
                await ReplyAsync($"Error! Couldn't retrive Guid '{quoteId}' from Quote Dictonary!");
                return;
            }

            quote.Count--;
            QuoteCounter[quoteId] = quote;

            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;
            await originalMsg.UpdateAsync(x =>
            {
                x.Content = CreateQuoteText(quote);
            });

            SaveQuotes();
        }
    }
}
