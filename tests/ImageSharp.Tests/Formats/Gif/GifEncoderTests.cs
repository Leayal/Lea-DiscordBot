﻿// <copyright file="GifEncoderTests.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System.IO;
    using Xunit;

    using ImageSharp.Formats;
    using ImageSharp.PixelFormats;

    public class GifEncoderTests
    {
        private const PixelTypes PixelTypes = Tests.PixelTypes.StandardImageClass | Tests.PixelTypes.RgbaVector | Tests.PixelTypes.Argb32;

        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes)]
        public void EncodeGeneratedPatterns<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                provider.Utility.SaveTestOutputFile(image, "gif", new GifEncoder());
            }
        }

        [Fact]
        public void Encode_IgnoreMetadataIsFalse_CommentsAreWritten()
        {
            EncoderOptions options = new EncoderOptions()
            {
                IgnoreMetadata = false
            };

            TestFile testFile = TestFile.Create(TestImages.Gif.Rings);

            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    input.Save(memStream, new GifFormat(), options);

                    memStream.Position = 0;
                    using (Image<Rgba32> output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(1, output.MetaData.Properties.Count);
                        Assert.Equal("Comments", output.MetaData.Properties[0].Name);
                        Assert.Equal("ImageSharp", output.MetaData.Properties[0].Value);
                    }
                }
            }
        }

        [Fact]
        public void Encode_IgnoreMetadataIsTrue_CommentsAreNotWritten()
        {
            GifEncoderOptions options = new GifEncoderOptions()
            {
                IgnoreMetadata = true
            };

            TestFile testFile = TestFile.Create(TestImages.Gif.Rings);

            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    input.SaveAsGif(memStream, options);

                    memStream.Position = 0;
                    using (Image<Rgba32> output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(0, output.MetaData.Properties.Count);
                    }
                }
            }
        }

        [Fact]
        public void Encode_CommentIsToLong_CommentIsTrimmed()
        {
            using (Image<Rgba32> input = new Image<Rgba32>(1, 1))
            {
                string comments = new string('c', 256);
                input.MetaData.Properties.Add(new ImageProperty("Comments", comments));

                using (MemoryStream memStream = new MemoryStream())
                {
                    input.Save(memStream, new GifFormat());

                    memStream.Position = 0;
                    using (Image<Rgba32> output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(1, output.MetaData.Properties.Count);
                        Assert.Equal("Comments", output.MetaData.Properties[0].Name);
                        Assert.Equal(255, output.MetaData.Properties[0].Value.Length);
                    }
                }
            }
        }
    }
}
