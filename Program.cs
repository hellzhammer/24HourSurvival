
using Microsoft.Xna.Framework;

public class Program
{
    static Game game;
    public static void Main(string[] args)
    {
        game = new _24HourSurvival.SimpleSurvival();
        game.Run();
    }

    public static void SwitchGame(Game _game)
    {
        game.Dispose();
        game = _game;
        game.Run();
    }
}
