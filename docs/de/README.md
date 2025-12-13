# ICLTools Dokumentation

## Überblick

ICLTools ist eine Sammlung von Produktivitätswerkzeugen für Autodesk Revit, die entwickelt wurden, um BIM-Workflows zu optimieren und die Parameterverwaltungsfunktionen zu verbessern.

## Verfügbare Tools

### 1. Parameter Pro
Fortschrittliches Parameterverwaltungstool mit Vorlagenunterstützung, Stapeloperationen und intelligenter Kategoriebehandlung.

**Funktionen:**
- Mehrfachauswahl-Parameterverwaltung
- Kategoriebasierte Parameterbindung
- Vorlagen speichern/laden Funktionalität
- Kategorien für vorhandene Parameter zusammenführen
- Such- und Filterfunktionen
- Unterstützung für Revit 2023-2026

**Dokumentation:** [Parameter Pro Benutzerhandbuch](ParameterPro_Benutzerhandbuch.md)

### 2. Zukünftige Tools
Weitere Tools werden dem BIMKraft-Tab hinzugefügt, wenn die Suite erweitert wird.

---

## Installation

### Voraussetzungen
- Autodesk Revit 2023, 2024, 2025 oder 2026
- Windows 10 oder neuer
- .NET Framework 4.8 (Revit 2023-2024) oder .NET 8.0 (Revit 2025-2026)

### Installationsschritte

1. Schließen Sie Revit, falls es läuft
2. Kopieren Sie `BIMKraft.dll` nach:
   - `%AppData%\Autodesk\Revit\Addins\[VERSION]\`
   - Ersetzen Sie [VERSION] durch Ihre Revit-Version (2023, 2024, 2025 oder 2026)
3. Kopieren Sie `BIMKraft.addin` in denselben Ordner
4. Kopieren Sie alle erforderlichen Abhängigkeits-DLLs (z.B. `Newtonsoft.Json.dll`) in denselben Ordner
5. Starten Sie Revit
6. Suchen Sie nach der Registerkarte **BIMKraft** im Menüband

### Überprüfung

Nach der Installation sollten Sie sehen:
- Eine neue Registerkarte **BIMKraft** im Revit-Menüband
- Panel **Parameter Tools** mit der Schaltfläche **Parameter Pro**

---

## Erste Schritte

### Erstmalige Verwendung

1. **Gemeinsame Parameter konfigurieren**
   - Stellen Sie sicher, dass Ihre gemeinsame Parameterdatei in Revit geladen ist
   - Gehen Sie zu Verwalten → Projektparameter → Gemeinsame Parameter

2. **Ein Tool starten**
   - Klicken Sie auf die BIMKraft-Registerkarte
   - Wählen Sie das Tool aus, das Sie verwenden möchten
   - Folgen Sie dem toolspezifischen Workflow

3. **Funktionen erkunden**
   - Jedes Tool hat integrierte Tooltips
   - Siehe individuelle Tool-Dokumentation für detaillierte Anweisungen

---

## Dokumentation nach Tool

- [Parameter Pro Benutzerhandbuch](ParameterPro_Benutzerhandbuch.md) - Vollständige Anleitung für Parameterverwaltung

---

## Projektstruktur

Für Entwickler und diejenigen, die ICLTools erweitern möchten:

Siehe [Projektstruktur-Anleitung](../PROJECT_STRUCTURE.md) für Informationen über:
- Hinzufügen neuer Tools
- Projektorganisation
- Best Practices
- Entwicklungsworkflow

---

## Support & Fehlerbehebung

### Häufige Probleme

**BIMKraft-Registerkarte erscheint nicht**
- Überprüfen Sie, ob DLL-Dateien im richtigen Addins-Ordner sind
- Prüfen Sie, ob die .addin-Datei mit Ihrer Revit-Version übereinstimmt
- Starten Sie Revit neu

**Tools werden nicht geladen**
- Prüfen Sie Revit-Versionskompatibilität
- Stellen Sie sicher, dass alle Abhängigkeits-DLLs vorhanden sind
- Überprüfen Sie Windows-Ereignisanzeige für Fehlerdetails

**Gemeinsame Parameter nicht gefunden**
- Überprüfen Sie, ob die gemeinsame Parameterdatei in Revit geladen ist
- Prüfen Sie den Dateipfad unter Verwalten → Gemeinsame Parameter
- Stellen Sie sicher, dass Parameternamen exakt übereinstimmen

### Hilfe erhalten

1. Prüfen Sie die toolspezifische Dokumentation
2. Lesen Sie den Fehlerbehebungsabschnitt in jedem Handbuch
3. Wenden Sie sich an Ihren BIM-Manager oder IT-Support
4. Prüfen Sie das Projekt-Repository auf bekannte Probleme

---

## Versionsinformationen

### Aktuelle Version: 1.2

**Neuerungen:**
- Entfernung von Standardkategorien für volle Benutzerkontrolle
- Verbessertes Mehrfachauswahlverhalten
- Hinzugefügte Kategorie-Zusammenführungsfunktion
- Verbesserte Vorlagenverwaltung
- Mehrversions-Revit-Unterstützung

**Unterstützte Revit-Versionen:**
- Revit 2023 (.NET Framework 4.8)
- Revit 2024 (.NET Framework 4.8)
- Revit 2025 (.NET 8.0)
- Revit 2026 (.NET 8.0)

---

## Best Practices

### Gemeinsame Parameter
- Pflegen Sie eine zentrale gemeinsame Parameterdatei für Ihr Team
- Verwenden Sie konsistente Namenskonventionen
- Dokumentieren Sie Parameterzwecke und -verwendung
- Versionskontrolle für Ihre gemeinsame Parameterdatei

### Vorlagen
- Erstellen Sie Vorlagen für häufige Szenarien
- Teilen Sie Vorlagen mit Teammitgliedern
- Dokumentieren Sie Vorlagenzwecke
- Regelmäßige Sicherung von Vorlagendateien

### Workflow-Integration
- Schulen Sie Teammitglieder in der Tool-Verwendung
- Etablieren Sie Standard-Workflows
- Dokumentieren Sie unternehmensspezifische Prozesse
- Regelmäßige Überprüfung und Updates

---

## Feedback & Beiträge

Wir begrüßen Feedback und Vorschläge für neue Funktionen:
- Melden Sie Fehler oder Probleme
- Fordern Sie neue Funktionen an
- Teilen Sie Workflow-Verbesserungen
- Tragen Sie zur Dokumentation bei

---

## Lizenz

[Ihre Lizenzinformationen hier]

---

## Credits

Entwickelt von Maria Simon (bimkraft.de)
Für Fragen und Support wenden Sie sich an Ihr BIM-Team

---

**ICLTools** - Verbesserung der Revit-Produktivität

*Letzte Aktualisierung: 2024*
