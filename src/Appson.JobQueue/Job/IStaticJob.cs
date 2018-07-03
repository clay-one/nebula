using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job
{
    [Contract]
    public interface IStaticJob
    {
        Task EnsureJobsDefined();
    }
}