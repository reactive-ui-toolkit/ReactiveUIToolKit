using System.Runtime.CompilerServices;

// Allow the sibling test project to access internal types (parsers, emitter, validators)
[assembly: InternalsVisibleTo("Ruitk.SourceGenerator.Tests")]
