using DomainModel.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DataAccessLibrary.Settings;

public class DatabaseLibrarySettings : ILibrarySettings
{
    private readonly IConfiguration _configuration;

    public DatabaseLibrarySettings(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Citeste valorile din sectiunea "LibrarySettings" a fisierului appsettings.json
    public int DOMENII => int.Parse(_configuration["LibrarySettings:DOMENII"] ?? "3");
    public int NMC => int.Parse(_configuration["LibrarySettings:NMC"] ?? "5");
    public int PER => int.Parse(_configuration["LibrarySettings:PER"] ?? "30");
    public int C => int.Parse(_configuration["LibrarySettings:C"] ?? "3");
    public int D => int.Parse(_configuration["LibrarySettings:D"] ?? "2");
    public int L => int.Parse(_configuration["LibrarySettings:L"] ?? "12");
    public int LIM => int.Parse(_configuration["LibrarySettings:LIM"] ?? "30");
    public int DELTA => int.Parse(_configuration["LibrarySettings:DELTA"] ?? "14");
    public int NCZ => int.Parse(_configuration["LibrarySettings:NCZ"] ?? "2");
    public int PERSIMP => int.Parse(_configuration["LibrarySettings:PERSIMP"] ?? "10");
}