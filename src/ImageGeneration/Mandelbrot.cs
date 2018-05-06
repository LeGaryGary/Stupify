using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageGeneration
{
    public class Mandelbrot
    {
        private readonly int _imageWidth;
        private readonly int _imageHeight;
        private readonly Complex _from;
        private readonly Complex _to;
        private const int EscapeMagnitude = 2;
        private readonly int _maxIterations;

        public Mandelbrot(int width, int height, Complex from, Complex to, int maxIterations)
        {
            _imageWidth = width;
            _imageHeight = height;
            _from = from;
            _to = to;
            _maxIterations = maxIterations;
        }
        
        public void GenerateImage(string filePath)
        {
            using (var image = new Image<Rgba32>(_imageWidth, _imageHeight))
            {
                for (var y = 0; y < _imageHeight; y++)
                {
                    for (var x = 0; x < _imageWidth; x++)
                    {
                        var complex = MapCoordToComplex(x, y);
                        var iterations = CalculateMandelbrotIterations(complex);
                        var colour = MapIterationsToColour(iterations);
                        image[x, y] = colour;
                    }
                }

                using (var stream = File.OpenWrite(filePath))
                {
                    image.SaveAsPng(stream);
                }
            }
        }

        private Rgba32 MapIterationsToColour(int n)
        {
            var progress = (float) n / _maxIterations;
            if (progress > 0.5)
            {
                return new Rgba32(progress * 255, 255, progress * 255);
            }

            return new Rgba32(0, progress, 0);
        }

        private int CalculateMandelbrotIterations(Complex complex)
        {
            var modifiedComplex = complex;
            var n = 0;
            while (modifiedComplex.Magnitude < EscapeMagnitude && n < _maxIterations)
            {
                n++;
                modifiedComplex = Complex.Pow(modifiedComplex, 2) + complex;
            }
            return n;
        }

        private Complex MapCoordToComplex(int x, int y)
        {
            var real = (double) x / _imageWidth * (_to.Real - _from.Real) + _from.Real;
            var imaginary = (double) y / _imageHeight * (_to.Imaginary - _from.Imaginary) + _from.Imaginary;
            return new Complex(real, imaginary);
        }
    }
}
