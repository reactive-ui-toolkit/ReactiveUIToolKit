using System.Text;
using Ruitk.SourceGenerator.Tools;
using Xunit;

namespace Ruitk.SourceGenerator.Tests
{
    /// <summary>
    /// Behaviour contract for the 0.12.0 rebrand codemod (<see cref="BrandMigrator"/>,
    /// <see cref="ScanRules"/>, <see cref="FileEncodings"/>).
    ///
    /// <para>Written against post-rebrand audit findings H1, H5, M2, M3, M7, M8, M9, L7 and L9.
    /// The tool rewrites CUSTOMERS' source, so every property its XML docs assert — replacement
    /// ORDER, token BOUNDARIES, frozen-identity survival, encoding round-trip, and idempotency —
    /// is pinned here. Before this file the codemod was referenced by nothing outside its own
    /// source files (M9).</para>
    /// </summary>
    public sealed class BrandCodemodTests
    {
        private static string Migrate(string text) => BrandMigrator.Migrate(text, out _);

        private static string Migrate(string text, out int count) =>
            BrandMigrator.Migrate(text, out count);

        // ── C1 composites, and their ordering against the bare token ────────────────

        [Fact]
        public void Composites_RunBeforeBareToken()
        {
            // If C2 ran first, "ReactiveUITKConfig" would be untouched (the right boundary
            // rejects it), but "__ReactiveUITK_MediaHost" WOULD match (underscore is excluded
            // by the lookahead? no — '_' IS in [A-Za-z_], so it is rejected too). The ordering
            // guarantee that actually matters is that the composite table wins, verbatim.
            Assert.Equal("RuitkConfig", Migrate("ReactiveUITKConfig"));
            Assert.Equal("__Ruitk_MediaHost", Migrate("__ReactiveUITK_MediaHost"));
            Assert.Equal("__Ruitk_VideoPeer", Migrate("__ReactiveUITK_VideoPeer"));
            Assert.Equal("__Ruitk_AudioPeer", Migrate("__ReactiveUITK_AudioPeer"));
            Assert.Equal("__Ruitk_Sfx", Migrate("__ReactiveUITK_Sfx"));
            Assert.Equal("__Ruitk_RT_512x512", Migrate("__ReactiveUITK_RT_512x512"));
        }

        [Fact]
        public void Composites_AndBareToken_Coexist_InOneFile()
        {
            const string src = """
                using ReactiveUITK.Core;
                var cfg = ReactiveUITKConfig.Load();
                var host = GameObject.Find("__ReactiveUITK_MediaHost");
                """;
            string outText = Migrate(src, out int count);

            Assert.Contains("using Ruitk.Core;", outText);
            Assert.Contains("RuitkConfig.Load()", outText);
            Assert.Contains("\"__Ruitk_MediaHost\"", outText);
            Assert.DoesNotContain("ReactiveUITK", outText);
            Assert.Equal(3, count);
        }

        // ── C2 bare token: BOTH boundaries (L7) ────────────────────────────────────

        [Theory]
        [InlineData("using ReactiveUITK.Core;", "using Ruitk.Core;")]
        [InlineData("namespace ReactiveUITK.Generated", "namespace Ruitk.Generated")]
        [InlineData("global::ReactiveUITK.V", "global::Ruitk.V")]
        [InlineData("\"references\": [\"ReactiveUITK.Runtime\"]", "\"references\": [\"Ruitk.Runtime\"]")]
        [InlineData("typeof(ReactiveUITK.Core.VirtualNode)", "typeof(Ruitk.Core.VirtualNode)")]
        [InlineData("ReactiveUITK", "Ruitk")]
        public void BareToken_RewritesRealUsages(string src, string expected)
            => Assert.Equal(expected, Migrate(src));

        [Theory]
        [InlineData("ReactiveUITKX")]         // right boundary: alnum follows
        [InlineData("ReactiveUITKStuff")]     // right boundary
        [InlineData("ReactiveUITK_Thing")]    // right boundary: '_' is excluded
        public void BareToken_RespectsRightBoundary(string src)
            => Assert.Equal(src, Migrate(src));

        [Fact]
        public void Composites_AreUnboundariedByDesign_AndWinOverTheBareToken()
        {
            // The C1 table is a LITERAL substring replacement with no boundary of its own —
            // that is deliberate (it is how "__ReactiveUITK_RT_" catches every "<w>x<h>" suffix).
            // A consequence: "ReactiveUITKConfigX" is claimed by C1 and becomes "RuitkConfigX",
            // even though the C2 right boundary alone would have rejected it. Pinned so the
            // ordering is not "fixed" into a regression.
            Assert.Equal("RuitkConfigX", Migrate("ReactiveUITKConfigX"));
            Assert.Equal("__Ruitk_RT_1920x1080", Migrate("__ReactiveUITK_RT_1920x1080"));
        }

