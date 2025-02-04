﻿using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.API.Client.Models.Base;
using Refit;
using System.ComponentModel;

namespace Ethereal.FAF.API.Client
{
    public class Pagination
    {
        [AliasAs("page[totals]")]
        public bool PageTotals { get; set; } = true;
        [AliasAs("page[size]")]
        public int PageSize { get; set; }
        [AliasAs("page[number]")]
        public int PageNumber { get; set; }
    }
    public class Sorting
    {
        public ListSortDirection SortDirection;
        [AliasAs("sort")]
        public string Sort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Parameter) || Parameter is "None") return null;
                return
                    (SortDirection is ListSortDirection.Descending ?
                    "-" : string.Empty) +
                    Parameter;
            }
        }
        public string Parameter;
    }
    public class Filtration
    {

    }
    /// <summary>
    /// 
    /// </summary>
    public interface IFafUserService
    {
        /// <summary>
        /// Update account password
        /// </summary>
        /// <param name="currentPassword">Current Password</param>
        /// <param name="newPassword">New password</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Post("/users/changePassword")]
        public Task<ApiResponse<object>> ChangePassword(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update account email
        /// </summary>
        /// <param name="newEmail">New Email</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Post("/users/changeEmail")]
        public Task<HttpResponseMessage> ChangeEmail(string newEmail, string currentPassword, CancellationToken cancellationToken = default);
    }
    public interface IFafApiClient
    {
        [Get("/featuredMods/{featuredMod}/files/latest")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetLatestAsync(int featuredMod, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/featuredMods/{featuredMod}/files/{version}")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(int featuredMod, int version, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/data/coturnServer")]
        Task<ApiResponse<ApiUniversalResult<CoturnServer[]>>> GetCoturnServersAsync(CancellationToken cancellationToken = default);

        [Get("/data/map")]
        Task<ApiResponse<ApiMapsResult>> GetMapsAsync(
            [AliasAs("filter")]
            //[Query(Format = ";")]
            string filter = null,
            Sorting? sorting = null,
            Pagination? pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);
        [Get("/data/mod")]
        Task<ApiResponse<ModsResult>> GetModsAsync(string filter = null, Sorting sorting = null, Pagination pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);

        #region ModerationReport
        /// <summary>
        /// Get all reports
        /// </summary>
        /// <returns></returns>
        [Get("/data/moderationReport")]
        Task<ApiResponse<Response<ModerationReport>>> GetModerationReportAsync();
        #endregion



        #region Games
        [Get("/data/game")]
        Task<ApiResponse<Response<Model<Game>[]>>> GetGamesAsync(string filter, Sorting sorting = default, Pagination pagination = default,
            [AliasAs("include")] [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<Response<Model<Game>[]>>> GetPlayerGamesAsync(string login, Sorting sorting = default, Pagination pagination = default,
            string[] include = null, CancellationToken cancellationToken = default)
            => GetGamesAsync($"playerStats.player.login=={login}", sorting, pagination, include, cancellationToken);

        Task<ApiResponse<Response<Model<Game>[]>>> GetPlayerGamesAsync(long id, Sorting sorting = default, Pagination pagination = default,
            string[] include = null, CancellationToken cancellationToken = default)
            => GetGamesAsync($"playerStats.player.id=={id}", sorting, pagination, include, cancellationToken);

        #endregion
    }
    public interface IFafContentClient
    {
        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);

        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <example>https://content.faforever.com/maps/astro_crater_battles_4x4_rich_huge.v0004.zip</example>
        /// <param name="map">Map name</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Get("/maps/{map}.zip")]
        Task<ApiResponse<Stream>> GetMapStreamAsync(string map, CancellationToken cancellationToken = default);
    }
}
