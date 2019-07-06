using System;
using System.IO;

namespace NetMediaInfoLib.Components
{
    internal class MemoryStreamProvider
    {
        byte[] BackingStore { get; }

        public MemoryStreamProvider(byte[] backingStore)
        {
            BackingStore = backingStore ?? throw new ArgumentNullException(nameof(backingStore));
        }

        public MemoryStream GetStream()
        {
            return new MemoryStream(BackingStore, false);
        }
    }
}
