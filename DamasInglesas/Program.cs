using System;
using System.Collections.Generic;

public enum TipoPieza { None, Man, King }
public enum Jugador { None, White, Black }

public class Piezas
{
    public TipoPieza Tipo { get; set; }
    public Jugador Titular { get; set; }
    public int Fila { get; set; }
    public int Columna { get; set; }

    public Piezas(TipoPieza tipo, Jugador titular, int fila, int columna)
    {
        Tipo = tipo;
        Titular = titular;
        Fila = fila;
        Columna = columna;
    }
}

public class Tablero
{
    public Piezas[,] Cuadros { get; private set; } = new Piezas[8, 8];

    public Tablero()
    {
        IniciarJuego();
    }

    private void IniciarJuego()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if ((i + j) % 2 == 1)
                {
                    if (i < 3) Cuadros[i, j] = new Piezas(TipoPieza.Man, Jugador.Black, i, j);
                    else if (i > 4) Cuadros[i, j] = new Piezas(TipoPieza.Man, Jugador.White, i, j);
                    else Cuadros[i, j] = new Piezas(TipoPieza.None, Jugador.None, i, j);
                }
                else
                {
                    Cuadros[i, j] = new Piezas(TipoPieza.None, Jugador.None, i, j);
                }
            }
        }
    }

    public Tablero Clonar()
    {
        var nvaTabla = new Tablero();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var pieza = Cuadros[i, j];
                nvaTabla.Cuadros[i, j] = new Piezas(pieza.Tipo, pieza.Titular, pieza.Fila, pieza.Columna);
            }
        }
        return nvaTabla;
    }

    public void Mostrar()
    {
        Console.WriteLine("  0 1 2 3 4 5 6 7 ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(i +  " ");
            for (int j = 0; j < 8; j++)
            {
                var pieza = Cuadros[i, j];
                switch (pieza.Titular)
                {
                    case Jugador.White: Console.Write("W "); break;
                    case Jugador.Black: Console.Write("B "); break;
                    default: Console.Write(". "); break;
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public bool EsMovimientoValido(int iniciarX, int iniciarY, int terminarX, int terminarY, Jugador jugador)
    {
        var pieza = Cuadros[iniciarX, iniciarY];
        if (pieza.Titular != jugador || !EstaDentroDelLimite(terminarX, terminarY)) return false;

        // Check regular move
        if (Math.Abs(terminarX - iniciarX) == 1 && Math.Abs(terminarY - iniciarY) == 1 && Cuadros[terminarX, terminarY].Titular == Jugador.None)
            return true;

        // Check jump move
        if (Math.Abs(terminarX - iniciarX) == 2 && Math.Abs(terminarY - iniciarY) == 2)
        {
            int mitX = (iniciarX + terminarX) / 2;
            int mitY = (iniciarY + terminarY) / 2;
            var mitPieza = Cuadros[mitX, mitY];
            return mitPieza.Titular != Jugador.None && mitPieza.Titular != jugador;
        }

        return false;
    }

    public void MovimientoPieza(int iniciarX, int iniciarY, int terminarX, int terminarY)
    {
        var pieza = Cuadros[iniciarX, iniciarY];
        Cuadros[terminarX, terminarY] = pieza;
        pieza.Fila = terminarX;
        pieza.Columna = terminarY;
        Cuadros[iniciarX, iniciarY] = new Piezas(TipoPieza.None, Jugador.None, iniciarX, iniciarY); // Clear original spot

        // Promote to King if applicable
        if ((terminarX == 0 && pieza.Titular == Jugador.White) || (terminarX == 7 && pieza.Titular == Jugador.Black))
        {
            pieza.Tipo = TipoPieza.King;
        }
    }

    public bool EstaDentroDelLimite(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}

internal class MovimientosGenerador
{
    public static List<Tablero> GetSuccessors(Tablero tablero, Jugador jugador)
    {
        var sucesores = new List<Tablero>();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tablero.Cuadros[i, j].Titular == jugador)
                {
                    GenerarMovimientosPorPieza(tablero, i, j, jugador, sucesores);
                }
            }
        }

        return sucesores;
    }

    private static void GenerarMovimientosPorPieza(Tablero tablero, int x, int y, Jugador jugador, List<Tablero> sucesores)
    {
        var pieza = tablero.Cuadros[x, y];
        var direcciones = pieza.Tipo == TipoPieza.King
            ? new (int dx, int dy)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }
            : (jugador == Jugador.White ? new (int dx, int dy)[] { (-1, -1), (-1, 1) } : new (int dx, int dy)[] { (1, -1), (1, 1) });

        foreach (var (dx, dy) in direcciones)
        {
            IntentarAgregarMovimiento(tablero, x, y, x + dx, y + dy, sucesores);
            IntentarAgregarSalto(tablero, x, y, x + dx, y + dy, x + 2 * dx, y + 2 * dy, jugador, sucesores);
        }
    }

    private static void IntentarAgregarMovimiento(Tablero tablero, int x, int y, int nuevoX, int nuevoY, List<Tablero> sucesores)
    {
        if (tablero.EsMovimientoValido(x, y, nuevoX, nuevoY, tablero.Cuadros[x, y].Titular))
        {
            var nvoTablero = tablero.Clonar();
            nvoTablero.MovimientoPieza(x, y, nuevoX, nuevoY);
            sucesores.Add(nvoTablero);
        }
    }

    private static void IntentarAgregarSalto(Tablero tablero, int x, int y, int mitX, int mitY, int nuevoX, int nuevoY, Jugador jugador, List<Tablero> sucesores)
    {
        if (tablero.EsMovimientoValido(x, y, nuevoX, nuevoY, jugador))
        {
            var nvoTablero = tablero.Clonar();
            nvoTablero.MovimientoPieza(x, y, nuevoX, nuevoY);
            nvoTablero.Cuadros[mitX, mitY] = new Piezas(TipoPieza.None, Jugador.None, mitX, mitY); // Remove captured piece

            var masCapturas = new List<Tablero>();
            GenerarMovimientosPorPieza(nvoTablero, nuevoX, nuevoY, jugador, masCapturas);

            if (masCapturas.Count > 0)
            {
                sucesores.AddRange(masCapturas);
            }
            else
            {
                sucesores.Add(nvoTablero); // No further captures, add the current board
            }
        }
    }
}

