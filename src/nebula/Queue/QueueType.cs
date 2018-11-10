namespace Nebula.Queue
{
    public static class QueueType
    {
        public const string Redis = "Redis";
        public const string Inline = "Inline";
        public const string InMemory = "InMemory";
        public const string Delayed = "Delayed";
        public const string Kafka = "Kafka";
        public const string Null = "Null";
    }
}