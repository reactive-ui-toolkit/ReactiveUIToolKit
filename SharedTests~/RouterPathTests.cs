using System.Collections.Generic;
using Ruitk.Router;
using Xunit;

namespace Ruitk.Shared.Tests
{
    // RouterPath is pure string logic with no Unity coupling and, until now, no tests.
    // These assertions encode what a router is expected to do rather than what the
    // current implementation happens to do - a failure here is a finding, not a
    // test that needs relaxing.
    public sealed class RouterPathTests
    {
        [Theory]
        [InlineData("", "/")]
        [InlineData("/", "/")]
        [InlineData("//", "/")]
        [InlineData("users", "/users")]
        [InlineData("/users", "/users")]
        [InlineData("/users/", "/users")]
        [InlineData("/users//posts", "/users/posts")]
        [InlineData("/users/posts/", "/users/posts")]
        public void Normalize_producesALeadingSlashAndNoTrailingSlash(string input, string expected)
        {
            Assert.Equal(expected, RouterPath.Normalize(input));
        }

        [Fact]
        public void Normalize_isIdempotent()
        {
            foreach (var raw in new[] { "", "/", "users/", "/a//b/", "/a/b" })
            {
                var once = RouterPath.Normalize(raw);
                Assert.Equal(once, RouterPath.Normalize(once));
            }
        }

        [Theory]
        [InlineData("/", "users", "/users")]
        [InlineData("/app", "users", "/app/users")]
        [InlineData("/app/", "users", "/app/users")]
        [InlineData("/app", "/users", "/users")]
        [InlineData("/app", "", "/app")]
        public void Combine_treatsALeadingSlashAsAbsolute(string basePath, string relative, string expected)
        {
            Assert.Equal(expected, RouterPath.Combine(basePath, relative));
        }

        [Fact]
        public void SplitSegments_dropsEmptySegments()
        {
            Assert.Equal(new[] { "a", "b" }, RouterPath.SplitSegments("/a/b"));
            Assert.Equal(new[] { "a", "b" }, RouterPath.SplitSegments("/a//b/"));
            Assert.Empty(RouterPath.SplitSegments("/"));
            Assert.Empty(RouterPath.SplitSegments(""));
        }

        [Fact]
        public void ParseQuery_handlesTheOrdinaryShapes()
        {
            var q = RouterPath.ParseQuery("?a=1&b=2");
            Assert.Equal("1", q["a"]);
            Assert.Equal("2", q["b"]);

            // A leading '?' is optional.
            Assert.Equal("1", RouterPath.ParseQuery("a=1")["a"]);

            // A key with no '=' is present with an empty value, not absent.
            Assert.True(RouterPath.ParseQuery("flag").ContainsKey("flag"));

            Assert.Empty(RouterPath.ParseQuery(""));
            Assert.Empty(RouterPath.ParseQuery("?"));
        }

        [Fact]
        public void ParseQuery_decodesPercentEncoding()
        {
            var q = RouterPath.ParseQuery("?name=a%20b&sym=%26");
            Assert.Equal("a b", q["name"]);
            Assert.Equal("&", q["sym"]);
        }

        [Fact]
        public void BuildQuery_roundTripsThroughParseQuery()
        {
            var original = new Dictionary<string, string>
            {
                ["name"] = "a b",
                ["sym"] = "&",
                ["n"] = "1",
            };

            var built = RouterPath.BuildQuery(original);
            var reparsed = RouterPath.ParseQuery(built);

            Assert.Equal(original.Count, reparsed.Count);
            foreach (var kv in original)
                Assert.Equal(kv.Value, reparsed[kv.Key]);
        }

        [Fact]
        public void BuildQuery_returnsEmptyForNoPairs()
        {
            Assert.Equal(string.Empty, RouterPath.BuildQuery(new Dictionary<string, string>()));
        }

        [Fact]
        public void Parse_separatesPathFromQuery()
        {
            var loc = RouterPath.Parse("/users?page=2");
            Assert.Equal("/users", loc.Path);
            Assert.Equal("2", loc.Query["page"]);
        }

        [Fact]
        public void Parse_handlesAPathWithNoQuery()
        {
            var loc = RouterPath.Parse("/users");
            Assert.Equal("/users", loc.Path);
            Assert.Empty(loc.Query);
        }

        [Fact]
        public void Parse_carriesStateThrough()
        {
            var state = new object();
            Assert.Same(state, RouterPath.Parse("/x", state).State);
        }

        [Theory]
        [InlineData("/app/users", "/app", "/users")]
        [InlineData("/app", "/app", "/")]
        [InlineData("/other", "/app", "/other")]
        [InlineData("/app/users", "", "/app/users")]
        public void StripBasename_removesOnlyAMatchingPrefix(string path, string basename, string expected)
        {
            Assert.Equal(expected, RouterPath.StripBasename(path, basename));
        }

        [Fact]
        public void WithBasename_isTheInverseOfStripBasename()
        {
            foreach (var basename in new[] { "", "/app", "/deep/base" })
            {
                foreach (var path in new[] { "/", "/users", "/users/1" })
                {
                    var withBase = RouterPath.WithBasename(path, basename);
                    Assert.Equal(path, RouterPath.StripBasename(withBase, basename));
                }
            }
        }

        [Fact]
        public void StripBasename_doesNotMatchAPartialSegment()
        {
            // "/application" must not be treated as living under the "/app" basename.
            Assert.Equal("/application", RouterPath.StripBasename("/application", "/app"));
        }
    }
}
