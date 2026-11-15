using System.Globalization;
namespace SistemaFacturacion;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("es-MX");
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es-MX");
        Application.Run(new Presentacion.FormPrincipal());
    }
}
