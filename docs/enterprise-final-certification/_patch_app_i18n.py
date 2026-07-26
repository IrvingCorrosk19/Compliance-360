from pathlib import Path

path = Path(r"c:\Proyectos\Compliance 360\src\Compliance360.Web\wwwroot\app.js")
text = path.read_text(encoding="utf-8")
repls = [
    ('"Identificando..."', 't("Common.Identifying")'),
    ('"Validando..."', 't("Common.Validating")'),
    ('"Verificando..."', 't("Common.Verifying")'),
    ('"Guardando..."', 't("Common.Saving")'),
    ('"Generando..."', 't("Common.Generating")'),
    ('"Programando..."', 't("Common.Scheduling")'),
    ('"Creando..."', 't("Common.Creating")'),
    ('"Actualizando..."', 't("Common.Updating")'),
    ('"Procesando..."', 't("Common.Processing")'),
    ('"Restableciendo..."', 't("Common.Resetting")'),
    ('"Completando..."', 't("Common.Completing")'),
    ('"Storage provider test ejecutado"', 't("Dashboard.StorageProviderTestEjecutado")'),
    ('"Storage provider creado"', 't("Dashboard.StorageProviderCreado")'),
    ('"Email provider creado"', 't("Users.EmailProviderCreado")'),
    ('"Reportes estandar creados o verificados."', 't("Dashboard.ReportsEstandarCreadosOVerificados")'),
    ('"Reporte programado correctamente."', 't("Dashboard.ReporteProgramadoCorrectamente")'),
    ('"Accion ejecutada y auditada."', 't("Dashboard.AccionEjecutadaYAuditada")'),
]
total = 0
for old, new in repls:
    n = text.count(old)
    if n:
        text = text.replace(old, new)
        total += n
        print(f"{n}x {old}")
path.write_text(text, encoding="utf-8")
print("total", total)
