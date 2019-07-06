using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetMediaInfoLib.Components;
using Newtonsoft.Json;

namespace NetMediaInfoLib.Sources
{
    public class TMDBTVShows : TMDBBase<ITVShow>
    {
        private class TVShowInfo : ITVShow
        {
            [JsonIgnore]
            public ISource<ITVShow> Parent => ParentConcrete;
            [JsonIgnore]
            public TMDBTVShows ParentConcrete { get; set; }

            [JsonIgnore]
            public bool Populated { get; set; } = false;

            public string ID { get; set; }
            [JsonProperty("name")]
            public string Title { get; set; }
            [JsonProperty("overview")]
            public string Description { get; set; }
            [JsonIgnore]
            public IReadOnlyList<string> Genres { get; set; }
            [JsonProperty("first_air_date")]
            public string ReleaseDate { get; set; }
            [JsonIgnore]
            public IReadOnlyList<ITVSeason> Children => Seasons;


            [JsonProperty("genres")]
            public IList<GenreInfo> GenreInfos { get; set; }
            [JsonProperty("poster_path")]
            public string CoverArtUri { get; set; }

            public List<TVSeasonInfo> Seasons { get; set; }

            private MemoryStreamProvider Image { get; set; }
            public bool HasImage => Image != null;
            public MemoryStream GetImage() => Image?.GetStream();

            public async Task<bool> GetDetailsAsync(CancellationToken ct)
            {
                if (Populated)
                {
                    return true;
                }

                var config = await ParentConcrete.GetConfigurationAsync(ct);
                if (config == null)
                {
                    return false;
                }

                var response = await ParentConcrete.GetResponseJSONObjectAsync<TVShowInfo>($"tv/{ID}", string.Empty, ct);
                if (response == null)
                {
                    return false;
                }

                Description = response.Description;
                GenreInfos = response.GenreInfos;
                Genres = ParentConcrete.GetGenreNames(GenreInfos);
                Seasons = response.Seasons;
                foreach(var i in Seasons)
                {
                    i.ParentConcrete = this;
                    i.CoverArtUri = ParentConcrete.GetAbsoluteCoverImageUri(i.CoverArtUri, config);
                }

                var imgResponse = await ParentConcrete.Client.GetAsync(CoverArtUri, ct);
                if (ct.IsCancellationRequested || !imgResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var imageBytes = await imgResponse.Content.ReadAsByteArrayAsync();
                Image = new MemoryStreamProvider(imageBytes);

                Populated = true;
                return true;
            }
        }

        private class TVSeasonInfo : ITVSeason
        {
            [JsonIgnore]
            public ITVShow Parent => ParentConcrete;
            [JsonIgnore]
            public TVShowInfo ParentConcrete { get; set; }

            public bool Populated { get; set; } = false;

            public string ID { get; set; }
            [JsonProperty("season_number")]
            public int Number { get; set; }
            [JsonProperty("name")]
            public string Title { get; set; }
            [JsonProperty("overview")]
            public string Description { get; set; }
            [JsonProperty("air_date")]
            public string ReleaseDate { get; set; }

            [JsonProperty("poster_path")]
            public string CoverArtUri { get; set; }

            [JsonProperty("number_of_episodes")]
            public int NumEpisodes { get; set; }

            [JsonProperty("episodes")]
            public List<TVEpisodeInfo> Episodes { get; set; }
            public IReadOnlyList<ITVEpisode> Children => Episodes;

            private MemoryStreamProvider Image { get; set; }
            public bool HasImage => Image != null;
            public MemoryStream GetImage() => Image?.GetStream();

            public async Task<bool> GetDetailsAsync(CancellationToken ct)
            {
                if (Populated)
                {
                    return true;
                }

                var response = await ParentConcrete.ParentConcrete.GetResponseJSONObjectAsync<TVSeasonInfo>($"tv/{Parent.ID}/season/{Number}", string.Empty, ct);
                if (response == null)
                {
                    return false;
                }

                Description = response.Description;
                Episodes = response.Episodes;
                foreach (var i in Episodes)
                {
                    i.Parent = this;
                }

                var imgResponse = await ParentConcrete.ParentConcrete.Client.GetAsync(CoverArtUri, ct);
                if (ct.IsCancellationRequested || !imgResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var imageBytes = await imgResponse.Content.ReadAsByteArrayAsync();
                Image = new MemoryStreamProvider(imageBytes);

                Populated = true;
                return true;
            }
        }

        private class TVEpisodeInfo : ITVEpisode
        {
            [JsonIgnore]
            public ITVSeason Parent { get; set; }

            public string ID { get; set; }
            [JsonProperty("episode_number")]
            public int Number { get; set; }
            [JsonProperty("name")]
            public string Title { get; set; }
            [JsonProperty("overview")]
            public string Description { get; set; }
            [JsonProperty("air_date")]
            public string ReleaseDate { get; set; }
        }

        public override string Name => "TMDB TV Shows";
        public override string IDTagLabel { get; } = "TMDB_TVSHOWID";

        public override async Task<ITVShow> GetAsync(string id, CancellationToken ct)
        {
            var config = await GetConfigurationAsync(ct);
            if (config == null)
            {
                return null;
            }

            var output = await GetResponseJSONObjectAsync<TVShowInfo>($"tv/{id}", string.Empty, ct);
            output.ParentConcrete = this;
            output.CoverArtUri = GetAbsoluteCoverImageUri(output.CoverArtUri, config);
            return output;
        }

        public override async Task<IReadOnlyList<ITVShow>> SearchAsync(string query, CancellationToken ct)
        {
            if (query.Length < MinQueryLength)
            {
                return new ITVShow[0];
            }

            var config = await GetConfigurationAsync(ct);
            if (config == null)
            {
                return null;
            }

            var response = await GetResponseJSONObjectAsync<SearchResponse<TVShowInfo>>("search/tv", $"query={query}&include_adult=true", ct);
            if (response == null)
            {
                return null;
            }

            foreach (var i in response.Results)
            {
                i.ParentConcrete = this;
                i.CoverArtUri = GetAbsoluteCoverImageUri(i.CoverArtUri, config);
            }

            return response.Results;
        }

        public TMDBTVShows(string apiKey) : base(apiKey)
        {
        }
    }
}
