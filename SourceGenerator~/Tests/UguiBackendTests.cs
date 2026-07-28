using System.Collections.Generic;
using Ruitk.Language;
using Ruitk.Language.Formatter;
using Ruitk.Language.Parser;
using Ruitk.SourceGenerator.Tests.Helpers;
using Xunit;

namespace Ruitk.SourceGenerator.Tests;

/// <summary>
/// Pins for the @backend directive and the ugui element vocabulary: parsing,
/// diagnostics, formatter round-trip, and SG emission through the U factory
/// surface. The test compilation has no Ruitk.Ugui assembly reference,
/// so ugui tag resolution exercises the ugui FALLBACK map — the same table
/// the first-load path uses in a real project.
/// </summary>
public class UguiBackendTests
{
    private const string TestPath = "C:/p/Assets/UI/Test.uitkx";

    private static string UguiFile(string markup) =>
        "@backend ugui\n\nexport VirtualNode MyComp() {\n  return (\n" + markup + "\n  );\n}";

    private static DirectiveSet Parse(string source, out List<ParseDiagnostic> diags)
    {
        diags = new List<ParseDiagnostic>();
        return DirectiveParser.Parse(source, TestPath, diags);
    }

    // ── Directive parsing ─────────────────────────────────────────────────────

    [Fact]
    public void BackendDirective_ParsesIntoDirectiveSet()
    {
        var ds = Parse(UguiFile("<Panel/>"), out _);
        Assert.Equal("ugui", ds.Backend);
    }

    [Fact]
    public void BackendUitk_NormalizesToNull()
    {
        var ds = Parse(
            "@backend uitk\n\nexport VirtualNode MyComp() {\n  return ( <Label/> );\n}",
            out _
        );
        Assert.Null(ds.Backend);
    }

    [Fact]
    public void NoBackendDirective_DefaultsToNull()
    {
        var ds = Parse("export VirtualNode MyComp() {\n  return ( <Label/> );\n}", out _);
        Assert.Null(ds.Backend);
    }

    [Fact]
    public void UnknownBackendValue_Raises2111_AndStillParses()
    {
        var ds = Parse(
            "@backend imgui\n\nexport VirtualNode MyComp() {\n  return ( <Label/> );\n}",
            out var diags
        );
        Assert.Contains(diags, d => d.Code == "UITKX2111");
        Assert.Null(ds.Backend);
        Assert.NotEmpty(ds.ComponentDeclarations);
    }

    [Fact]
    public void DuplicateBackend_Raises2111_FirstWins()
    {
        var ds = Parse(
            "@backend ugui\n@backend uitk\n\nexport VirtualNode MyComp() {\n  return ( <Panel/> );\n}",
            out var diags
        );
        Assert.Contains(diags, d => d.Code == "UITKX2111");
        Assert.Equal("ugui", ds.Backend);
    }

    [Fact]
    public void UssInUguiFile_Raises2112Warning()
    {
        Parse(
            "@backend ugui\n@uss \"./styles.uss\"\n\nexport VirtualNode MyComp() {\n  return ( <Panel/> );\n}",
            out var diags
        );
        Assert.Contains(diags, d => d.Code == "UITKX2112");
    }

    // ── SG emission ──────────────────────────────────────────────────────────

