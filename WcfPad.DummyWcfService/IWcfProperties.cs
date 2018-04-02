using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfPad.DummyWcfService
{
    public interface IWcfProperties
    {
        DummyEnum Enum { get; set; }
        int Int { get; set; }
        string String { get; set; }
        double Double { get; set; }
        bool Bool { get; set; }
        DateTime DateTime { get; set; }
        Dictionary<int, string> Dictionary { get; set; }
        KeyValuePair<double, DateTime> KeyValuePair { get; set; }
        Guid Guid { get; set; }
        TimeSpan TimeSpan { get; set; }
    }
}