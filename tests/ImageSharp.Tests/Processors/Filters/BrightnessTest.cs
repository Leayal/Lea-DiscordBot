﻿// <copyright file="BrightnessTest.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System.IO;

    using ImageSharp.PixelFormats;

    using Xunit;

    public class BrightnessTest : FileTestBase
    {
        public static readonly TheoryData<int> BrightnessValues
        = new TheoryData<int>
        {
            50 ,
           -50 ,
        };

        [Theory]
        [MemberData(nameof(BrightnessValues))]
        public void ImageShouldApplyBrightnessFilter(int value)
        {
            string path = this.CreateOutputDirectory("Brightness");

            foreach (TestFile file in Files)
            {
                string filename = file.GetFileName(value);
                using (Image<Rgba32> image = file.CreateImage())
                using (FileStream output = File.OpenWrite($"{path}/{filename}"))
                {
                    image.Brightness(value).Save(output);
                }
            }
        }

        [Theory]
        [MemberData(nameof(BrightnessValues))]
        public void ImageShouldApplyBrightnessFilterInBox(int value)
        {
            string path = this.CreateOutputDirectory("Brightness");

            foreach (TestFile file in Files)
            {
                string filename = file.GetFileName(value + "-InBox");
                using (Image<Rgba32> image = file.CreateImage())
                using (FileStream output = File.OpenWrite($"{path}/{filename}"))
                {
                    image.Brightness(value, new Rectangle(10, 10, image.Width / 2, image.Height / 2)).Save(output);
                }
            }
        }
    }
}