        [Fact]
        public void BareToken_RespectsLeftBoundary_L7()
        {
            // Any text merely ENDING in the token must not be rewritten. The dominant real
            // case is the frozen VS2022 identity, which is also covered by the frozen guard;
            // this pins the boundary itself.
            Assert.Equal("MyReactiveUITK", Migrate("MyReactiveUITK"));
            Assert.Equal("Contoso.ReactiveUITK", Migrate("Contoso.ReactiveUITK"));
            Assert.Equal("x9ReactiveUITK", Migrate("x9ReactiveUITK"));
        }

        // ── F: frozen marketplace identities survive (C1-class regression) ─────────

        [Theory]
        [InlineData("\"editor.defaultFormatter\": \"ReactiveUITK.uitkx\"")]
        [InlineData("<Identity Id=\"UitkxVsix.ReactiveUITK\" />")]
        [InlineData("https://marketplace.visualstudio.com/items?itemName=ReactiveUITK.uitkx")]
        [InlineData("https://marketplace.visualstudio.com/items?itemName=ReactiveUITK.uitkx-visualstudio")]
        [InlineData("https://marketplace.visualstudio.com/manage/publishers/ReactiveUITK")]
        [InlineData("vsce login ReactiveUITK")]
        [InlineData("ext install ReactiveUITK.uitkx")]
        public void FrozenMarketplaceIdentities_RoundTripVerbatim(string src)
        {
            string outText = Migrate(src, out int count);
            Assert.Equal(src, outText);
            Assert.Equal(0, count);
        }

        [Fact]
        public void FrozenIdentity_SurvivesAlongsideARealRename_InTheSameFile()
        {
            const string src = """
                // Install with: ext install ReactiveUITK.uitkx
                using ReactiveUITK.Core;
                """;
            string outText = Migrate(src);

            Assert.Contains("ext install ReactiveUITK.uitkx", outText);
            Assert.Contains("using Ruitk.Core;", outText);
        }

        // ── C1b: menu root (M2) ───────────────────────────────────────────────────

        [Fact]
        public void MenuRoot_RewritesToTheDisplayName_NotTheIdentifier()
        {
            // '/' is not in [A-Za-z_], so without this rule the bare token turns the menu path
            // into the plausible-but-wrong "Ruitk/HMR Mode" — a wrong path that greps clean.
            Assert.Equal(
                "[MenuItem(\"Reactive UI Toolkit/My Tool\")]",
                Migrate("[MenuItem(\"ReactiveUITK/My Tool\")]"));

            Assert.Equal(
                "EditorApplication.ExecuteMenuItem(\"Reactive UI Toolkit/HMR Mode\");",
                Migrate("EditorApplication.ExecuteMenuItem(\"ReactiveUITK/HMR Mode\");"));
        }

        [Fact]
        public void MenuRoot_OnlyFiresInsideAStringLiteral()
        {
            // Unquoted "ReactiveUITK/" is not a menu path — a comment about a folder, say.
            // It falls through to C2, which is the correct identifier treatment.
            Assert.Equal("// see Ruitk/Resources", Migrate("// see ReactiveUITK/Resources"));
        }

        // ── C3 define ─────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("#if REACTIVEUITK_HAS_TEST_FRAMEWORK", "#if RUITK_HAS_TEST_FRAMEWORK")]
        [InlineData("-define:REACTIVEUITK_HAS_TEST_FRAMEWORK", "-define:RUITK_HAS_TEST_FRAMEWORK")]
        public void Define_IsRenamed(string src, string expected)
            => Assert.Equal(expected, Migrate(src));

        // ── D: install path, all prefixes (M7/M8) + boundary ──────────────────────

        [Theory]
        [InlineData("\"Assets/ReactiveUIToolKit/config.json\"", "\"Assets/ReactiveUIToolkit/config.json\"")]
        [InlineData(@"""Assets\ReactiveUIToolKit""", @"""Assets\ReactiveUIToolkit""")]
        [InlineData(@"""Assets\\ReactiveUIToolKit""", @"""Assets\\ReactiveUIToolkit""")]
        // M8 — the package's own idiom is a BARE segment, which the old rules never matched.
        [InlineData(
            "Path.Combine(Application.dataPath, \"ReactiveUIToolKit\", \"config.json\")",
            "Path.Combine(Application.dataPath, \"ReactiveUIToolkit\", \"config.json\")")]
        [InlineData("\"ReactiveUIToolKit/config.json\"", "\"ReactiveUIToolkit/config.json\"")]
        // M8 — the Packages/-prefixed (embedded UPM) form.
        [InlineData("\"Packages/ReactiveUIToolKit/Runtime\"", "\"Packages/ReactiveUIToolkit/Runtime\"")]
        public void InstallPath_RewritesEveryPrefixForm(string src, string expected)
            => Assert.Equal(expected, Migrate(src));

