using System.Threading;
using System.Threading.Tasks;

namespace NetMediaInfoLib
{
    public interface IAsyncDetails
    {
        bool Populated { get; }
        Task<bool> GetDetailsAsync(CancellationToken ct);
    }
}
