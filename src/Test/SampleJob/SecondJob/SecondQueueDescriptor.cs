using ComposerCore.Attributes;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.SampleJob.SecondJob
{
    [Component]
    [Contract]
    public class SecondQueueDescriptor : QueueDescriptor<FirstJobStep>
    {
        public SecondQueueDescriptor() : base(
            typeof(SecondJobQueue<SecondJobStep>).AssemblyQualifiedName)
        {
        }
    }
}