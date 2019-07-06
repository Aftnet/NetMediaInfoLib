using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetMediaInfoLib.Sources
{
    public abstract class TMDBBase<T> : ISource<T>
    {
        protected class TMDBDateConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
        {
            public TMDBDateConverter()
            {
                DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal;
            }
        }

        protected class Configuration
        {
            public class ImagesConfig
            {
                [JsonProperty("base_url")]
                public string BaseUrl { get; set; }
                [JsonProperty("secure_base_url")]
                public string SecureBaseUrl { get; set; }
                [JsonProperty("backdrop_sizes")]
                public IList<string> BackdropSizes { get; set; }
                [JsonProperty("logo_sizes")]
                public IList<string> LogoSizes { get; set; }
                [JsonProperty("poster_sizes")]
                public IList<string> PosterSizes { get; set; }
                [JsonProperty("profile_sizes")]
                public IList<string> ProfileSizes { get; set; }
                [JsonProperty("still_sizes")]
                public IList<string> StillSizes { get; set; }

                public string SelectedPosterSize { get; set; }
            }

            public ImagesConfig Images { get; set; }
        }

        protected class GenreInfo
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        protected class SearchResponse<U> where U : class
        {
            public int Page { get; set; }
            public IReadOnlyList<U> Results { get; set; }
        }

        private static Uri APIUri { get; } = new Uri("https://api.themoviedb.org/3/");

        private string APIKey { get; }

        protected const int MinQueryLength = 3;

        private readonly Lazy<HttpClient> client = new Lazy<HttpClient>(() => new HttpClient(), LazyThreadSafetyMode.ExecutionAndPublication);
        protected HttpClient Client => client.Value;
        private static Configuration CurrentConfig { get; set; }

        public abstract string Name { get; }
        public abstract string IDTagLabel { get; }

        public abstract Task<T> GetAsync(string id, CancellationToken ct);
        public abstract Task<IReadOnlyList<T>> SearchAsync(string query, CancellationToken ct);

        public TMDBBase(string apiKey)
        {
            APIKey = apiKey;
        }

        protected async Task<Configuration> GetConfigurationAsync(CancellationToken ct)
        {
            if (CurrentConfig != null)
            {
                return CurrentConfig;
            }

            var response = await GetResponseJSONObjectAsync<Configuration>("configuration", string.Empty, ct);
            CurrentConfig = response;
            CurrentConfig.Images.SelectedPosterSize = CurrentConfig.Images.PosterSizes[CurrentConfig.Images.PosterSizes.Count() - 2];

            return CurrentConfig;
        }

        protected async Task<U> GetResponseJSONObjectAsync<U>(string relativePath, string query, CancellationToken ct)
        {
            var json = await GetResponseStringAsync(relativePath, query, ct);
            if (json == null)
            {
                return default(U);
            }

            var output = JsonConvert.DeserializeObject<U>(json);
            return output;
        }

        private async Task<string> GetResponseStringAsync(string relativePath, string query, CancellationToken ct)
        {
            var builder = new UriBuilder(APIUri);
            builder.Path += relativePath;
            builder.Query = query + $"&api_key={APIKey}";

            var response = await Client.GetAsync(builder.Uri, ct);
            if (ct.IsCancellationRequested || !response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        protected IReadOnlyList<string> GetGenreNames(IEnumerable<GenreInfo> genreInfos)
        {
            return genreInfos.Where(d => !string.IsNullOrEmpty(d.Name)).Select(d => d.Name).OrderBy(d => d).ToArray();
        }

        protected string GetAbsoluteCoverImageUri(string relativeUri, Configuration config)
        {
            return $"{config.Images.SecureBaseUrl}{config.Images.SelectedPosterSize}{relativeUri}";
        }
    }
}
