namespace BitBotterich.util
{
    public class TenorStructs
    {
        public struct Result
        {
            public List<ResultItem> results {  get; set; }
        }

        public struct ResultItem
        {
            public MediaFormats media_formats { get; set; }
        }

        public struct MediaFormats
        {
            public Gif gif { get; set; }
        }

        public struct Gif
        {
            public string url { get; set; }
        }
    }
}
