using NUnit.Framework;
using Ruitk.Core;
using Ruitk.Core.Config;
using Ruitk.Core.Diagnostics;

namespace Ruitk.Ugui.Tests
{
    /// <summary>
    /// EditMode tests for the JSON settings store (<see cref="RuitkSettings"/>) and the
    /// three-hop resolution chain (<see cref="BuildDefinesConfig"/>): parse-string cases
    /// (missing key = default, unknown key ignored, bad enum value = default, case
    /// insensitivity), the canonical-schema round-trip, the tri-state mapping, and the
    /// resolution-order proof seeding each hop — the settings-campaign functional smoke,
    /// pinned as a real test.
    /// </summary>
    public class RuitkSettingsJsonTests
    {
        /// <summary>The §3 canonical schema body — the writer must emit exactly this at defaults.</summary>
        private const string CanonicalDefaultJson =
            "{\n"
            + "  \"environment\": \"auto\",\n"
            + "  \"time_slicing\": true,\n"
            + "  \"time_slice_ms\": 2.0,\n"
            + "  \"frame_budget_ms\": 4.0,\n"
            + "  \"host_node_pool\": true,\n"
            + "  \"hook_validation\": \"auto\",\n"
            + "  \"strict_diagnostics\": \"auto\",\n"
            + "  \"strict_mode\": false,\n"
            + "  \"trace_level\": \"none\",\n"
            + "  \"diff_tracing\": false,\n"
            + "  \"diagnostics_output_folder\": \"\"\n"
            + "}\n";

        [SetUp]
        public void SetUp()
        {
            ResetStores();
        }

        [TearDown]
        public void TearDown()
        {
            ResetStores();
        }

        private static void ResetStores()
        {
            RuitkSettings.SetActive(null);
            RuitkSettings.SuppressResourceLoadForTests = false;
            RuitkSettings.Invalidate();
            RuitkConfig.SetCurrentForTests(null);
        }

        private static void AssertAllDefaults(RuitkSettings s)
        {
            Assert.AreEqual(RuitkEnvironment.Auto, s.environment);
            Assert.IsTrue(s.timeSlicing);
            Assert.AreEqual(2.0f, s.timeSliceMs);
            Assert.AreEqual(4.0f, s.frameBudgetMs);
            Assert.IsTrue(s.hostNodePool);
            Assert.AreEqual(RuitkTriState.Auto, s.hookValidation);
            Assert.AreEqual(RuitkTriState.Auto, s.strictDiagnostics);
            Assert.IsFalse(s.strictMode);
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.None, s.traceLevel);
            Assert.IsFalse(s.diffTracing);
            Assert.AreEqual("", s.diagnosticsOutputFolder);
        }

        // ── Parse cases ───────────────────────────────────────────────────────

        [Test]
        public void Parse_EmptyObject_YieldsAllDefaults()
        {
            AssertAllDefaults(RuitkSettings.Parse("{}"));
        }

        [Test]
        public void Parse_NullOrWhitespace_YieldsAllDefaults()
        {
            AssertAllDefaults(RuitkSettings.Parse(null));
            AssertAllDefaults(RuitkSettings.Parse(""));
            AssertAllDefaults(RuitkSettings.Parse("   \n"));
        }

        [Test]
        public void Parse_CanonicalDefaultBody_YieldsAllDefaults()
        {
            AssertAllDefaults(RuitkSettings.Parse(CanonicalDefaultJson));
        }

        [Test]
        public void CanonicalJson_OfDefaults_IsTheCanonicalSchemaByteForByte()
        {
            Assert.AreEqual(CanonicalDefaultJson, new RuitkSettings().ToCanonicalJson());
        }

