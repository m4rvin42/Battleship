using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Battleship;

public partial class MainWindow : Window
{
    private readonly Random _random = new();
    private readonly Button[,] _playerButtons = new Button[Board.Size, Board.Size];
    private readonly Button[,] _enemyButtons = new Button[Board.Size, Board.Size];
    private Board _playerBoard = null!;
    private Board _enemyBoard = null!;
    private bool _gameOver;

    public MainWindow()
    {
        InitializeComponent();
        CreateGrid(PlayerGrid, _playerButtons, isTarget: false);
        CreateGrid(EnemyGrid, _enemyButtons, isTarget: true);
        StartGame();
    }

    private void CreateGrid(UniformGrid grid, Button[,] buttons, bool isTarget)
    {
        grid.Children.Add(new TextBlock()); // Leere Ecke zwischen den Achsen.
        for (int column = 0; column < Board.Size; column++)
            grid.Children.Add(CreateLabel(((char)('A' + column)).ToString()));

        for (int row = 0; row < Board.Size; row++)
        {
            grid.Children.Add(CreateLabel((row + 1).ToString()));
            for (int column = 0; column < Board.Size; column++)
            {
                var button = new Button
                {
                    Style = (Style)FindResource("CellButton"),
                    Tag = (row, column),
                    IsEnabled = isTarget
                };
                if (isTarget)
                    button.Click += EnemyCell_Click;
                buttons[row, column] = button;
                grid.Children.Add(button);
            }
        }
    }

    private static TextBlock CreateLabel(string text) => new()
    {
        Text = text,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    private void NewGame_Click(object sender, RoutedEventArgs e) => StartGame();

    private void StartGame()
    {
        _playerBoard = new Board(_random);
        _enemyBoard = new Board(_random);
        _gameOver = false;
        StatusText.Text = "Du bist dran!";
        LastMoveText.Text = "Wähle ein Feld unter „Deine Ziele“. Deine Schiffe siehst du links.";
        RefreshBoards();
    }

    private void EnemyCell_Click(object sender, RoutedEventArgs e)
    {
        if (_gameOver)
            return;

        var (row, column) = ((int, int))((Button)sender).Tag;
        ShotResult playerResult = _enemyBoard.Shoot(row, column);
        if (playerResult == ShotResult.AlreadyShot)
            return; // Ein doppelter Klick verbraucht keinen Zug.

        LastMoveText.Text = $"Du: {Coordinate(row, column)} – {Describe(playerResult)}.";
        if (_enemyBoard.AllShipsSunk)
        {
            _gameOver = true;
            StatusText.Text = $"Gewonnen! Du hast {_enemyBoard.ShotsTaken} Schüsse gebraucht.";
        }
        else
        {
            // Auch nach einem Treffer wechseln wir den Spieler.
            var target = _playerBoard.ChooseTarget(_random);
            ShotResult computerResult = _playerBoard.Shoot(target.Row, target.Column);
            LastMoveText.Text += $" Computer: {Coordinate(target.Row, target.Column)} – {Describe(computerResult)}.";
            _gameOver = _playerBoard.AllShipsSunk;
            StatusText.Text = _gameOver ? "Der Computer hat gewonnen. Versuch es noch einmal!" : "Du bist dran!";
        }

        RefreshBoards();
    }

    private void RefreshBoards()
    {
        DrawBoard(_playerBoard, _playerButtons, showShips: true, isTarget: false);
        DrawBoard(_enemyBoard, _enemyButtons, showShips: _gameOver, isTarget: true);
        PlayerFleetText.Text = $"{_playerBoard.RemainingShips} von 3 Schiffen übrig";
        EnemyFleetText.Text = $"{_enemyBoard.RemainingShips} von 3 Schiffen übrig · {_enemyBoard.ShotsTaken} Schüsse";
    }

    private void DrawBoard(Board board, Button[,] buttons, bool showShips, bool isTarget)
    {
        for (int row = 0; row < Board.Size; row++)
            for (int column = 0; column < Board.Size; column++)
            {
                CellState state = board.GetCellState(row, column, showShips);
                var (symbol, color, description) = state switch
                {
                    CellState.Ship => ("■", "#C5DBEB", "Schiff"),
                    CellState.Miss => ("•", "#DFE8EF", "Wasser, bereits beschossen"),
                    CellState.Hit => ("×", "#FFCDAD", "Treffer"),
                    CellState.Sunk => ("✓", "#B7E2CC", "Schiff versenkt"),
                    _ => ("", "#F1F7FC", "Unbeschossen")
                };
                Button button = buttons[row, column];
                button.Content = symbol;
                button.Background = (Brush)new BrushConverter().ConvertFromString(color)!;
                button.Foreground = Brushes.MidnightBlue;
                button.IsEnabled = isTarget && !_gameOver && state == CellState.Hidden;
                string name = $"{Coordinate(row, column)}: {description}";
                button.ToolTip = name;
                AutomationProperties.SetName(button, name);
            }
    }

    private static string Coordinate(int row, int column) => $"{(char)('A' + column)}{row + 1}";

    private static string Describe(ShotResult result) => result switch
    {
        ShotResult.Hit => "Treffer",
        ShotResult.Sunk => "Schiff versenkt",
        ShotResult.Miss => "Wasser",
        _ => "bereits beschossen"
    };
}