internal class Heuristica
{
    public static int Evaluar(Tablero tablero, Jugador jugador)
    {
        int ventajaJugador = 0;
        int ventajaOponente = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piezas pieza = tablero.Cuadros[i, j];
                if (pieza.Titular == jugador)
                {
                    ventajaJugador += EvaluarPiezaEstrategicamente(pieza, i, j, jugador, tablero);
                }
                else if (pieza.Titular == Oponente(jugador))
                {
                    ventajaOponente += EvaluarPiezaEstrategicamente(pieza, i, j, Oponente(jugador), tablero);
                }
            }
        }

        return ventajaJugador - ventajaOponente;
    }

    private static int EvaluarPiezaEstrategicamente(Piezas pieza, int renglon, int columna, Jugador jugador, Tablero tablero)
    {
        int valor = 0;

        if (pieza.Tipo == TipoPieza.Man)
        {
            valor += 5; // Base value for a regular piece
            if (jugador == Jugador.White)
                valor += 7 - renglon; // Closer to promotion for whites
            else
                valor += renglon; // Closer to promotion for blacks
        }
        else if (pieza.Tipo == TipoPieza.King)
        {
            valor += 10; // Base value for a king (more mobility)
        }

        // Bonus for being protected
        if (EstaProtegido(tablero, renglon, columna, jugador))
        {
            valor += 3;
        }

        // Penalty for being exposed
        if (EstaExpuesto(tablero, renglon, columna, jugador))
        {
            valor -= 2;
        }

        return valor;
    }

    public static bool EstaProtegido(Tablero tablero, int renglon, int columna, Jugador jugador)
    {
        int direccion = jugador == Jugador.White ? 1 : -1;
        int[] dx = { direccion, direccion };
        int[] dy = { -1, 1 };

        foreach (var d in Enumerable.Range(0, 2))
        {
            int nvoRenglon = renglon + dx[d];
            int nvaColumna = columna + dy[d];
            if (tablero.EstaDentroDelLimite(nvoRenglon, nvaColumna) && tablero.Cuadros[nvoRenglon, nvaColumna].Titular == jugador)
            {
                return true;
            }
        }

        return false;
    }

    public static bool EstaExpuesto(Tablero tablero, int renglon, int columna, Jugador jugador)
    {
        int direccion = jugador == Jugador.White ? 1 : -1;
        int[] dx = { direccion, direccion };
        int[] dy = { -1, 1 };

        foreach (var d in Enumerable.Range(0, 2))
        {
            int nvoRenglon = renglon + dx[d];
            int nvaColumna = columna + dy[d];
            if (tablero.EstaDentroDelLimite(nvoRenglon, nvaColumna) && tablero.Cuadros[nvoRenglon, nvaColumna].Titular == Jugador.None)
            {
                return true;
            }
        }

        return false;
    }

    private static Jugador Oponente(Jugador jugador)
    {
        return jugador == Jugador.White ? Jugador.Black : Jugador.White;
    }
}

