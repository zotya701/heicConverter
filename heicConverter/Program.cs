using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace heicConverter
{
  public enum Format
  {
    JPG,
    JPEG = JPG,
    PNG,
    BMP
  }

  public class Program
  {
    public static async Task<int> Main(string[] args)
    {
      var wRootCommand = new RootCommand("Converts .heic file(s) to the desired format.");

      wRootCommand.AddOption(new Option(new string[] { "--input", "-i" })
      {
        Description = "The path to the image file(s) that is(are) to be converted.",
        Argument = new Argument<string>(),
        Required = true
      });

      wRootCommand.AddOption(new Option(new string[] { "--format", "-f" })
      {
        Description = "The format to convert to.",
        Argument = new Argument<Format>(),
        Required = true
      });

      wRootCommand.Handler = CommandHandler.Create<string, Format>(Convert);

      return await wRootCommand.InvokeAsync(args);
    }

    public static void Convert(string input, Format format)
    {

    }
  }
}
