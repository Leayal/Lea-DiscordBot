﻿// <copyright file="WithBlankImagesAttribute.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Triggers passing <see cref="TestImageProvider{TPixel}"/> instances which produce a blank image of size width * height.
    /// One <see cref="TestImageProvider{TPixel}"/> instance will be passed for each the pixel format defined by the pixelTypes parameter
    /// </summary>
    public class WithTestPatternImagesAttribute : ImageDataAttributeBase
    {
        /// <summary>
        /// Triggers passing an <see cref="TestImageProvider{TPixel}"/> that produces a test pattern image of size width * height
        /// </summary>
        /// <param name="width">The required width</param>
        /// <param name="height">The required height</param>
        /// <param name="pixelTypes">The requested parameter</param>
        /// <param name="additionalParameters">Additional theory parameter values</param>
        public WithTestPatternImagesAttribute(int width, int height, PixelTypes pixelTypes, params object[] additionalParameters)
            : this(null, width, height, pixelTypes,additionalParameters)
        {
        }

        /// <summary>
        /// Triggers passing an <see cref="TestImageProvider{TPixel}"/> that produces a test pattern image of size width * height
        /// </summary>
        /// <param name="width">The required width</param>
        /// <param name="height">The required height</param>
        /// <param name="pixelTypes">The requested parameter</param>
        /// <param name="additionalParameters">Additional theory parameter values</param>
        public WithTestPatternImagesAttribute(string memberData, int width, int height, PixelTypes pixelTypes, params object[] additionalParameters)
            : base(memberData, pixelTypes, additionalParameters)
        {
            this.Width = width;
            this.Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        protected override string GetFactoryMethodName(MethodInfo testMethod) => "TestPattern";

        protected override object[] GetFactoryMethodArgs(MethodInfo testMethod, Type factoryType) => new object[] { this.Width, this.Height };
    }
}