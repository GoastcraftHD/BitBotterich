using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using BitBotterich.util;
using static BitBotterich.util.SpotifySearch;

namespace BitBotterich.commands
{
    [Group("spotify", "All the Spotify related commands")]
    public class SpotifyCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;

        public SpotifyCommands(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("search", "Searches for a Spotify artist")]
        public async Task Search(string artistName)
        {
            await DisplaySearch(artistName, 0, true);
        }

        public async Task DisplaySearch(string artistName, int searchPage, bool firstTimeCall)
        {
            int amountOfResultsPerPage = 10;
            ArtistSearchResult? result = await SpotifyUtil.SearchArtist(artistName, amountOfResultsPerPage, amountOfResultsPerPage * searchPage);

            if (result is null)
            {
                await ReplyAsync("Error: Could not retrieve data from Spotify!");
                return;
            }

            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("Results")
                .WithDescription($"Select one of the artists (Displaying {amountOfResultsPerPage * searchPage}-{amountOfResultsPerPage * searchPage + amountOfResultsPerPage}/{result!.artists!.total})");

            SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select an artist")
                .WithCustomId("artistSearch")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (Artist artist in result!.artists!.items!)
            {
                embedBuilder.AddField(artist.name, "Followers: " + artist.followers!.total);
                menuBuilder.AddOption(artist.name, $"{artist.id},{artistName},{searchPage}");
            }

            ButtonBuilder nextButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse(":arrow_right:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplaySearch;{artistName},{searchPage + 1},false");

            ButtonBuilder prevButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse($":arrow_left:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplaySearch;{artistName},{searchPage - 1},false");

            if (searchPage <= 0)
            {
                prevButtonBuilder.IsDisabled = true;
            }

            if (amountOfResultsPerPage * searchPage >= result.artists.total)
            {
                nextButtonBuilder.IsDisabled = true;
            }

            ComponentBuilder componentBuilder = new ComponentBuilder().WithSelectMenu(menuBuilder).WithButton(prevButtonBuilder).WithButton(nextButtonBuilder);

            if (firstTimeCall)
            {
                await RespondAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
            }
            else
            {
                SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;

                if (originalMsg is null)
                {
                    await ReplyAsync("Error: Could not retrieve original Message!");
                    return;
                }

                await originalMsg.UpdateAsync(x =>
                {
                    x.Embed = embedBuilder.Build();
                    x.Components = componentBuilder.Build();
                });
            }
        }

        [ComponentInteraction("artistSearch", true)]
        public async Task SearchMenuHandler(string[] selections)
        {
            string[] selection = selections.First().Split(',');
            string artistId = selection[0];
            string searchArtistName = selection[1];
            int searchPage = int.Parse(selection[2]);

            await DisplayArtist(artistId, searchArtistName, searchPage);
        }

