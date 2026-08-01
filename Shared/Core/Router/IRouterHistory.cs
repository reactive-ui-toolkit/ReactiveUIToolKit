using System;

namespace Ruitk.Router
{
    public interface IRouterHistory
    {
        RouterLocation Location { get; }

        int EntryCount { get; }

        int Index { get; }

        bool CanGo(int delta);

        void Go(int delta);

        void Push(string path, object state = null);

        void Replace(string path, object state = null);

        /// <summary>
        /// Subscribes to location changes. Implementations must invoke <paramref name="listener"/>
        /// once with the current location before returning, so a subscriber never has to seed
        /// itself separately, and then on every subsequent navigation. Disposing the returned
        /// handle stops further callbacks.
        /// </summary>
        IDisposable Listen(Action<RouterLocation> listener);

        IDisposable RegisterBlocker(Func<RouterLocation, RouterLocation, bool> blocker);
    }
}
