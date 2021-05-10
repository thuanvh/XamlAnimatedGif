using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
    internal class GifDataStream
    {
        public GifHeader Header { get; private set; }
        public GifColor[] GlobalColorTable { get; set; }
        public IList<GifFrame> Frames { get; set; }
        public IList<GifExtension> Extensions { get; set; }
        public ushort RepeatCount { get; set; }

        private GifDataStream()
        {
        }

        //internal static async Task<GifDataStream> ReadAsync(Stream stream)
        //{
        //    var file = new GifDataStream();
        //    await file.ReadInternalAsync(stream).ConfigureAwait(false);
        //    Debug.WriteLine($"ReadAsync finish");
        //    return file;
        //}

        //private async Task ReadInternalAsync(Stream stream)
        //{
        //    Debug.WriteLine($"ReadInternalAsync");
        //    Header = await GifHeader.ReadAsync(stream).ConfigureAwait(false);

        //    Debug.WriteLine($"Check GlobalColorTable");
        //    if (Header.LogicalScreenDescriptor.HasGlobalColorTable)
        //    {
        //        GlobalColorTable = await GifHelpers.ReadColorTableAsync(stream, Header.LogicalScreenDescriptor.GlobalColorTableSize).ConfigureAwait(false);
        //    }

        //    Debug.WriteLine($"Check ReadFramesAsync");
        //    await ReadFramesAsync(stream).ConfigureAwait(false);

        //    Debug.WriteLine($"Check netscapeExtension");
        //    var netscapeExtension =
        //                    Extensions
        //                        .OfType<GifApplicationExtension>()
        //                        .FirstOrDefault(GifHelpers.IsNetscapeExtension);

        //    Debug.WriteLine($"Check RepeatCount");
        //    RepeatCount = netscapeExtension != null
        //        ? GifHelpers.GetRepeatCount(netscapeExtension)
        //        : (ushort)1;
        //    Debug.WriteLine($"ReadInternalAsync end");
        //}

        //private async Task ReadFramesAsync(Stream stream)
        //{
        //    List<GifFrame> frames = new List<GifFrame>();
        //    List<GifExtension> controlExtensions = new List<GifExtension>();
        //    List<GifExtension> specialExtensions = new List<GifExtension>();
        //    int hash = stream.GetHashCode();
        //    Debug.WriteLine($"ReadFramesAsync from stream {hash}");
        //    while (true)
        //    {
        //        try
        //        {
        //            Debug.WriteLine($"ReadFramesAsync stream {hash}: Read async");
        //            bool loadasync = false;
        //            GifBlock block;
        //            if(loadasync)
        //                block = await GifBlock.ReadAsync(stream, controlExtensions).ConfigureAwait(false);
        //            else
        //                block = GifBlock.Read(stream, controlExtensions);

        //            if (block.Kind == GifBlockKind.GraphicRendering)
        //                controlExtensions = new List<GifExtension>();

        //            if (block is GifFrame frame)
        //            {
        //                Debug.WriteLine($"ReadFramesAsync stream {hash}: Add frame");
        //                frames.Add(frame);
        //            }
        //            else if (block is GifExtension extension)
        //            {
        //                Debug.WriteLine($"ReadFramesAsync stream {hash}: Add GifExtension");
        //                switch (extension.Kind)
        //                {
        //                    case GifBlockKind.Control:
        //                        controlExtensions.Add(extension);
        //                        break;
        //                    case GifBlockKind.SpecialPurpose:
        //                        specialExtensions.Add(extension);
        //                        break;

        //                        // Just discard plain text extensions for now, since we have no use for it
        //                }
        //            }
        //            else if (block is GifTrailer)
        //            {
        //                Debug.WriteLine($"ReadFramesAsync stream {hash}: Add trailer -> break");
        //                break;
        //            }
        //            else
        //            {
        //                Debug.WriteLine($"ReadFramesAsync stream {hash}: Block is nothing");
        //            }
        //        }
        //        // Follow the same approach as Firefox:
        //        // If we find extraneous data between blocks, just assume the stream
        //        // was successfully terminated if we have some successfully decoded frames
        //        // https://dxr.mozilla.org/firefox/source/modules/libpr0n/decoders/gif/nsGIFDecoder2.cpp#894-909
        //        catch (UnknownBlockTypeException) when (frames.Count > 0)
        //        {
        //            break;
        //        }
        //    }
        //    Debug.WriteLine($"ReadFramesAsync stream {hash}: out of loop");
        //    this.Frames = frames.AsReadOnly();
        //    this.Extensions = specialExtensions.AsReadOnly();
        //}

        internal static GifDataStream Read(Stream stream)
        {
            var file = new GifDataStream();
            file.ReadInternal(stream);
            Debug.WriteLine($"ReadAsync finish");
            return file;
        }

        private void ReadInternal(Stream stream)
        {
            Debug.WriteLine($"ReadInternalAsync");
            Header = GifHeader.Read(stream);

            Debug.WriteLine($"Check GlobalColorTable");
            if (Header.LogicalScreenDescriptor.HasGlobalColorTable)
            {
                GlobalColorTable = GifHelpers.ReadColorTable(stream, Header.LogicalScreenDescriptor.GlobalColorTableSize);
            }

            Debug.WriteLine($"Check ReadFramesAsync");
            ReadFrames(stream);

            Debug.WriteLine($"Check netscapeExtension");
            var netscapeExtension =
                            Extensions
                                .OfType<GifApplicationExtension>()
                                .FirstOrDefault(GifHelpers.IsNetscapeExtension);

            Debug.WriteLine($"Check RepeatCount");
            RepeatCount = netscapeExtension != null
                ? GifHelpers.GetRepeatCount(netscapeExtension)
                : (ushort)1;
            Debug.WriteLine($"ReadInternalAsync end");
        }

        private void ReadFrames(Stream stream)
        {
            List<GifFrame> frames = new List<GifFrame>();
            List<GifExtension> controlExtensions = new List<GifExtension>();
            List<GifExtension> specialExtensions = new List<GifExtension>();
            int hash = stream.GetHashCode();
            Debug.WriteLine($"ReadFramesAsync from stream {hash}");
            while (true)
            {
                try
                {
                    Debug.WriteLine($"ReadFramesAsync stream {hash}: Read async");
                    bool loadasync = false;
                    GifBlock block;
                    //if (loadasync)
                    //    block = GifBlock.ReadAsync(stream, controlExtensions).ConfigureAwait(false);
                    //else
                        block = GifBlock.Read(stream, controlExtensions);

                    if (block.Kind == GifBlockKind.GraphicRendering)
                        controlExtensions = new List<GifExtension>();

                    if (block is GifFrame frame)
                    {
                        Debug.WriteLine($"ReadFramesAsync stream {hash}: Add frame");
                        frames.Add(frame);
                    }
                    else if (block is GifExtension extension)
                    {
                        Debug.WriteLine($"ReadFramesAsync stream {hash}: Add GifExtension");
                        switch (extension.Kind)
                        {
                            case GifBlockKind.Control:
                                controlExtensions.Add(extension);
                                break;
                            case GifBlockKind.SpecialPurpose:
                                specialExtensions.Add(extension);
                                break;

                                // Just discard plain text extensions for now, since we have no use for it
                        }
                    }
                    else if (block is GifTrailer)
                    {
                        Debug.WriteLine($"ReadFramesAsync stream {hash}: Add trailer -> break");
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"ReadFramesAsync stream {hash}: Block is nothing");
                    }
                }
                // Follow the same approach as Firefox:
                // If we find extraneous data between blocks, just assume the stream
                // was successfully terminated if we have some successfully decoded frames
                // https://dxr.mozilla.org/firefox/source/modules/libpr0n/decoders/gif/nsGIFDecoder2.cpp#894-909
                catch (UnknownBlockTypeException) when (frames.Count > 0)
                {
                    break;
                }
            }
            Debug.WriteLine($"ReadFramesAsync stream {hash}: out of loop");
            this.Frames = frames.AsReadOnly();
            this.Extensions = specialExtensions.AsReadOnly();
        }
    }
}
