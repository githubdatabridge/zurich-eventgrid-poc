namespace Shared.Lib.Services
{
    public class BlobEvent
    {
        public string Topic { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public string Id { get; set; }
        public BlobEventData Data { get; set; }
        public string DataVersion { get; set; }
        public string MetadataVersion { get; set; }
        public DateTime EventTime { get; set; }
        public class BlobEventData
        {
            public string Api { get; set; }
            public string ClientRequestId { get; set; }
            public string RequestId { get; set; }
            public string ETag { get; set; }
            public string ContentType { get; set; }
            public int ContentLength { get; set; }
            public string BlobType { get; set; }
            public string Url { get; set; }
            public string Sequencer { get; set; }
        }

        public string FileName => Subject.Split('/').Last();
    }
}