using ComposerCore.Attributes;
using Nebula.Storage.Model;

namespace Test.SampleJob.FirstJob
{
    [Component]
    [Contract]
    public class FirstQueueDescriptor : QueueDescriptor<FirstJobStep>
    {
        public FirstQueueDescriptor() : base(
            typeof(FirstJobQueue<FirstJobStep>).AssemblyQualifiedName)
        {
        }
    }
}