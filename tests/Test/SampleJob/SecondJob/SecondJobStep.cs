using Nebula.Queue;

namespace Test.SampleJob.SecondJob
{
    public class SecondJobStep : IJobStep
    {
        public int Number { get; set; }
    }
}