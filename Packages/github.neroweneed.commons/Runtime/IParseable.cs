namespace NeroWeNeed.Commons {
    public interface IParseable {
        public object Parse(string value);
    }
    public interface IParseable<TOutput> : IParseable {
        public new TOutput Parse(string value);
    }
}