namespace DomainModel.Interfaces;

/// <summary>
/// Interfata pentru gestionarea pragurilor si limitelor bibliotecii.
/// Permite modificarea valorilor fara recompilarea aplicatiei.
/// </summary>
public interface ILibrarySettings
{
    int DOMENII { get; }    // Max domenii per carte
    int NMC { get; }        // Max carti intr-o perioada PER
    int PER { get; }        // Perioada de timp (zile/luni) pentru NMC
    int C { get; }          // Max carti la un singur imprumut
    int D { get; }          // Max carti dintr-un domeniu in ultimele L luni
    int L { get; }          // Numarul de luni pentru limita D
    int LIM { get; }        // Valoarea limita pentru suma prelungirilor (zile)
    int DELTA { get; }      // Interval minim intre imprumuturi pentru aceeasi carte
    int NCZ { get; }        // Max carti pe zi pentru cititori obisnuiti
    int PERSIMP { get; }    // Max carti pe zi pentru personalul bibliotecii
}