﻿// <copyright file="PngEncoderTests.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using ImageSharp.Formats;

namespace ImageSharp.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ImageSharp.IO;
    using ImageSharp.PixelFormats;

    using Xunit;

    public class PngEncoderTests : FileTestBase
    {
        private const PixelTypes PixelTypes = Tests.PixelTypes.StandardImageClass | Tests.PixelTypes.RgbaVector | Tests.PixelTypes.Argb32;

        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes, PngColorType.RgbWithAlpha)]
        [WithTestPatternImages(100, 100, PixelTypes, PngColorType.Rgb)]
        [WithTestPatternImages(100, 100, PixelTypes, PngColorType.Palette)]
        [WithTestPatternImages(100, 100, PixelTypes, PngColorType.Grayscale)]
        [WithTestPatternImages(100, 100, PixelTypes, PngColorType.GrayscaleWithAlpha)]
        public void EncodeGeneratedPatterns<TPixel>(TestImageProvider<TPixel> provider, PngColorType pngColorType)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                PngEncoderOptions options = new PngEncoderOptions()
                                                {
                                                    PngColorType = pngColorType
                                                };
                provider.Utility.TestName += "_" + pngColorType;

                provider.Utility.SaveTestOutputFile(image, "png", new PngEncoder(), options);
            }
        }

        [Theory]
        [WithBlankImages(1, 1, PixelTypes.All)]
        public void WritesFileMarker<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, new PngEncoder());
                
                byte[] data = ms.ToArray().Take(8).ToArray(); 
                byte[] expected = {
                    0x89, // Set the high bit.
                    0x50, // P
                    0x4E, // N
                    0x47, // G
                    0x0D, // Line ending CRLF
                    0x0A, // Line ending CRLF
                    0x1A, // EOF
                    0x0A // LF
                };

                Assert.Equal(expected, data);
            }
        }
    }
}