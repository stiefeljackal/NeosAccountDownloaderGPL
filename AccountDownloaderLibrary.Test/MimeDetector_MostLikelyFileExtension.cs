﻿using AccountDownloaderLibrary.Mime;
using MimeDetective;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace AccountDownloaderLibrary.Test;

public class MimeDetector_MostLikelyFileExtension
{
    private static readonly (MimeDetector mimeDetector, MockFileSystem mockFs) _testMimeDetectorTuple = Utility.CreateMimeDetectorTuple();

    [Theory]
    [MemberData(nameof(ByteArray))]
    public void MostLikelyFileExtension_HeaderBytes_ReturnsTheExpectedExtensionFromHeader(string? expectedExt, byte[] headerBytes)
    {
        Assert.Equal(expectedExt, _testMimeDetectorTuple.mimeDetector.MostLikelyFileExtension(headerBytes));
    }

    [Theory]
    [MemberData(nameof(Streams))]
    public void MostLikelyFileExtension_Stream_ReturnsTheExpectedExtensionsFromHeader(string? expectedExt, MemoryStream stream)
    {
        Assert.Equal(expectedExt, _testMimeDetectorTuple.mimeDetector.MostLikelyFileExtension(stream));
        stream.Dispose();
    }

    [Theory]
    [MemberData(nameof(FilePaths))]
    public void MostLikelyFileExtension_FilePath_ReturnsTheExpectedExtensionsFromHeader(string? expectedExt, string filePath, byte[] headerBytes)
    {
        _testMimeDetectorTuple.mockFs.AddFile(filePath, new MockFileData(headerBytes));

        Assert.Equal(expectedExt, _testMimeDetectorTuple.mimeDetector.MostLikelyFileExtension(filePath));
    }

