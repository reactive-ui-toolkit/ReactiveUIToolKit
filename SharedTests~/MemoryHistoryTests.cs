using System.Collections.Generic;
using Ruitk.Router;
using Xunit;

namespace Ruitk.Shared.Tests
{
    // MemoryHistory is the router's back/forward stack. It is pure state management with
    // no Unity coupling and had no tests; the interesting cases are the ones every history
    // implementation gets wrong at least once - pushing after going back, blockers, and
    // listener lifetime.
    public sealed class MemoryHistoryTests
    {
        [Fact]
        public void StartsAtTheInitialPath()
        {
            Assert.Equal("/", new MemoryHistory().Location.Path);
            Assert.Equal("/start", new MemoryHistory("/start").Location.Path);
        }

        [Fact]
        public void PushMovesForwardAndGoStepsBack()
        {
            var h = new MemoryHistory("/a");
            h.Push("/b");
            h.Push("/c");
            Assert.Equal("/c", h.Location.Path);

            h.Go(-1);
            Assert.Equal("/b", h.Location.Path);
            h.Go(-1);
            Assert.Equal("/a", h.Location.Path);
        }

        [Fact]
        public void CanGoReportsTheEdgesHonestly()
        {
            var h = new MemoryHistory("/a");
            Assert.False(h.CanGo(-1));

            h.Push("/b");
            Assert.True(h.CanGo(-1));
            Assert.False(h.CanGo(1));

            h.Go(-1);
            Assert.True(h.CanGo(1));
        }

        [Fact]
        public void GoBeyondAnEdgeDoesNotMove()
        {
            var h = new MemoryHistory("/a");
            h.Go(-5);
            Assert.Equal("/a", h.Location.Path);

            h.Push("/b");
            h.Go(10);
            Assert.Equal("/b", h.Location.Path);
        }

        [Fact]
        public void PushAfterGoingBackTruncatesTheForwardEntries()
        {
            var h = new MemoryHistory("/a");
            h.Push("/b");
            h.Push("/c");
            h.Go(-2);
            Assert.Equal("/a", h.Location.Path);

            h.Push("/d");

            Assert.Equal("/d", h.Location.Path);
            // "/b" and "/c" are gone - going forward from here must not resurrect them.
            Assert.False(h.CanGo(1));
        }

        [Fact]
        public void ReplaceSwapsTheCurrentEntryWithoutGrowingHistory()
        {
            var h = new MemoryHistory("/a");
            h.Push("/b");
            h.Replace("/c");

            Assert.Equal("/c", h.Location.Path);
            h.Go(-1);
            Assert.Equal("/a", h.Location.Path);
            // Replace must not have left "/b" behind as a separate entry.
            h.Go(1);
            Assert.Equal("/c", h.Location.Path);
        }

        [Fact]
        public void ListenEmitsTheCurrentLocationImmediatelyOnSubscribe()
        {
            // Part of the IRouterHistory contract: a subscriber is handed the current
            // location straight away so it never has to seed itself separately.
            var h = new MemoryHistory("/a");
            var seen = new List<string>();
            h.Listen(loc => seen.Add(loc.Path));

            Assert.Equal(new[] { "/a" }, seen);
        }

        [Fact]
        public void ListenersFireOnNavigationAndStopAfterDispose()
        {
            var h = new MemoryHistory("/a");
            var seen = new List<string>();
            var sub = h.Listen(loc => seen.Add(loc.Path));

            h.Push("/b");
            h.Replace("/c");
            h.Go(-1);

            // Leading "/a" is the emit-on-subscribe above.
            Assert.Equal(new[] { "/a", "/b", "/c", "/a" }, seen);

            sub.Dispose();
            h.Push("/d");
            Assert.Equal(4, seen.Count);
        }

        [Fact]
        public void ABlockerCanVetoNavigation()
        {
            var h = new MemoryHistory("/a");
            using (h.RegisterBlocker((from, to) => to.Path != "/blocked"))
            {
                h.Push("/blocked");
                Assert.Equal("/a", h.Location.Path);

                h.Push("/allowed");
                Assert.Equal("/allowed", h.Location.Path);
            }
        }

        [Fact]
        public void DisposingABlockerRestoresNavigation()
        {
            var h = new MemoryHistory("/a");
            var blocker = h.RegisterBlocker((from, to) => false);

            h.Push("/b");
            Assert.Equal("/a", h.Location.Path);

            blocker.Dispose();
            h.Push("/b");
            Assert.Equal("/b", h.Location.Path);
        }

        [Fact]
        public void PushCarriesQueryAndState()
        {
            var h = new MemoryHistory("/a");
            var state = new object();
            h.Push("/b?x=1", state);

            Assert.Equal("/b", h.Location.Path);
            Assert.Equal("1", h.Location.Query["x"]);
            Assert.Same(state, h.Location.State);
        }
    }
}
