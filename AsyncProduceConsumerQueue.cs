using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class AsyncProducerConsumerQueue<T> : IDisposable
{
    private readonly Action<T> m_consumer;
    private readonly BlockingCollection<T> m_queue;
    private readonly CancellationTokenSource m_cancelTokenSrc;

    public AsyncProducerConsumerQueue(Action<T> consumer)
    {
        if (consumer == null)
        {
            throw new ArgumentNullException(nameof(consumer));
        }

        m_consumer = consumer;
        m_queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        m_cancelTokenSrc = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await ConsumeLoopAsync(m_cancelTokenSrc.Token);
        });
    }

    public async Task ProduceAsync(T value)
    {
        m_queue.Add(value);
    }

    public async Task<int> GetQueueSize()
    {
        return m_queue.Count;
    }

    private async Task ConsumeLoopAsync(CancellationToken cancelToken)
    {
        foreach (var item in m_queue.GetConsumingEnumerable(cancelToken))
        {
            try
            {
                //var item = m_queue.Take(cancelToken);
                m_consumer(item);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        // while (!cancelToken.IsCancellationRequested)
        // {
            
        // }
    }

    #region IDisposable
    private bool m_isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!m_isDisposed)
        {
            if (disposing)
            {
                m_cancelTokenSrc.Cancel();
                m_cancelTokenSrc.Dispose();
                m_queue.Dispose();
            }

            m_isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
