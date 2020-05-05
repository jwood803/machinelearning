using System;
using Microsoft.ML.Runtime;

namespace Microsoft.ML.TestFrameworkCommon.Attributes
{
    public sealed class LogMessageKind : Attribute
    {
        public ChannelMessageKind MessageKind { get; }
        public LogMessageKind(ChannelMessageKind messageKind)
        {
            MessageKind = messageKind;
        }
    }
}
