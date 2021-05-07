using System;
using System.Threading.Tasks;

namespace heicConverterCLI
{
  public class Program
  {
    public static async Task<int> Main(string[] args)
    {
      return await heicConverter.Program.Main(args);
    }
  }
}
