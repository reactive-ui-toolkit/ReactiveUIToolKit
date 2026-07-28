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

        IDisposable Listen(Action<RouterLocation> listener);

        IDisposable RegisterBlocker(Func<RouterLocation, RouterLocation, bool> blocker);
    }
}
