using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitBotterich.commands
{
    [Discord.Interactions.Group("zitat", "Alle Commands für zitate.")]
    public class QuoteCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private InteractionHandler _handler;

        public QuoteCommand(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("create", "Erstellt einen neuen Zitat Zähler.")]
        public async Task Say(string zitat, string name)
            => await RespondAsync(zitat + " : " + name);
    }
}
