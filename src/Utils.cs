namespace Snake;


public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right,
}

public static class DirectionExtension
{
    extension(Direction direction)
    {
        public bool IsOpposite(Direction other)
        {
            if (direction != other
                && direction != Direction.None
                && other != Direction.None
                && ((direction == Direction.Up && other == Direction.Down)
                    || (direction == Direction.Down && other == Direction.Up)
                    || (direction == Direction.Left && other == Direction.Right)
                    || (direction == Direction.Right && other == Direction.Left)))
                return true;
            return false;
        }
    }
}

public readonly struct Coordinates
{
    public Coordinates(int x, int y, string? annotation = null)
    {
        X = x;
        Y = y;
        Annotation = annotation ?? "";
    }

    public int X { get; init; }
    public int Y { get; init; }
    public string Annotation { get; init; }

    public override string ToString()
    {
        if ( Annotation != "" )
            return $"({X}, {Y}; {Annotation})";
        return $"({X}, {Y})";
    }

    public Coordinates OffsetX(int offset)
    {
        return new Coordinates(X + offset, Y);
    }

    public Coordinates OffsetY(int offset)
    {
        return new Coordinates(X, Y + offset);
    }
}

public readonly struct BoxDimensions
{
    public BoxDimensions(int width, int height, Coordinates origin)
    {
        Width = width;
        Height = height;
        Origin = origin;
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public Coordinates Origin { get; init; }

    public int XStart
    {
        get => Origin.X;
    }

    public int XEnd
    {
        get => Origin.X + Width - 1;
    }

    public int YStart
    {
        get => Origin.Y;
    }

    public int YEnd
    {
        get => Origin.Y + Height - 1;
    }

    public int Area
    {
        get => Width * Height;
    }

    public override string ToString()
    {
        return $"({Width} × {Height}; origin: {Origin.X}, {Origin.Y})";
    }

    public BoxDimensions GetInnerDimensions(int offset = 1)
    {
        int width = Width - (2 * offset);
        int height = Height - (2 * offset);
        return new BoxDimensions(width, height, new Coordinates(Origin.X + offset, Origin.Y + offset));
    }

    public bool ContainsPoint(Coordinates point, bool excludeBorder = true)
    {
        if (excludeBorder)
            return point.X > XStart && point.X < XEnd && point.Y > YStart && point.Y < YEnd;
        return point.X >= XStart && point.X <= XEnd && point.Y >= YStart && point.Y <= YEnd;
    }
}

public readonly struct BoxSymbols
{
    public BoxSymbols(char topLeft, char topRight, char bottomLeft, char bottomRight, char vertical, char horizontal, char fill = ' ')
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        Vertical = vertical;
        Horizontal = horizontal;
        Fill = fill;
    }

    public char TopLeft { get; init; }
    public char TopRight { get; init; }
    public char BottomLeft { get; init; }
    public char BottomRight { get; init; }
    public char Vertical { get; init; }
    public char Horizontal { get; init; }
    public char Fill { get; init; }

    public char[] ToArray()
    {
        return [TopLeft, TopRight, BottomLeft, BottomRight, Vertical, Horizontal, Fill];
    }

    public static char Transliterate(char symbol, BoxSymbols fromSet, BoxSymbols toSet)
    {
        int index = Array.FindIndex(fromSet.ToArray(), c => c == symbol);
        if (index >= 0)
            return toSet.ToArray()[index];
        return symbol;
    }

    private static readonly BoxSymbols s_squareSingle = new('┌', '┐', '└', '┘', '│', '─');
    public static BoxSymbols SquareSingle { get => s_squareSingle; }
    private static readonly BoxSymbols s_squareDouble = new('╔', '╗', '╚', '╝', '║', '═');
    public static BoxSymbols SquareDouble { get => s_squareDouble; }
    private static readonly BoxSymbols s_roundSingle = new('╭', '╮', '╰', '╯', '│', '─');
    public static BoxSymbols RoundSingle { get => s_roundSingle; }
}
