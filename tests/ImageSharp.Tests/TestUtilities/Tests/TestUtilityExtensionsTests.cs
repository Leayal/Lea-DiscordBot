﻿// <copyright file="FlagsHelper.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    using ImageSharp.PixelFormats;

    using Xunit;
    using Xunit.Abstractions;

    public class TestUtilityExtensionsTests
    {
        public TestUtilityExtensionsTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ITestOutputHelper Output { get; }

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

        [Fact]
        public void Baz()
        {
            Type type = typeof(Rgba32).GetTypeInfo().Assembly.GetType("ImageSharp.Rgba32");
            this.Output.WriteLine(type.ToString());

            Type fake = typeof(Rgba32).GetTypeInfo().Assembly.GetType("ImageSharp.dsaada_DASqewrr");
            Assert.Null(fake);
        }

        [Theory]
        [WithFile(TestImages.Bmp.Car, PixelTypes.Rgba32, true)]
        [WithFile(TestImages.Bmp.Car, PixelTypes.Rgba32, false)]
        public void IsEquivalentTo_WhenFalse<TPixel>(TestImageProvider<TPixel> provider, bool compareAlpha)
            where TPixel : struct, IPixel<TPixel>
        {
            Image<TPixel> a = provider.GetImage();
            Image<TPixel> b = provider.GetImage();
            b = b.OilPaint(3, 2);

            Assert.False(a.IsEquivalentTo(b, compareAlpha));
        }

        [Theory]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.Rgba32 | PixelTypes.Bgr565, true)]
        [WithMemberFactory(nameof(CreateTestImage), PixelTypes.Rgba32 | PixelTypes.Bgr565, false)]
        public void IsEquivalentTo_WhenTrue<TPixel>(TestImageProvider<TPixel> provider, bool compareAlpha)
            where TPixel : struct, IPixel<TPixel>
        {
            Image<TPixel> a = provider.GetImage();
            Image<TPixel> b = provider.GetImage();

            Assert.True(a.IsEquivalentTo(b, compareAlpha));
        }

        [Theory]
        [InlineData(PixelTypes.Rgba32, typeof(Rgba32))]
        [InlineData(PixelTypes.Argb32, typeof(Argb32))]
        [InlineData(PixelTypes.HalfVector4, typeof(HalfVector4))]
        [InlineData(PixelTypes.StandardImageClass, typeof(Rgba32))]
        public void ToType(PixelTypes pt, Type expectedType)
        {
            Assert.Equal(pt.ToType(), expectedType);
        }

        [Theory]
        [InlineData(typeof(Rgba32), PixelTypes.Rgba32)]
        [InlineData(typeof(Argb32), PixelTypes.Argb32)]
        public void GetPixelType(Type clrType, PixelTypes expectedPixelType)
        {
            Assert.Equal(expectedPixelType, clrType.GetPixelType());
        }

        private static void AssertContainsPixelType<T>(
            PixelTypes pt,
            IEnumerable<KeyValuePair<PixelTypes, Type>> pixelTypesExp)
        {
            Assert.Contains(new KeyValuePair<PixelTypes, Type>(pt, typeof(T)), pixelTypesExp);

        }

        [Fact]
        public void ToTypes()
        {
            PixelTypes pixelTypes = PixelTypes.Alpha8 | PixelTypes.Bgr565 | PixelTypes.Rgba32 | PixelTypes.HalfVector2 | PixelTypes.StandardImageClass;

            IEnumerable<KeyValuePair<PixelTypes, Type>> expanded = pixelTypes.ExpandAllTypes();

            Assert.Equal(expanded.Count(), 5);

            AssertContainsPixelType<Alpha8>(PixelTypes.Alpha8, expanded);
            AssertContainsPixelType<Bgr565>(PixelTypes.Bgr565, expanded);
            AssertContainsPixelType<Rgba32>(PixelTypes.Rgba32, expanded);
            AssertContainsPixelType<HalfVector2>(PixelTypes.HalfVector2, expanded);
            AssertContainsPixelType<Rgba32>(PixelTypes.StandardImageClass, expanded);
        }

        [Fact]
        public void ToTypes_All()
        {
            KeyValuePair<PixelTypes, Type>[] expanded = PixelTypes.All.ExpandAllTypes().ToArray();

            Assert.True(expanded.Length >= TestUtilityExtensions.GetAllPixelTypes().Length - 2);
            AssertContainsPixelType<Rgba32>(PixelTypes.Rgba32, expanded);
            AssertContainsPixelType<Rgba32>(PixelTypes.StandardImageClass, expanded);
        }
    }
}
