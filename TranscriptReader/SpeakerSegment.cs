using System.Collections.Generic;
using System.Linq;
using System.Text;
using TranscriptReader.Transcript;

namespace TranscriptReader {
  /// <summary>
  /// SpeakerSegment objects are meant to store a segment of speaker text assembled from individual WordInstanceObj items.
  /// </summary>
  public class SpeakerSegment {
    /// <summary>
    /// The channel the speaker was recorded on.
    /// </summary>
    public int Channel { get; private set; }

    /// <summary>
    /// The average confidence of all tokens that are part of the segment.
    /// </summary>
    public double Confidence { get; private set; }

    /// <summary>
    /// The start time of the first token in the segment
    /// </summary>
    public long StartTime { get; private set; } = -1;

    /// <summary>
    /// The end time of the last token in the segment.
    /// </summary>
    public long EndTime { get; private set; } = -1;

    /// <summary>
    /// The word instances that make up the segment.
    /// </summary>
    private List<WordInstanceObj> WordInstances { get; set; } = new List<WordInstanceObj>();

    /// <summary>
    /// Indexer, allowing index value access to the words within the segment.
    /// </summary>
    /// <param name="i">The index of the word instance to retreive</param>
    /// <returns>The word instance stored at index i</returns>
    public WordInstanceObj this[int i] {
      get => WordInstances[i];
      private set => WordInstances[i] = value;
    }

    /// <summary>
    /// Attempts to add a WordInstanceObj to the segment.  If the wordInstance is the first instance being stored in the segment, it establishes the channel for the segment.
    /// If the word instance is not the first instance stored, it will only be added if the channel matches the channel of previously added word instances.
    /// As new word instances are added, the segment confidence, start time, and end time are adjusted.
    /// </summary>
    /// <param name="wordInstance">The WordInstanceObj to attempt to add</param>
    /// <returns>true if the wordInstance is successfully added, false if the wordInstance cannot be added due to a channel conflict</returns>
    public bool TryAdd(WordInstanceObj wordInstance) {
      if(WordInstances.Count == 0) {
        Channel = wordInstance.Channel;
      } else if(Channel != wordInstance.Channel) {
        return false;
      }

      WordInstances.Add(wordInstance);

      Confidence += (Confidence - wordInstance.Score) / WordInstances.Count;

      if(StartTime == -1 || wordInstance.StartTime < StartTime) {
        StartTime = wordInstance.StartTime;
      }
      if(EndTime == -1 || wordInstance.EndTime > EndTime) {
        EndTime = wordInstance.EndTime;
      }

      return true;
    }

    /// <summary>
    /// Override of the ToString method.
    /// Creates a string representation of the wordInstances within the segment, prefixed as either "Patron: " or "Agent: " depending on the channel.
    /// All word instances are lowercased.
    /// </summary>
    /// <returns>A string value representing the segment of text.</returns>
    public override string ToString() {
      StringBuilder segStr = new StringBuilder(Channel == 0 ? "Patron: " : "Agent: ");

      segStr.AppendLine(string.Join(" ", WordInstances.Select(word => word.Text.ToLower())));
      segStr.AppendLine();

      return segStr.ToString();
    }
  }
}
