using Newtonsoft.Json;

namespace TranscriptReader.Transcript {
  /// <summary>
  /// Represents word instances as read from the transcript file.
  /// Includes additions of Text and Channel properties
  /// </summary>
  public class WordInstanceObj {
    /// <summary>
    /// The text of the word.  As this is not part of the wordInstance definition in the Json, this is ignored for serialization.
    /// </summary>
    [JsonIgnore]
    public string Text { get; set; }

    /// <summary>
    /// The channel in which the speaker was recorded on
    /// </summary>
    [JsonIgnore]
    public int Channel { get; set; }

    /// <summary>
    /// The Id of the word
    /// </summary>
    [JsonProperty("phrase")]
    public string Id { get; set; }

    /// <summary>
    /// The confidence score 0 - 100 of the word
    /// </summary>
    [JsonProperty("score")]
    public int Score { get; set; }

    /// <summary>
    /// Start time in milliseconds of the word instance
    /// </summary>
    [JsonProperty("startMs")]
    public long StartTime { get; set; }

    /// <summary>
    /// End time in milliseconds of the word instance.
    /// </summary>
    [JsonProperty("endTime")]
    public long EndTime { get; set; }
  }
}
