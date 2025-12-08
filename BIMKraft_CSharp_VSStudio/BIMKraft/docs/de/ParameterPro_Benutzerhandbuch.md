# BIM Kraft Parameter Pro - Benutzerhandbuch

## Überblick

BIM Kraft Parameter Pro ist ein fortschrittliches Parameterverwaltungstool für Autodesk Revit, das den Prozess des Hinzufügens gemeinsamer Parameter zu Ihren Projekten mit leistungsstarken Funktionen wie Kategorieauswahl, Vorlagenverwaltung und Stapeloperationen optimiert.

## Inhaltsverzeichnis

- [Erste Schritte](#erste-schritte)
- [Oberflächenübersicht](#oberflächenübersicht)
- [Grundlegender Arbeitsablauf](#grundlegender-arbeitsablauf)
- [Funktionen](#funktionen)
- [Erweiterte Funktionen](#erweiterte-funktionen)
- [Tipps & Best Practices](#tipps--best-practices)
- [Fehlerbehebung](#fehlerbehebung)

---

## Erste Schritte

### Tool starten

1. Öffnen Sie Ihr Revit-Projekt
2. Navigieren Sie zur Registerkarte **ICL** im Revit-Menüband
3. Klicken Sie auf die Schaltfläche **Parameter Pro** im Bereich "Parameter Tools"

### Voraussetzungen

- Autodesk Revit 2023, 2024, 2025 oder 2026
- Eine in Revit konfigurierte gemeinsame Parameterdatei
- Entsprechende Berechtigungen zum Ändern von Projektparametern

---

## Oberflächenübersicht

Das Parameter Pro-Fenster ist in drei Hauptbereiche unterteilt:

### 1. Verfügbare Parameter (Linkes Panel)

- **Suchfeld**: Parameter in Echtzeit nach Namen filtern
- **Parameterbaum**: Hierarchische Ansicht aller Parameter aus Ihrer gemeinsamen Parameterdatei, gruppiert nach Kategorie
- **Auswahlzusammenfassung**: Zeigt die Anzahl der ausgewählten Parameter an

### 2. Parameterkonfiguration (Rechtes Panel)

- **Kategorie-Kontrollkästchen**: Wählen Sie aus, auf welche Revit-Kategorien die Parameter angewendet werden sollen
- **Parametergruppe**: Wählen Sie die Parametergruppe (z.B. Daten, Identitätsdaten usw.)
- **Bindungstyp**: Wählen Sie Exemplar- oder Typ-Bindung

### 3. Untere Steuerelemente

- **Vorlagen-Bereich**: Parameterkonfigurationen speichern, laden und löschen
- **Aktionsschaltflächen**:
  - Kontrollkästchen "Vorhandene Parameter aktualisieren"
  - Kontrollkästchen "Kategorien zusammenführen"
  - Schaltfläche "Parameter zum Projekt hinzufügen"
  - Schaltfläche "Abbrechen"

---

## Grundlegender Arbeitsablauf

### Parameter zu Ihrem Projekt hinzufügen

1. **Parameter auswählen**
   - Klicken Sie auf die Kontrollkästchen neben den Parameternamen, um sie auszuwählen
   - Verwenden Sie Umschalt+Klick, um einen Bereich von Parametern auszuwählen
   - Ausgewählte Parameter bleiben für einfache Mehrfachauswahl aktiviert

2. **Kategorien konfigurieren**
   - Im rechten Bereich aktivieren Sie die Kategorien, in denen Parameter verfügbar sein sollen
   - Sie können mehrere Kategorien auswählen (z.B. Türen, Fenster, Wände)

3. **Parametergruppe wählen**
   - Wählen Sie die entsprechende Parametergruppe aus dem Dropdown-Menü
   - Standard ist "Daten"

4. **Bindungstyp auswählen**
   - **Exemplar**: Parameterwerte sind für jede Elementinstanz eindeutig
   - **Typ**: Parameterwerte werden von allen Elementen desselben Typs geteilt

5. **Zum Projekt hinzufügen**
   - Klicken Sie auf "Parameter zum Projekt hinzufügen"
   - Überprüfen Sie den Ergebnisdialog, der Erfolge, Fehler und übersprungene Parameter anzeigt

---

## Funktionen

### 1. Mehrfachauswahl-Funktionalität

**Normaler Klick**: Einzelne Parameterauswahl umschalten
- Klicken Sie auf einen Parameter, um ihn auszuwählen
- Klicken Sie erneut, um die Auswahl aufzuheben
- Zuvor ausgewählte Parameter bleiben ausgewählt

**Umschalt+Klick**: Bereichsauswahl
- Wählen Sie einen Parameter aus
- Halten Sie die Umschalttaste gedrückt und klicken Sie auf einen anderen Parameter
- Alle Parameter dazwischen werden basierend auf der ersten Auswahl ausgewählt/abgewählt

### 2. Suchen und Filtern

- Geben Sie im Suchfeld Text ein, um Parameter nach Namen zu filtern
- Die Suche unterscheidet nicht zwischen Groß- und Kleinschreibung
- Ergebnisse werden in Echtzeit aktualisiert
- Die Suche funktioniert über alle Parametergruppen hinweg

### 3. Vorlagenverwaltung

Vorlagen ermöglichen es Ihnen, Parameterkonfigurationen zu speichern und wiederzuverwenden.

**Vorlage erstellen**:
1. Parameter auswählen
2. Kategorien, Parametergruppe und Bindungstyp konfigurieren
3. Auf "Vorlage speichern" klicken
4. Vorlagennamen eingeben
5. Vorlage wird als JSON-Datei in Ihrem Benutzerverzeichnis gespeichert

**Vorlage laden**:
1. Wählen Sie eine Vorlage aus dem Dropdown-Menü
2. Klicken Sie auf "Vorlage laden"
3. Alle Parameter und Konfigurationen aus der Vorlage werden geladen

**Vorlage löschen**:
1. Wählen Sie eine Vorlage aus dem Dropdown-Menü
2. Klicken Sie auf "Vorlage löschen"
3. Löschen bestätigen

**Vorlagen-Speicherort**:
`%AppData%\ICLParameterPro\Presets\`

### 4. Vorhandene Parameter aktualisieren

Wenn das Kontrollkästchen "Vorhandene Parameter aktualisieren" aktiviert ist:
- Parameter, die bereits im Projekt vorhanden sind, werden aktualisiert
- Ohne diese Option werden vorhandene Parameter übersprungen

### 5. Kategorien zusammenführen

Wenn das Kontrollkästchen "Kategorien zusammenführen" aktiviert ist:
- Neue Kategorien werden zu vorhandenen Parameterbindungen **hinzugefügt**
- Vorhandene Kategorien werden **beibehalten**
- Perfekt zum schrittweisen Hinzufügen von Kategorien zu Parametern

**Beispiel**:
- Parameter "ICL_Farbe" existiert mit Kategorie "Türen"
- Sie laden eine Vorlage mit "ICL_Farbe" konfiguriert für "Fenster"
- Mit aktiviertem "Kategorien zusammenführen": Parameter gilt jetzt für "Türen" UND "Fenster"
- Ohne Zusammenführen: Parameter würde nur für "Fenster" gelten (ersetzt vorhandene)

---

## Erweiterte Funktionen

### Einheitliche Konfiguration

Wenn mehrere Parameter ausgewählt sind:
- Ein einzelnes Konfigurationspanel steuert alle ausgewählten Parameter
- Alle Parameter erhalten dieselben Kategoriebindungen, Parametergruppe und Bindungstyp
- Effizient für Batch-Parametereinrichtung

### Parametergruppierung

Parameter werden gruppiert nach ihrem Gruppennamen aus der gemeinsamen Parameterdatei angezeigt, was das Auffinden verwandter Parameter erleichtert.

### Echtzeit-Auswahlzusammenfassung

Der untere Bereich des linken Panels zeigt an, wie viele Parameter derzeit ausgewählt sind, und liefert sofortiges Feedback während der Auswahl.

---

## Tipps & Best Practices

### 1. Verwenden Sie Vorlagen für häufige Szenarien

Erstellen Sie Vorlagen für häufig verwendete Parametersätze:
- "Fensterparameter" - Alle fensterbezogenen Parameter mit Fensterkategorien
- "Oberflächenparameter" - Oberflächenbezogene Parameter für mehrere Kategorien
- "Projektdaten" - Standard-Projektinformationsparameter

### 2. Beginnen Sie ohne Standardkategorien

Ab der neuesten Version sind standardmäßig keine Kategorien ausgewählt, wenn Sie einen Parameter auswählen. Dies gibt Ihnen die volle Kontrolle, um nur die benötigten Kategorien auszuwählen.

### 3. Kategorien für bestehende Projekte zusammenführen

Beim Hinzufügen von Parametern zu bestehenden Projekten:
1. Aktivieren Sie "Vorhandene Parameter aktualisieren"
2. Aktivieren Sie "Kategorien zusammenführen"
3. Laden Sie Ihre Vorlage

Dies stellt sicher, dass Sie vorhandene Kategoriebindungen nicht versehentlich entfernen.

### 4. Organisieren mit Parametergruppen

Verwenden Sie aussagekräftige Parametergruppen, um Parameter in Eigenschaftspaletten zu organisieren:
- **Identitätsdaten**: Grundlegende Identifikationsinformationen
- **Daten**: Allgemeine Datenfelder
- **Bemaßungen**: Größen- und Messparameter
- **Materialien und Oberflächen**: Erscheinungsbezogene Parameter

### 5. Suche effizient nutzen

- Geben Sie Teilnamen ein, um Parameter schnell zu finden
- Verwenden Sie konsistente Namenskonventionen in Ihrer gemeinsamen Parameterdatei

### 6. Testen Sie zuerst mit einem Parameter

Bevor Sie Parameter in großen Mengen hinzufügen:
1. Wählen Sie einen Testparameter aus
2. Konfigurieren und fügen Sie ihn hinzu
3. Überprüfen Sie, ob er korrekt in Revit erscheint
4. Fahren Sie dann mit Stapeloperationen fort

---

## Fehlerbehebung

### Parameter erscheinen nicht im Projekt

**Überprüfen Sie**:
- Ist die gemeinsame Parameterdatei noch in Revit geladen?
- Schauen Sie in der richtigen Kategorie?
- War die Operation "Parameter hinzufügen" erfolgreich? (Prüfen Sie den Ergebnisdialog)

### Fehler "Keine gültigen Kategorien"

**Lösung**:
- Wählen Sie mindestens eine Kategorie aus, bevor Sie Parameter hinzufügen
- Stellen Sie sicher, dass die ausgewählten Kategorien in Ihrer Revit-Vorlage vorhanden sind

### Meldung "Aktualisierung fehlgeschlagen"

**Mögliche Ursachen**:
- Parameter ist von einem anderen Benutzer gesperrt (gemeinschaftliches Projekt)
- Unzureichende Berechtigungen
- Parameterdefinition in gemeinsamer Parameterdatei geändert

**Lösung**:
- Stellen Sie sicher, dass Sie Bearbeitungsrechte haben
- Mit Zentral synchronisieren (gemeinschaftliche Projekte)
- Überprüfen Sie die Integrität der gemeinsamen Parameterdatei

### Vorlage wird nicht geladen

**Überprüfen Sie**:
- Existiert die Vorlagendatei noch unter `%AppData%\ICLParameterPro\Presets\`?
- Ist die JSON-Datei gültig? (In Texteditor öffnen zur Überprüfung)
- Existieren die Parameter in der Vorlage in Ihrer aktuellen gemeinsamen Parameterdatei?

### Kategorien werden nicht zusammengeführt

**Überprüfen Sie**:
- Sowohl "Vorhandene Parameter aktualisieren" als auch "Kategorien zusammenführen" sind aktiviert
- Oder nur "Kategorien zusammenführen" ist aktiviert (funktioniert unabhängig)
- Parametername stimmt exakt mit vorhandenem Parameter überein

---

## Tastaturkürzel

- **Umschalt + Klick**: Bereichsauswahl in Parameterliste
- **Strg + F**: Suchfeld fokussieren (falls verfügbar)

---

## Parameter-Ergebnisdialog

Nach dem Hinzufügen von Parametern zeigt ein Ergebnisdialog:

### Erfolgreich hinzugefügte/aktualisierte Parameter
Parameter, die erfolgreich hinzugefügt wurden oder deren Kategorien zusammengeführt wurden

### Übersprungene vorhandene Parameter
Parameter, die bereits existieren, aber nicht aktualisiert wurden (wenn "Vorhandene aktualisieren" nicht aktiviert ist)

### Fehlgeschlagene Parameter
Parameter, die aufgrund von Fehlern nicht hinzugefügt werden konnten, mit Fehlermeldungen zur Fehlerbehebung

---

## Unterstützte Revit-Versionen

- **Revit 2023**: .NET Framework 4.8
- **Revit 2024**: .NET Framework 4.8
- **Revit 2025**: .NET 8.0
- **Revit 2026**: .NET 8.0

---

## Über Vorlagen

Vorlagen werden als JSON-Dateien gespeichert, die Folgendes enthalten:
- Liste der Parameternamen
- Kategoriebindungen für jeden Parameter
- Parametergruppenzuweisung
- Bindungstyp (Exemplar/Typ)

Sie können Vorlagendateien mit Kollegen teilen, indem Sie die JSON-Dateien aus Ihrem Vorlagenordner kopieren.

---

## Zusätzliche Ressourcen

- **Projekt-Repository**: Prüfen Sie auf Updates und melden Sie Probleme
- **Gemeinsame Parameterdatei**: Stellen Sie sicher, dass Ihr Team eine standardisierte gemeinsame Parameterdatei verwendet
- **Schulungsmaterialien**: Wenden Sie sich an Ihren BIM-Manager für unternehmensspezifische Arbeitsabläufe

---

## Versionsgeschichte

### Neueste Version
- Entfernung der Standard-Kategorien "Ansichten" und "Pläne" - volle Benutzerkontrolle
- Verbessertes Auswahlverhalten - keine Strg-Taste mehr für Mehrfachauswahl nötig
- Hinzugefügte Funktion "Kategorien zusammenführen" für schrittweises Hinzufügen von Kategorien
- Verbesserte Vorlagenverwaltung
- Mehrversions-Revit-Unterstützung (2023-2026)

---

## Support

Für Probleme, Feature-Anfragen oder Fragen:
- Wenden Sie sich an Ihre IT-/BIM-Abteilung
- Prüfen Sie das Repository auf bekannte Probleme
- Lesen Sie diese Dokumentation für häufige Lösungen

---

**BIM Kraft Parameter Pro** - Vereinfachung der Parameterverwaltung in Revit

*Dokumentationsversion 1.2 - 2024*
