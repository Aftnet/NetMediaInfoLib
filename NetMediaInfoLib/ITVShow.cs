using System.Collections.Generic;
using System.IO;

namespace NetMediaInfoLib
{
    public interface ITVShow : IAsyncDetails, IImageProvider, IChild<ISource<ITVShow>>, IParent<ITVSeason>
    {
        string ID { get; }
        string Title { get; }
        string Description { get; }
        IReadOnlyList<string> Genres { get; }
        string ReleaseDate { get; }
    }
}
