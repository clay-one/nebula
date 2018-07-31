using Nebula.Queue;

namespace Test.SampleJob.FirstJob
{
    public class FirstJobStep : IJobStep
    {
        public int Number { get; set; }
    }
}