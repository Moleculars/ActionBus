using System;

namespace Bb.Contracts
{

    /// <summary>
    /// Represents a subscription to a given queue inside the message broker.
    /// </summary>
    public interface IBrokerSubscription : IDisposable
    {
    }
}
