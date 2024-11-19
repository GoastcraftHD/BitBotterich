using Newtonsoft.Json;

namespace BitBotterich.util
{
    public static class QuoteHelper
    {
        public struct Quote
        {
            public string Text;
            public string Name;
            public int Count;
        }

        public static Dictionary<string, Quote> QuoteCounter = new Dictionary<string, Quote>();

        public static void SaveQuotes()
        {
            string quoteJson = JsonConvert.SerializeObject(QuoteCounter);
            File.WriteAllText("./data/quotes.json", quoteJson);
        }

        public static void LoadQuotes()
        {
            string quoteJson = File.ReadAllText("./data/quotes.json");
            QuoteCounter = JsonConvert.DeserializeObject<Dictionary<string, Quote>>(quoteJson);
        }

        public static string CreateQuoteText(Quote quote)
        {
            return $"{quote.Text}\n" +
                   $"-{quote.Name}\n" +
                   $"({quote.Count})";
        }
    }
}