    [Fact]
    public void UguiElement_EmitsThroughUFactory()
    {
        var result = GeneratorTestHelper.Run(UguiFile("<Image/>"));

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("global::Ruitk.Ugui.U.Image("),
            "Expected a U.Image( call for the ugui vocabulary"
        );
    }

    [Fact]
    public void UguiElement_RentsFromUguiPropsPool()
    {
        var result = GeneratorTestHelper.Run(UguiFile("<Image preserveAspect={true}/>"));

        Assert.True(
            result.SourceContains(
                "global::Ruitk.Ugui.UguiBaseProps.__Rent<global::Ruitk.Ugui.UguiImageProps>"
            ),
            "Expected ugui family pool rent"
        );
    }

    [Fact]
    public void UguiNestedLayout_EmitsUguiFactories()
    {
        var result = GeneratorTestHelper.Run(
            UguiFile("<VerticalLayoutGroup spacing={8f}><Text text=\"hi\"/></VerticalLayoutGroup>")
        );

        Assert.True(
            result.SourceContains("global::Ruitk.Ugui.U.VerticalLayoutGroup("),
            "Expected U.VerticalLayoutGroup"
        );
        Assert.True(
            result.SourceContains("global::Ruitk.Ugui.U.Text("),
            "Expected typed U.Text for the <Text> tag"
        );
    }

    [Fact]
    public void UitkFile_EmissionUnchanged_ByBackendMachinery()
    {
        var result = GeneratorTestHelper.Run(
            "export VirtualNode MyComp() {\n  return ( <Label text=\"x\"/> );\n}"
        );

        Assert.True(result.SourceContains("V.Label("), "UITK files must keep bare V.* emission");
        Assert.False(result.SourceContains("Ruitk.Ugui"), "No ugui leakage into UITK files");
    }

    // ── Vocabulary contract: every ugui tag resolves to its U factory ────────

    [Fact]
    public void UguiVocabulary_EveryTag_EmitsItsUFactory()
    {
        var vocabulary = new (string tag, string method)[]
        {
            ("Canvas", "Canvas"),
            ("Panel", "Panel"),
            ("Image", "Image"),
            ("RawImage", "RawImage"),
            ("Text", "Text"),
            ("Button", "Button"),
            ("HorizontalLayoutGroup", "HorizontalLayoutGroup"),
            ("VerticalLayoutGroup", "VerticalLayoutGroup"),
            ("GridLayoutGroup", "GridLayoutGroup"),
            ("Toggle", "Toggle"),
            ("ToggleGroup", "ToggleGroup"),
            ("Slider", "Slider"),
            ("Scrollbar", "Scrollbar"),
            ("ScrollRect", "ScrollRect"),
            ("Dropdown", "Dropdown"),
            ("InputField", "InputField"),
            ("Prefab", "Prefab"),
            ("UitkHost", "UitkHost"),
        };

        foreach (var (tag, method) in vocabulary)
        {
            var result = GeneratorTestHelper.Run(UguiFile($"<{tag}/>"));
            Assert.True(
                result.SourceContains($"global::Ruitk.Ugui.U.{method}("),
                $"Tag <{tag}> did not emit U.{method}("
            );
        }
    }

    // ── Cross-backend imports (UITKX2113) ────────────────────────────────────

    [Fact]
    public void UguiFile_ImportingUitkComponent_Raises2113()
    {
        var result = GeneratorTestHelper.RunMultiple(
            new[]
            {
                (
                    "Card.uitkx",
                    "export VirtualNode Card() {\n  return ( <Label text=\"x\"/> );\n}\n"
                ),
                (
                    "Screen.uitkx",
                    "@backend ugui\nimport { Card } from \"./Card\"\n"
                        + "export VirtualNode Screen() {\n  return ( <Panel><Card/></Panel> );\n}\n"
                ),
            },
            "Screen.uitkx"
        );

        Assert.True(
            result.HasDiagnostic("UITKX2113"),
            "Expected UITKX2113 for a ugui file importing a uitk peer"
        );
    }

    [Fact]
    public void SameBackendImport_NoCrossBackendDiagnostic()
    {
        var result = GeneratorTestHelper.RunMultiple(
            new[]
            {
                (
                    "Card.uitkx",
                    "@backend ugui\nexport VirtualNode Card() {\n  return ( <Text text=\"x\"/> );\n}\n"
                ),
                (
                    "Screen.uitkx",
                    "@backend ugui\nimport { Card } from \"./Card\"\n"
                        + "export VirtualNode Screen() {\n  return ( <Panel><Card/></Panel> );\n}\n"
                ),
            },
            "Screen.uitkx"
        );

        Assert.False(result.HasDiagnostic("UITKX2113"), "Same-backend import must not flag");
    }

    // ── Formatter round-trip ─────────────────────────────────────────────────

    [Fact]
    public void Formatter_PreservesBackendDirective()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var formatted = formatter.Format(UguiFile("<Panel/>"), "Test.uitkx");

        Assert.Contains("@backend ugui", formatted);
        Assert.True(
            formatted.IndexOf("@backend ugui", System.StringComparison.Ordinal)
                < formatted.IndexOf("export", System.StringComparison.Ordinal),
            "@backend must stay in the preamble, before declarations"
        );
    }
}
