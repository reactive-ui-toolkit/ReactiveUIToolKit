using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Ruitk.Runtime")]
[assembly: InternalsVisibleTo("Ruitk.Ugui")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Ruitk.Editor")]
#endif
