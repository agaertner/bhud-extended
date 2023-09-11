using Blish_HUD.Modules.Managers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public static class ContentsManagerExtensions
    {
        private static Logger Logger = Logger.GetLogger<ContentsManager>();

        /// <summary>
        /// Extracts a file from the module archive.
        /// </summary>
        /// <param name="contentsManager">The module's assigned <see cref="ContentsManager"/> object.</param>
        /// <param name="refFilePath">A file path relative to the ref folder inside the module archive.</param>
        /// <param name="outFilePath">Destination of the file.</param>
        /// <param name="overwrite">If any existing file at the destination should be overwritten.</param>
        public static async Task Extract(this ContentsManager contentsManager, string refFilePath, string outFilePath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(refFilePath))
            {
                throw new ArgumentException($"{nameof(refFilePath)} cannot be empty.", nameof(refFilePath));
            }

            if (string.IsNullOrEmpty(outFilePath))
            {
                throw new ArgumentException($"{nameof(outFilePath)} cannot be empty.", nameof(outFilePath));
            }

            if (!overwrite && File.Exists(outFilePath)) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outFilePath)!);

                using var stream = contentsManager.GetFileStream(refFilePath);
                if (stream == null)
                {
                    throw new FileNotFoundException($"File not found: '{refFilePath}'");
                }
                stream.Position = 0;
                using var file = File.Create(outFilePath);
                file.Position = 0;
                await stream.CopyToAsync(file);
            }
            catch (IOException e)
            {
                Logger.Warn(e, e.Message);
            }
        }
    }
}
