using System.IO;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
    internal class GifImageData
    {
        public byte LzwMinimumCodeSize { get; set; }
        public long CompressedDataStartOffset { get; set; }

        private GifImageData()
        {
        }

        //internal static async Task<GifImageData> ReadAsync(Stream stream)
        //{
        //    var imgData = new GifImageData();
        //    await imgData.ReadInternalAsync(stream).ConfigureAwait(false);
        //    return imgData;
        //}

        internal static GifImageData Read(Stream stream)
        {
            var imgData = new GifImageData();
            imgData.ReadInternal(stream);
            return imgData;
        }

        //private async Task ReadInternalAsync(Stream stream)
        //{
        //    LzwMinimumCodeSize = (byte)stream.ReadByte();
        //    CompressedDataStartOffset = stream.Position;
        //    await GifHelpers.ConsumeDataBlocksAsync(stream).ConfigureAwait(false);
        //}

        private void ReadInternal(Stream stream)
        {
            LzwMinimumCodeSize = (byte)stream.ReadByte();
            CompressedDataStartOffset = stream.Position;
            GifHelpers.ConsumeDataBlocks(stream);
        }
    }
}
