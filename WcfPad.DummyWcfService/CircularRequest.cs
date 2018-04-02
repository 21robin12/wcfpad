using System.Runtime.Serialization;

namespace WcfPad.DummyWcfService
{
    [DataContract]
    public class CircularRequest
    {
        [DataMember]
        public CircularRequestItem Item { get; set; }
    }

    [DataContract]
    public class CircularRequestItem
    {
        [DataMember]
        public CircularRequestItem2 Child { get; set; }

        [DataMember]
        public int Value { get; set; }
    }

    [DataContract]
    public class CircularRequestItem2
    {
        [DataMember]
        public CircularRequestItem Child { get; set; }

        [DataMember]
        public int Value { get; set; }
    }
}