using System.IO;

namespace NetMediaInfoLib
{
    public interface IImageProvider
    {
        bool HasImage { get; }
        MemoryStream GetImage();
    }
}
