namespace Snake;
public class Fruit
{
    public Fruit(Coordinates position, char? fruitSymbol = null, int? color = null)
    {
        if (!fruitSymbol.HasValue)
            fruitSymbol = FruitSelection[s_random.Next(0, FruitSelection.Length)];
        if (!color.HasValue)
            color = ColorSelection[s_random.Next(0, ColorSelection.Length)];
        Symbol = fruitSymbol.Value;
        Color = color.Value;
        Position = position;
    }
    private static Random s_random = new();
    private static readonly char[] _fruitSelection = ['•', '◦', '▴', '■', '□', '᛭', '⨯', 'ꚛ', '★', '☆'];
    public static char[] FruitSelection { get => _fruitSelection; }
    private static readonly int[] _colorSelection = [8, 9, 10, 11, 12, 13, 14, 15];
    public static int[] ColorSelection { get => _colorSelection; }
    public char Symbol { get; init; }
    public int Color { get; init; }
    public Coordinates Position { get; init; }

    public void Draw()
    {
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(UserInterface.Ansify(Symbol.ToString(), color: Color));
    }

    public static Coordinates FindSpawnPosition(Snake Snake)
    {
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        while (true)
        {
            var trial = new Coordinates(
                s_random.Next(innerGameArea.XStart, innerGameArea.XEnd),
                s_random.Next(innerGameArea.YStart, innerGameArea.YEnd)
            );
            if (!Snake.CollidesWithPoint(trial))
                return trial;
        }
    }

}
