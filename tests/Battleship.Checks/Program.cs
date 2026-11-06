using Battleship;

// Kleine ausführbare Prüfungen ohne zusätzliche Testpakete.
for (int seed = 0; seed < 250; seed++)
{
    CheckFleet(seed);
    CheckRandomShots(seed);
}
Console.WriteLine("OK: 250 Flotten und 250 vollständige Schussfolgen geprüft.");

static void CheckFleet(int seed)
{
    var board = new Board(new Random(seed));
    var ships = new List<(int Row, int Column)>();
    for (int row = 0; row < Board.Size; row++)
        for (int column = 0; column < Board.Size; column++)
        {
            Require(board.GetCellState(row, column, false) == CellState.Hidden,
                "Die gegnerische Flotte muss am Anfang verborgen sein.");
            if (board.GetCellState(row, column, true) == CellState.Ship)
                ships.Add((row, column));
        }

    Require(ships.Count == 9 && board.RemainingShips == 3 && !board.AllShipsSunk,
        "Am Anfang müssen drei Schiffe mit zusammen neun Feldern vorhanden sein.");

    var sunkCells = new HashSet<(int Row, int Column)>();
    var sunkLengths = new List<int>();
    foreach (var cell in ships)
    {
        ShotResult result = board.Shoot(cell.Row, cell.Column);
        Require(result is ShotResult.Hit or ShotResult.Sunk, "Ein Schiff muss getroffen werden.");
        if (result == ShotResult.Sunk)
        {
            var parts = ships.Where(position =>
                board.GetCellState(position.Row, position.Column, false) == CellState.Sunk
                && !sunkCells.Contains(position)).ToList();
            bool horizontal = parts.All(part => part.Row == parts[0].Row);
            bool vertical = parts.All(part => part.Column == parts[0].Column);
            Require(horizontal || vertical, "Jedes Schiff muss gerade liegen.");
            int[] axis = parts.Select(part => horizontal ? part.Column : part.Row).Order().ToArray();
            Require(axis[^1] - axis[0] + 1 == parts.Count, "Ein Schiff darf keine Lücken haben.");
            sunkLengths.Add(parts.Count);
            sunkCells.UnionWith(parts);
        }

        Require(board.RemainingShips == 3 - sunkLengths.Count,
            "Nur vollständig getroffene Schiffe dürfen als versenkt zählen.");
        int shotCount = board.ShotsTaken;
        Require(board.Shoot(cell.Row, cell.Column) == ShotResult.AlreadyShot,
            "Wiederholte Schüsse müssen erkannt werden.");
        Require(board.ShotsTaken == shotCount, "Ein wiederholter Schuss darf nicht mitzählen.");
    }

    Require(sunkLengths.Order().SequenceEqual(new[] { 2, 3, 4 }),
        "Die Schiffe müssen genau die Längen 2, 3 und 4 haben.");
    Require(board.AllShipsSunk && board.ShotsTaken == 9,
        "Nach neun gezielten Treffern muss die ganze Flotte versenkt sein.");
}

static void CheckRandomShots(int seed)
{
    var random = new Random(seed);
    var board = new Board(random);
    var targets = new HashSet<(int Row, int Column)>();
    int hits = 0;
    int misses = 0;
    for (int turn = 0; turn < Board.Size * Board.Size; turn++)
    {
        var target = board.ChooseTarget(random);
        Require(targets.Add(target), "Der Computer darf kein Feld zweimal auswählen.");
        ShotResult result = board.Shoot(target.Row, target.Column);
        if (result == ShotResult.Miss)
        {
            misses++;
            Require(board.GetCellState(target.Row, target.Column, false) == CellState.Miss,
                "Ein Fehlschuss muss als Wasser angezeigt werden.");
            int shotCount = board.ShotsTaken;
            Require(board.Shoot(target.Row, target.Column) == ShotResult.AlreadyShot
                && board.ShotsTaken == shotCount, "Auch doppelte Fehlschüsse dürfen nicht zählen.");
        }
        else
        {
            Require(result is ShotResult.Hit or ShotResult.Sunk, "Jeder Schuss muss gültig sein.");
            hits++;
        }
        Require(board.AllShipsSunk == (hits == 9), "Das Spielende muss beim letzten Treffer eintreten.");
    }

    Require(hits == 9 && misses == 55 && board.ShotsTaken == 64 && board.RemainingShips == 0,
        "Ein vollständig beschossenes Feld muss neun Treffer und 55 Fehlschüsse enthalten.");
    bool rejected = false;
    try { board.ChooseTarget(random); }
    catch (InvalidOperationException) { rejected = true; }
    Require(rejected, "Ein volles Spielfeld darf kein weiteres Ziel liefern.");

    var freshBoard = new Board(random);
    Require(freshBoard.ShotsTaken == 0 && freshBoard.RemainingShips == 3,
        "Ein neues Spielfeld muss alle Schüsse zurücksetzen und drei Schiffe haben.");
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
