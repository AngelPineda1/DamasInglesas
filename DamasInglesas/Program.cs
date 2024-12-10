using System;
using System.Collections.Generic;

public enum PieceType { None, Man, King }
public enum Player { None, White, Black }

public class Piezas
{
    public PieceType Type { get; set; }
    public Player Owner { get; set; }
    public int Fila { get; set; }
    public int Columna { get; set; }

    public Piezas(PieceType type, Player owner, int fila, int columna)
    {
        Type = type;
        Owner = owner;
        Fila = fila;
        Columna = columna;
    }
}

public class Tablero
{
    public Piezas[,] Squares { get; private set; } = new Piezas[8, 8];

    public Tablero()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if ((i + j) % 2 == 1)
                {
                    if (i < 3) Squares[i, j] = new Piezas(PieceType.Man, Player.Black, i, j);
                    else if (i > 4) Squares[i, j] = new Piezas(PieceType.Man, Player.White, i, j);
                    else Squares[i, j] = new Piezas(PieceType.None, Player.None, i, j);
                }
                else
                {
                    Squares[i, j] = new Piezas(PieceType.None, Player.None, i, j);
                }
            }
        }
    }

    public Tablero Clone()
    {
        var newBoard = new Tablero();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var piece = Squares[i, j];
                newBoard.Squares[i, j] = new Piezas(piece.Type, piece.Owner, piece.Fila, piece.Columna);
            }
        }
        return newBoard;
    }

    public void Display()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var piece = Squares[i, j];
                switch (piece.Owner)
                {
                    case Player.White: Console.Write("W "); break;
                    case Player.Black: Console.Write("B "); break;
                    default: Console.Write(". "); break;
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public bool IsMoveValid(int startX, int startY, int endX, int endY, Player player)
    {
        var piece = Squares[startX, startY];
        if (piece.Owner != player || !IsInBounds(endX, endY)) return false;

        // Check regular move
        if (Math.Abs(endX - startX) == 1 && Math.Abs(endY - startY) == 1 && Squares[endX, endY].Owner == Player.None)
            return true;

        // Check jump move
        if (Math.Abs(endX - startX) == 2 && Math.Abs(endY - startY) == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            var midPiece = Squares[midX, midY];
            return midPiece.Owner != Player.None && midPiece.Owner != player;
        }

        return false;
    }

    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        var piece = Squares[startX, startY];
        Squares[endX, endY] = piece;
        piece.Fila = endX;
        piece.Columna = endY;
        Squares[startX, startY] = new Piezas(PieceType.None, Player.None, startX, startY); // Clear original spot

        // Promote to King if applicable
        if ((endX == 0 && piece.Owner == Player.White) || (endX == 7 && piece.Owner == Player.Black))
        {
            piece.Type = PieceType.King;
        }
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}

internal class MovimientosGenerador
{
    public static List<Tablero> GetSuccessors(Tablero board, Player player)
    {
        var successors = new List<Tablero>();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board.Squares[i, j].Owner == player)
                {
                    GenerateMovesForPiece(board, i, j, player, successors);
                }
            }
        }

        return successors;
    }

    private static void GenerateMovesForPiece(Tablero board, int x, int y, Player player, List<Tablero> successors)
    {
        var piece = board.Squares[x, y];
        var directions = piece.Type == PieceType.King
            ? new (int dx, int dy)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }
            : (player == Player.White ? new (int dx, int dy)[] { (-1, -1), (-1, 1) } : new (int dx, int dy)[] { (1, -1), (1, 1) });

        foreach (var (dx, dy) in directions)
        {
            TryAddMove(board, x, y, x + dx, y + dy, successors);
            TryAddJump(board, x, y, x + dx, y + dy, x + 2 * dx, y + 2 * dy, player, successors);
        }
    }

    private static void TryAddMove(Tablero board, int x, int y, int newX, int newY, List<Tablero> successors)
    {
        if (board.IsMoveValid(x, y, newX, newY, board.Squares[x, y].Owner))
        {
            var newBoard = board.Clone();
            newBoard.MovePiece(x, y, newX, newY);
            successors.Add(newBoard);
        }
    }

    private static void TryAddJump(Tablero board, int x, int y, int midX, int midY, int newX, int newY, Player player, List<Tablero> successors)
    {
        if (board.IsMoveValid(x, y, newX, newY, player))
        {
            var newBoard = board.Clone();
            newBoard.MovePiece(x, y, newX, newY);
            newBoard.Squares[midX, midY] = new Piezas(PieceType.None, Player.None, midX, midY); // Remove captured piece

            var furtherCaptures = new List<Tablero>();
            GenerateMovesForPiece(newBoard, newX, newY, player, furtherCaptures);

            if (furtherCaptures.Count > 0)
            {
                successors.AddRange(furtherCaptures);
            }
            else
            {
                successors.Add(newBoard); // No further captures, add the current board
            }
        }
    }
}