        public async Task DisplayArtist(string artistId, string searchArtistName, int searchPage)
        {
            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;
            Artist? artist = SpotifyUtil.GetArtist(artistId);

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

            if (artist is null)
            {
                await ReplyAsync("Error: Could not retrieve Artist from Spotify!");
                return;
            }

            string genreList = "";

            foreach (string genre in artist.genres!)
            {
                genreList += genre + ", ";
            }

            if (!String.IsNullOrEmpty(genreList))
                genreList = genreList.Substring(0, genreList.Length - 2);
            else
                genreList = "None";

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(artist.name)
            .WithThumbnailUrl(artist.images!.First().url)
            .WithUrl(artist.external_urls!.spotify)
            .AddField("Followers", artist.followers!.total, true)
            .AddField("Popularity", artist.popularity, true)
            .AddField("Genres", genreList);

            ButtonBuilder backButtonBuilder = new ButtonBuilder()
            .WithLabel("Back")
            .WithStyle(ButtonStyle.Secondary)
            .WithCustomId($"CallMethodBtn:DisplaySearch;{searchArtistName},{searchPage},false");

            ButtonBuilder showSongsButtonBuilder = new ButtonBuilder()
            .WithLabel("Show Songs")
            .WithStyle(ButtonStyle.Secondary)
            .WithCustomId($"showSongsBtn:{artistId},{artist.name},all");

            ComponentBuilder componentBuilder = new ComponentBuilder().WithButton(backButtonBuilder).WithButton(showSongsButtonBuilder);

            await originalMsg.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = componentBuilder.Build();
            });
        }

        [ComponentInteraction("showSongsBtn:*", true)]
        public async Task ShowArtistsAlbums(string artistInfo)
        {
            object[] args = ConvertArray(artistInfo.Split(','));
            string artistId = (string)args[0];
            string artistName = (string)args[1];
            string filter = (string)args[2];

            await DisplayArtistsAlbums(artistId, artistName, filter, 0);
        }
        
        public async Task DisplayArtistsAlbums(string artistId, string artistName, string filter, int albumPage)
        {
            int amountOfAlbumsPerPage = 10;
            AlbumCollection? albumCollection = SpotifyUtil.GetArtistsAlbums(artistId, filter == "all" ? null : filter, amountOfAlbumsPerPage, amountOfAlbumsPerPage * albumPage);

            if (albumCollection is null)
            {
                await ReplyAsync("Error: Could not retrieve data from Spotify!");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"{artistName}'s Songs")
                .WithDescription($"Select one of the songs (Displaying {amountOfAlbumsPerPage * albumPage}-{amountOfAlbumsPerPage * albumPage + amountOfAlbumsPerPage}/{albumCollection?.total})");

            SelectMenuBuilder menu = new SelectMenuBuilder()
                .WithMinValues(1)
                .WithMaxValues(1)
                .WithPlaceholder("Select a song")
                .WithCustomId("displaySong");

            foreach (Album album in albumCollection!.items!)
            {
                embed.AddField(album.name, album.release_date);
                menu.AddOption(album.name, $"{album.id},{album.album_group},{artistId},{artistName},{filter},{albumPage}");
            }

            ButtonBuilder backBtn = new ButtonBuilder()
                .WithLabel("Back")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplayArtist;{artistId},{artistName},0");

            ButtonBuilder allFilterBtn = new ButtonBuilder()
                .WithLabel("All")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"setFilter:{artistId},{artistName},all");

            ButtonBuilder singleFilterBtn = new ButtonBuilder()
                .WithLabel("Single")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"setFilter:{artistId},{artistName},single");

            ButtonBuilder albumFilterBtn = new ButtonBuilder()
                .WithLabel("Album")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"setFilter:{artistId},{artistName},album");

            ButtonBuilder appearsOnFilterBtn = new ButtonBuilder()
                .WithLabel("Appears On")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"setFilter:{artistId},{artistName},appears_on");

            ButtonBuilder compilationFilterBtn = new ButtonBuilder()
                .WithLabel("Compilation")
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"setFilter:{artistId},{artistName},compilation");

            switch (filter)
            {
                case "all":
                    allFilterBtn.IsDisabled = true;
                    break;
                case "single":
                    singleFilterBtn.IsDisabled = true;
                    break;
                case "album":
                    albumFilterBtn.IsDisabled = true;
                    break;
                case "appears_on":
                    appearsOnFilterBtn.IsDisabled = true;
                    break;
                case "compilation":
                    compilationFilterBtn.IsDisabled = true;
                    break;
            }

            ButtonBuilder nextButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse(":arrow_right:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplayArtistsAlbums;{artistId},{artistName},{filter},{albumPage + 1}");

            ButtonBuilder prevButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse($":arrow_left:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplayArtistsAlbums;{artistId},{artistName},{filter},{albumPage - 1}");


            ComponentBuilder componentBuilder = new ComponentBuilder();

            if (albumCollection.items.Count != 0)
            {
                componentBuilder.WithSelectMenu(menu);
            }

            if (albumPage <= 0)
            {
                prevButtonBuilder.IsDisabled = true;
            }

            if (amountOfAlbumsPerPage * albumPage + amountOfAlbumsPerPage >= albumCollection.total)
            {
                nextButtonBuilder.IsDisabled = true;
            }

            ActionRowBuilder row = new ActionRowBuilder().WithButton(allFilterBtn).WithButton(singleFilterBtn).WithButton(albumFilterBtn).WithButton(appearsOnFilterBtn).WithButton(compilationFilterBtn);
            componentBuilder.WithButton(backBtn).WithButton(prevButtonBuilder).WithButton(nextButtonBuilder).AddRow(row);

            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

            await originalMsg.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = componentBuilder.Build();
            });
        }

        [ComponentInteraction("setFilter:*", true)]
        private async Task SetAlbumFilter(string filterInfo)
        {
            object[] args = ConvertArray(filterInfo.Split(','));
            string artistId = (string)args[0];
            string artistName = (string)args[1];
            string filter = (string)args[2];

            await DisplayArtistsAlbums(artistId, artistName, filter, 0);
        }

        [ComponentInteraction("displaySong", true)]
        private async Task AlbumMenuHandler(string[] selections)
        {
            object[] selection = ConvertArray(selections.First().Split(','));
            string albumId = (string)selection[0];
            string albumGroup = (string)selection[1];
            string artistId = (string)selection[2];
            string artistName = (string)selection[3];
            string filter = (string)selection[4];
            int albumPage = (int)selection[5];

            if (albumGroup == "single")
            {
                AlbumFull? album = SpotifyUtil.GetAlbum(albumId);

                if (album is null)
                {
                    await ReplyAsync("Error: Could not retrieve album from Spotify!");
                    return;
                }

                await DisplayTrack(album.tracks!.items!.First().id!, "DisplayArtistsAlbums", new object[]{artistId, artistName, filter, albumPage});
            }
            else if (albumGroup == "track")
            {
                await DisplayTrack(albumId, "DisplayArtistsAlbums", new object[] { artistId, artistName, filter, albumPage });
            }
            else
            {
                await DisplayAlbumContent(albumId, artistId, artistName, filter, albumPage, 0);
            }
        }

        public async Task DisplayAlbumContent(string albumId, string artistId, string artistName, string filter, int albumPage, int songPage)
        {
            AlbumFull? album = SpotifyUtil.GetAlbum(albumId);

            if (album is null)
            {
                await ReplyAsync("Error: Could not retrieve album from Spotify!");
                return;
            }

            int numOfSongsPerPage = 10;

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"Songs in {album.name}")
                .WithDescription($"Select one of the songs (Displaying {numOfSongsPerPage * songPage}-{numOfSongsPerPage * songPage + numOfSongsPerPage}/{album?.total_tracks})")
                .WithUrl(album!.external_urls!.spotify);

            SelectMenuBuilder menu = new SelectMenuBuilder()
                .WithMaxValues(1)
                .WithMinValues(1)
                .WithPlaceholder("Select a song")
                .WithCustomId("displaySong");

            for (int i = numOfSongsPerPage * songPage; i < album.total_tracks; i++)
            {
                if (i == numOfSongsPerPage * songPage + numOfSongsPerPage)
                    break;

                SimpleTrack track = album.tracks!.items![i];

                string artists = "";
                foreach (ArtistShort artist in track.artists!)
                {
                    artists += $"{artist.name},";
                }

                artists = artists.Substring(0, artists.Length - 1);

                embed.AddField(track.name, artists);
                menu.AddOption(track.name, $"{track.id},track,{artistId},{artistName},{filter},{albumPage}");
            }

            ButtonBuilder backBtn = new ButtonBuilder()
                .WithLabel("Back")
                .WithCustomId($"CallMethodBtn:DisplayArtistsAlbums;{artistId},{artistName},{filter},{albumPage}")
                .WithStyle(ButtonStyle.Secondary);

            ButtonBuilder nextButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse(":arrow_right:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplayAlbumContent;{albumId},{artistId},{artistName},{filter},{albumPage},{songPage + 1}");

            ButtonBuilder prevButtonBuilder = new ButtonBuilder()
                .WithEmote(Emoji.Parse($":arrow_left:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId($"CallMethodBtn:DisplayAlbumContent;{albumId},{artistId},{artistName},{filter},{albumPage},{songPage - 1}");

            if (songPage <= 0)
            {
                prevButtonBuilder.IsDisabled = true;
            }

            if (numOfSongsPerPage * songPage + numOfSongsPerPage >= album.total_tracks)
            {
                nextButtonBuilder.IsDisabled = true;
            }

            ComponentBuilder componentBuilder = new ComponentBuilder().WithSelectMenu(menu).WithButton(backBtn).WithButton(prevButtonBuilder).WithButton(nextButtonBuilder);

            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

            await originalMsg.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = componentBuilder.Build();
            });
        }

        public async Task DisplayTrack(string trackId, string backMethod, object[] args)
        {
            TrackFull? track = SpotifyUtil.GetTrack(trackId);

            if (track is null)
            {
                await ReplyAsync("Error: Could not retrieve track from Spotify!");
                return;
            }

            string artists = "";
            foreach (ArtistShort artist in track.artists!)
            {
                artists += $"{artist.name},";
            }

            artists = artists.Substring(0, artists.Length - 1);

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(track.name)
                .WithUrl(track.external_urls!.spotify)
                .WithImageUrl(track.album!.images!.First().url)
                .AddField("Popularity", track.popularity, true)
                .AddField("Length", TimeSpan.FromMilliseconds(track.duration_ms).ToString(@"mm\:ss"), true)
                .AddField("Released", track.album.release_date, true)
                .AddField("Artists", artists, true);
            
            string customId = $"CallMethodBtn:{backMethod};";

            foreach (object arg in args)
            {
                customId += $"{arg},";
            }

            customId = customId.Substring(0, customId.Length - 1);

            ButtonBuilder backBtn = new ButtonBuilder()
            .WithLabel("Back")
                .WithCustomId(customId)
                .WithStyle(ButtonStyle.Secondary);

            ComponentBuilder componentBuilder = new ComponentBuilder().WithButton(backBtn);

            SocketMessageComponent? originalMsg = Context.Interaction as SocketMessageComponent;

            if (originalMsg is null)
            {
                await ReplyAsync("Error: Could not retrieve original Message!");
                return;
            }

            await originalMsg.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = componentBuilder.Build();
            });
        }

        [ComponentInteraction("CallMethodBtn:*;*", true)]
        private async Task CallMethodButton(string methodName, string combinedArgs)
        {
            object[] args = ConvertArray(combinedArgs.Split(','));

            typeof(SpotifyCommands).GetMethod(methodName)?.Invoke(this, args);

            await Task.CompletedTask;
        }

        private object[] ConvertArray(string[] args)
        {
            object[] convertedArray = new object[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                if (int.TryParse(args[i], out int intResult))
                {
                    convertedArray[i] = intResult;
                }
                else if (float.TryParse(args[i], out float floatResult))
                {
                    convertedArray[i] = floatResult;
                }
                else if (bool.TryParse(args[i], out bool boolResult))
                {
                    convertedArray[i] = boolResult;
                }
                else
                {
                    convertedArray[i] = args[i];
                }
            }

            return convertedArray;
        }
    }
}
