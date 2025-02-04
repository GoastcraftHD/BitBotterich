using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using static BitBotterich.util.QuoteHelper;
using BitBotterich.util;

namespace BitBotterich.commands
{
    public class QuoteCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
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

        [SlashCommand("lade-zitate", "Lädt alle ungeladenen Zitate.")]
        public async Task RefreshQuotes()
        {
#if DEBUG
            ulong botId = 742192192562659360;
#else
            ulong botId = 1285187180318167081;
#endif

            await RespondAsync("Starte landen von Zitaten, dies kann ein paar Sekunden dauen.");

            int quoteAmount = 0;

            var messagesEnumarator = Context.Channel.GetMessagesAsync(1000).GetAsyncEnumerator();
            
            while (await messagesEnumarator.MoveNextAsync())
            {
                foreach (IMessage message in messagesEnumarator.Current)
                {
                    if (message.Author.Id == botId)
                    {
                        var messageComponents = message.Components;
                        if (messageComponents?.Count == 0)
                            continue;

                        ActionRowComponent? component = messageComponents?.ElementAt(0) as ActionRowComponent;
                        string? customId = component?.Components.ElementAt(0)?.CustomId;
                        string[]? splitCustomId = customId?.Split(':');

                        if (splitCustomId is null || !splitCustomId[0].Equals("addQuoteCounter"))
                            continue;

                        string quoteId = splitCustomId[1];

                        if (string.IsNullOrEmpty(quoteId))
                            continue;

                        string[] lines = message.Content.Split('\n');

                        Quote quote = new Quote();
                        quote.Text = lines[0];
                        quote.Name = lines[1].Substring(1);
                        quote.Count = int.Parse(lines[2].Substring(1, lines.Length - 2));

                        if (!QuoteCounter.TryGetValue(quoteId, out var counter))
                        {
                            QuoteCounter.Add(quoteId, quote);
                            quoteAmount++;
                        }
                    }
                }
            }

            SaveQuotes();
            await ReplyAsync($"Es wurden {quoteAmount} Zitate geladen.");
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

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

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

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

            await originalMsg.UpdateAsync(x =>
            {
                x.Content = CreateQuoteText(quote);
            });

            SaveQuotes();
        }
    }
}
