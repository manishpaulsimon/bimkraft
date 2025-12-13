# Enhanced Warnings Browser für Revit

Ein PyRevit-Tool zur erweiterten Anzeige und Navigation von Revit-Warnungen mit verbesserter Funktionalität.

## Features

### 🔍 **Erweiterte Warningenanzeige**
- **Zusätzliche Spalten**: Element IDs, Elementnamen, Ebenen, Ansichten, Kategorien
- **Bessere Übersicht**: Alle wichtigen Informationen in einer Tabelle
- **Sortier- und Filtermöglichkeiten**: Durch WPF DataGrid

### 🎯 **Intelligentes Element-Highlighting**
- **Auto-Highlight**: Automatische rote Markierung bei Auswahl von Warnungen
- **Manuelle Kontrolle**: Option zum Ein-/Ausschalten des Auto-Highlights
- **Zoom-Funktion**: Direkte Navigation zu problematischen Elementen
- **Mehrere Elemente**: Gleichzeitige Markierung aller betroffenen Elemente

### 📋 **Verbesserte Exportfunktion**
- **HTML-Export**: Umfassender Report mit allen Spalten
- **Strukturierte Darstellung**: Professionelle HTML-Tabelle
- **ICL-Branding**: Firmenspezifische Formatierung

### 💻 **Benutzerfreundlichkeit**
- **Non-Modal Dialog**: Arbeiten Sie parallel in Revit weiter
- **Bewegbar**: Fenster auf zweiten Bildschirm verschiebbar
- **Keine Transaction-Blockierung**: Nahtloser Workflow

## Installation

### Voraussetzungen
- Autodesk Revit 2019 oder neuer
- PyRevit installiert ([Download hier](https://github.com/eirannejad/pyRevit))

### Installation Steps

1. **PyRevit Extension erstellen**:
   ```
   Erstellen Sie einen neuen Ordner in Ihrem PyRevit Extensions Verzeichnis:
   %APPDATA%\pyRevit\Extensions\ICL.extension\ICL.tab\Warnings.panel\Enhanced Warnings.pushbutton\
   ```

2. **Dateien kopieren**:
   - `script.py` → in den pushbutton Ordner
   - `__init__.py` → in den pushbutton Ordner

3. **PyRevit neu laden**:
   - In Revit: PyRevit Tab → Settings → Reload PyRevit

### Ordnerstruktur
```
ICL.extension/
├── ICL.tab/
│   └── Warnings.panel/
│       └── Enhanced Warnings.pushbutton/
│           ├── script.py
│           └── __init__.py
```

## Verwendung

### 1. **Tool starten**
- Klicken Sie auf "Enhanced Warnings" in der ICL-Tab
- Das Fenster öffnet sich non-modal

### 2. **Warnings durchsehen**
- Alle Warnungen werden mit erweiterten Informationen angezeigt
- Nutzen Sie die horizontale/vertikale Scroll-Leiste bei Bedarf

### 3. **Element Navigation**
- **Auto-Highlight aktiviert**: Elemente werden automatisch rot markiert bei Auswahl
- **Manuell markieren**: Button "Markieren" klicken
- **Zoom zu Element**: Button "Zoom zu Element" für direkte Navigation
- **Highlights löschen**: Button "Löschen" zum Entfernen der Markierungen

### 4. **Export**
- Klicken Sie "Export HTML" für umfassenden Report
- Wählen Sie Speicherort
- Report öffnet sich automatisch im Browser

## Technische Details

### **Spalten-Beschreibung**
| Spalte | Beschreibung |
|--------|--------------|
| Fehlermeldung | Original Revit Warnung |
| Anzahl | Anzahl betroffener Elemente |
| Element IDs | Revit Element IDs (für Support) |
| Elementnamen | Namen der betroffenen Elemente |
| Ebenen | Geschosse/Ebenen der Elemente |
| Ansichten | Ansichten wo Elemente sichtbar sind |
| Kategorien | Revit-Kategorien der Elemente |

### **Highlighting-System**
- **Farbe**: Rot (RGB: 255, 0, 0)
- **Transparenz**: 30%
- **Linienstärke**: Standard mit roter Farbe
- **Bereich**: Nur in der aktuellen Ansicht

### **Performance-Optimierungen**
- **View-Limitation**: Maximal 3 Ansichten pro Element angezeigt
- **Lazy Loading**: Informationen werden bei Bedarf geladen
- **Transaction-Management**: Optimierte Revit-Transaktionen

## Troubleshooting

### **Häufige Probleme**

**Problem**: Tool startet nicht
- **Lösung**: PyRevit neu laden, Revit neustarten

**Problem**: Elemente werden nicht markiert
- **Lösung**: Überprüfen Sie, ob Elemente in aktueller Ansicht sichtbar sind

**Problem**: Export funktioniert nicht
- **Lösung**: Überprüfen Sie Schreibrechte im Zielordner

**Problem**: Fenster verschwindet
- **Lösung**: Das Fenster ist non-modal - schauen Sie in der Taskleiste

### **Debug-Informationen**
Das Tool schreibt Fehlermeldungen in das PyRevit-Log:
- PyRevit → Settings → Toggle Debug Mode
- Schauen Sie in die Konsole für detaillierte Fehlermeldungen

## Entwickelt für ICL Ingenieur Consult GmbH

### **Anpassungen für ICL-Workflow**
- Deutsche Benutzeroberfläche
- ICL-Branding in Exporten
- Optimiert für deutsche Planungsstandards
- Integration in ICL PyRevit Extension Suite

### **Support**
Bei Fragen oder Problemen:
- **Intern**: Wenden Sie sich an Arch
- **Dokumentation**: Siehe ICL BIM-Standards
- **Updates**: Verfügbar über ICL Git Repository

## Changelog

### Version 1.0 (Dezember 2025)
- ✅ Erweiterte Spalten-Anzeige
- ✅ Auto-Highlighting System
- ✅ Non-Modal Dialog
- ✅ HTML-Export mit ICL-Branding
- ✅ Deutsche Lokalisierung
- ✅ Performance-Optimierungen

## Lizenz

Entwickelt für ICL Ingenieur Consult GmbH, Leipzig.
Alle Rechte vorbehalten.
