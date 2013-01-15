using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Chinchilla
{
    public class WorkerPoolWorker : Worker
    {
        private readonly BlockingCollection<IDelivery> deliveries;

        private readonly Thread thread;

        public WorkerPoolWorker(
            BlockingCollection<IDelivery> deliveries,
            IDeliveryProcessor connectedProcessor)
            : base(connectedProcessor)
        {
            this.deliveries = deliveries;

            thread = new Thread(StartTakingMessages);
        }

        public override string WorkerType
        {
            get { return "WorkerPool"; }
        }

        public bool IsStopping { get; set; }

        public void Start()
        {
            thread.Start();
        }

        public void Join()
        {
            thread.Join();
        }

        public void StopAndJoin()
        {
            IsStopping = true;
            thread.Join();
        }

        public void BeforeStartMessagePump()
        {
            Status = WorkerStatus.Idle;
        }

        public void StartTakingMessages()
        {
            BeforeStartMessagePump();
            PumpMessages();
        }

        private void PumpMessages()
        {
            while (!IsStopping)
            {
                IDelivery delivery;

                try
                {
                    delivery = deliveries.Take();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                Deliver(delivery);
            }
        }
    }
}