using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    public interface IFinalizableJobProcessor<TItem> : IJobProcessor<TItem> where TItem : IJobStep
    {
        void Initialize();
    }
}