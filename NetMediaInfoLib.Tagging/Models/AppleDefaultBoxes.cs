using System;
using TagLib;

namespace NetMediaInfoLib.Tagging.Models
{
    internal static class AppleDefaultBoxes
    {
        public enum ITunesMediaTypes
        {
            Music = 1,
            Audiobook = 2,
            MusicVideo = 6,
            Movie = 9,
            TVShow = 10,
            Booklet = 11,
            Ringtone = 14
        }

        public static readonly ReadOnlyByteVector AlbumArtist = "aART";
        public static readonly ReadOnlyByteVector Album = FixIdLen3("alb");
        public static readonly ReadOnlyByteVector Artist = FixIdLen3("ART");
        public static readonly ReadOnlyByteVector Comment = FixIdLen3("cmt");
        public static readonly ReadOnlyByteVector Conductor = "cond";
        //public static readonly ReadOnlyByteVector Covr = "covr";
        //public static readonly ReadOnlyByteVector Co64 = "co64";
        public static readonly ReadOnlyByteVector Compilation = "cpil";
        public static readonly ReadOnlyByteVector Copyright = "cprt";
        //public static readonly ReadOnlyByteVector Data = "data";
        public static readonly ReadOnlyByteVector Date = FixIdLen3("day");
        public static readonly ReadOnlyByteVector Description = "desc";
        public static readonly ReadOnlyByteVector DiskNumber = "disk";
        //public static readonly ReadOnlyByteVector Dtag = "dtag";
        //public static readonly ReadOnlyByteVector Esds = "esds";
        //public static readonly ReadOnlyByteVector Ilst = "ilst";
        //public static readonly ReadOnlyByteVector Free = "free";
        public static readonly ReadOnlyByteVector Genre = FixIdLen3("gen");
        public static readonly ReadOnlyByteVector GenreId = "geID";
        //public static readonly ReadOnlyByteVector Grp = FixIdLen3("grp");
        //public static readonly ReadOnlyByteVector Hdlr = "hdlr";
        public static readonly ReadOnlyByteVector Lyrics = FixIdLen3("lyr");
        //public static readonly ReadOnlyByteVector Mdat = "mdat";
        //public static readonly ReadOnlyByteVector Mdia = "mdia";
        //public static readonly ReadOnlyByteVector Meta = "meta";
        //public static readonly ReadOnlyByteVector Mean = "mean";
        //public static readonly ReadOnlyByteVector Minf = "minf";
        //public static readonly ReadOnlyByteVector Moov = "moov";
        //public static readonly ReadOnlyByteVector Mvhd = "mvhd";
        public static readonly ReadOnlyByteVector Name = "name";
        public static readonly ReadOnlyByteVector Role = "role";
        public static readonly ReadOnlyByteVector Skip = "skip";
        public static readonly ReadOnlyByteVector SortAlbumArtist = "soaa"; // Album Artist Sort
        public static readonly ReadOnlyByteVector SortArtist = "soar"; // Performer Sort
        public static readonly ReadOnlyByteVector SortComposer = "soco"; // Composer Sort
        public static readonly ReadOnlyByteVector SortTrackTitle = "sonm"; // Track Title Sort
        public static readonly ReadOnlyByteVector SortAlbumTitle = "soal"; // Album Title Sort
        public static readonly ReadOnlyByteVector Stbl = "stbl";
        public static readonly ReadOnlyByteVector Stco = "stco";
        public static readonly ReadOnlyByteVector Stsd = "stsd";
        public static readonly ReadOnlyByteVector Subt = "Subt";
        public static readonly ReadOnlyByteVector Text = "text";
        public static readonly ReadOnlyByteVector BPM = "tmpo";
        public static readonly ReadOnlyByteVector Trak = "trak";
        public static readonly ReadOnlyByteVector TrackNumber = "trkn";
        public static readonly ReadOnlyByteVector Udta = "udta";
        public static readonly ReadOnlyByteVector Url = FixIdLen3("url");
        public static readonly ReadOnlyByteVector Uuid = "uuid";
        public static readonly ReadOnlyByteVector Composer = FixIdLen3("wrt");
        //public static readonly ReadOnlyByteVector DASH = "----";

        // Handler types.
        //public static readonly ReadOnlyByteVector Soun = "soun";
        //public static readonly ReadOnlyByteVector Vide = "vide";

        // Another handler type, found in wild in audio file ripped using iTunes
        //public static readonly ReadOnlyByteVector Alis = "alis";

        // Video specific
        public static readonly ReadOnlyByteVector ITunesAdvisory = "rtng";
        public static readonly ReadOnlyByteVector ITunesGapless = "pgap";
        public static readonly ReadOnlyByteVector ITunesHDVideo = "hdvd"; //Boolean flag for HD video (0/1)
        public static readonly ReadOnlyByteVector ITunesMediaType = "stik"; // Normal (Music) - 1 | Audiobook - 2 | Music Video - 6 | Movie - 9 | TV Show - 10 | Booklet - 11 | Ringtone - 14
        public static readonly ReadOnlyByteVector ShowMovement = "shwm";
        public static readonly ReadOnlyByteVector TvEpisodeNumber = "tves";
        public static readonly ReadOnlyByteVector TvEpisodeId = "tven";
        public static readonly ReadOnlyByteVector TvNetworkName = "tvnn";
        public static readonly ReadOnlyByteVector TvSeasonNumber = "tvsn";
        public static readonly ReadOnlyByteVector TvShowName = "tvsh";
        public static readonly ReadOnlyByteVector SortTvShowName = "sosn";
        public static readonly ReadOnlyByteVector WORK = FixIdLen3("wrk");

        private static ReadOnlyByteVector FixIdLen3(ByteVector id)
        {
            if (id.Count != 3)
            {
                throw new ArgumentException();
            }

            return new ReadOnlyByteVector(0xa9, id[0], id[1], id[2]);
        }
    }
}
