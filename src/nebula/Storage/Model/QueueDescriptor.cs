using System;
using ComposerCore;
using ComposerCore.Attributes;
using Nebula.Queue;

namespace Nebula.Storage.Model
{
    public class QueueDescriptor
    {
        public QueueDescriptor()
        {
        }

        public QueueDescriptor(string queueType)
        {
            var type = Type.GetType(queueType);

            QueueType = type.Name;
            AssemblyQualifiedName = queueType;
        }

        public string QueueType { get; set; }

        public string AssemblyQualifiedName { get; set; }

        public IJobQueue<TStep> GetQueue<TStep>(IComposer composer) where TStep : IJobStep
        {
           return composer.GetComponent<IJobQueue<TStep>>(QueueType);
        }
    }

    public class QueueDescriptor<TStep> : QueueDescriptor where TStep : IJobStep
    {
        public QueueDescriptor(string queueType) : base(queueType)
        {

        }

        public IJobQueue<TStep> GetQueue(IComposer composer)
        {
            return composer.GetComponent<IJobQueue<TStep>>(QueueType);
        }
    }
}