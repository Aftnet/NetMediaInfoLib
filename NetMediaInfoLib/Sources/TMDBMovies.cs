using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetMediaInfoLib.Components;
using Newtonsoft.Json;

namespace NetMediaInfoLib.Sources
{
    public class TMDBMovies : TMDBBase<IMovie>
    {
        private class MovieInfo : IMovie
        {
            [JsonIgnore]
            public ISource<IMovie> Parent => ParentConcrete;
            [JsonIgnore]
            public TMDBMovies ParentConcrete { get; set; }

            [JsonIgnore]
            public bool Populated { get; set; } = false;

            public string ID { get; set; }
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("overview")]
            public string Description { get; set; }
            [JsonIgnore]
            public IReadOnlyList<string> Genres { get; set; }
            [JsonProperty("release_date")]
            public string ReleaseDate { get; set; }
            
            [JsonProperty("genres")]
            public IList<GenreInfo> GenreInfos { get; set; }
            [JsonProperty("poster_path")]
            public string CoverArtUri { get; set; }

            private MemoryStreamProvider Image { get; set; }
            public bool HasImage => Image != null;
            public MemoryStream GetImage() => Image?.GetStream();

            public async Task<bool> GetDetailsAsync(CancellationToken ct)
            {
                if (Populated)
                {
                    return true;
                }

                var response = await ParentConcrete.GetResponseJSONObjectAsync<MovieInfo>($"movie/{ID}", string.Empty, ct);
                if (response == null)
                {
                    return false;
                }

                var imgResponse = await ParentConcrete.Client.GetAsync(CoverArtUri, ct);
                if (ct.IsCancellationRequested || !imgResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                Description = response.Description;
                GenreInfos = response.GenreInfos;
                Genres = ParentConcrete.GetGenreNames(GenreInfos);
                var imageBytes = await imgResponse.Content.ReadAsByteArrayAsync();
                Image = new MemoryStreamProvider(imageBytes);
                Populated = true;
                return true;
            }
        }

        public override string Name { get; } = "TMDB Movies";
        public override string IDTagLabel { get; } = "TMDB_MOVIEID";

        public override async Task<IMovie> GetAsync(string id, CancellationToken ct)
        {
            var config = await GetConfigurationAsync(ct);
            if (config == null)
            {
                return null;
            }

            var output = await GetResponseJSONObjectAsync<MovieInfo>($"movie/{id}", string.Empty, ct);
            output.ParentConcrete = this;
            output.CoverArtUri = GetAbsoluteCoverImageUri(output.CoverArtUri, config);
            return output;
        }

        public override async Task<IReadOnlyList<IMovie>> SearchAsync(string query, CancellationToken ct)
        {
            if (query.Length < MinQueryLength)
            {
                return new IMovie[0];
            }

            var config = await GetConfigurationAsync(ct);
            if (config == null)
            {
                return null;
            }

            var response = await GetResponseJSONObjectAsync<SearchResponse<MovieInfo>>("search/movie", $"query={query}&include_adult=true", ct);
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

        public TMDBMovies(string apiKey) : base(apiKey)
        {
        }
    }
}
