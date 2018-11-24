using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranscriptReader.Transcript;

namespace TranscriptReader {
  /// <summary>
  /// This application takes a transcript json file and produces 3 files from it:
  /// 1 - transcript.txt.  This file contains a human readable dialog from the transcription, indicating the Patron and Agent speaker for each turn.
  /// 2 - snippet.txt.  This file will contain a single exchange between the patron and the agent.  The selected exchange is the one with the best confidence for the 2 segments.
  /// 3 - worst.txt.  This file contains a list of words whith the 10 lowest confidence scores (note: This is the 10 lowest confidence scores, not the 10 worst words).
  /// </summary>
  class Program {
    /// <summary>
    /// This is the entry point of the application.
    /// </summary>
    /// <param name="args">A list of command line arguments.  In this case, 1 argument is required, and only 1, which must be the name of the json file containing the transcript.</param>
    static void Main(string[] args) {
      if(args.Length != 1) {
        ShowHelp();
        return;
      }
      if(!File.Exists(args[0])) {
        Console.WriteLine($"Unable to locate file: '{args[0]}'");
        ShowHelp();
        return;
      }

      try {
        TranscriptObj transcript = TranscriptObj.Create(args[0]);
        List<SpeakerSegment> segments = transcript.GetSegments().OrderBy(seg => seg.StartTime).ThenBy(seg => seg.EndTime).ToList();

        Task writeTranscriptTask = WriteTranscriptAsync("transcript.txt", segments);
        Task writeSnippetTask = WriteSnippetAsync("snippet.txt", segments);
        Task writeWorstTask = WriteWorstAsync("worst.txt", transcript.GetWorstWords());

        Task.WaitAll(writeTranscriptTask, writeSnippetTask, writeWorstTask);
      } catch(Exception err) {
        Trace.WriteLine(err.ToString());
      }
    }

    /// <summary>
    /// This method writes the human readable transcript.
    /// </summary>
    /// <param name="filename">The name of the file which should be written.  If the file already exists it will be overwritten.  If the file is in use, an error will occur.</param>
    /// <param name="segments">The list of ordered segments to be written to the file.</param>
    /// <returns>A the Task object performing the action.</returns>
    private static Task WriteTranscriptAsync(string filename, List<SpeakerSegment> segments) {
      return Task.Run(() => {
        using(FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          using(StreamWriter writer = new StreamWriter(fStream, new UTF8Encoding(false))) {
            foreach(SpeakerSegment segment in segments) {
              writer.WriteLine(segment.ToString());
            }
          }
        }
      });
    }

    /// <summary>
    /// This method writes the best pair of segments, as determined by their averaged, and confidence scores.
    /// </summary>
    /// <param name="filename">The name of the file which should be written.  If the file already exists it will be overwritten.  If the file is in use, an error will occur.</param>
    /// <param name="segments">The list of ordered segments within which the best pair will be found and written to the file.</param>
    /// <returns>A the Task object performing the action.</returns>
    private static Task WriteSnippetAsync(string filename, List<SpeakerSegment> segments) {
      return Task.Run(() => {
        //Find the best pair of segments
        (SpeakerSegment first, SpeakerSegment second, double confidence) bestSegments = (segments[0], segments[1], (segments[0].Confidence + segments[1].Confidence) / 2);
        for(int index = 1; index < segments.Count - 1; index++) {
          double confidence = (segments[index].Confidence + segments[index + 1].Confidence) / 2;

          if(confidence > bestSegments.confidence) {
            bestSegments = (segments[index], segments[index + 1], confidence);
          }
        }

        //Write the best segments
        using(FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          using(StreamWriter writer = new StreamWriter(fStream, new UTF8Encoding(false))) {
            writer.WriteLine(bestSegments.first.ToString());
            writer.WriteLine(bestSegments.second.ToString());
          }
        }

      });
    }

    /// <summary>
    /// This method writes the words for the worst 10 confidence scores to a file.
    /// If multiple words are associated with a single score within this range, each of the words will be written out.
    /// </summary>
    /// <param name="filename">The name of the file which should be written.  If the file already exists it will be overwritten.  If the file is in use, an error will occur.</param>
    /// <param name="words">The list of words to write to the file</param>
    /// <returns>A the Task object performing the action.</returns>
    private static Task WriteWorstAsync(string filename, List<WordInstanceObj> words) {
      return Task.Run(() => {
        using(FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          using(StreamWriter writer = new StreamWriter(fStream, new UTF8Encoding(false))) {
            foreach(WordInstanceObj word in words) {
              writer.WriteLine($"{word.Text} {word.Score}");
            }
          }
        }
      });
    }

    static void ShowHelp() {
      Console.WriteLine("This application requires one parameter, of the transcript json file to be processed.");
    }

  } //class
} //namespace
