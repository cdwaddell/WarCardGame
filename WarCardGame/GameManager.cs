using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyApplication5
{
    public static class Helpers
    {
        private static RNGCryptoServiceProvider _provider = new RNGCryptoServiceProvider();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do _provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class GameManager
    {
        private const int NumberOfPlayers = 2;

        public GameManager()
        {
            Deck = new List<Card>(52);
            Players = new List<Player>(NumberOfPlayers);

            foreach (var suite in (Suite[])Enum.GetValues(typeof(Suite)))
            {
                foreach(var value in Enumerable.Range(2, 13))
                {
                    Deck.Add(new Card(suite, value));
                }
            }
        }

        private List<Card> Deck { get; }
        private List<Player> Players { get; }
        private int CurrentPlayer { get; set; }
        public bool HasEnded { get; internal set; }

        public void Start()
        {
            //shuffle deck
            Deck.Shuffle();

            //create players
            for (int i = 0; i < 2; i++)
            {
                CreatePlayer(i);
            }

            //deal cards
            var count = 0;
            foreach (var card in Deck)
            {
                var playerToGetCard = count % NumberOfPlayers;

                Players[playerToGetCard].ReceiveCard(card);

                count++;
            }

            //pick a player to go first
            CurrentPlayer = 0;
        }

        private void CreatePlayer(int index)
        {
            Console.WriteLine($"What is player {index + 1}'s name?");
            var player1Name = Console.ReadLine();
            Players.Add(item: new Player(player1Name, index));
        }

        public void ProgressTurn()
        {
            int winner = 0;
            var allCardsPlayed = ProgressTurn(Players, ref winner);

            var winningPlayer = Players[winner];
            foreach(var card in allCardsPlayed)
            {
                winningPlayer.ReceiveCard(card.Card);
            }

            Console.WriteLine($"Player {winningPlayer.Name} won!");
            winningPlayer.GamesWon++;

            //Check if game has ended
            var losingPlayers = new List<Player>();
            foreach (var player in Players)
            {
                if (!player.HasCards)
                {
                    losingPlayers.Add(player);
                }
            }

            foreach(var loser in losingPlayers)
            {
                Players.Remove(loser);
            }

            if (Players.Count == 1)
                EndGame(Players.First());

            CurrentPlayer = (CurrentPlayer + 1) % NumberOfPlayers;
        }
        
        public List<PlayerCard> ProgressTurn(List<Player> currentPlayers, ref int winner)
        {
            //pick the next card on the player's deck
            var cardsInPlay = new List<PlayerCard>();
            foreach (var player in currentPlayers)
            {
                var cardToPlay = player.PlayCard();
                cardsInPlay.Add(new PlayerCard(player.Index, cardToPlay));
            }

            //compare the cards
            Card winningCard = null;
            List<PlayerCard> warCards = new List<PlayerCard>();
            foreach(var playedCard in cardsInPlay)
            {
                Console.WriteLine($"Player {Players[playedCard.PlayerIndex].Name} played {playedCard.Card}");

                var card = playedCard.Card;

                if(card == null)
                {
                    continue;
                }    

                if (winningCard == null)
                {
                    winningCard = card;
                    winner = playedCard.PlayerIndex;
                    warCards.Add(playedCard);
                }

                else if (card.Value > winningCard.Value)
                {
                    warCards.Clear();
                    warCards.Add(playedCard);
                    winningCard = card;

                    winner = playedCard.PlayerIndex;
                }

                //war?
                else if (card.Value == winningCard.Value)
                {
                    warCards.Add(playedCard);
                }
            }

            if(warCards.Count > 1)
            {
                Console.WriteLine("War started");
                var allWarCards = ProgressTurn(warCards.Select(pc => Players[pc.PlayerIndex]).ToList(), ref winner);
                cardsInPlay.AddRange(allWarCards);
            }

            return cardsInPlay;
        }

        public void EndGame(Player player)
        {
            HasEnded = true;
            //Declare a winner
            Console.WriteLine($"Player { player.Name } is the winner with { player.GamesWon } victories");
        }
    }

    public class Player
    {
        public Player(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public string Name { get; }
        public int Index { get; }
        private Queue<Card> Hand { get; } = new Queue<Card>();
        private List<Card> NextHand { get; } = new List<Card>();

        public void ReceiveCard(Card card)
        {
            NextHand.Add(card);
        }

        public Card PlayCard()
        {
            if (Hand.Count == 0)
            {
                NextHand.Shuffle();
                foreach (var card in NextHand)
                {
                    Hand.Enqueue(card);
                }
                NextHand.Clear();
            }

            if (Hand.Count == 0)
                return null;

            return Hand.Dequeue();
        }

        public bool HasCards => (Hand.Count + NextHand.Count) != 0;

        public int GamesWon { get; set; }
    }

    public class Card
    {
        public Card(Suite suite, int value)
        {
            Value = value;
            Suite = suite;
        }
        public Suite Suite { get; }
        public int Value { get; }
        public string DisplayName => GetValue();

        private string GetValue()
        {
            switch(Value)
            {
                case (11):
                {
                    return "Jack";
                }
                case (12):
                {
                    return "Queen";
                }
                case (13):
                {
                    return "King";
                }
                case (14):
                {
                    return "Ace";
                }
                default:
                {
                    return Value.ToString();
                }
            }
        }

        public override string ToString()
        {
            return $"{DisplayName} of {Suite}";
        }
    }

    public class PlayerCard
    {
        public PlayerCard(int index, Card card)
        {
            Card = card;
            PlayerIndex = index;
        }
        public int PlayerIndex { get; }
        public Card Card { get; }
    }

    public enum Suite
    {
        Spades,
        Clubs,
        Diamonds,
        Hearts
    }
}
