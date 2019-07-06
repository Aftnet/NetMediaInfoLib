using System.Collections.Generic;
namespace NetMediaInfoLib
{
    public interface IParent<T>
    {
        IReadOnlyList<T> Children { get; }
    }
}
