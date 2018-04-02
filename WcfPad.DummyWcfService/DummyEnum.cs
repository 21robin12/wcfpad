using System.Runtime.Serialization;

namespace WcfPad.DummyWcfService
{
    [DataContract]
    public enum DummyEnum
    {
        [EnumMember]
        First = 1,

        [EnumMember]
        Second = 2
    }
}