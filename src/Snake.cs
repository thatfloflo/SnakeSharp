
namespace Snake;

/// <summary>Encapsulates the possible states of a <see cref="Snake"/> in the game.</summary>
public enum SnakeState
{
    Normal,
    Chomping,
    Dead
}

/// <summary>Represents a snake in a game of snake.</summary>
public class Snake
{
    /// <summary>Instantiates a new snake to use in a game.</summary>
    /// <param name="maxLength">The maximal length the snake is allowed to grow to (usually <c>&lt;=</c> the area of the game area).</param>
    /// <param name="spawnPosition">The initial coordinates at which the snake is spawned.</param>
    public Snake(int maxLength, Coordinates spawnPosition)
    {
        Length = 1;
        Path = new Coordinates[maxLength];
        Head = spawnPosition;
        CurrentDirection = Direction.None;
        PreviousDirection = Direction.None;
        State = SnakeState.Normal;
    }

    /// <summary>The current length of the snake, including the head. Always greater than 0.</summary>
    public int Length { get; set; }
    /// <summary>Array containing the path along which the snake has travelled.</summary>
    /// <remarks>
    /// <para>
    /// The path array is always of the <i>maxLength</i> that was specified when the instance was created
    /// and will thus contain more coordinate entries, possibly with default values, than the length of the
    /// snake or the moves made so far. You must use the instance's <see cref="Length"/> property to determine
    /// the number of <b>relevant</b> path entries when examining the snake's path. You would usually want
    /// to ignore any indices past the snake's <c>Length - 1</c>.
    /// </para>
    /// <para>
    /// The individual <see cref="Coordinates"/>'s annotation property contains the symbol that was drawn to
    /// the screen when the snake moved across that position.
    /// </para>
    /// </remarks>
    public Coordinates[] Path { get; set; }
    /// <summary>The coordinates for the current head of the snake.</summary>
    public Coordinates Head { get; set; }
    /// <summary>The direction in which the snake is currently moving.</summary>
    public Direction CurrentDirection { get; set; }
    /// <summary>The direction in which the snake was moving before the current move.</summary>
    public Direction PreviousDirection { get; set; }
    /// <summary>The <see cref="SnakeState"/> of the snake.</summary>
    public SnakeState State { get; set; }

    private void ShiftPathArrayRight()
    {
        for (int i = Path.Length - 2; i >= 0; i--)
            Path[i + 1] = Path[i];
    }

