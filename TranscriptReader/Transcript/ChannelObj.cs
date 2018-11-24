using Newtonsoft.Json;
using System.Collections.Generic;

namespace TranscriptReader.Transcript {
  /// <summary>
  /// Channel object read from the transcript json
  /// </summary>
  public class ChannelObj {
    /// <summary>
    /// The channel used when recroding the speaker.  Channel 0 will refer to the patron, and channel 1 the agent.
    /// This is not the specification within the homework text though, which says the patron will be channel 0 and the client will be channel 1
    /// </summary>
    [JsonProperty("channel")]
    public int Channel { get; set; }

    /// <summary>
    /// A dictionary of words with the word Id as the Key, and the word text as the value.
    /// </summary>
    [JsonProperty("word")]
    public Dictionary<string, string> Words { get; set; }

    /// <summary>
    /// Information for each word including time offsets, and confidence scores.
    /// </summary>
    [JsonProperty("wordInstance")] //The example in the homework is inconsistant with what this object is called -> "wordInstance" vs. "wordData"  I'm assuming "wordInstance" as this is the more filled out example
    public List<WordInstanceObj> WordInstances { get; set; }
  }
}
