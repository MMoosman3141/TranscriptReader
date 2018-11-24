using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TranscriptReader.Transcript {
  /// <summary>
  /// Transcript object read from the transcript josn.
  /// Include method to create a new transcript object form a file, as well as methods to get segments and worst words.
  /// </summary>
  public class TranscriptObj {
    private const int WOST_WORD_BOUND = 10;

    /// <summary>
    /// List of channel objects.  Generally speaking there should only be 1 or 2 channels for any given transcript.
    /// </summary>
    [JsonProperty("channels")]
    List<ChannelObj> Channels { get; set; }

    /// <summary>
    /// The name of a transcript json file to be deserialized.
    /// </summary>
    /// <param name="filename">The name of a transcript json file to be deserialized</param>
    public static TranscriptObj Create(string filename) {
      if(!File.Exists(filename)) {
        throw new FileNotFoundException($"Unable to located specified file '{filename}'");
      }

      TranscriptObj transcript = JsonConvert.DeserializeObject<TranscriptObj>(File.ReadAllText(filename));

      //Put the text of each word into its appropriate wordInstance
      foreach(ChannelObj channel in transcript.Channels) {
        foreach(WordInstanceObj wordInstance in channel.WordInstances) {
          wordInstance.Text = channel.Words[wordInstance.Id];
          wordInstance.Channel = channel.Channel;
        }
      }

      return transcript;
    }

    /// <summary>
    /// Method to retrieve the speaker segments from the transcript.
    /// </summary>
    /// <returns>A List of SpeakerSegment objects representing speaker turns in order</returns>
    public List<SpeakerSegment> GetSegments() {
      //Get all wordInstances in the correct order, first by start time then by end time.
      IEnumerable<WordInstanceObj> allWordInstances = from channel in Channels
                                                      from wordInstance in channel.WordInstances
                                                      orderby wordInstance.StartTime, wordInstance.EndTime
                                                      select wordInstance;

      List<SpeakerSegment> segments = new List<SpeakerSegment>();

      SpeakerSegment segment = new SpeakerSegment();
      foreach(WordInstanceObj wordInst in allWordInstances) {
        if(!segment.TryAdd(wordInst)) {  //TryAdd returns false if the channel changes without adding the word to the segment
          segments.Add(segment);
          segment = new SpeakerSegment();
          segment.TryAdd(wordInst);
        }
      }

      return segments;
    }

    /// <summary>
    /// Method to retrieve the words with the lowest confidence values.
    /// </summary>
    /// <returns>A list of WordInstance objects which have the worst confidence values</returns>
    public List<WordInstanceObj> GetWorstWords() {
      IEnumerable<int> worstTenScores = (from channel in Channels
                                         from wordInstance in channel.WordInstances
                                         orderby wordInstance.Score descending
                                         select wordInstance.Score).Take(10);

      int worstScore = worstTenScores.Max();

      IEnumerable<WordInstanceObj> worstWords = from channel in Channels
                                                from wordInstance in channel.WordInstances
                                                where wordInstance.Score <= worstScore
                                                orderby wordInstance.Score descending
                                                select wordInstance;

      return worstWords.ToList();
    }

  } //class
} //namespace
