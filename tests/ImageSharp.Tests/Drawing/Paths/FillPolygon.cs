﻿
namespace ImageSharp.Tests.Drawing.Paths
{
    using System;
    using ImageSharp;
    using ImageSharp.Drawing.Brushes;
    using Xunit;
    using ImageSharp.Drawing;
    using System.Numerics;
    using SixLabors.Shapes;
    using ImageSharp.Drawing.Processors;
    using ImageSharp.PixelFormats;

    public class FillPolygon : IDisposable
    {
        GraphicsOptions noneDefault = new GraphicsOptions();
        Rgba32 color = Rgba32.HotPink;
        SolidBrush<Rgba32> brush = Brushes.Solid(Rgba32.HotPink);
        Vector2[] path = new Vector2[] {
                    new Vector2(10,10),
                    new Vector2(20,10),
                    new Vector2(20,10),
                    new Vector2(30,10),
                };
        private ProcessorWatchingImage img;

        public FillPolygon()
        {
            this.img = new Paths.ProcessorWatchingImage(10, 10);
        }

        public void Dispose()
        {
            img.Dispose();
        }

        [Fact]
        public void CorrectlySetsBrushAndPath()
        {
            img.FillPolygon(brush, path);

            Assert.NotEmpty(img.ProcessorApplications);
            FillRegionProcessor<Rgba32> processor = Assert.IsType<FillRegionProcessor<Rgba32>>(img.ProcessorApplications[0].processor);

            Assert.Equal(GraphicsOptions.Default, processor.Options);

            ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
            Polygon polygon = Assert.IsType<Polygon>(region.Shape);
            LinearLineSegment segemnt = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

            Assert.Equal(brush, processor.Brush);
        }

        [Fact]
        public void CorrectlySetsBrushPathAndOptions()
        {
            img.FillPolygon(brush, path, noneDefault);

            Assert.NotEmpty(img.ProcessorApplications);
            FillRegionProcessor<Rgba32> processor = Assert.IsType<FillRegionProcessor<Rgba32>>(img.ProcessorApplications[0].processor);

            Assert.Equal(noneDefault, processor.Options);

            ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
            Polygon polygon = Assert.IsType<Polygon>(region.Shape);
            LinearLineSegment segemnt = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

            Assert.Equal(brush, processor.Brush);
        }

        [Fact]
        public void CorrectlySetsColorAndPath()
        {
            img.FillPolygon(color, path);

            Assert.NotEmpty(img.ProcessorApplications);
            FillRegionProcessor<Rgba32> processor = Assert.IsType<FillRegionProcessor<Rgba32>>(img.ProcessorApplications[0].processor);

            Assert.Equal(GraphicsOptions.Default, processor.Options);

            ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
            Polygon polygon = Assert.IsType<Polygon>(region.Shape);
            LinearLineSegment segemnt = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

            SolidBrush<Rgba32> brush = Assert.IsType<SolidBrush<Rgba32>>(processor.Brush);
            Assert.Equal(color, brush.Color);
        }

        [Fact]
        public void CorrectlySetsColorPathAndOptions()
        {
            img.FillPolygon(color, path, noneDefault);

            Assert.NotEmpty(img.ProcessorApplications);
            FillRegionProcessor<Rgba32> processor = Assert.IsType<FillRegionProcessor<Rgba32>>(img.ProcessorApplications[0].processor);

            Assert.Equal(noneDefault, processor.Options);

            ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
            Polygon polygon = Assert.IsType<Polygon>(region.Shape);
            LinearLineSegment segemnt = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

            SolidBrush<Rgba32> brush = Assert.IsType<SolidBrush<Rgba32>>(processor.Brush);
            Assert.Equal(color, brush.Color);
        }
    }
}