        [Theory]
        [InlineData("\"Assets/ReactiveUIToolKitExtras/x.cs\"")]  // user folder — must NOT match
        [InlineData("\"MyReactiveUIToolKit\"")]
        [InlineData("ReactiveUIToolKit2")]
        public void InstallPath_RespectsWordBoundaries(string src)
            => Assert.Equal(src, Migrate(src));

        [Fact]
        public void InstallPath_LeavesTheAlreadyCorrectCasingAlone()
        {
            const string src = "\"Assets/ReactiveUIToolkit/config.json\"";
            Assert.Equal(src, Migrate(src, out int count));
            Assert.Equal(0, count);
        }

        // ── Idempotency ───────────────────────────────────────────────────────────

        [Fact]
        public void SecondPass_IsAGenuineNoOp()
        {
            const string src = """
                using ReactiveUITK.Core;
                using ReactiveUITK.Refresh;

                #if REACTIVEUITK_HAS_TEST_FRAMEWORK
                namespace ReactiveUITK.Samples
                {
                    [MenuItem("ReactiveUITK/Demos/Thing")]
                    internal static class Thing
                    {
                        const string Cfg = "Assets/ReactiveUIToolKit/config.json";
                        const string Bare = "ReactiveUIToolKit";
                        static readonly string Host = "__ReactiveUITK_MediaHost";
                        // ext install ReactiveUITK.uitkx
                        static ReactiveUITKConfig C => global::ReactiveUITK.Cfg.Get();
                    }
                }
                #endif
                """;

            string first = Migrate(src, out int firstCount);
            Assert.True(firstCount > 0);

            string second = Migrate(first, out int secondCount);
            Assert.Equal(0, secondCount);
            Assert.Equal(first, second);

            // And the pass really did its job.
            Assert.DoesNotContain("ReactiveUITKConfig", first);
            Assert.DoesNotContain("REACTIVEUITK_HAS_TEST_FRAMEWORK", first);
            Assert.DoesNotContain("ReactiveUIToolKit", first);
            Assert.Contains("[MenuItem(\"Reactive UI Toolkit/Demos/Thing\")]", first);
            Assert.Contains("ext install ReactiveUITK.uitkx", first);   // frozen, still there
        }

        [Fact]
        public void UnrelatedText_IsReferenceUnchanged()
        {
            const string src = "public sealed class Foo { }\n";
            Assert.Equal(src, Migrate(src, out int count));
            Assert.Equal(0, count);
        }

        // ── Line endings (verified in the audit; pinned so it stays true) ──────────

        [Fact]
        public void LineEndings_ArePreserved()
        {
            const string src = "using ReactiveUITK.Core;\r\nusing ReactiveUITK.V;\r\n";
            string outText = Migrate(src);
            Assert.Equal("using Ruitk.Core;\r\nusing Ruitk.V;\r\n", outText);
        }

        // ── ScanRules: skips (H1, L9) and extensions (M4) ──────────────────────────

        [Theory]
        [InlineData("Assets/ReactiveUITK/UITKX_GeneratorTrigger.g.cs")]   // H1 — 0.11.x trigger folder
        [InlineData("Assets/Ruitk/UITKX_GeneratorTrigger.g.cs")]          // new trigger folder
        [InlineData("Assets/Ruitk/Resources/x.cs")]
        [InlineData("Assets/ReactiveUIToolKit/Runtime/X.cs")]             // package, old casing
        [InlineData("Assets/ReactiveUIToolkit/Runtime/X.cs")]             // package, new casing
        [InlineData("Packages/com.reactiveuitoolkit/Runtime/X.cs")]       // L9 — embedded UPM
        [InlineData("Assets/Thing~/X.cs")]
        [InlineData("Library/X.cs")]
        [InlineData("Assets/Foo/obj/X.cs")]
        [InlineData("Assets/Foo/bin/X.cs")]
        [InlineData(".git/X.cs")]
        [InlineData("Temp/X.cs")]
        public void IsSkipped_SkipsProtectedFolders(string relativePath)
            => Assert.True(ScanRules.IsSkipped(relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar)));

