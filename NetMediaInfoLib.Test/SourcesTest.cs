using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMediaInfoLib.Sources;
using Xunit;

namespace NetMediaInfoLib.Test
{
    public class SourcesTest
    {
        private const string TMDBAPIKey = "363d023d3312c5e0b054f5d7f271bb6a";

        public static IEnumerable<object[]> SearchMoviesParams
        {
            get
            {
                yield return new object[] { new TMDBMovies(TMDBAPIKey) };
            }
        }

        public static IEnumerable<object[]> SearchTVShowsParams
        {
            get
            {
                yield return new object[] { new TMDBTVShows(TMDBAPIKey) };
            }
        }

        [Theory]
        [MemberData(nameof(SearchMoviesParams))]
        public async Task TestMovieSource(ISource<IMovie> source)
        {
            var query = "Spider Man";

            var movies = await source.SearchAsync(query, CancellationToken.None);
            Assert.NotEmpty(movies);
            Assert.Empty(movies.Where(d => string.IsNullOrEmpty(d.ID)));
            Assert.Empty(movies.Where(d => string.IsNullOrEmpty(d.Title)));
            Assert.NotEmpty(movies.Where(d => !string.IsNullOrEmpty(d.Description)));
            Assert.NotEmpty(movies.Where(d => !string.IsNullOrEmpty(d.ReleaseDate)));
            
            var movie = movies.First();
            var success = await movie.GetDetailsAsync(CancellationToken.None);
            Assert.True(success);

            Assert.NotEmpty(movie.ID);
            Assert.NotEmpty(movie.Title);
            Assert.NotEmpty(movie.Description);
            Assert.NotEmpty(movie.Genres);
            Assert.Empty(movie.Genres.Where(d => string.IsNullOrEmpty(d)));
            using (var stream = movie.GetImage())
            {
                Assert.NotNull(stream);
                Assert.False(stream.CanWrite);
                Assert.True(stream.Length > 0);
            }

            var getMovie = await source.GetAsync(movie.ID, CancellationToken.None);
            Assert.NotNull(getMovie);
            success = await getMovie.GetDetailsAsync(CancellationToken.None);
            Assert.True(success);

            Assert.Equal(movie.ID, getMovie.ID);
            Assert.Equal(movie.Title, getMovie.Title);
            Assert.Equal(movie.Description, getMovie.Description);
            Assert.Equal(movie.Genres, getMovie.Genres);
            Assert.Equal(movie.ReleaseDate, getMovie.ReleaseDate);
            using (var stream = getMovie.GetImage())
            {
                Assert.NotNull(stream);
                Assert.False(stream.CanWrite);
                Assert.True(stream.Length > 0);
            }
        }

        [Theory]
        [MemberData(nameof(SearchTVShowsParams))]
        public async Task SearchTVShowsWorks(ISource<ITVShow> source)
        {
            var query = "Game of Thrones";

            var shows = await source.SearchAsync(query, CancellationToken.None);
            Assert.NotEmpty(shows);
            Assert.Empty(shows.Where(d => string.IsNullOrEmpty(d.ID)));
            Assert.Empty(shows.Where(d => string.IsNullOrEmpty(d.Title)));
            Assert.NotEmpty(shows.Where(d => !string.IsNullOrEmpty(d.Description)));
            Assert.NotEmpty(shows.Where(d => !string.IsNullOrEmpty(d.ReleaseDate)));

            var show = shows.First();
            var success = await show.GetDetailsAsync(CancellationToken.None);
            Assert.True(success);

            Assert.NotEmpty(show.ID);
            Assert.NotEmpty(show.Title);
            Assert.NotEmpty(show.Description);
            Assert.NotEmpty(show.Genres);
            Assert.Empty(show.Genres.Where(d => string.IsNullOrEmpty(d)));
            using (var stream = show.GetImage())
            {
                Assert.NotNull(stream);
                Assert.False(stream.CanWrite);
                Assert.True(stream.Length > 0);
            }

            Assert.NotEmpty(show.Children);
            Assert.Empty(show.Children.Where(d => d.Parent != show));
            Assert.Empty(show.Children.Where(d => d.Number < 0));
            Assert.Empty(show.Children.Where(d => string.IsNullOrEmpty(d.Title)));
            Assert.NotEmpty(show.Children.Where(d => !string.IsNullOrEmpty(d.Description)));
            Assert.NotEmpty(show.Children.Where(d => !string.IsNullOrEmpty(d.ReleaseDate)));

            var season = show.Children[0];
            success = await season.GetDetailsAsync(CancellationToken.None);
            Assert.True(success);

            Assert.True(season.Number >= 0);
            Assert.NotEmpty(season.Title);
            Assert.NotNull(season.Description);
            Assert.NotNull(season.ReleaseDate);
            using (var stream = season.GetImage())
            {
                Assert.NotNull(stream);
                Assert.False(stream.CanWrite);
                Assert.True(stream.Length > 0);
            }

            Assert.NotEmpty(season.Children);
            Assert.Empty(season.Children.Where(d => d.Parent != season));
            Assert.Empty(season.Children.Where(d => d.Number < 0));
            Assert.Empty(season.Children.Where(d => string.IsNullOrEmpty(d.Title)));
            Assert.NotEmpty(season.Children.Where(d => !string.IsNullOrEmpty(d.Description)));
            Assert.NotEmpty(season.Children.Where(d => !string.IsNullOrEmpty(d.ReleaseDate)));

            var getSeries = await source.GetAsync(show.ID, CancellationToken.None);
            Assert.NotNull(getSeries);
            success = await getSeries.GetDetailsAsync(CancellationToken.None);
            Assert.True(success);

            Assert.Equal(show.ID, getSeries.ID);
            Assert.Equal(show.Title, getSeries.Title);
            Assert.Equal(show.Description, getSeries.Description);
            Assert.Equal(show.Genres, getSeries.Genres);
            Assert.Equal(show.ReleaseDate, getSeries.ReleaseDate);
            using (var stream = getSeries.GetImage())
            {
                Assert.NotNull(stream);
                Assert.False(stream.CanWrite);
                Assert.True(stream.Length > 0);
            }
        }
    }
}
