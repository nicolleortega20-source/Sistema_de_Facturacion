using System.ComponentModel;
using System.Globalization;
using SistemaFacturacion.Dominio;
using SistemaFacturacion.Persistencia;
using SistemaFacturacion.Servicios;
namespace SistemaFacturacion.Presentacion;
public partial class FormPrincipal : Form
{
    private readonly ServicioIdiomas _idiomas = new();
    private readonly ProcesadorLotes _procesador = new();
    private readonly ConfiguracionJson _configuracionJson = new();
    private readonly ExportadorCsv _exportador = new();
    private readonly RepositorioEnvios _repositorio;
    private ConfiguracionTarifas? _tarifas;
    private ResultadoLote? _resultado;
    private bool _guardado;
    public FormPrincipal()
    {
        InitializeComponent();
        botonesMenu = new Button[] { btnMenuProcesar, btnMenuAuditoria, btnMenuConfiguracion, btnMenuReportes };
        vistas = new Panel[] { panelAuditoria, panelConfiguracion, panelReportes };
        btnMenuProcesar.Click += (_, _) => Navegar(0);
        btnMenuAuditoria.Click += (_, _) => Navegar(1);
        btnMenuConfiguracion.Click += (_, _) => Navegar(2);
        btnMenuReportes.Click += (_, _) => Navegar(3);
        btnActualizarHistorial.Click += (_, _) => CargarHistorial();
        dgvHistorial.SelectionChanged += (_, _) => MostrarDetalleHistorial();
        btnRecargarConfig.Click += (_, _) => RecargarConfiguracion();
        btnExportarLote.Click += btnExportar_Click;
        btnExportarHistorial.Click += (_, _) => ExportarHistorial();
        string carpetaDatos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SistemaFacturacion");
        Directory.CreateDirectory(carpetaDatos);
        _repositorio = new RepositorioEnvios(Path.Combine(carpetaDatos, "envios.db"));
        cmbIdioma.SelectedIndex = 0;
        txtEntrada.Text = "PACK-001; Juan Perez; Zona A; 12.5; 150.0\r\nPACK-002; Maria Lopez; Zona C; -5.0; 200.0\r\nPACK-003; Pedro Gomez; Zona B; ABC; 100.0\r\nPACK-004; Ana Ruiz; Zona C; 23.5; 1500.0";
        try
        {
            _tarifas = _configuracionJson.Cargar(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
            lblConfiguracion.Text = T("Configuración cargada correctamente", "Configuration loaded successfully");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException)
        {
            lblConfiguracion.Text = T("Error de configuración", "Configuration error");
            MessageBox.Show(ex.Message, T("Configuración", "Configuration"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        try { _repositorio.Inicializar(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, T("Error de base de datos", "Database error")); }
        ActualizarIdioma();
    }
    private string T(string es, string en) => _idiomas.Texto(es, en);
    private void cmbIdioma_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _idiomas.Idioma = Math.Max(0, cmbIdioma.SelectedIndex);
        ActualizarIdioma();
    }
    private void ActualizarIdioma()
    {
        Text = T("Sistema de Facturación", "Billing System");
        lblTitulo.Text = T("Procesamiento de Facturación", "Billing Processing");
        lblIndicadorProcesados.Text = T("Procesados", "Processed");
        lblIndicadorValidos.Text = T("Válidos", "Valid");
        lblIndicadorErrores.Text = T("Con error", "With errors");
        lblIndicadorTotal.Text = T("Total facturado", "Total billed");
        lblIdioma.Text = T("Idioma:", "Language:");
        lblEntrada.Text = T("Ingreso de lote de paquetes", "Shipment batch entry");
        lblTabla.Text = T("Resultados del procesamiento", "Processing results");
        lblDetalleTitulo.Text = T("Detalle del paquete", "Shipment details");
        lblErrores.Text = T("Registro de errores", "Error log");
        btnProcesar.Text = T("Procesar lote", "Process batch");
        btnLimpiar.Text = T("Limpiar pantalla", "Clear screen");
        btnSalir.Text = T("Salir", "Exit");
        btnGuardar.Text = T("Guardar SQLite", "Save SQLite");
        btnExportar.Text = T("Exportar CSV", "Export CSV");
        lblConfiguracion.Text = _tarifas is null ? T("Error de configuración", "Configuration error") : T("Configuración cargada correctamente", "Configuration loaded successfully");
        ActualizarTabla();
        ActualizarResumen();
        ActualizarErrores();
        if (dgvPaquetes.CurrentRow?.DataBoundItem is Paquete paquete) MostrarDetalle(paquete);
        if (botonesMenu is not null)
        {
            string[] menu = { T("Procesar lotes", "Process batches"), T("Auditoría de envíos", "Shipment audit"),
                T("Configuración JSON", "JSON settings"), T("Reportes CSV", "CSV reports") };
            for (int i = 0; i < botonesMenu.Length; i++) botonesMenu[i].Text = "▸  " + menu[i];
            lblHistorial.Text = T("Historial y auditoría de envíos", "Shipment history and audit");
            lblConfigVista.Text = T("Configuración de tarifas", "Rate configuration");
            lblReportesVista.Text = T("Reportes y exportación CSV", "CSV reports and export");
            btnActualizarHistorial.Text = T("Actualizar", "Refresh");
            btnRecargarConfig.Text = T("Recargar JSON", "Reload JSON");
            btnExportarLote.Text = T("Exportar lote actual", "Export current batch");
            btnExportarHistorial.Text = T("Exportar historial", "Export history");
            lblReporteInfo.Text = T("El lote actual se exporta desde memoria; el historial se obtiene de SQLite.",
                "The current batch is exported from memory; history is read from SQLite.");
            MostrarConfiguracion();
            if (dgvHistorial.DataSource is not null) CargarHistorial();
        }
    }
    private void btnProcesar_Click(object? sender, EventArgs e)
    {
        if (_tarifas is null) { MessageBox.Show(T("Falta una configuración JSON válida.", "A valid JSON configuration is missing.")); return; }
        if (string.IsNullOrWhiteSpace(txtEntrada.Text)) { MessageBox.Show(T("Ingrese un lote.", "Enter a batch.")); return; }
        try
        {
            _resultado = _procesador.Procesar(txtEntrada.Text, _tarifas);
            _guardado = false;
            ActualizarErrores();
            ActualizarTabla(); ActualizarResumen();
            txtDetalle.Clear();
            if (_resultado.Paquetes.Length > 0) MostrarDetalle(_resultado.Paquetes[0]);
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException or InvalidOperationException)
        { MessageBox.Show(ex.Message, T("Error de procesamiento", "Processing error")); }
    }
    private void ActualizarTabla()
    {
        dgvPaquetes.DataSource = null;
        if (_resultado is null) return;
        dgvPaquetes.DataSource = new BindingList<Paquete>(_resultado.Paquetes);
        string[] es = { "Código", "Cliente", "Zona", "Peso (kg)", "Valor declarado", "Subtotal", "Recargo", "Penalización", "Seguro", "Costo fijo", "IVA", "Total" };
        for (int i = 0; i < dgvPaquetes.Columns.Count && i < es.Length; i++)
        {
            dgvPaquetes.Columns[i].HeaderText = T(es[i], es[i]);
            if (i >= 4) dgvPaquetes.Columns[i].DefaultCellStyle.Format = "N2";
        }
    }
    private void ActualizarResumen()
    {
        int validos = _resultado?.Paquetes.Length ?? 0;
        int errores = _resultado?.Errores.Length ?? 0;
        lblContadorProcesados.Text = (validos + errores).ToString();
        lblContadorValidos.Text = validos.ToString();
        lblContadorErrores.Text = errores.ToString();
        lblContadorTotal.Text = (_resultado?.TotalLote ?? 0m).ToString("C2");
        lblResumen.Text = _resultado is null ? T("Sin procesar", "Not processed") :
            $"{T("Válidos", "Valid")}: {_resultado.Paquetes.Length} | {T("Errores", "Errors")}: {_resultado.Errores.Length} | {T("Total del lote", "Batch total")}: {_resultado.TotalLote:C2}";
    }
    private void ActualizarErrores()
    {
        txtErrores.Lines = _resultado is null ? Array.Empty<string>() : Array.ConvertAll(_resultado.Errores, _idiomas.Error);
    }
    private void MostrarDetalle(Paquete p)
    {
        txtDetalle.Text = $"{T("Código", "Code")}: {p.Codigo}\r\n{T("Cliente", "Customer")}: {p.Cliente}\r\n{T("Zona", "Zone")}: {p.Zona}\r\n" +
            $"{T("Peso", "Weight")}: {p.PesoKg} kg\r\n{T("Valor declarado", "Declared value")}: {p.ValorDeclarado:C2}\r\n\r\n" +
            $"{T("Subtotal peso", "Weight subtotal")}: {p.SubtotalPeso:C2}\r\n{T("Recargo zona", "Zone surcharge")}: {p.RecargoZona:C2}\r\n" +
            $"{T("Penalización", "Weight fee")}: {p.PenalizacionPeso:C2}\r\n{T("Seguro", "Insurance")}: {p.Seguro:C2}\r\n" +
            $"{T("Costo fijo", "Fixed fee")}: {p.CostoFijo:C2}\r\n{T("IVA", "VAT")}: {p.Iva:C2}\r\n" +
            $"{T("TOTAL", "TOTAL")}: {p.Total:C2}";
    }
    private void dgvPaquetes_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && dgvPaquetes.Rows[e.RowIndex].DataBoundItem is Paquete p) MostrarDetalle(p);
    }
    private void btnLimpiar_Click(object? sender, EventArgs e)
    {
        txtEntrada.Clear(); txtErrores.Clear(); txtDetalle.Clear(); _resultado = null; _guardado = false;
        ActualizarTabla(); ActualizarResumen();
    }
    private void btnSalir_Click(object? sender, EventArgs e)
    {
        var respuesta = MessageBox.Show(
            T("¿Deseas salir del sistema?", "Do you want to exit the application?"),
            T("Confirmar salida", "Confirm exit"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (respuesta == DialogResult.Yes) Close();
    }
    private void btnExportar_Click(object? sender, EventArgs e)
    {
        if (_resultado is null || _resultado.Paquetes.Length == 0) { MessageBox.Show(T("No hay datos válidos.", "No valid data.")); return; }
        using SaveFileDialog dialogo = new() { Filter = "CSV (*.csv)|*.csv", FileName = "auditoria_envios.csv", DefaultExt = "csv" };
        if (dialogo.ShowDialog() != DialogResult.OK) return;
        try
        {
            _exportador.Exportar(dialogo.FileName, _resultado.Paquetes);
            MessageBox.Show(T("CSV exportado correctamente.", "CSV exported successfully."));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        { MessageBox.Show(ex.Message, T("Error de exportación", "Export error")); }
    }
    private void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (_resultado is null || _resultado.Paquetes.Length == 0) { MessageBox.Show(T("No hay datos válidos.", "No valid data.")); return; }
        if (_guardado) { MessageBox.Show(T("Este lote ya se guardó.", "This batch has already been saved.")); return; }
        try
        {
            int cantidad = _repositorio.GuardarLote(_resultado.Paquetes);
            _guardado = true;
            MessageBox.Show($"{cantidad} {T("paquetes guardados.", "shipments saved.")}");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, T("Error de base de datos", "Database error")); }
    }
    private void Navegar(int indice)
    {
        for (int i = 0; i < botonesMenu.Length; i++)
        {
            botonesMenu[i].BackColor = i == indice ? Morado : MoradoOscuro;
            botonesMenu[i].ForeColor = i == indice ? Color.White : Color.FromArgb(255, 226, 202);
        }
        panelProcesamiento.Visible = indice == 0;
        foreach (Panel vista in vistas) vista.Visible = false;
        if (indice == 0) return;
        Panel activa = vistas[indice - 1];
        activa.Visible = true;
        activa.BringToFront();
        if (indice == 1) CargarHistorial();
        if (indice == 2) MostrarConfiguracion();
    }
    private void CargarHistorial()
    {
        try
        {
            dgvHistorial.DataSource = null;
            dgvHistorial.DataSource = _repositorio.ConsultarHistorial();
            txtDetalleHistorial.Clear();
            if (dgvHistorial.Columns.Count > 0)
            {
                string[] titulos = { "Código", "Cliente", "Zona", "Peso (kg)", "Valor declarado", "Subtotal", "Recargo", "Penalización", "Seguro", "Costo fijo", "IVA", "Total" };
                for (int i = 0; i < dgvHistorial.Columns.Count && i < titulos.Length; i++)
                    dgvHistorial.Columns[i].HeaderText = T(titulos[i], titulos[i]);
            }
        }
        catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or IOException or UnauthorizedAccessException)
        { MessageBox.Show(ex.Message, T("Error de base de datos", "Database error")); }
    }
    private void MostrarDetalleHistorial()
    {
        if (dgvHistorial.CurrentRow?.DataBoundItem is not Paquete p) return;
        txtDetalleHistorial.Text = $"{T("Código", "Code")}: {p.Codigo}   |   {T("Cliente", "Customer")}: {p.Cliente}   |   {T("Zona", "Zone")}: {p.Zona}\r\n" +
            $"{T("Peso", "Weight")}: {p.PesoKg} kg   |   {T("Valor declarado", "Declared value")}: {p.ValorDeclarado:C2}\r\n" +
            $"{T("Subtotal", "Subtotal")}: {p.SubtotalPeso:C2}   |   {T("Recargo", "Surcharge")}: {p.RecargoZona:C2}   |   {T("Penalización", "Penalty")}: {p.PenalizacionPeso:C2}\r\n" +
            $"{T("Seguro", "Insurance")}: {p.Seguro:C2}   |   {T("Costo fijo", "Fixed fee")}: {p.CostoFijo:C2}   |   {T("IVA", "VAT")}: {p.Iva:C2}   |   {T("TOTAL", "TOTAL")}: {p.Total:C2}";
    }
    private void MostrarConfiguracion()
    {
        txtConfiguracion.Text = _tarifas is null ? T("Error de configuración", "Configuration error") :
            $"{T("Precio base por kg", "Base price per kg")}: {_tarifas.PrecioBaseKg:C2}\r\n" +
            $"{T("Tasa de IVA", "VAT rate")}: {_tarifas.TasaIva:P2}\r\n" +
            $"{T("Costo fijo", "Fixed fee")}: {_tarifas.CostoFijoEnvio:C2}\r\n\r\nappsettings.json";
    }
    private void RecargarConfiguracion()
    {
        try
        {
            _tarifas = _configuracionJson.Cargar(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
            lblConfiguracion.Text = T("Configuración cargada correctamente", "Configuration loaded successfully");
            MostrarConfiguracion();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException)
        { MessageBox.Show(ex.Message, T("Error de configuración", "Configuration error")); }
    }
    private void ExportarHistorial()
    {
        try
        {
            Paquete[] historial = _repositorio.ConsultarHistorial();
            if (historial.Length == 0) { MessageBox.Show(T("No hay datos válidos.", "No valid data.")); return; }
            using SaveFileDialog dialogo = new() { Filter = "CSV (*.csv)|*.csv", FileName = "historial_envios.csv", DefaultExt = "csv" };
            if (dialogo.ShowDialog() != DialogResult.OK) return;
            _exportador.Exportar(dialogo.FileName, historial);
            MessageBox.Show(T("CSV exportado correctamente.", "CSV exported successfully."));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or Microsoft.Data.Sqlite.SqliteException)
        { MessageBox.Show(ex.Message, T("Error de exportación", "Export error")); }
    }
}
