﻿using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace beta.Models.Server
{
    public static class PlayerInfoExtensions
    {
        public static PlayerInfoMessage Update(this PlayerInfoMessage orig, PlayerInfoMessage newP)
        {
            orig.login = newP.login;
            orig.Avatar = newP.Avatar;

            orig.ratings = newP.ratings;
            orig.Updated = DateTime.Now;
            return orig;
        }
    }

    public class PlayerInfoMessage : ViewModel, IServerMessage, IPlayer
    {
        public ServerCommand Command { get; set; }

        #region Custom properties

        #region Flag
        private ImageSource _Flag;
        public ImageSource Flag => _Flag ??= App.Current.Resources["Flag." + country] as ImageSource;
        #endregion

        #region GameState
        private GameState _GameState;
        public GameState GameState
        {
            get => _GameState;
            set
            {
                if (Set(ref _GameState, value))
                    OnPropertyChanged(nameof(GameStatusImage));
            }
        }
        #endregion

        #region GameStatusImage
        public object GameStatusImage => GameState switch
        {
            GameState.Open => App.Current.Resources["PlayerGameStatus.open"],
            GameState.Playing => App.Current.Resources["PlayerGameStatus.playing"],
            GameState.Host => App.Current.Resources["PlayerGameStatus.host"],
            GameState.Playing5 => App.Current.Resources["PlayerGameStatus.playing5"],
            GameState.None => null,
            _=> null
        };
        #endregion

        #region Game
        private GameInfoMessage _Game;
        public GameInfoMessage Game
        {
            get => _Game;
            set
            {
                if (Set(ref _Game, value))
                {
                    if (value is null)
                    {
                        GameState = GameState.None;
                        return;
                    }
                    if (value.launched_at is not null)
                    {
                        var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(value.launched_at.Value);
                        GameState = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
                        return;
                    }

                    GameState = value.host.Equals(login, StringComparison.OrdinalIgnoreCase) ? GameState.Host : GameState.Open;
                }
            }
        }
        #endregion

        // SOCIAL
        #region Note
        //TODO
        private PlayerNoteVM _Note = new();
        public PlayerNoteVM Note
        {
            get => _Note;
            set => Set(ref _Note, value);
        }
        #endregion

        // CHAT PROPERTIES FOR SORTING
        #region IsChatModerator
        private bool _IsChatModerator;
        public bool IsChatModerator
        {
            get => _IsChatModerator;
            set => Set(ref _IsChatModerator, value);
        }
        #endregion

        // SOCIAL
        // CHAT PROPERTIES FOR SORTING
        #region RelationShip
        private PlayerRelationShip _RelationShip;
        public PlayerRelationShip RelationShip
        {
            get => _RelationShip;
            set => Set(ref _RelationShip, value);
        }
        #endregion

        public string LoginWithClan => clan == null ? login : $"[{clan}] {login}";

        public DateTime? Updated { get; set; }

        #endregion

        public string command { get; set; }

        public int id { get; set; }

        #region login
        private string _login;
        public string login
        {
            get => _login;
            set => Set(ref _login, value);
        } 
        #endregion

        public string country { get; set; }
        public string clan { get; set; }

        #region ratings
        private Dictionary<string, Rating> _ratings;
        public Dictionary<string, Rating> ratings
        {
            get => _ratings;
            set
            {
                if (value != _ratings)
                {
                    if (_ratings is not null)
                        foreach (var item in value)
                        {
                            if (_ratings.TryGetValue(item.Key, out Rating rating))
                            {
                                if (rating is null) continue;
                                rating.RatingDifference[0] += item.Value.rating[0] - rating.rating[0];
                                rating.RatingDifference[1] += item.Value.rating[1] - rating.rating[1];
                                rating.GamesDifference++;
                            }
                            else
                            {
                                _ratings.Add(item.Key, item.Value);

                                _ratings[item.Key].RatingDifference[0] += item.Value.rating[0];
                                _ratings[item.Key].RatingDifference[1] += item.Value.rating[1];
                                _ratings[item.Key].GamesDifference++;
                            };
                        }
                    else _ratings = value;
                    OnPropertyChanged(nameof(ratings));
                }
            }
        }
        #endregion

        // TODO RENAME to PlayerAvatarData?

        #region Avatar
        private PlayerAvatar _Avatar;
        [JsonPropertyName("avatar")]
        public PlayerAvatar Avatar
        {
            get => _Avatar;
            set => Set(ref _Avatar, value);
        } 
        #endregion

        public PlayerInfoMessage[] players { get; set; }
    }
}
