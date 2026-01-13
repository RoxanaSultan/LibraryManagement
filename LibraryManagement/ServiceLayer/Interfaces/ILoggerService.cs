namespace ServiceLayer.Interfaces;

/// <summary>
/// Interfata pentru serviciul de logging al aplicatiei.
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// Inregistreaza un mesaj de informare.
    /// </summary>
    /// <param name="message">Mesajul de logat.</param>
    void LogInformation(string message);

    /// <summary>
    /// Inregistreaza o avertizare legata de o regula de business incalcata.
    /// </summary>
    /// <param name="message">Mesajul de avertizare.</param>
    void LogWarning(string message);

    /// <summary>
    /// Inregistreaza o eroare critica sau o exceptie.
    /// </summary>
    /// <param name="message">Mesajul de eroare.</param>
    /// <param name="exception">Exceptia capturata.</param>
    void LogError(string message, System.Exception exception = null);
}