public class Minimax
{
    private const int MaxDepth = 5;

    public static Tablero EncontrarMejorMovimiento(Tablero tablero, Jugador jugador)
    {
        return ValorMaximo(tablero, MaxDepth, jugador, int.MinValue, int.MaxValue).Item2;
    }

    private static (int, Tablero) ValorMaximo(Tablero tablero, int depth, Jugador jugador, int alpha, int beta)
    {
        if (depth == 0) return (Heuristica.Evaluar(tablero, jugador), tablero);

        int valor = int.MinValue;
        Tablero bestBoard = null;

        var sucesores = MovimientosGenerador.GetSuccessors(tablero, jugador);
        foreach (var successor in sucesores)
        {
            int nvoValor = MinValue(successor, depth - 1, Oponente(jugador), alpha, beta).Item1;
            if (nvoValor > valor)
            {
                valor = nvoValor;
                bestBoard = successor;
            }
            alpha = Math.Max(alpha, valor);

            if (beta <= alpha) break;
        }

        return (valor, bestBoard);
    }

    private static (int, Tablero) MinValue(Tablero tablero, int depth, Jugador jugador, int alpha, int beta)
    {
        if (depth == 0) return (Heuristica.Evaluar(tablero, jugador), tablero);

        int valor = int.MaxValue;
        Tablero bestBoard = null;

        var sucesores = MovimientosGenerador.GetSuccessors(tablero, jugador);
        foreach (var successor in sucesores)
        {
            int newValue = ValorMaximo(successor, depth - 1, Oponente(jugador), alpha, beta).Item1;
            if (newValue < valor)
            {
                valor = newValue;
                bestBoard = successor;
            }
            beta = Math.Min(beta, valor);

            if (beta <= alpha) break;
        }

        return (valor, bestBoard);
    }

    private static Jugador Oponente(Jugador jugador)
    {
        return jugador == Jugador.White ? Jugador.Black : Jugador.White;
    }
}

public class Juego
{
    private Tablero _board;
    private Jugador _currentPlayer;

    public Juego()
    {
        _board = new Tablero();
        _currentPlayer = Jugador.White;
    }

    public void Jugar()
    {
        while (true)
        {
            _board.Mostrar();
            if (_currentPlayer == Jugador.White)
            {
                MovimientoUsuario();
            }
            else
            {
                Console.WriteLine("Turno de Oponente IA:");
                var bestMove = Minimax.EncontrarMejorMovimiento(_board, _currentPlayer);
                if (bestMove != null)
                {
                    _board = bestMove;
                    Console.WriteLine("Oponente IA se ha movido:");
                    _board.Mostrar();
                }
                else
                {
                    Console.WriteLine("No hay movimientos disponibles. Juego terminado.");
                    break;
                }
            }

            _currentPlayer = _currentPlayer == Jugador.White ? Jugador.Black : Jugador.White;
        }
    }

    private void MovimientoUsuario()
    {
        Console.WriteLine("Ingresa tu movimiento (ejemplo: '5 2 4 3' para moverse de (5,2) a (4,3)):");
        var input = Console.ReadLine().Split(' ');
        if (input.Length == 4)
        {
            int startX = int.Parse(input[0]);
            int startY = int.Parse(input[1]);
            int endX = int.Parse(input[2]);
            int endY = int.Parse(input[3]);

            if (_board.EsMovimientoValido(startX, startY, endX, endY, Jugador.White))
            {
                _board.MovimientoPieza(startX, startY, endX, endY);
            }
            else
            {
                Console.WriteLine("Movimiento inválido, intente de nuevo.");
                MovimientoUsuario(); // Recursive call to try again
            }
        }
        else
        {
            Console.WriteLine("Entrada de datos inválida, intente de nuevo.");
            MovimientoUsuario(); // Recursive call to try again
        }
    }
}

public class Program
{
    public static void Main()
    {
        var juego = new Juego();
        juego.Jugar();
    }
}