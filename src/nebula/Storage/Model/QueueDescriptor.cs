using System;
using ComposerCore;
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
            QueueType = queueType;
        }

        public string QueueType { get; set; }

        public IJobQueue<TStep> GetQueue<TStep>(IComposer composer) where TStep : IJobStep
        {
            return (IJobQueue<TStep>) composer.GetComponent(Type.GetType(QueueType));
        }
    }

    public class QueueDescriptor<TStep> : QueueDescriptor where TStep : IJobStep
    {
        public IJobQueue<TStep> GetQueue(IComposer composer)
        {
            return (IJobQueue<TStep>) composer.GetComponent(Type.GetType(QueueType));
        }
    }
}