using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XFCovidTrack.Models
{
    public class MessageContract
    {
        [JsonProperty("Origintime")]
        public DateTime Origintime { get; set; }

        [JsonProperty("Type")]
        public MessageTypeEnum Type { get; set; }

        [JsonProperty("Data")]
        public string Data { get; set; }      

        [JsonProperty("Destination")]
        public string Destination { get; set; }

        [JsonProperty("Source")]
        public string Source { get; set; }

        [JsonProperty("messageIdeMessageIdentifierntifier")]
        public int MessageIdentifier { get; set; }
    }

    public enum MessageTypeEnum
    {
        A,
        B,
        C,
        D,
        S
    }
}
