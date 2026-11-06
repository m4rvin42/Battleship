# Battleship – kleine WPF-Lernvorlage

Ein einfaches Schiffe-versenken-Spiel gegen den Computer. Die Vorlage ist für
einen Praktikanten gedacht: normale C#-Klassen, zweidimensionale Arrays,
Schleifen und WPF-Klick-Ereignisse. Keine zusätzlichen Bibliotheken.

## Starten

Voraussetzungen: **Windows 10/11**, **.NET 10 SDK** und zum bequemen Bearbeiten
**Visual Studio 2026** mit der Workload **„.NET-Desktopentwicklung“**.

```powershell
git clone --branch dev/wpf-vorlage https://github.com/m4rvin42/Battleship.git
cd Battleship
dotnet run --project Battleship
```

Alternativ `Battleship.sln` in Visual Studio öffnen, das Projekt `Battleship`
als Startprojekt auswählen und **F5** drücken.

## Spielregeln dieser Vorlage

- Beide Seiten haben ein **8 × 8** großes Feld und drei Schiffe mit **4, 3 und 2** Feldern.
- Schiffe werden automatisch waagerecht oder senkrecht platziert. Sie dürfen
  sich berühren, aber nicht überlappen.
- Links siehst du deine Schiffe. Rechts klickst du auf die verborgene Flotte.
- Nach jedem gültigen Schuss schießt der Computer genau einmal zufällig zurück,
  auch nach einem Treffer. Doppelte Schüsse sind gesperrt.
- Wer zuerst alle drei gegnerischen Schiffe versenkt, gewinnt. Bei Spielende
  werden die übrigen Schiffe sichtbar und weitere Schüsse sind gesperrt.
- **Neues Spiel** stellt beide Flotten neu auf und setzt die Schüsse zurück.

## So ist der Code aufgebaut

| Datei | Aufgabe |
| --- | --- |
| `Battleship/Board.cs` | Schiffe platzieren, Schüsse auswerten, versenkte Schiffe zählen |
| `Battleship/MainWindow.xaml` | Fenster, Beschriftungen und Gestaltung |
| `Battleship/MainWindow.xaml.cs` | Spielfelder erzeugen, Klicks verarbeiten, Computerzug und Anzeige |
| `Battleship/App.xaml` | Startfenster festlegen |
| `tests/Battleship.Checks/Program.cs` | Spiellogik ohne Oberfläche prüfen |

**Lesereihenfolge:** Starte in `Board.Shoot()`, danach `GetCellState()` und
`MainWindow.EnemyCell_Click()`. Lies zuletzt die zufällige Platzierung im
`Board`-Konstruktor.

`_ships[row, column]` speichert Wasser (`0`) oder die Nummer eines Schiffes
(`1` bis `3`). `_shots[row, column]` merkt sich, ob auf das Feld geschossen wurde.
Die Nummer verbindet die einzelnen Felder eines Schiffes: Erst wenn alle
getroffen sind, ist das Schiff versenkt.

Die Oberfläche legt die Koordinaten jedes Buttons in dessen `Tag` ab. Ein Klick
liest sie aus, ruft `Shoot()` auf und zeichnet beide Felder neu. Der Computer
wählt aus einer Liste freier Ziele; dadurch schießt er nie zweimal auf dasselbe
Feld. Auch die Platzierung wählt aus gültigen Positionen und braucht keine
zufälligen Wiederholungsschleifen.

Die Oberfläche benutzt bewusst Code-behind. Für diese kleine Übung lässt sich
der Ablauf so direkt verfolgen. Die Spiellogik bleibt unabhängig von WPF.

## Kleine Aufgaben für den Praktikanten

1. Farben und Texte in der Oberfläche ändern und die passenden Stellen finden.
2. Einen Zähler für Treffer ergänzen. Vorher klären: Zählt ein versenktes Schiff
   auch als Treffer? Was passiert bei einem doppelten Schuss?
3. Eine andere Flotte ausprobieren. Dabei auch die Beschriftungen und Prüfungen
   anpassen und sicherstellen, dass alle Schiffe auf das Feld passen.
4. Den Computer nach einem Treffer die Nachbarfelder prüfen lassen. Doppelte
   Schüsse und Randfelder müssen weiterhin korrekt behandelt werden.
5. Als größere Erweiterung: eigene Schiffe vor dem Start manuell platzieren.

**Gute Gesprächsfragen:** Warum gibt es zwei Arrays? Warum prüft die Oberfläche
nach dem Spielerschuss sofort auf Sieg? Welche Aufgabe gehört zur Spiellogik,
welche zur Oberfläche?

## Prüfen

```powershell
dotnet build Battleship.sln --configuration Release
dotnet run --project tests/Battleship.Checks --configuration Release
```

Die kleinen Konsolenprüfungen untersuchen 250 zufällige Flotten und 250
vollständige Schussfolgen: Schiffslängen, gerade Platzierung ohne Lücken,
verborgene Felder, Treffer, versenkte Schiffe, doppelte Schüsse, Computerziele
und Spielende. Sie benötigen keine Testpakete und können auch unter Linux oder
macOS mit dem .NET 10 SDK ausgeführt werden. Die WPF-Oberfläche läuft unter Windows.

GitHub Actions baut die Lösung auf Windows und führt dieselben Prüfungen aus.

Zusätzlich unter Windows manuell prüfen: eine Runde spielen, ein bereits
beschossenes Feld anklicken, einen Sieg bzw. eine Niederlage erreichen und
„Neues Spiel“ sowohl während als auch nach einer Runde verwenden. Die
Zielfelder lassen sich auch mit Tab und Leertaste bedienen.
