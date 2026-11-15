using System.Text.Json;
namespace SistemaFacturacion.Servicios;
public sealed class ServicioIdiomas
{
    private readonly Dictionary<string, string[]> _traducciones;
    public int Idioma { get; set; }
    public ServicioIdiomas()
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "Traducciones.json");
        _traducciones = File.Exists(ruta)
            ? JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(ruta)) ?? new()
            : new();
    }
    public string Texto(string espanol, string ingles = "")
    {
        if (Idioma == 0) return espanol;
        if (_traducciones.TryGetValue(espanol, out string[]? opciones) && Idioma - 1 < opciones.Length)
            return opciones[Idioma - 1];
        return Idioma == 1 && ingles.Length > 0 ? ingles : espanol;
    }
    public string Error(string mensaje)
    {
        int separador = mensaje.IndexOf(": ", StringComparison.Ordinal);
        if (mensaje.StartsWith("Línea ", StringComparison.Ordinal) && separador >= 0)
        {
            string numero = mensaje.Substring(6, separador - 6);
            return $"{Texto("Línea")} {numero}: {Texto(mensaje[(separador + 2)..])}";
        }
        return Texto(mensaje);
    }
}