internal class Heuristica
{
    public static int Evaluate(Tablero board, Player player)
    {
        int playerAdvantage = 0;
        int opponentAdvantage = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piezas piece = board.Squares[i, j];
                if (piece.Owner == player)
                {
                    playerAdvantage += EvaluatePieceStrategically(piece, i, j, player, board);
                }
                else if (piece.Owner == Opponent(player))
                {
                    opponentAdvantage += EvaluatePieceStrategically(piece, i, j, Opponent(player), board);
                }
            }
        }

        return playerAdvantage - opponentAdvantage;
    }

    private static int EvaluatePieceStrategically(Piezas piece, int row, int col, Player player, Tablero board)
    {
        int value = 0;

        if (piece.Type == PieceType.Man)
        {
            value += 5; // Base value for a regular piece
            if (player == Player.White)
                value += 7 - row; // Closer to promotion for whites
            else
                value += row; // Closer to promotion for blacks
        }
        else if (piece.Type == PieceType.King)
        {
            value += 10; // Base value for a king (more mobility)
        }

        // Bonus for being protected
        if (IsProtected(board, row, col, player))
        {
            value += 3;
        }

        // Penalty for being exposed
        if (IsExposed(board, row, col, player))
        {
            value -= 2;
        }

        return value;
    }

    public static bool IsProtected(Tablero board, int row, int col, Player player)
    {
        int direction = player == Player.White ? 1 : -1;
        int[] dx = { direction, direction };
        int[] dy = { -1, 1 };

        foreach (var d in Enumerable.Range(0, 2))
        {
            int newRow = row + dx[d];
            int newCol = col + dy[d];
            if (board.IsInBounds(newRow, newCol) && board.Squares[newRow, newCol].Owner == player)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsExposed(Tablero board, int row, int col, Player player)
    {
        int direction = player == Player.White ? 1 : -1;
        int[] dx = { direction, direction };
        int[] dy = { -1, 1 };

        foreach (var d in Enumerable.Range(0, 2))
        {
            int newRow = row + dx[d];
            int newCol = col + dy[d];
            if (board.IsInBounds(newRow, newCol) && board.Squares[newRow, newCol].Owner == Player.None)
            {
                return true;
            }
        }

        return false;
    }

    private static Player Opponent(Player player)
    {
        return player == Player.White ? Player.Black : Player.White;
    }
}

public class Minimax
{
    private const int MaxDepth = 5;

    public static Tablero FindBestMove(Tablero board, Player player)
    {
        return MaxValue(board, MaxDepth, player, int.MinValue, int.MaxValue).Item2;
    }

    private static (int, Tablero) MaxValue(Tablero board, int depth, Player player, int alpha, int beta)
    {
        if (depth == 0) return (Heuristica.Evaluate(board, player), board);

        int value = int.MinValue;
        Tablero bestBoard = null;

        var successors = MovimientosGenerador.GetSuccessors(board, player);
        foreach (var successor in successors)
        {
            int newValue = MinValue(successor, depth - 1, Opponent(player), alpha, beta).Item1;
            if (newValue > value)
            {
                value = newValue;
                bestBoard = successor;
            }
            alpha = Math.Max(alpha, value);

            if (beta <= alpha) break;
        }

        return (value, bestBoard);
    }

    private static (int, Tablero) MinValue(Tablero board, int depth, Player player, int alpha, int beta)
    {
        if (depth == 0) return (Heuristica.Evaluate(board, player), board);

        int value = int.MaxValue;
        Tablero bestBoard = null;

        var successors = MovimientosGenerador.GetSuccessors(board, player);
        foreach (var successor in successors)
        {
            int newValue = MaxValue(successor, depth - 1, Opponent(player), alpha, beta).Item1;
            if (newValue < value)
            {
                value = newValue;
                bestBoard = successor;
            }
            beta = Math.Min(beta, value);

            if (beta <= alpha) break;
        }

        return (value, bestBoard);
    }

    private static Player Opponent(Player player)
    {
        return player == Player.White ? Player.Black : Player.White;
    }
}

public class Juego
{
    private Tablero _board;
    private Player _currentPlayer;

    public Juego()
    {
        _board = new Tablero();
        _currentPlayer = Player.White;
    }

    public void Play()
    {
        while (true)
        {
            _board.Display();
            if (_currentPlayer == Player.White)
            {
                UserMove();
            }
            else
            {
                Console.WriteLine("AI's turn:");
                var bestMove = Minimax.FindBestMove(_board, _currentPlayer);
                if (bestMove != null)
                {
                    _board = bestMove;
                    Console.WriteLine("AI moved:");
                    _board.Display();
                }
                else
                {
                    Console.WriteLine("No moves available. Game Over.");
                    break;
                }
            }

            _currentPlayer = _currentPlayer == Player.White ? Player.Black : Player.White;
        }
    }

    private void UserMove()
    {
        Console.WriteLine("Enter your move (e.g., '5 2 4 3' to move from (5,2) to (4,3)):");
        var input = Console.ReadLine().Split(' ');
        if (input.Length == 4)
        {
            int startX = int.Parse(input[0]);
            int startY = int.Parse(input[1]);
            int endX = int.Parse(input[2]);
            int endY = int.Parse(input[3]);

            if (_board.IsMoveValid(startX, startY, endX, endY, Player.White))
            {
                _board.MovePiece(startX, startY, endX, endY);
            }
            else
            {
                Console.WriteLine("Invalid move, try again.");
                UserMove(); // Recursive call to try again
            }
        }
        else
        {
            Console.WriteLine("Invalid input format, try again.");
            UserMove(); // Recursive call to try again
        }
    }
}

public class Program
{
    public static void Main()
    {
        var juego = new Juego();
        juego.Play();
    }
}