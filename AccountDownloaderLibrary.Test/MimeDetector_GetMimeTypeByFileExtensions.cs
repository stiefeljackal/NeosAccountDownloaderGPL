using AccountDownloaderLibrary.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Test;

public class MimeDetector_GetMimeTypeByFileExtensions
{
    [Theory]
    [InlineData("image/png", "png")]
    [InlineData("video/mp4", "mp4")]
    [InlineData("image/x-tga", "tga")]
    [InlineData("application/octet-stream", "hdr")]
    [InlineData("image/vnd.adobe.photoshop", "pdd")]
    [InlineData("image/bmp", "bmp")]
    [InlineData("audio/mpeg3", "mp3")]
    [InlineData("image/tiff", "tif")]
    [InlineData("application/ogg", "ogg")]
    [InlineData("application/meshx", "meshx")]
    [InlineData("application/octet-stream", "zip")]
    [InlineData("image/webp", "webp")]
    [InlineData("audio/x-flac", "flac")]
    [InlineData("image/x-exr", "exr")]
    [InlineData("image/jpeg", "jpg")]
    [InlineData("application/x-lzma-stream", "7zbson")]
    [InlineData("application/octet-stream", "mkv")]
    [InlineData("application/octet-stream", "dds")]
    [InlineData("application/octet-stream", "dbf")]
    [InlineData("application/octet-stream", "fbx")]
    [InlineData("application/octet-stream", "animx")]
    [InlineData("application/octet-stream", "jpc")]
    public void GetMimeTypeByFileExtensions_ValidExtension_ReturnsTheAssociatedMimeType(string expectedMimeType, string extension)
    {
        Assert.Equal(expectedMimeType, MimeDetector.Instance.GetMimeTypeByFileExtension(extension));
    }

    [Fact]
    public void GetMimeTypeByFileExtensions_NullOrEmptyString_ReturnsNull()
    {
        Assert.Null(MimeDetector.Instance.GetFileExtensionByMimeType(""));
        Assert.Null(MimeDetector.Instance.GetFileExtensionByMimeType(null));
    }
}
