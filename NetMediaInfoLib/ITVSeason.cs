using System.IO;

namespace NetMediaInfoLib
{
    public interface ITVSeason : IAsyncDetails, IImageProvider, IChild<ITVShow>, IParent<ITVEpisode>
    {
        int Number { get; }
        string Title { get; }
        string Description { get; }
        string ReleaseDate { get; }
    }
}
