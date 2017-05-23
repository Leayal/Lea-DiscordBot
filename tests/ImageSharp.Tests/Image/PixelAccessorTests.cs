﻿// <copyright file="PixelAccessorTests.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System;
    using System.Numerics;

    using ImageSharp.PixelFormats;

    using Xunit;

    /// <summary>
    /// Tests the <see cref="PixelAccessor"/> class.
    /// </summary>
    public class PixelAccessorTests
    {
        public static Image<TPixel> CreateTestImage<TPixel>(GenericFactory<TPixel> factory)
            where TPixel : struct, IPixel<TPixel>
        {
            Image<TPixel> image = factory.CreateImage(10, 10);
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Vector4 v = new Vector4(i, j, 0, 1);
                        v /= 10;

                        TPixel color = default(TPixel);
                        color.PackFromVector4(v);

                        pixels[i, j] = color;
                    }
                }
            }
            return image;
        }

        [Theory]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.All, ComponentOrder.Xyz)]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.All, ComponentOrder.Zyx)]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.All, ComponentOrder.Xyzw)]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.All, ComponentOrder.Zyxw)]
        public void CopyTo_Then_CopyFrom_OnFullImageRect<TPixel>(TestImageProvider<TPixel> provider, ComponentOrder order)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> src = provider.GetImage())
            {
                using (Image<TPixel> dest = new Image<TPixel>(src.Width, src.Height))
                {
                    using (PixelArea<TPixel> area = new PixelArea<TPixel>(src.Width, src.Height, order))
                    {
                        using (PixelAccessor<TPixel> srcPixels = src.Lock())
                        {
                            srcPixels.CopyTo(area, 0, 0);
                        }

                        using (PixelAccessor<TPixel> destPixels = dest.Lock())
                        {
                            destPixels.CopyFrom(area, 0, 0);
                        }
                    }

                    Assert.True(src.IsEquivalentTo(dest, false));
                }
            }
        }

        // TODO: Need a processor in the library with this signature
        private static void Fill<TPixel>(Image<TPixel> image, Rectangle region, TPixel color)
             where TPixel : struct, IPixel<TPixel>
        {
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                for (int y = region.Top; y < region.Bottom; y++)
                {
                    for (int x = region.Left; x < region.Right; x++)
                    {
                        pixels[x, y] = color;
                    }
                }
            }
        }

        [Theory]
        [WithBlankImages(16, 16, PixelTypes.All, ComponentOrder.Xyz)]
        [WithBlankImages(16, 16, PixelTypes.All, ComponentOrder.Zyx)]
        [WithBlankImages(16, 16, PixelTypes.All, ComponentOrder.Xyzw)]
        [WithBlankImages(16, 16, PixelTypes.All, ComponentOrder.Zyxw)]
        public void CopyToThenCopyFromWithOffset<TPixel>(TestImageProvider<TPixel> provider, ComponentOrder order)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> destImage = new Image<TPixel>(8, 8))
            {
                using (Image<TPixel> srcImage = provider.GetImage())
                {
                    Fill(srcImage, new Rectangle(4, 4, 8, 8), NamedColors<TPixel>.Red);
                    using (PixelAccessor<TPixel> srcPixels = srcImage.Lock())
                    {
                        using (PixelArea<TPixel> area = new PixelArea<TPixel>(8, 8, order))
                        {
                            srcPixels.CopyTo(area, 4, 4);

                            using (PixelAccessor<TPixel> destPixels = destImage.Lock())
                            {
                                destPixels.CopyFrom(area, 0, 0);
                            }
                        }
                    }
                }

                provider.Utility.SourceFileOrDescription = order.ToString();
                provider.Utility.SaveTestOutputFile(destImage, "bmp");

                using (Image<TPixel> expectedImage = new Image<TPixel>(8, 8).Fill(NamedColors<TPixel>.Red))
                {
                    Assert.True(destImage.IsEquivalentTo(expectedImage));
                }
            }
        }


        [Fact]
        public void CopyFromZYX()
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1, 1))
            {
                CopyFromZYX(image);
            }
        }
        
        [Fact]
        public void CopyFromZYXW()
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1, 1))
            {
                CopyFromZYXW(image);
            }
        }
        
        [Fact]
        public void CopyToZYX()
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1, 1))
            {
                CopyToZYX(image);
            }
        }
        
        [Fact]
        public void CopyToZYXW()
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1, 1))
            {
                CopyToZYXW(image);
            }
        }
        
        private static void CopyFromZYX<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                byte red = 1;
                byte green = 2;
                byte blue = 3;
                byte alpha = 255;

                using (PixelArea<TPixel> row = new PixelArea<TPixel>(1, ComponentOrder.Zyx))
                {
                    row.Bytes[0] = blue;
                    row.Bytes[1] = green;
                    row.Bytes[2] = red;

                    pixels.CopyFrom(row, 0);

                    Rgba32 color = (Rgba32)(object)pixels[0, 0];
                    Assert.Equal(red, color.R);
                    Assert.Equal(green, color.G);
                    Assert.Equal(blue, color.B);
                    Assert.Equal(alpha, color.A);
                }
            }
        }

        private static void CopyFromZYXW<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                byte red = 1;
                byte green = 2;
                byte blue = 3;
                byte alpha = 4;

                using (PixelArea<TPixel> row = new PixelArea<TPixel>(1, ComponentOrder.Zyxw))
                {
                    row.Bytes[0] = blue;
                    row.Bytes[1] = green;
                    row.Bytes[2] = red;
                    row.Bytes[3] = alpha;

                    pixels.CopyFrom(row, 0);

                    Rgba32 color = (Rgba32)(object)pixels[0, 0];
                    Assert.Equal(red, color.R);
                    Assert.Equal(green, color.G);
                    Assert.Equal(blue, color.B);
                    Assert.Equal(alpha, color.A);
                }
            }
        }

        private static void CopyToZYX<TPixel>(Image<TPixel> image)
          where TPixel : struct, IPixel<TPixel>
        {
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                byte red = 1;
                byte green = 2;
                byte blue = 3;

                using (PixelArea<TPixel> row = new PixelArea<TPixel>(1, ComponentOrder.Zyx))
                {
                    pixels[0, 0] = (TPixel)(object)new Rgba32(red, green, blue);

                    pixels.CopyTo(row, 0);

                    Assert.Equal(blue, row.Bytes[0]);
                    Assert.Equal(green, row.Bytes[1]);
                    Assert.Equal(red, row.Bytes[2]);
                }
            }
        }

        private static void CopyToZYXW<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (PixelAccessor<TPixel> pixels = image.Lock())
            {
                byte red = 1;
                byte green = 2;
                byte blue = 3;
                byte alpha = 4;

                using (PixelArea<TPixel> row = new PixelArea<TPixel>(1, ComponentOrder.Zyxw))
                {
                    pixels[0, 0] = (TPixel)(object)new Rgba32(red, green, blue, alpha);

                    pixels.CopyTo(row, 0);

                    Assert.Equal(blue, row.Bytes[0]);
                    Assert.Equal(green, row.Bytes[1]);
                    Assert.Equal(red, row.Bytes[2]);
                    Assert.Equal(alpha, row.Bytes[3]);
                }
            }
        }
    }
}
