from pathlib import Path
import json

path = Path(r"c:\Proyectos\Compliance 360\src\Compliance360.Web\wwwroot\alert-center.js")
text = path.read_text(encoding="utf-8")

repls = [
    (
        '${item.state === "Archived" ? "Restaurar" : "Archivar"}',
        '${item.state === "Archived" ? escapeHtml(tr("AlertCenter.Restore", "Restaurar")) : escapeHtml(tr("AlertCenter.Archive", "Archivar"))}',
    ),
    (
        'aria-label="${item.isFavorite ? "Quitar favorito" : "Marcar favorito"}"',
        'aria-label="${item.isFavorite ? tr("AlertCenter.UnmarkFavorite", "Quitar favorito") : tr("AlertCenter.MarkFavorite", "Marcar favorito")}"',
    ),
    (
        '<button id="alert-open-recipients" class="btn" type="button">Destinatarios</button>',
        '<button id="alert-open-recipients" class="btn" type="button">${escapeHtml(tr("AlertCenter.Recipients", "Destinatarios"))}</button>',
    ),
    (
        '<button id="alert-refresh" class="btn" type="button">Actualizar</button>',
        '<button id="alert-refresh" class="btn" type="button">${escapeHtml(tr("AlertCenter.Refresh", "Actualizar"))}</button>',
    ),
    (
        '<button id="alert-mark-selected" class="btn" type="button" ${view.selected.size ? "" : "disabled"}>Marcar selección leída</button>',
        '<button id="alert-mark-selected" class="btn" type="button" ${view.selected.size ? "" : "disabled"}>${escapeHtml(tr("AlertCenter.MarkSelectionRead", "Marcar selección leída"))}</button>',
    ),
    (
        '<div class="empty-state"><h2>Sin notificaciones</h2><p>No hay elementos que coincidan con los filtros seleccionados.</p></div>',
        '<div class="empty-state"><h2>${escapeHtml(tr("AlertCenter.EmptyTitle", "Sin notificaciones"))}</h2><p>${escapeHtml(tr("AlertCenter.EmptyBody", "No hay elementos que coincidan con los filtros seleccionados."))}</p></div>',
    ),
    (
        'placeholder="Asunto o contenido"',
        'placeholder="${escapeHtml(tr("AlertCenter.SearchPlaceholder", "Asunto o contenido"))}"',
    ),
    (
        "<label>Buscar<input",
        '<label>${escapeHtml(tr("AlertCenter.Search", "Buscar"))}<input',
    ),
    (
        '<article class="metric-card"><span>Favoritas</span>',
        '<article class="metric-card"><span>${escapeHtml(tr("AlertCenter.FavoritesMetric", "Favoritas"))}</span>',
    ),
    (
        "<p>Cargando notificaciones persistentes…</p>",
        '<p>${escapeHtml(tr("AlertCenter.LoadingInbox", "Cargando notificaciones persistentes…"))}</p>',
    ),
    (
        "<p>Alertas regulatorias persistentes, aisladas por tenant y usuario.</p>",
        '<p>${escapeHtml(tr("AlertCenter.InboxSubtitle", "Alertas regulatorias persistentes, aisladas por tenant y usuario."))}</p>',
    ),
]

total = 0
for old, new in repls:
    c = text.count(old)
    if c:
        text = text.replace(old, new)
        total += c
        print(f"{c}x OK: {old[:70]}")
    else:
        print(f"MISS: {old[:90]}")

path.write_text(text, encoding="utf-8")
print("total", total)

en_path = Path(r"c:\Proyectos\Compliance 360\src\Compliance360.Web\wwwroot\locales\en.json")
es_path = Path(r"c:\Proyectos\Compliance 360\src\Compliance360.Web\wwwroot\locales\es.json")
en = json.loads(en_path.read_text(encoding="utf-8"))
es = json.loads(es_path.read_text(encoding="utf-8"))
en.update(
    {
        "AlertCenter.FavoritesMetric": "Favorites",
        "AlertCenter.LoadingInbox": "Loading persistent notifications…",
        "AlertCenter.InboxSubtitle": "Persistent regulatory alerts, isolated by tenant and user.",
        "Common.AutosaveListo": "Autosave ready",
        "Dashboard.UseLetrasNumerosGuionesOPuntos": "Use letters, numbers, hyphens or periods.",
    }
)
es.update(
    {
        "AlertCenter.FavoritesMetric": "Favoritas",
        "AlertCenter.LoadingInbox": "Cargando notificaciones persistentes…",
        "AlertCenter.InboxSubtitle": "Alertas regulatorias persistentes, aisladas por tenant y usuario.",
    }
)
en_path.write_text(json.dumps(en, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
es_path.write_text(json.dumps(es, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print("parity", len(en), len(es), sorted(set(en) ^ set(es))[:10])
