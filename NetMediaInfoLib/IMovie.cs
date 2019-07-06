using System.Collections.Generic;
using System.IO;

namespace NetMediaInfoLib
{
    public interface IMovie : IAsyncDetails, IImageProvider, IChild<ISource<IMovie>>
    {
        string ID { get; }
        string Title { get; }
        string Description { get; }
        IReadOnlyList<string> Genres { get; }
        string ReleaseDate { get; }
    }
}
