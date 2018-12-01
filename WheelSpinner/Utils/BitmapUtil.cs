using SkiaSharp;
using System;

namespace WheelSpinner.Utils
{
    public class BitmapUtil
    {
        /// <summary>
        /// Given the relative path to an embedded resource image (.jpg, .png or .bmp), returns an <see cref="SKBitmap"/> object with that image.
        ///
        /// <para>The path is relative to the assembly</para>
        /// <para>Use a dot to separate the path components, e.g. "MyAssets.MyPicture.jpg" </para>
        /// </summary>
        /// <param name="relativePath">Path to the embedded resource image.</param>
        /// <returns></returns>
        public static SKBitmap LoadBitmapFromResource(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new Exception("Provided resource path cannot be null or empty");
            }

            SKBitmap bitmap;
            var currentAssembly = typeof(BitmapUtil).Assembly;
            using (var resourceStream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.{relativePath}"))
            {
                bitmap = SKBitmap.Decode(resourceStream);
            }

            return bitmap;
        }
    }
}