    /// <summary>Pushes the current position onto the <see cref="Path"/> array.</summary>
    /// <remarks>
    /// You do not have to call <see cref="StorePosition"/> if you call <see cref="Move"/>,
    /// as <see cref="Move"/> automatically stores the current position before moving the head.
    /// </remarks>
    public void StorePosition()
    {
        ShiftPathArrayRight();
        var bodyShape = 'o';
        if (CurrentDirection == PreviousDirection)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.Horizontal;
                    break;
                case Direction.Up:
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.Vertical;
                    break;
            }
        }
        else if (PreviousDirection == Direction.Up)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BoxSymbols.SquareDouble.TopLeft;
                    break;
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.TopRight;
                    break;
            }
        }
        else if (PreviousDirection == Direction.Down)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BoxSymbols.SquareDouble.BottomLeft;
                    break;
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.BottomRight;
                    break;
            }
        }
        else if (PreviousDirection == Direction.Right)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BoxSymbols.SquareDouble.BottomRight;
                    break;
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.TopRight;
                    break;
            }
        }
        else if (PreviousDirection == Direction.Left)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BoxSymbols.SquareDouble.BottomLeft;
                    break;
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.TopLeft;
                    break;
            }
        }
        Path[0] = new Coordinates(Head.X, Head.Y, bodyShape.ToString());
    }

    /// <summary>Moves the snake one step in the specified direction.</summary>
    /// <param name="direction">The direction to move into (must not be <see cref="Direction.None"/>).</param>
    public void Move(Direction direction)
    {
        PreviousDirection = CurrentDirection;
        CurrentDirection = direction;
        StorePosition();
        Coordinates? newPosition = SimulateMove(direction);
        if (newPosition.HasValue)
            Head = newPosition.Value;
    }

    /// <summary>Simulates a move into the specified direction.</summary>
    /// <param name="direction">The direction to move into (must not be <see cref="Direction.None"/>).</param>
    /// <returns>The coordinates at which the snake's head would be if it were to move into the specified direction.</returns>
    public Coordinates? SimulateMove(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Head.OffsetY(-1);
            case Direction.Down:
                return Head.OffsetY(+1);
            case Direction.Left:
                return Head.OffsetX(-1);
            case Direction.Right:
                return Head.OffsetX(+1);
            default:
                return null;
        }
    }

    /// <summary>Draws the snake to the user interface's game area.</summary>
    /// <remarks>
    /// The snake only redraws the characters along its own path over its own length (and possibly
    /// the position of its former tail). It will not overdraw or delete anything else drawn on 
    /// the screen. If you need to clear and redraw the game area afresh, call
    /// <see cref="UserInterface.ClearGameArea"/> before calling <see cref="Draw"/>.
    /// </remarks>
    /// <param name="deleteOldTail">
    /// Whether to delete the tail (the last character) of the snake's last drawing cycle
    /// before redrawing the snake. If set to False, the snake's former tail will persist.
    /// </param>
    public void Draw(bool deleteOldTail = true)
    {
        if (deleteOldTail)
        {
            Coordinates tailPoint = Path[int.Max(0, Length - 1)];
            if (tailPoint.X != 0 && tailPoint.Y != 0)
            {
                Console.SetCursorPosition(tailPoint.X, tailPoint.Y);
                Console.Write(' ');
            }
        }
        var color = 255;
        for (var i = 0; i < Length - 1; i++)
        {
            Console.SetCursorPosition(Path[i].X, Path[i].Y);
            char bodySymbol = DetermineSnakeDrawSymbol(Path[i].Annotation, 'x');
            if (i == Length - 2)
                bodySymbol = ThinOutTail(bodySymbol);
            Console.Write(UserInterface.Ansify(bodySymbol.ToString(), color));
            if (color > 240)
                color--;
        }
        char headSymbol = DetermineSnakeDrawSymbol('ö', 'X', 'Ö');
        Console.SetCursorPosition(Head.X, Head.Y);
        Console.Write(UserInterface.Ansify(headSymbol.ToString(), 15));
    }

    private char ThinOutTail(char symbol)
    {
        return BoxSymbols.Transliterate(symbol, BoxSymbols.SquareDouble, BoxSymbols.SquareSingle);
    }

    private char DetermineSnakeDrawSymbol(string? normal, char dead, char? chomping = null, char fallback = 'o')
    {
        if (normal == null || normal.Length < 1)
        {
            normal = fallback.ToString();
        }
        return DetermineSnakeDrawSymbol(normal.ToCharArray()[0], dead, chomping);
    }

    /// <summary>Checks whether the snake collides with the specified point.</summary>
    /// <param name="point">The point to check for collision.</param>
    /// <param name="includeHead">Whether to include the snake's head position or ignore it.</param>
    /// <returns>True if the snake collides with <i>point</i>, False otherwise.</returns>
    public bool CollidesWithPoint(Coordinates point, bool includeHead = true)
    {
        if (includeHead && Head.X == point.X && Head.Y == point.Y)
            return true;
        int index = Array.FindIndex(Path, 0, Length - 1, coords => coords.X == point.X && coords.Y == point.Y);
        return index >= 0;
    }

    private char DetermineSnakeDrawSymbol(char normal, char dead, char? chomping = null)
    {
        if (!chomping.HasValue)
            chomping = normal;
        switch (State)
        {
            case SnakeState.Dead:
                return dead;
            case SnakeState.Chomping:
                return chomping.Value;
            default:
                return normal;
        }
    }
}
