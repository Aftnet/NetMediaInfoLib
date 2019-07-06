using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NetMediaInfoLib.Tagging.Models;
using TagLib;
using TagLib.Mpeg4;
using static TagLib.File;

namespace NetMediaInfoLib.Tagging
{
    public class Tagger : ITagger
    {
        private const string MkvMediaTypeLabel = "MEDIA_TYPE";
        private const int HDVideoMinWidth = 1280;
        private const int HDVideoMinHeight = 720;

        private ISet<string> MkvExtensions { get; } = new HashSet<string> { ".mkv" };
        private ISet<string> Mp4Extensions { get; } = new HashSet<string> { ".mp4", ".m4v" };

        public IReadOnlyCollection<string> SupportedVideoExtensions { get; }
        public IReadOnlyCollection<string> SupportedAudioExtensions { get; } = new string[] { };

        private static readonly Lazy<Tagger> instance = new Lazy<Tagger>(() => new Tagger(), LazyThreadSafetyMode.PublicationOnly);
        public static ITagger Instance => instance.Value;

        private Tagger()
        {
            SupportedVideoExtensions = MkvExtensions.Concat(Mp4Extensions).ToArray();
        }

        public bool Tag(IMovie payload, IFileAbstraction target)
        {
            try
            {
                if (MkvExtensions.Contains(Path.GetExtension(target.Name)))
                {
                    return TagMkvMovie(payload, target);
                }
                else if (Mp4Extensions.Contains(Path.GetExtension(target.Name)))
                {
                    return TagMp4Movie(payload, target);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Data);
                return false;
            }

            return false;
        }

        public bool Tag(ITVEpisode payload, IFileAbstraction target)
        {
            try
            {
                if (MkvExtensions.Contains(Path.GetExtension(target.Name)))
                {
                    return TagMkvTVEpisode(payload, target);
                }
                else if (Mp4Extensions.Contains(Path.GetExtension(target.Name)))
                {
                    return TagMp4TVEpisode(payload, target);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Data);
                return false;
            }

            return false;
        }

        private bool TagMkvMovie(IMovie payload, IFileAbstraction target)
        {
            using (var file = Create(target))
            {
                file.RemoveTags(TagTypes.AllTags);
                var tag = (TagLib.Matroska.Tag)file.GetTag(TagTypes.Matroska, true);
                SetBasicProperties(tag, payload.Title, payload.Genres, payload.Description, payload.GetImage());

                SetPropertyIfValid(tag, "MOVIE", MkvMediaTypeLabel);
                SetPropertyIfValid(tag, payload.ID, payload.Parent.IDTagLabel);
                file.Save();
            }

            return true;
        }

        private bool TagMp4Movie(IMovie payload, IFileAbstraction target)
        {
            using (var file = Create(target))
            {
                file.RemoveTags(TagTypes.AllTags);
                var tag = (TagLib.Mpeg4.AppleTag)file.GetTag(TagTypes.Apple, true);
                SetBasicProperties(tag, payload.Title, payload.Genres, payload.Description, payload.GetImage());

                SetPropertyIfValid(tag, $"https://www.themoviedb.org/movie/{payload.ID}", AppleDefaultBoxes.Url);
                SetItunesMediaProperties(tag, file.Properties, AppleDefaultBoxes.ITunesMediaTypes.Movie);
                file.Save();
            }

            return true;
        }

        private bool TagMkvTVEpisode(ITVEpisode payload, IFileAbstraction target)
        {
            using (var file = Create(target))
            {
                file.RemoveTags(TagTypes.AllTags);
                var tag = (TagLib.Matroska.Tag)file.GetTag(TagTypes.Matroska, true);
                GetTVEpisodeData(payload, out var title, out var genres, out var description, out var image);
                SetBasicProperties(tag, title, genres, description, image);

                SetPropertyIfValid(tag, "TV_SHOW", MkvMediaTypeLabel);
                SetPropertyIfValid(tag, payload.Parent.Parent.ID, payload.Parent.Parent.Parent.IDTagLabel);
                SetPropertyIfValid(tag, payload.Parent.Number.ToString(), "SEASON_NUMBER");
                SetPropertyIfValid(tag, payload.Number.ToString(), "EPISODE_NUMBER");
                SetPropertyIfValid(tag, payload.Parent.Parent.Title, "SHOW_TITLE");
                SetPropertyIfValid(tag, payload.Parent.Title, "SEASON_TITLE");
                file.Save();
            }

            return true;
        }

