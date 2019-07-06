using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetMediaInfoLib
{
    public interface ISource<T>
    {
        string IDTagLabel { get; }
        Task<T> GetAsync(string id, CancellationToken ct);
        Task<IReadOnlyList<T>> SearchAsync(string query, CancellationToken ct);
    }
}
