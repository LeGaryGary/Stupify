using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Discord.Commands;
using ImageGeneration;

namespace StupifyConsoleApp.Commands.Modules
{
    public class ImageGeneration : ModuleBase<CommandContext>
    {
        private const int MaxIterations = 1000;

        [Command("Mandelbrot", RunMode = RunMode.Async)]
        public async Task GenerateMandelbrotAsync(int iterations)
        {
            if (iterations > MaxIterations)
            {
                await ReplyAsync($"Thats too many iterations! max is {MaxIterations}");
                return;
            }

            var mandelbrot = new Mandelbrot(1024, 768, new Complex(-1.5, -1), new Complex(0.5, 1), iterations);
            await GenerateAndSendAsync(mandelbrot);
        }

        [Command("Mandelbrot", RunMode = RunMode.Async)]
        public async Task GenerateMandelbrotAsync(double realPosition, double imaginaryPosition, double zoom, int iterations)
        {
            if (iterations > MaxIterations)
            {
                await ReplyAsync($"Thats too many iterations! max is {MaxIterations}");
                return;
            }

            var fromReal = realPosition - (1 / zoom);
            var fromImaginary = imaginaryPosition - (1 / zoom);
            var toReal = realPosition + (1 / zoom);
            var toImaginary = imaginaryPosition + (1 / zoom);

            var mandelbrot = new Mandelbrot(1024, 768, new Complex(fromReal, fromImaginary), new Complex(toReal, toImaginary), iterations);
            await GenerateAndSendAsync(mandelbrot);
        }

        private async Task GenerateAndSendAsync(Mandelbrot mandelbrot)
        {
            Directory.CreateDirectory(Config.DataDirectory + "\\Mandelbrot");
            var path = Config.DataDirectory + $"\\Mandelbrot\\{Guid.NewGuid()}.png";
            await Task.Run(() => mandelbrot.GenerateImage(path));
            await Context.Channel.SendFileAsync(path);
        }
    }
}
