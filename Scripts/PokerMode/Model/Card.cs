namespace PokerMode
{
    public class Card
    {
        public CardVisuals Visuals { get; set; }
        public int Rank { get; set; }
        public Suit Suit { get; set; }
        
        public Card(int rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Card(Card toCopy)
        {
            Rank = toCopy.Rank;
            Suit = toCopy.Suit;
        }
        
        public override string ToString()
        {
            string rank = Rank switch
            {
                14 => "Ace",
                13 => "King",
                12 => "Queen",
                11 => "Jack",
                _ => Rank.ToString()
            };
            return $"{rank} of {Suit}";
        }
    }
}