        private bool TagMp4TVEpisode(ITVEpisode payload, IFileAbstraction target)
        {
            using (var file = Create(target))
            {
                file.RemoveTags(TagTypes.AllTags);
                var tag = (AppleTag)file.GetTag(TagTypes.Apple, true);
                GetTVEpisodeData(payload, out var title, out var genres, out var description, out var image);
                SetBasicProperties(tag, title, genres, description, image);

                SetPropertyIfValid(tag, $"https://www.themoviedb.org/tv/{payload.Parent.Parent.ID}", AppleDefaultBoxes.Url);
                tag.SetData(AppleDefaultBoxes.TvSeasonNumber, ByteVector.FromInt(payload.Parent.Number), 0);
                tag.SetData(AppleDefaultBoxes.TvEpisodeNumber, ByteVector.FromInt(payload.Number), 0);
                SetPropertyIfValid(tag, payload.Parent.Parent.Title, AppleDefaultBoxes.TvShowName);
                SetItunesMediaProperties(tag, file.Properties, AppleDefaultBoxes.ITunesMediaTypes.TVShow);
                file.Save();
            }

            return true;
        }

        private void GetTVEpisodeData(ITVEpisode payload, out string title, out IEnumerable<string> genres, out string description, out Stream image)
        {
            title = payload.Title;
            genres = payload.Parent.Parent.Genres;
            description = GetValueRecursive(d => !string.IsNullOrEmpty(d), () => payload.Description, () => payload.Parent.Description, () => payload.Parent.Parent.Description);
            image = GetValueRecursive(d => d != null, () => payload.Parent.GetImage(), () => payload.Parent.Parent.GetImage());
        }

        private T GetValueRecursive<T>(Func<T, bool> validityTest, params Func<T>[] valueGetters)
        {
            var output = default(T);
            foreach(var i in valueGetters)
            {
                output = i();
                if(validityTest(output))
                {
                    break;
                }
            }

            return output;
        }

        private void SetPropertyIfValid(Tag target, string value, Action<Tag, string> setter)
        {
            SetPropertyIfValid<Tag>(target, value, setter);
        }

        private void SetPropertyIfValid(TagLib.Matroska.Tag target, string value, string key)
        {
            SetPropertyIfValid(target, value, (d, e) => d.Set(key, null, e));
        }

        private void SetPropertyIfValid(AppleTag target, string value, ByteVector key)
        {
            SetPropertyIfValid(target, value, (d, e) => d.SetText(key, e));
        }

        private void SetPropertyIfValid<T>(T target, string value, Action<T, string> setter)
        {
            SetPropertyIfValid(target, value, d => !string.IsNullOrEmpty(d), setter);
        }

        private void SetPropertyIfValid<T, U>(T target, U value, Func<U, bool> checker, Action<T, U> setter)
        {
            if (checker(value))
            {
                setter(target, value);
            }
        }

        private void SetBasicProperties(Tag target, string title, IEnumerable<string> genres, string description, Stream image)
        {
            SetPropertyIfValid(target, title, (d, e) => d.Title = e);
            SetPropertyIfValid(target, genres, d => d != null && d.Any(), (d, e) => d.Genres = e.ToArray());
            SetPropertyIfValid(target, description, (d, e) => d.Description = e);
            SetPropertyIfValid(target, image, d => d != null, (d, e) => d.Pictures = new Picture[] { new Picture(ByteVector.FromStream(e)) });
        }

        private void SetItunesMediaProperties(AppleTag target, Properties fileProperties, AppleDefaultBoxes.ITunesMediaTypes mediaType)
        {
            var hdVideoTagValue = fileProperties.VideoWidth >= HDVideoMinWidth && fileProperties.VideoHeight >= HDVideoMinHeight ? 1 : 0;
            target.SetData(AppleDefaultBoxes.ITunesHDVideo, new ByteVector(new byte[] { Convert.ToByte(hdVideoTagValue) }), 0);
            target.SetData(AppleDefaultBoxes.ITunesMediaType, new ByteVector(new byte[] { Convert.ToByte((int)mediaType) }), 0);
        }
    }
}
