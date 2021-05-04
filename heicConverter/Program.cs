using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace heicConverter
{
  public enum Format
  {
    JPG,
    JPEG = JPG,
    PNG,
    BMP,
    TIFF
  }

  public class Program
  {
    public static Dictionary<Format, Guid> Encoders = new Dictionary<Format, Guid>()
    {
      { Format.JPEG, BitmapEncoder.JpegEncoderId },
      { Format.PNG , BitmapEncoder.PngEncoderId  },
      { Format.BMP , BitmapEncoder.BmpEncoderId  },
      { Format.TIFF, BitmapEncoder.TiffEncoderId },
    };

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

    // https://github.com/ejohnson-dotnet/heic2jpg/blob/master/Program.cs
    public static async Task Convert(string input, Format format)
    {
      try
      {
        var wInputFile = await StorageFile.GetFileFromPathAsync(input);
        using (var wInputStream = await wInputFile.OpenReadAsync())
        {
          var wDecoder = await BitmapDecoder.CreateAsync(wInputStream);
          var wBitmap = await wDecoder.GetSoftwareBitmapAsync();
          var wOutputName = Path.GetFileName(Path.ChangeExtension(input, $".{format}".ToLower()));
          var wParentFolder = await wInputFile.GetParentAsync();
          var wOutputFile = await wParentFolder.CreateFileAsync(wOutputName, CreationCollisionOption.ReplaceExisting);
          using (var wOutputStream = await wOutputFile.OpenAsync(FileAccessMode.ReadWrite))
          {
            var wEncoder = await BitmapEncoder.CreateAsync(Encoders[format], wOutputStream);
            wEncoder.SetSoftwareBitmap(wBitmap);
            wEncoder.IsThumbnailGenerated = true;
            try
            {
              await wEncoder.FlushAsync();
            }
            catch (Exception e)
            {
              const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
              switch (e.HResult)
              {
                case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                  // If the encoder does not support writing a thumbnail, then try again
                  // but disable thumbnail generation.
                  wEncoder.IsThumbnailGenerated = false;
                  break;
                default:
                  throw;
              }
            }
            if (wEncoder.IsThumbnailGenerated == false)
            {
              await wEncoder.FlushAsync();
            }
          }
          var wPropertiesToRetrieve = SystemProperties.Union(SystemGpsProperties).Union(SystemImageProperties).Union(SystemPhotoProperties).ToList();
          var wProperties = await wInputFile.Properties.RetrievePropertiesAsync(wPropertiesToRetrieve);
          foreach (var wProperty in wProperties)
          {
            try
            {
              await wOutputFile.Properties.SavePropertiesAsync(new[] { wProperty });
            }
            catch (Exception e)
            {
              Console.WriteLine($"Key: {wProperty.Key}, Value: {wProperty.Value}");
              Console.WriteLine(e.ToString());
            }
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }

    // https://docs.microsoft.com/en-us/windows/win32/wic/system
    public static string[] SystemProperties =
    {
      //"System.ApplicationName",
      "System.Author",
      "System.Comment",
      "System.Copyright",
      "System.DateAcquired",
      "System.Keywords",
      "System.Rating",
      //"System.SimpleRating",
      "System.Subject",
      "System.Title"
    };

    // https://docs.microsoft.com/en-us/windows/win32/wic/system-gps
    public static string[] SystemGpsProperties =
    {
      //"System.GPS.Altitude",
      "System.GPS.AltitudeDenominator",
      "System.GPS.AltitudeNumerator",
      "System.GPS.AltitudeRef",
      "System.GPS.AreaInformation",
      "System.GPS.Date",
      //"System.GPS.DestBearing",
      "System.GPS.DestBearingDenominator",
      "System.GPS.DestBearingNumerator",
      "System.GPS.DestBearingRef",
      //"System.GPS.DestDistance",
      "System.GPS.DestDistanceDenominator",
      "System.GPS.DestDistanceNumerator",
      "System.GPS.DestDistanceRef",
      //"System.GPS.DestLatitude",
      "System.GPS.DestLatitudeDenominator",
      "System.GPS.DestLatitudeNumerator",
      "System.GPS.DestLatitudeRef",
      //"System.GPS.DestLongitude",
      "System.GPS.DestLongitudeDenominator",
      "System.GPS.DestLongitudeNumerator",
      "System.GPS.DestLongitudeRef",
      "System.GPS.Differential",
      //"System.GPS.DOP",
      "System.GPS.DOPDenominator",
      "System.GPS.DOPNumerator",
      //"System.GPS.ImgDirection",
      "System.GPS.ImgDirectionDenominator",
      "System.GPS.ImgDirectionNumerator",
      "System.GPS.ImgDirectionRef",
      //"System.GPS.Latitude",
      //"System.GPS.LatitudeDecimal",
      "System.GPS.LatitudeDenominator",
      "System.GPS.LatitudeNumerator",
      "System.GPS.LatitudeRef",
      //"System.GPS.Longitude",
      //"System.GPS.LongitudeDecimal",
      "System.GPS.LongitudeDenominator",
      "System.GPS.LongitudeNumerator",
      "System.GPS.LongitudeRef",
      "System.GPS.MapDatum",
      "System.GPS.MeasureMode",
      "System.GPS.ProcessingMethod",
      "System.GPS.Satellites",
      //"System.GPS.Speed",
      "System.GPS.SpeedDenominator",
      "System.GPS.SpeedNumerator",
      "System.GPS.SpeedRef",
      "System.GPS.Status",
      //"System.GPS.Track",
      "System.GPS.TrackDenominator",
      "System.GPS.TrackNumerator",
      "System.GPS.TrackRef",
      "System.GPS.VersionID"
    };

    // https://docs.microsoft.com/en-us/windows/win32/wic/system-image
    public static string[] SystemImageProperties =
    {
      //"System.Image.BitDepth",
      //"System.Image.ColorSpace",
      //"System.Image.CompressedBitsPerPixel",
      "System.Image.CompressedBitsPerPixelDenominator",
      "System.Image.CompressedBitsPerPixelNumerator",
      //"System.Image.Compression",
      //"System.Image.CompressionText",
      //"System.Image.Dimensions",
      //"System.Image.HorizontalResolution",
      //"System.Image.HorizontalSize",
      //"System.Image.ImageID",
      //"System.Image.ResolutionUnit",
      //"System.Image.VerticalResolution"
      //"System.Image.VerticalSize"
    };

    // https://docs.microsoft.com/en-us/windows/win32/wic/system-photo
    public static string[] SystemPhotoProperties =
    {
      //"System.Photo.Aperture",
      "System.Photo.ApertureDenominator",
      "System.Photo.ApertureNumerator",
      //"System.Photo.Brightness",
      "System.Photo.BrightnessDenominator",
      "System.Photo.BrightnessNumerator",
      "System.Photo.CameraManufacturer",
      "System.Photo.CameraModel",
      "System.Photo.CameraSerialNumber",
      "System.Photo.Contrast",
      //"System.Photo.ContrastText",
      "System.Photo.DateTaken",
      //"System.Photo.DigitalZoom",
      "System.Photo.DigitalZoomDenominator",
      "System.Photo.DigitalZoomNumerator",
      "System.Photo.Event",
      "System.Photo.EXIFVersion",
      //"System.Photo.ExposureBias",
      "System.Photo.ExposureBiasDenominator",
      "System.Photo.ExposureBiasNumerator",
      //"System.Photo.ExposureIndex",
      "System.Photo.ExposureIndexDenominator",
      "System.Photo.ExposureIndexNumerator",
      "System.Photo.ExposureProgram",
      //"System.Photo.ExposureProgramText",
      //"System.Photo.ExposureTime",
      "System.Photo.ExposureTimeDenominator",
      "System.Photo.ExposureTimeNumerator",
      "System.Photo.Flash",
      //"System.Photo.FlashEnergy",
      "System.Photo.FlashEnergyDenominator",
      "System.Photo.FlashEnergyNumerator",
      "System.Photo.FlashManufacturer",
      "System.Photo.FlashModel",
      //"System.Photo.FlashText",
      //"System.Photo.FNumber",
      "System.Photo.FNumberDenominator",
      "System.Photo.FNumberNumerator",
      //"System.Photo.FocalLength",
      "System.Photo.FocalLengthDenominator",
      "System.Photo.FocalLengthInFilm",
      "System.Photo.FocalLengthNumerator",
      //"System.Photo.FocalPlaneXResolution",
      "System.Photo.FocalPlaneXResolutionDenominator",
      "System.Photo.FocalPlaneXResolutionNumerator",
      //"System.Photo.FocalPlaneYResolution",
      "System.Photo.FocalPlaneYResolutionDenominator",
      "System.Photo.FocalPlaneYResolutionNumerator",
      //"System.Photo.GainControl",
      "System.Photo.GainControlDenominator",
      "System.Photo.GainControlNumerator",
      //"System.Photo.GainControlText",
      "System.Photo.ISOSpeed",
      "System.Photo.LensManufacturer",
      "System.Photo.LensModel",
      "System.Photo.LightSource",
      "System.Photo.MakerNote",
      "System.Photo.MakerNoteOffset",
      //"System.Photo.MaxAperture",
      "System.Photo.MaxApertureDenominator",
      "System.Photo.MaxApertureNumerator",
      "System.Photo.MeteringMode",
      //"System.Photo.MeteringModeText",
      //"System.Photo.Orientation",
      //"System.Photo.OrientationText",
      //"System.Photo.PeopleNames",
      //"System.Photo.PhotometricInterpretation",
      //"System.Photo.PhotometricInterpretationText",
      "System.Photo.ProgramMode",
      //"System.Photo.ProgramModeText",
      //"System.Photo.RelatedSoundFile",
      "System.Photo.Saturation",
      //"System.Photo.SaturationText",
      "System.Photo.Sharpness",
      //"System.Photo.SharpnessText",
      //"System.Photo.ShutterSpeed",
      "System.Photo.ShutterSpeedDenominator",
      "System.Photo.ShutterSpeedNumerator",
      //"System.Photo.SubjectDistance",
      "System.Photo.SubjectDistanceDenominator",
      "System.Photo.SubjectDistanceNumerator",
      //"System.Photo.TagViewAggregate",
      "System.Photo.TranscodedForSync",
      "System.Photo.WhiteBalance",
      //"System.Photo.WhiteBalanceText"
    };
  }
}
