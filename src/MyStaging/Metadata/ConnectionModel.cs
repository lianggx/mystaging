namespace MyStaging.Metadata
{
    public class ConnectionModel
    {
        public bool ReadOnly { get; set; }

        public string ConnectionString { get; set; }

        public long Used { get; internal set; }

        public long Error { get; internal set; }
    }
}
