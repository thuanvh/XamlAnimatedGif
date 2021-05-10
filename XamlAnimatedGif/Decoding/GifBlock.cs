using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
    internal abstract class GifBlock
    {
        //internal static async Task<GifBlock> ReadAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
        //{
        //    int blockId = await stream.ReadByteAsync().ConfigureAwait(false);
        //    if (blockId < 0)
        //        throw new EndOfStreamException();
        //    return blockId switch
        //    {
        //        GifExtension.ExtensionIntroducer => await GifExtension.ReadAsync(stream, controlExtensions).ConfigureAwait(false),
        //        GifFrame.ImageSeparator => await GifFrame.ReadAsync(stream, controlExtensions).ConfigureAwait(false),
        //        GifTrailer.TrailerByte => await GifTrailer.ReadAsync().ConfigureAwait(false),
        //        _ => throw GifHelpers.UnknownBlockTypeException(blockId),
        //    };
        //}

        internal static GifBlock Read(Stream stream, IEnumerable<GifExtension> controlExtensions)
        {
            
                int blockId = stream.ReadByte();
                if (blockId < 0)
                    throw new EndOfStreamException();
                return blockId switch
                {
                    GifExtension.ExtensionIntroducer => GifExtension.Read(stream, controlExtensions),
                    GifFrame.ImageSeparator => GifFrame.Read(stream, controlExtensions),
                    GifTrailer.TrailerByte => GifTrailer.Read(),
                    _ => throw GifHelpers.UnknownBlockTypeException(blockId),
                };
            
        }

        internal abstract GifBlockKind Kind { get; }
    }
}
