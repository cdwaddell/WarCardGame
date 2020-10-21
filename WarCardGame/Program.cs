using System;

namespace MyApplication5
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new GameManager();
            game.Start();

            while(!game.HasEnded)
            {
                game.ProgressTurn();
            }
        }
    }
}
