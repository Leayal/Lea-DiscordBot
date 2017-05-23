﻿// <copyright file="PngDecoderTests.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System.Text;
    using Xunit;

    using ImageSharp.Formats;
    using ImageSharp.PixelFormats;

    public class PngDecoderTests
    {
        private const PixelTypes PixelTypes = Tests.PixelTypes.StandardImageClass | Tests.PixelTypes.RgbaVector | Tests.PixelTypes.Argb32;

        public static readonly string[] TestFiles =
            {
                TestImages.Png.Splash, TestImages.Png.Indexed, TestImages.Png.Interlaced, TestImages.Png.FilterVar,
                TestImages.Png.ChunkLength1, TestImages.Png.ChunkLength2
            };

        [Theory]
        [WithFileCollection(nameof(TestFiles), PixelTypes)]
        public void DecodeAndReSave<TPixel>(TestImageProvider<TPixel> imageProvider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = imageProvider.GetImage())
            {
                imageProvider.Utility.SaveTestOutputFile(image, "bmp");
            }
        }

        [Fact]
        public void Decode_IgnoreMetadataIsFalse_TextChunckIsRead()
        {
            PngDecoderOptions options = new PngDecoderOptions()
            {
                IgnoreMetadata = false
            };

            TestFile testFile = TestFile.Create(TestImages.Png.Blur);

            using (Image<Rgba32> image = testFile.CreateImage(options))
            {
                Assert.Equal(1, image.MetaData.Properties.Count);
                Assert.Equal("Software", image.MetaData.Properties[0].Name);
                Assert.Equal("paint.net 4.0.6", image.MetaData.Properties[0].Value);
            }
        }

        [Fact]
        public void Decode_IgnoreMetadataIsTrue_TextChunksAreIgnored()
        {
            PngDecoderOptions options = new PngDecoderOptions()
            {
                IgnoreMetadata = true
            };

            TestFile testFile = TestFile.Create(TestImages.Png.Blur);

            using (Image<Rgba32> image = testFile.CreateImage(options))
            {
                Assert.Equal(0, image.MetaData.Properties.Count);
            }
        }

        [Fact]
        public void Decode_TextEncodingSetToUnicode_TextIsReadWithCorrectEncoding()
        {
            PngDecoderOptions options = new PngDecoderOptions()
            {
                TextEncoding = Encoding.Unicode
            };

            TestFile testFile = TestFile.Create(TestImages.Png.Blur);

            using (Image<Rgba32> image = testFile.CreateImage(options))
            {
                Assert.Equal(1, image.MetaData.Properties.Count);
                Assert.Equal("潓瑦慷敲", image.MetaData.Properties[0].Name);
            }
        }
    }
}