        [Test]
        public void CanonicalJson_RoundTrips_NonDefaultValues()
        {
            var settings = new RuitkSettings
            {
                environment = RuitkEnvironment.Production,
                timeSlicing = false,
                timeSliceMs = 1.5f,
                frameBudgetMs = 8.0f,
                hostNodePool = false,
                hookValidation = RuitkTriState.On,
                strictDiagnostics = RuitkTriState.Off,
                strictMode = true,
                traceLevel = DiagnosticsConfig.TraceLevel.Verbose,
                diffTracing = true,
                diagnosticsOutputFolder = "Logs/Custom",
            };

            var parsed = RuitkSettings.Parse(settings.ToCanonicalJson());

            Assert.AreEqual(RuitkEnvironment.Production, parsed.environment);
            Assert.IsFalse(parsed.timeSlicing);
            Assert.AreEqual(1.5f, parsed.timeSliceMs);
            Assert.AreEqual(8.0f, parsed.frameBudgetMs);
            Assert.IsFalse(parsed.hostNodePool);
            Assert.AreEqual(RuitkTriState.On, parsed.hookValidation);
            Assert.AreEqual(RuitkTriState.Off, parsed.strictDiagnostics);
            Assert.IsTrue(parsed.strictMode);
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.Verbose, parsed.traceLevel);
            Assert.IsTrue(parsed.diffTracing);
            Assert.AreEqual("Logs/Custom", parsed.diagnosticsOutputFolder);
        }

        [Test]
        public void Parse_UnknownKey_IsIgnored()
        {
            var parsed = RuitkSettings.Parse(
                "{ \"environment\": \"production\", \"not_a_known_key\": 123 }"
            );
            Assert.AreEqual(RuitkEnvironment.Production, parsed.environment);
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.None, parsed.traceLevel);
        }

        [Test]
        public void Parse_MissingKeys_KeepDefaults()
        {
            var parsed = RuitkSettings.Parse("{ \"trace_level\": \"basic\" }");
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.Basic, parsed.traceLevel);
            // Everything else untouched by the partial document:
            Assert.AreEqual(RuitkEnvironment.Auto, parsed.environment);
            Assert.IsTrue(parsed.timeSlicing);
            Assert.AreEqual(2.0f, parsed.timeSliceMs);
            Assert.IsTrue(parsed.hostNodePool);
        }

        [Test]
        public void Parse_BadEnumValues_FallBackToDefaults()
        {
            // Editor-only warnings fire for each unknown value; LogAssert tolerates them.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            try
            {
                var parsed = RuitkSettings.Parse(
                    "{ \"environment\": \"staging\", \"trace_level\": \"chatty\", "
                        + "\"hook_validation\": \"maybe\", \"strict_diagnostics\": \"sometimes\" }"
                );
                Assert.AreEqual(RuitkEnvironment.Auto, parsed.environment);
                Assert.AreEqual(DiagnosticsConfig.TraceLevel.None, parsed.traceLevel);
                Assert.AreEqual(RuitkTriState.Auto, parsed.hookValidation);
                Assert.AreEqual(RuitkTriState.Auto, parsed.strictDiagnostics);
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void Parse_IsCaseInsensitive()
        {
            var parsed = RuitkSettings.Parse(
                "{ \"environment\": \"PRODUCTION\", \"trace_level\": \"Basic\", "
                    + "\"hook_validation\": \"On\", \"strict_diagnostics\": \"OFF\" }"
            );
            Assert.AreEqual(RuitkEnvironment.Production, parsed.environment);
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.Basic, parsed.traceLevel);
            Assert.AreEqual(RuitkTriState.On, parsed.hookValidation);
            Assert.AreEqual(RuitkTriState.Off, parsed.strictDiagnostics);
        }

        // ── Tri-state mapping (auto in editor context = on) ───────────────────

        [Test]
        public void MapTriState_Table()
        {
            Assert.IsTrue(BuildDefinesConfig.MapTriState(RuitkTriState.On));
            Assert.IsFalse(BuildDefinesConfig.MapTriState(RuitkTriState.Off));
            // These tests run inside the editor, so Auto maps to ON here.
            Assert.IsTrue(BuildDefinesConfig.MapTriState(RuitkTriState.Auto));
        }

        // ── Resolution order: JSON store → legacy config.json → defaults ──────

        [Test]
        public void ResolutionOrder_JsonStore_WinsOverEverything()
        {
            RuitkSettings.SuppressResourceLoadForTests = true;
            RuitkSettings.Invalidate();
            RuitkConfig.SetCurrentForTests(
                RuitkConfig.Parse(
                    "{ \"envVariables\": { \"env\": \"development\", \"traceLevel\": \"Basic\", "
                        + "\"diffTracing\": true } }"
                )
            );
            RuitkSettings.SetActive(
                RuitkSettings.Parse(
                    "{ \"environment\": \"production\", \"trace_level\": \"verbose\", "
                        + "\"diff_tracing\": false }"
                )
            );

            Assert.AreEqual("production", BuildDefinesConfig.ResolveEnvironment());
            Assert.AreEqual(
                DiagnosticsConfig.TraceLevel.Verbose,
                BuildDefinesConfig.ResolveTraceLevel()
            );
            Assert.IsFalse(BuildDefinesConfig.ResolveEnableDiffTracing());
        }

        [Test]
        public void ResolutionOrder_LegacyConfig_WhenNoJsonStore()
        {
            RuitkSettings.SetActive(null);
            RuitkSettings.SuppressResourceLoadForTests = true;
            RuitkSettings.Invalidate();
            RuitkConfig.SetCurrentForTests(
                RuitkConfig.Parse(
                    "{ \"envVariables\": { \"env\": \"development\", \"traceLevel\": \"Basic\", "
                        + "\"diffTracing\": true } }"
                )
            );

            Assert.AreEqual("development", BuildDefinesConfig.ResolveEnvironment());
            Assert.AreEqual(
                DiagnosticsConfig.TraceLevel.Basic,
                BuildDefinesConfig.ResolveTraceLevel()
            );
            Assert.IsTrue(BuildDefinesConfig.ResolveEnableDiffTracing());
        }

        [Test]
        public void ResolutionOrder_CompiledDefaults_WhenNeitherStoreExists()
        {
            RuitkSettings.SetActive(null);
            RuitkSettings.SuppressResourceLoadForTests = true;
            RuitkSettings.Invalidate();
            // An empty legacy document = the compiled defaults RuitkConfig carries.
            RuitkConfig.SetCurrentForTests(RuitkConfig.Parse("{}"));

            Assert.AreEqual("production", BuildDefinesConfig.ResolveEnvironment());
            Assert.AreEqual(
                DiagnosticsConfig.TraceLevel.None,
                BuildDefinesConfig.ResolveTraceLevel()
            );
            Assert.IsFalse(BuildDefinesConfig.ResolveEnableDiffTracing());
        }

        [Test]
        public void ResolutionOrder_LegacyParse_IsNeverThrowing()
        {
            var cfg = RuitkConfig.Parse("this is not json");
            Assert.AreEqual("production", cfg.EnvironmentLabel);
            Assert.AreEqual(DiagnosticsConfig.TraceLevel.None, cfg.TraceLevel);
            Assert.IsFalse(cfg.EnableDiffTracing);
        }
    }
}
