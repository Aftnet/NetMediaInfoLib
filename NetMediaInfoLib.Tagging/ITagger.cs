using System.Collections.Generic;
using static TagLib.File;

namespace NetMediaInfoLib.Tagging
{
    public interface ITagger
    {
        IReadOnlyCollection<string> SupportedVideoExtensions { get; }
        IReadOnlyCollection<string> SupportedAudioExtensions { get; }

        bool Tag(IMovie payload, IFileAbstraction target);
        bool Tag(ITVEpisode payload, IFileAbstraction target);
    }
}
