using AccountDownloaderLibrary.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Test;

public class MimeDetector_GetFileExtensionByMimeType
{
    [Theory]
    [InlineData("png", "image/png")]
    [InlineData("mp4", "video/mp4")]
    [InlineData("tga", "image/x-tga")]
    [InlineData("hdr", "image/vnd.radiance")]
    [InlineData("pdd", "image/vnd.adobe.photoshop")]
    [InlineData("bmp", "image/bmp")]
    [InlineData("mp3", "audio/mpeg3")]
    [InlineData("tif", "image/tiff")]
    [InlineData("ogg", "audio/ogg")]
    [InlineData("meshx", "application/meshx")]
    [InlineData("zip", "application/zip")]
    [InlineData("webp", "image/webp")]
    [InlineData("flac", "audio/flac")]
    [InlineData("exr", "image/x-exr")]
    [InlineData("jpg", "image/jpeg")]
    public void GetFileExtensionByMimeType_ValidMimeType_ReturnsTheAssociatedExtension(string expectedExt, string mimeType)
    {
        Assert.Equal(expectedExt, MimeDetector.Instance.GetFileExtensionByMimeType(mimeType));
    }

    [Fact]
    public void GetFileExtensionByMimeType_OctetStream_ReturnsNull()
    {
        Assert.Null(MimeDetector.Instance.GetFileExtensionByMimeType("application/octet-stream"));
    }

    [Fact]
    public void GetFileExtensionByMimeType_NullOrEmptyString_ReturnsNull()
    {
        Assert.Null(MimeDetector.Instance.GetFileExtensionByMimeType(""));
        Assert.Null(MimeDetector.Instance.GetFileExtensionByMimeType(null));
    }
}
