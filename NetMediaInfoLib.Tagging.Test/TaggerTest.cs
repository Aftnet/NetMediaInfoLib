using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NetMediaInfoLib.Sources;
using Xunit;

namespace NetMediaInfoLib.Tagging.Test
{
    public class TaggerTest
    {
        private ITagger Tagger => Tagging.Tagger.Instance;

        public static IEnumerable<object[]> TestFiles()
        {
            yield return new object[] { "sample.m4v" };
            yield return new object[] { "sample.mkv" };
        }

        [Theory]
        [MemberData(nameof(TestFiles))]
        public async Task TaggingMoviesWorks(string fileName)
        {
            var target = GetFile(fileName, $"sample_movie{Path.GetExtension(fileName)}");
            var imageFile = GetFile("Cover.jpg");

            IMovie movie;
            using (var imageStream = await ReadFileAsync(imageFile))
            {
                var movieMock = new Mock<IMovie>();
                movieMock.SetupGet(d => d.Parent).Returns(new TMDBMovies(string.Empty));
                movieMock.SetupGet(d => d.ID).Returns("test_id");
                movieMock.SetupGet(d => d.Title).Returns("Movie Title");
                movieMock.SetupGet(d => d.Description).Returns("Movie Description");
                movieMock.SetupGet(d => d.Genres).Returns(new string[] { "Genre1", "Genre2" });
                movieMock.SetupGet(d => d.ReleaseDate).Returns("2000-02-03");
                movieMock.Setup(d => d.GetImage()).Returns(imageStream);
                movie = movieMock.Object;

                Assert.True(Tagger.Tag(movie, new TagLib.File.LocalFileAbstraction(target.FullName)));
            }

            using (var file = TagLib.File.Create(target.FullName))
            {
                var tag = file.Tag;
                Assert.Equal(movie.Title, tag.Title);
                Assert.Equal(movie.Genres, tag.Genres);
                Assert.Equal(movie.Description, tag.Description);
                Assert.Single(tag.Pictures);
                Assert.True(tag.Pictures[0].Data.Count > 0);
            }

            //await target.DeleteAsync();
        }

        [Theory]
        [MemberData(nameof(TestFiles))]
        public async Task TaggingTVShowsWorks(string fileName)
        {
            var target = GetFile(fileName, $"sample_tv{Path.GetExtension(fileName)}");
            var imageFile = GetFile("Cover.jpg");

            ITVShow show;
            ITVSeason season;
            ITVEpisode episode;
            using (var imageStream = await ReadFileAsync(imageFile))
            {
                var showMock = new Mock<ITVShow>();
                showMock.SetupGet(d => d.Parent).Returns(new TMDBTVShows(string.Empty));
                showMock.SetupGet(d => d.ID).Returns("test_id");
                showMock.SetupGet(d => d.Title).Returns("Show Title");
                showMock.SetupGet(d => d.Description).Returns("Show Description");
                showMock.SetupGet(d => d.Genres).Returns(new string[] { "Genre1", "Genre2" });
                showMock.SetupGet(d => d.ReleaseDate).Returns("2000-01-01");
                showMock.Setup(d => d.GetImage()).Returns(imageStream);
                show = showMock.Object;

                var seasonMock = new Mock<ITVSeason>();
                seasonMock.SetupGet(d => d.Parent).Returns(show);
                seasonMock.SetupGet(d => d.Number).Returns(2);
                seasonMock.SetupGet(d => d.Title).Returns("Season Title");
                seasonMock.SetupGet(d => d.Description).Returns("Season Description");
                seasonMock.SetupGet(d => d.ReleaseDate).Returns("2002-02-02");
                seasonMock.Setup(d => d.GetImage()).Returns(imageStream);
                season = seasonMock.Object;

                var episodeMock = new Mock<ITVEpisode>();
                episodeMock.SetupGet(d => d.Parent).Returns(season);
                episodeMock.SetupGet(d => d.Number).Returns(5);
                episodeMock.SetupGet(d => d.Title).Returns("Episode Title");
                episodeMock.SetupGet(d => d.Description).Returns("Episode Description");
                episodeMock.SetupGet(d => d.ReleaseDate).Returns("2003-03-03");
                episode = episodeMock.Object;

                Assert.True(Tagger.Tag(episode, new TagLib.File.LocalFileAbstraction(target.FullName)));
            }

            using (var file = TagLib.File.Create(target.FullName))
            {
                var tag = file.Tag;
                Assert.Equal(episode.Title, tag.Title);
                Assert.Equal(episode.Parent.Parent.Genres, tag.Genres);
                Assert.Equal(episode.Description, tag.Description);
                Assert.Single(tag.Pictures);
                Assert.True(tag.Pictures[0].Data.Count > 0);
            }

            //await target.DeleteAsync();
        }

        private static FileInfo GetFile(string fileName, string copyName = null)
        {
            var fileInfo = new FileInfo(Path.Join("Samples", fileName));
            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.Exists);
            if (!string.IsNullOrEmpty(copyName))
            {
                fileInfo = fileInfo.CopyTo(copyName, true);
                Assert.NotNull(fileInfo);
            }

            return fileInfo;
        }

        private static async Task<MemoryStream> ReadFileAsync(FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                var output = new MemoryStream((int)stream.Length);
                await stream.CopyToAsync(output);
                output.Position = 0;
                return output;
            }
        }
    }
}
