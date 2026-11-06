namespace Battleship;

public enum CellState { Hidden, Ship, Miss, Hit, Sunk }
public enum ShotResult { Miss, Hit, Sunk, AlreadyShot }

// Diese Klasse kennt keine WPF-Steuerelemente: Hier stehen nur die Spielregeln.
public class Board
{
    public const int Size = 8;
    private static readonly int[] ShipLengths = [4, 3, 2];

    // 0 bedeutet Wasser, 1 bis 3 sind die Nummern der Schiffe.
    private readonly int[,] _ships = new int[Size, Size];
    private readonly bool[,] _shots = new bool[Size, Size];

    public int ShotsTaken { get; private set; }
    public bool AllShipsSunk => RemainingShips == 0;

    public int RemainingShips
    {
        get
        {
            int remaining = 0;
            for (int shipId = 1; shipId <= ShipLengths.Length; shipId++)
                if (!IsShipSunk(shipId))
                    remaining++;
            return remaining;
        }
    }

    public Board(Random random)
    {
        for (int index = 0; index < ShipLengths.Length; index++)
        {
            int length = ShipLengths[index];
            var positions = new List<(int Row, int Column, bool Horizontal)>();

            // Erst alle freien Startpositionen sammeln, dann eine auswählen.
            for (int row = 0; row < Size; row++)
                for (int column = 0; column < Size; column++)
                    foreach (bool horizontal in new[] { true, false })
                        if (CanPlaceShip(row, column, length, horizontal))
                            positions.Add((row, column, horizontal));

            var position = positions[random.Next(positions.Count)];
            for (int part = 0; part < length; part++)
            {
                int row = position.Row + (position.Horizontal ? 0 : part);
                int column = position.Column + (position.Horizontal ? part : 0);
                _ships[row, column] = index + 1;
            }
        }
    }

    public ShotResult Shoot(int row, int column)
    {
        if (_shots[row, column])
            return ShotResult.AlreadyShot;

        _shots[row, column] = true;
        ShotsTaken++;
        int shipId = _ships[row, column];
        if (shipId == 0)
            return ShotResult.Miss;

        return IsShipSunk(shipId) ? ShotResult.Sunk : ShotResult.Hit;
    }

    public CellState GetCellState(int row, int column, bool showShips)
    {
        int shipId = _ships[row, column];
        if (!_shots[row, column])
            return showShips && shipId != 0 ? CellState.Ship : CellState.Hidden;
        if (shipId == 0)
            return CellState.Miss;
        return IsShipSunk(shipId) ? CellState.Sunk : CellState.Hit;
    }

    // Der Computer wählt nur Felder aus, die er noch nicht beschossen hat.
    public (int Row, int Column) ChooseTarget(Random random)
    {
        var targets = new List<(int Row, int Column)>();
        for (int row = 0; row < Size; row++)
            for (int column = 0; column < Size; column++)
                if (!_shots[row, column])
                    targets.Add((row, column));

        if (targets.Count == 0)
            throw new InvalidOperationException("Es gibt keine freien Zielfelder mehr.");
        return targets[random.Next(targets.Count)];
    }

    private bool CanPlaceShip(int row, int column, int length, bool horizontal)
    {
        for (int part = 0; part < length; part++)
        {
            int nextRow = row + (horizontal ? 0 : part);
            int nextColumn = column + (horizontal ? part : 0);
            if (nextRow >= Size || nextColumn >= Size || _ships[nextRow, nextColumn] != 0)
                return false;
        }
        return true;
    }

    private bool IsShipSunk(int shipId)
    {
        for (int row = 0; row < Size; row++)
            for (int column = 0; column < Size; column++)
                if (_ships[row, column] == shipId && !_shots[row, column])
                    return false;
        return true;
    }
}
