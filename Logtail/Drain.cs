using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Logtail
{
    /// <summary>
    /// The Drain class is responsible for maintaining a queue of log events that need
    /// to be delivered to the server and periodically forwarding them to the server
    /// in batches.
    /// </summary>
    public sealed class Drain
    {
        private readonly int maxBatchSize;
        private readonly Client client;
        private readonly TimeSpan period;

        private object taskLock = new object();
        private readonly Task runningTask;

        private ConcurrentQueue<Log> queue = new ConcurrentQueue<Log>();
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a Logtail drain and starts periodic logs delivery.
        /// </summary>
        public Drain(
            Client client,
            TimeSpan? period = null,
            int maxBatchSize = 1000,
            CancellationToken? cancellationToken = null
        )
        {
            this.client = client;
            this.period = period ?? TimeSpan.FromMilliseconds(250);
            this.maxBatchSize = maxBatchSize;
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);

            runningTask = Task.Run(run);
        }

        /// <summary>
        /// Adds a single log event to a queue. The log event will be delivered later in a batch.
        /// This method will throw an exception if the Drain is stopped.
        /// </summary>
        public void Enqueue(Log log)
        {
            if (cancellationTokenSource.IsCancellationRequested) throw new DrainIsClosedException();

            queue.Enqueue(log);
        }

        /// <summary>
        /// Stops periodic logs delivery. The returned task will complete once the queue is flushed.
        /// </summary>
        public async Task Stop()
        {
            cancellationTokenSource.Cancel();
            await runningTask;
        }

        private async Task run() {
            var nextDelay = period;

            // XXX: We want the loop to run at least once, even if we stop
            //      the drain before the we manage to reach this point.
            do {
                var flushDuration = await delayed(flush, nextDelay);
                nextDelay = period - flushDuration;
            } while (!cancellationTokenSource.IsCancellationRequested);
        }

        private async Task flush() {
            while (!queue.IsEmpty) {
                var expectedItemsCount = Math.Min(maxBatchSize, queue.Count);
                var nextBatch = new List<Log>(expectedItemsCount);

                while (!queue.IsEmpty && nextBatch.Count < maxBatchSize) {
                    if (queue.TryDequeue(out var log)) nextBatch.Add(log);
                }

                if (nextBatch.Count > 0) {
                    await client.Send(nextBatch);
                }
            }
        }

        private async Task<TimeSpan> delayed(Func<Task> asyncAction, TimeSpan delay)
        {
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            try {
                await Task.Delay(delay, cancellationTokenSource.Token);
            } catch (TaskCanceledException) {
                // finish the rest of the loop to flush everything
            }

            var start = DateTimeOffset.UtcNow;

            await asyncAction();

            return DateTimeOffset.UtcNow - start;
        }
    }
}
