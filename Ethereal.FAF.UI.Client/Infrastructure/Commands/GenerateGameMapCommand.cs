﻿using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class GenerateGameMapCommand : Base.Command
    {
        private NotificationService Notification => App.Hosting.Services.GetService<NotificationService>();
        private IConfiguration Configuration => App.Hosting.Services.GetService<IConfiguration>();
        private MapsService MapsService;
        public override bool CanExecute(object parameter) => 
            parameter is Game game && 
            game.IsMapgen &&
            game.MapGeneratorState is MapGeneratorState.NotGenerated;
        public override void Execute(object parameter) => 
            Task.Run(async () =>
            {
                var game = (Game)parameter;
                game.MapGeneratorState = MapGeneratorState.Generating;
                MapsService ??= App.Hosting.Services.GetService<MapsService>();

                await MapsService.EnsureMapExistAsync(game.Mapname);

                var preview = Path.Combine(Configuration.GetMapsLocation(), game.Mapname, game.Mapname + "_preview.png");
                if (!File.Exists(preview))
                {
                    await Exception(game, $"Generated map preview not exists. Path: {preview}");
                    return;
                }
                game.SmallMapPreview = preview;
                game.MapGeneratorState = MapGeneratorState.Generated;
                var scenario = Configuration.GetMapFile(game.Mapname, "_scenario.lua");
                if (File.Exists(scenario))
                {
                    game.MapScenario = FA.Vault.MapScenario.FromFile(scenario);
                }
                Notification.Notify("Map generator", $"Map {game.Mapname} is generated", new System.Uri(preview));
            })
            .ContinueWith(async t =>
            {
                var game = (Game)parameter;
                if (t.IsFaulted)
                {
                    await Exception(game, t.Exception.ToString());
                }
            });
        private async Task Exception(Game game, string exception)
        {
            game.MapGeneratorException = exception;
            game.MapGeneratorState = MapGeneratorState.Faulted;
            Notification.Notify("Map generator", exception);
            await Task.Delay(System.TimeSpan.FromSeconds(5));
            game.MapGeneratorState = MapGeneratorState.NotGenerated;
            game.MapGeneratorException = null;
        }
    }
}
