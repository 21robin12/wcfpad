using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WcfPad.DummyWcfService
{
    [DataContract]
    public class Response : IWcfProperties
    {
        [DataMember]
        public DummyEnum Enum { get; set; }

        [DataMember]
        public int Int { get; set; }

        [DataMember]
        public string String { get; set; }

        [DataMember]
        public double Double { get; set; }

        [DataMember]
        public bool Bool { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public ResponseItem Item { get; set; }

        [DataMember]
        public IEnumerable<ResponseItem> ItemsEnumerable { get; set; }

        [DataMember]
        public IList<ResponseItem> ItemsList { get; set; }

        [DataMember]
        public ResponseItem[] ItemsArray { get; set; }

        [DataMember]
        public Dictionary<int, string> Dictionary { get; set; }

        [DataMember]
        public KeyValuePair<double, DateTime> KeyValuePair { get; set; }

        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }
    }

    [DataContract]
    public class ResponseItem : IWcfProperties
    {
        [DataMember]
        public DummyEnum Enum { get; set; }

        [DataMember]
        public int Int { get; set; }

        [DataMember]
        public string String { get; set; }

        [DataMember]
        public double Double { get; set; }

        [DataMember]
        public bool Bool { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public Dictionary<int, string> Dictionary { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

        [DataMember]
        public KeyValuePair<double, DateTime> KeyValuePair { get; set; }

        [DataMember]
        public Guid Guid { get; set; }
    }
}