    public static IEnumerable<object[]> ByteArray =>
        new List<object[]>
        {
            new object[] { "png", new byte [] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            new object[] { "png", new byte [] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52 } },
            new object[] { "mp4", new byte [] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32, 0x00, 0x00, 0x00, 0x00, 0x69, 0x73, 0x6F, 0x6D, 0x6D, 0x70, 0x34, 0x32, 0x00, 0x00, 0x0E, 0xE4, 0x6D, 0x6F, 0x6F, 0x76, 0x00, 0x00, 0x00, 0x6C, 0x6D, 0x76, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            new object[] { "tga", new byte [] { 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9F, 0x00, 0x9F, 0x00, 0x02, 0x00, 0x02, 0x20, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C } },
            new object[] { "mkv", new byte [] { 0x1A, 0x45, 0xDF, 0xA3, 0xA3, 0x42, 0x86, 0x81, 0x01, 0x42, 0xF7, 0x81, 0x01, 0x42, 0xF2, 0x81, 0x04, 0x42, 0xF3, 0x81, 0x08, 0x42, 0x82, 0x88, 0x6D, 0x61, 0x74, 0x72, 0x6F, 0x73, 0x6B, 0x61, 0x42, 0x87, 0x81, 0x04, 0x42, 0x85, 0x81, 0x02, 0x18, 0x53, 0x80, 0x67, 0x01, 0x00, 0x00, 0x00, 0x00, 0xDC } },
            new object[] { "hdr", new byte [] { 0x23, 0x3F, 0x52, 0x41, 0x44, 0x49, 0x41, 0x4E, 0x43, 0x45, 0x0A, 0x23, 0x20, 0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x64, 0x20, 0x77, 0x69, 0x74, 0x68, 0x20, 0x42, 0x6C, 0x65, 0x6E, 0x64, 0x65, 0x72, 0x0A, 0x45, 0x58, 0x50, 0x4F, 0x53, 0x55, 0x52, 0x45, 0x3D, 0x31, 0x0A, 0x47, 0x41, 0x4D, 0x4D, 0x41 } },
            new object[] { "dbf", new byte [] { 0x03, 0x80, 0x72, 0xF7, 0x46, 0xF6, 0x3A, 0x00, 0x39, 0x00, 0x00, 0x03, 0x52, 0x6F, 0x6F, 0x74, 0x00, 0xA2, 0x38, 0x00, 0x00, 0x02, 0x49, 0x44, 0x00, 0x25, 0x00, 0x00, 0x00, 0x36, 0x65, 0x33, 0x61, 0x30, 0x66, 0x36, 0x31, 0x2D, 0x30, 0x30, 0x34, 0x36, 0x2D, 0x34, 0x34, 0x38, 0x35, 0x2D, 0x39, 0x32 } },
            new object[] { "psd", new byte [] { 0x38, 0x42, 0x50, 0x53, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x08, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x0A, 0x38, 0x42, 0x49, 0x4D, 0x04, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x1C, 0x01, 0x5A, 0x00 } },
            new object[] { "bmp", new byte [] { 0x42, 0x4D, 0x36, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            new object[] { "dds", new byte [] { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x08, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            new object[] { "mp3", new byte [] { 0x49, 0x44, 0x33, 0x03, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x76, 0x54, 0x43, 0x4F, 0x4E, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x28, 0x30, 0x29, 0x50, 0x52, 0x49, 0x56, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x50, 0x65, 0x61, 0x6B, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x00, 0x29, 0x63, 0x00, 0x00, 0x50, 0x52 } },
            new object[] { "tif", new byte [] { 0x49, 0x49, 0x2A, 0x00, 0x10, 0xAD, 0x0F, 0x00, 0x80, 0x00, 0x20, 0x50, 0x38, 0x24, 0x16, 0x0D, 0x07, 0x84, 0x42, 0x61, 0x50, 0xB8, 0x64, 0x36, 0x1D, 0x0F, 0x88, 0x44, 0x62, 0x51, 0x38, 0xA4, 0x56, 0x2D, 0x17, 0x8C, 0x46, 0x63, 0x51, 0xB8, 0xE4, 0x76, 0x3D, 0x1F, 0x90, 0x48, 0x64, 0x52, 0x39, 0x24 } },
            new object[] { "fbx", new byte [] { 0x4B, 0x61, 0x79, 0x64, 0x61, 0x72, 0x61, 0x20, 0x46, 0x42, 0x58, 0x20, 0x42, 0x69, 0x6E, 0x61, 0x72, 0x79, 0x20, 0x20, 0x00, 0x1A, 0x00, 0xE8, 0x1C, 0x00, 0x00, 0x63, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x46, 0x42, 0x58, 0x48, 0x65, 0x61, 0x64, 0x65, 0x72, 0x45 } },
            new object[] { "tif", new byte [] { 0x4D, 0x4D, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08, 0x00, 0x17, 0x00, 0xFE, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x03 } },
            new object[] { "ogg", new byte [] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x15, 0xC4, 0x35, 0xA2, 0x01, 0x1E, 0x01, 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73, 0x00, 0x00, 0x00, 0x00, 0x01, 0x22, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x91, 0x93 } },
            new object[] { "animx", new byte [] { 0x05, 0x41, 0x6E, 0x69, 0x6D, 0x58, 0x00, 0x00, 0x00, 0x00, 0x15, 0x00, 0x00, 0x20, 0x3F, 0x14, 0x46, 0x69, 0x73, 0x68, 0x5F, 0x41, 0x72, 0x6D, 0x61, 0x74, 0x75, 0x72, 0x65, 0x7C, 0x41, 0x74, 0x74, 0x61, 0x63, 0x6B, 0x02, 0x5D, 0x00, 0x00, 0x20, 0x00, 0x46, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            new object[] { "meshx", new byte [] { 0x05, 0x4D, 0x65, 0x73, 0x68, 0x58, 0x06, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xF3, 0x01, 0x01, 0x00, 0x00, 0x02, 0x02, 0x02, 0x01, 0x03, 0xE4, 0x85, 0x01, 0xC9, 0x6A, 0xF9, 0xFF, 0xFF, 0xFF, 0xB3, 0xD8, 0xC9, 0xB4, 0xC0, 0x46, 0xF0 } },
            new object[] { "zip", new byte [] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0xEA, 0x5E, 0x54, 0x52, 0x2A, 0xC5, 0x37, 0xE5, 0x43, 0x01, 0x00, 0x00, 0x4A, 0x03, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x6D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x2E, 0x6A, 0x73, 0x6F, 0x6E, 0x8D, 0x91, 0x5D, 0x6B, 0xC2, 0x30, 0x14 } },
            new object[] { "webp", new byte [] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x01, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x4C, 0xF3, 0x00, 0x00, 0x00, 0x2F, 0x03, 0xC0, 0x3F, 0x00, 0x09, 0x05, 0x6D, 0x23, 0x21, 0xCF, 0xE4, 0xDF, 0x31, 0x5A, 0x88, 0xE8, 0x7F, 0xD4, 0x2F, 0x2A, 0x12, 0xE1, 0x6D, 0x66, 0xB6, 0x00, 0x08, 0xC2, 0xFF } },
            new object[] { "flac", new byte [] { 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x22, 0x10, 0x00, 0x10, 0x00, 0x00, 0x1E, 0xB3, 0x00, 0x36, 0x6A, 0x0A, 0xC4, 0x42, 0xF0, 0x00, 0x5B, 0x0E, 0xAE, 0x57, 0x82, 0xD9, 0xFB, 0xCA, 0x05, 0xDC, 0xBA, 0xB3, 0x83, 0xDA, 0xBC, 0x27, 0xE1, 0xE3, 0x07, 0x04, 0x00, 0x00, 0xC4, 0x20, 0x00, 0x00, 0x00 } },
            new object[] { "exr", new byte [] { 0x76, 0x2F, 0x31, 0x01, 0x02, 0x00, 0x00, 0x00, 0x63, 0x68, 0x61, 0x6E, 0x6E, 0x65, 0x6C, 0x73, 0x00, 0x63, 0x68, 0x6C, 0x69, 0x73, 0x74, 0x00, 0x49, 0x00, 0x00, 0x00, 0x41, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x42, 0x00, 0x01, 0x00 } },
            new object[] { "jpc", new byte [] { 0xFF, 0x4F, 0xFF, 0x51, 0x00, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x07, 0x01, 0x01, 0x07, 0x01, 0x01, 0x07, 0x01 } },
            new object[] { "jpg", new byte [] { 0xFF, 0xD8, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0x06, 0x04, 0x05, 0x06, 0x05, 0x04, 0x06, 0x06, 0x05, 0x06, 0x07, 0x07, 0x06, 0x08, 0x0A, 0x10, 0x0A, 0x0A, 0x09, 0x09, 0x0A, 0x14, 0x0E, 0x0F, 0x0C, 0x10, 0x17, 0x14, 0x18, 0x18, 0x17, 0x14, 0x16, 0x16, 0x1A, 0x1D, 0x25, 0x1F, 0x1A, 0x1B, 0x23, 0x1C, 0x16 } },
            new object[] { "mp3", new byte [] { 0xFF, 0xFB, 0x90, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x58, 0x69, 0x6E, 0x67, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 } },
            new object[] { "lzma", new byte [] { 0x5D, 0x00, 0x00, 0x20, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5A, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x7F, 0xEF, 0x9D, 0x44, 0x32, 0x46, 0xC3, 0x36, 0x64, 0x79, 0x96, 0x01, 0xCE, 0xF7, 0xD9, 0x03, 0x78, 0xE1, 0x18, 0x98, 0xF9, 0xEB, 0x29, 0xB3, 0x48, 0xA6 } },
            new object[] { "lzma", new byte [] { 0x5D, 0x00, 0x00, 0x20, 0x00, 0xCE, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x67, 0x0D } },
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new object[] { null, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x2, 0x3} }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        };

    public static IEnumerable<object[]> Streams =>
        ByteArray.Select(fileBytes => new object[] { fileBytes[0]?.ToString()!, new MemoryStream(fileBytes[1] as byte[] ?? new byte[] { }) });

    public static IEnumerable<object[]> FilePaths =>
        ByteArray.Select(fileBytes =>
        {
            var ext = fileBytes[0]?.ToString()!;
            return new object[] { ext, $"path/to/file.{ext}", fileBytes[1] as byte[] ?? new byte[] { } };
        });
}
