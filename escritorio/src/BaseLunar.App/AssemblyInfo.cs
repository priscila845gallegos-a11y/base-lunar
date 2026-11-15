using System.Runtime.CompilerServices;
using System.Windows;

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

// Las pruebas de pantalla completan las transiciones sin esperar el temporizador.
[assembly: InternalsVisibleTo("BaseLunar.PruebasUI")]
