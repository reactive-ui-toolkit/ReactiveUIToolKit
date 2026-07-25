using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ReactiveUITK.Runtime")]
[assembly: InternalsVisibleTo("ReactiveUITK.Ugui")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("ReactiveUITK.Editor")]
#endif
