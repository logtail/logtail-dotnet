using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Logtail
{
    public class Drain
    {
        private readonly int maxItemsPerFlush;
        private readonly Client client;
        private readonly TimeSpan period;

        private object taskLock = new object();
        private readonly Task runningTask;

        private ConcurrentQueue<Log> queue = new ConcurrentQueue<Log>();
        private CancellationTokenSource cancellationTokenSource;

        public Drain(
            Client client,
            TimeSpan? period = null,
            int maxItemsPerFlush = 1000,
            CancellationToken? cancellationToken = null
        )
        {
            this.client = client;
            this.period = period ?? TimeSpan.FromMilliseconds(250);
            this.maxItemsPerFlush = maxItemsPerFlush;
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);

            runningTask = Task.Run(run);
        }

        public void Enqueue(Log log)
        {
            if (cancellationTokenSource.IsCancellationRequested) throw new DrainIsClosedException();

            queue.Enqueue(log);
        }

        public async Task Stop()
        {
            cancellationTokenSource.Cancel();
            await runningTask;
        }

        private async Task run() {
            var nextDelay = period;

            while (!cancellationTokenSource.IsCancellationRequested) {
                var flushDuration = await delayed(flush, nextDelay);
                nextDelay = period - flushDuration;
            }
        }

        private async Task flush() {
            while (!queue.IsEmpty) {
                var expectedItemsCount = Math.Min(maxItemsPerFlush, queue.Count);
                var nextBatch = new List<Log>(expectedItemsCount);

                while (!queue.IsEmpty && nextBatch.Count < maxItemsPerFlush) {
                    if (queue.TryDequeue(out var log)) nextBatch.Add(log);
                }

                await client.Send(nextBatch);
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
