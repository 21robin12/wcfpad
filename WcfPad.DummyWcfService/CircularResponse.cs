using System.Runtime.Serialization;

namespace WcfPad.DummyWcfService
{
    [DataContract]
    public class CircularResponse
    {
        [DataMember]
        public CircularResponseItem Item { get; set; }
    }

    [DataContract(IsReference = true)]
    public class CircularResponseItem
    {
        [DataMember]
        public CircularResponseItem Child { get; set; }

        [DataMember]
        public int Value { get; set; }
    }
}