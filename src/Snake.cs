
namespace Snake;

public class Snake
{
    public Snake(int maxLength, Coordinates spawnPosition)
    {
        Length = 1;
        Path = new Coordinates[maxLength];
        Head = spawnPosition;
        CurrentDirection = Direction.None;
        PreviousDirection = Direction.None;
        State = SnakeState.Normal;
    }

    public int Length { get; set; }
    public Coordinates[] Path { get; set; }
    public Coordinates Head { get; set; }
    public Direction CurrentDirection { get; set; }
    public Direction PreviousDirection { get; set; }
    public SnakeState State { get; set; }

    private void ShiftPathArrayRight()
    {
        for (int i = Path.Length - 2; i >= 0; i--)
            Path[i + 1] = Path[i];
    }

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

    public void Move(Direction direction)
    {
        PreviousDirection = CurrentDirection;
        CurrentDirection = direction;
        StorePosition();
        Coordinates? newPosition = SimulateMove(direction);
        if (newPosition.HasValue)
            Head = newPosition.Value;
    }

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

public enum SnakeState
{
    Normal,
    Chomping,
    Dead
}
