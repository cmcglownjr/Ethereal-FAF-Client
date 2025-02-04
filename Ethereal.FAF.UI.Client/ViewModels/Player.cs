﻿using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class Player : PlayerInfoMessage
    {
        private string _FlagUri;
        public string FlagUri => _FlagUri ??= "/Resources/Images/Flags/" + Country + ".png";

        public string LoginWithClan => $"{(Clan is null ? null : '[' + Clan + "] ")}{Login}";

        [JsonPropertyName("players")]
        public new Player[] Players { get; set; }


        public int Global => Ratings.Global is null ? 0 : Ratings.Global.DisplayedRating;
        public int Ladder1v1 => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.DisplayedRating;
        public int Tmm2v2 => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.DisplayedRating;
        public int Tmm4v4 => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.DisplayedRating;
        public int GlobalGames => Ratings.Global is null ? 0 : Ratings.Global.number_of_games;
        public int Ladder1v1Games => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.number_of_games;
        public int Tmm2v2Games => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.number_of_games;
        public int Tmm4v4Games => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.number_of_games;

        public RatingType DisplayRatingType { get; set; }
        public int UniversalRatingDisplay => DisplayRatingType switch
        {
            RatingType.global => Global,
            RatingType.ladder_1v1 => Ladder1v1,
            RatingType.tmm_4v4_full_share => Tmm4v4,
            RatingType.tmm_4v4_share_until_death => Tmm4v4,
            RatingType.tmm_2v2 => Tmm2v2,
        };

        public int UniversalGameRatingDisplay => Game is null ? UniversalRatingDisplay :
            Game.RatingType switch
            {
                RatingType.global => Global,
                RatingType.ladder_1v1 => Ladder1v1,
                RatingType.tmm_4v4_full_share => Tmm4v4,
                RatingType.tmm_4v4_share_until_death => Tmm4v4,
                RatingType.tmm_2v2 => Tmm2v2,
            };



        // Player statuses

        #region IsLobbyConnected
        private bool _IsLobbyConnected;
        public bool IsLobbyConnected { get => _IsLobbyConnected; set => Set(ref _IsLobbyConnected, value); }
        #endregion

        public bool IsIrcConnected => !string.IsNullOrWhiteSpace(IrcUsername);
        #region IrcUsername
        private string _IrcUsername;
        public string IrcUsername
        {
            get => _IrcUsername;
            set
            {
                if (Set(ref _IrcUsername, value))
                {
                    OnPropertyChanged(nameof(IsIrcConnected));
                }
            }
        }
        #endregion

        #region IsInGame
        public bool IsInGame => Game is not null;
        public bool IsPlaying => Game?.State is GameState.Playing;
        #endregion

        #region Game
        private Game _Game;
        new public Game Game
        {
            get => _Game;
            set
            {
                if (Set(ref _Game, value))
                {
                    OnPropertyChanged(nameof(IsInGame));
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(UniversalGameRatingDisplay));
                }
            }
        }
        #endregion
    }
}
    