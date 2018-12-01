using SkiaSharp;

namespace WheelSpinner.Extensions
{
    // ReSharper disable once UnusedMember.Global
    public static class SKRectExtensions
    {
        /// <summary>
        /// Creates a new <see cref="SKRect"/> of the same size and shape as the original, but which is offset by the specified amount
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="delta">The difference between the new <see cref="SKRect"/> and the original</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static SKRect OffsetClone(this SKRect rect, SKPoint delta, OffsetMode mode = OffsetMode.Add)
        {
            if (mode == OffsetMode.Add)
            {
                return new SKRect(
                    rect.Left + delta.X,
                    rect.Top + delta.Y,
                    rect.Right + delta.X,
                    rect.Bottom + delta.Y
                );
            }
            else
            {
                return new SKRect(
                    rect.Left - delta.X,
                    rect.Top - delta.Y,
                    rect.Right - delta.X,
                    rect.Bottom - delta.Y
                );
            }
        }
    }

    public enum OffsetMode
    {
        Add,
        Subtract
    }
}