        [Theory]
        [InlineData("Assets/Game/UI/Screen.cs")]
        [InlineData("Assets/ReactiveUIToolKitExtras/X.cs")]  // user folder, NOT the package
        [InlineData("Assets/RuitkExtras/X.cs")]
        public void IsSkipped_LeavesUserFoldersAlone(string relativePath)
            => Assert.False(ScanRules.IsSkipped(relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar)));

        [Theory]
        [InlineData("X.cs")]
        [InlineData("Screen.uitkx")]
        [InlineData("Game.asmdef")]
        [InlineData("Game.asmref")]   // M4
        [InlineData("csc.rsp")]       // M4
        [InlineData("mcs.rsp")]       // M4
        public void IsScannedExtension_CoversEveryBrandCarryingFileKind(string name)
            => Assert.True(ScanRules.IsScannedExtension(name));

        [Theory]
        [InlineData("settings.json")]  // holds the frozen formatter id — deliberately not scanned
        [InlineData("Scene.unity")]
        [InlineData("X.cs.meta")]
        [InlineData("logo.png")]
        public void IsScannedExtension_ExcludesEverythingElse(string name)
            => Assert.False(ScanRules.IsScannedExtension(name));

        // ── FileEncodings: BOM round-trip + invalid-UTF-8 refusal (M3) ─────────────

        [Fact]
        public void Utf8Bom_IsDetected_AndWrittenBackIdentically()
        {
            const string text = "using ReactiveUITK.Core;\r\n";
            byte[] withBom = Concat(new byte[] { 0xEF, 0xBB, 0xBF }, Encoding.UTF8.GetBytes(text));

            Assert.True(FileEncodings.TryDecode(withBom, out string? decoded, out var bom));
            Assert.Equal(FileEncodings.BomKind.Utf8Bom, bom);
            Assert.Equal(text, decoded);

            // Unchanged text must re-encode to the ORIGINAL bytes — a codemod must not turn a
            // BOM'd file into a whole-file diff.
            Assert.Equal(withBom, FileEncodings.Encode(decoded!, bom));

            // And the migrated text keeps its BOM.
            byte[] migrated = FileEncodings.Encode(BrandMigrator.Migrate(decoded!, out _), bom);
            Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, migrated[..3]);
            Assert.Equal("using Ruitk.Core;\r\n", Encoding.UTF8.GetString(migrated, 3, migrated.Length - 3));
        }

        [Fact]
        public void Utf8WithoutBom_RoundTripsWithoutGainingOne()
        {
            const string text = "using ReactiveUITK.Core;\n";
            byte[] raw = Encoding.UTF8.GetBytes(text);

            Assert.True(FileEncodings.TryDecode(raw, out string? decoded, out var bom));
            Assert.Equal(FileEncodings.BomKind.Utf8NoBom, bom);
            Assert.Equal(raw, FileEncodings.Encode(decoded!, bom));
        }

        [Fact]
        public void Utf16_RoundTripsBothEndiannesses()
        {
            const string text = "using ReactiveUITK.Core;\n";

            byte[] le = Concat(new byte[] { 0xFF, 0xFE }, new UnicodeEncoding(false, false).GetBytes(text));
            Assert.True(FileEncodings.TryDecode(le, out string? dle, out var ble));
            Assert.Equal(FileEncodings.BomKind.Utf16Le, ble);
            Assert.Equal(text, dle);
            Assert.Equal(le, FileEncodings.Encode(dle!, ble));

            byte[] be = Concat(new byte[] { 0xFE, 0xFF }, new UnicodeEncoding(true, false).GetBytes(text));
            Assert.True(FileEncodings.TryDecode(be, out string? dbe, out var bbe));
            Assert.Equal(FileEncodings.BomKind.Utf16Be, bbe);
            Assert.Equal(text, dbe);
            Assert.Equal(be, FileEncodings.Encode(dbe!, bbe));
        }

        [Fact]
        public void NonUtf8Bytes_AreRefused_NotSilentlyMojibaked()
        {
            // Shift-JIS "日本語" — invalid as UTF-8. Decoding it with a replacing fallback would
            // turn every byte into U+FFFD and the write-back would destroy the user's text, with
            // no backup (M3 Failure B). The contract is: refuse, so the caller can skip + warn.
            byte[] shiftJis = { 0x93, 0xFA, 0x96, 0x7B, 0x8C, 0xEA };
            byte[] file = Concat(Encoding.ASCII.GetBytes("// "), Concat(shiftJis, Encoding.ASCII.GetBytes("\n")));

            Assert.False(FileEncodings.TryDecode(file, out string? decoded, out _));
            Assert.Null(decoded);
        }

        [Fact]
        public void PlainAsciiIsAlwaysDecodable()
        {
            byte[] file = Encoding.ASCII.GetBytes("class Foo { }\n");
            Assert.True(FileEncodings.TryDecode(file, out string? decoded, out var bom));
            Assert.Equal(FileEncodings.BomKind.Utf8NoBom, bom);
            Assert.Equal("class Foo { }\n", decoded);
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            var r = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, r, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, r, a.Length, b.Length);
            return r;
        }
    }
}
