using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
    internal static class GifHelpers
    {
        public static async Task<string> ReadStringAsync(Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            await stream.ReadAllAsync(bytes, 0, length).ConfigureAwait(false);
            return GetString(bytes);
        }

        public static string ReadString(Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            stream.ReadAll(bytes, 0, length);
            return GetString(bytes);
        }

        public static async Task ConsumeDataBlocksAsync(Stream sourceStream, CancellationToken cancellationToken = default)
        {
            await CopyDataBlocksToStreamAsync(sourceStream, Stream.Null, cancellationToken);
        }

        public static void ConsumeDataBlocks(Stream sourceStream)
        {
            //CopyDataBlocksToStream1(sourceStream, Stream.Null);
            MoveOverDataBlocksOfStream(sourceStream);
        }
        

        public static async Task<byte[]> ReadDataBlocksAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            await CopyDataBlocksToStreamAsync(stream, ms, cancellationToken);
            return ms.ToArray();
        }

        public static byte[] ReadDataBlocks(Stream stream)
        {
            long position = stream.Position;
            
            
            var b = CopyDataBlocks(stream); 

            stream.Position = position;

            using var ms = new MemoryStream();
            CopyDataBlocksToStream1(stream, ms);
            var a = ms.ToArray();

            return b;
        }

        public static async Task CopyDataBlocksToStreamAsync(Stream sourceStream, Stream targetStream, CancellationToken cancellationToken = default)
        {
            int len;
            // the length is on 1 byte, so each data sub-block can't be more than 255 bytes long
            byte[] buffer = new byte[255];
            while ((len = await sourceStream.ReadByteAsync(cancellationToken)) > 0)
            {
                await sourceStream.ReadAllAsync(buffer, 0, len, cancellationToken).ConfigureAwait(false);
#if LACKS_STREAM_MEMORY_OVERLOADS
                await targetStream.WriteAsync(buffer, 0, len, cancellationToken);
#else
                await targetStream.WriteAsync(buffer, 0, len, cancellationToken);
#endif
            }
        }

        public static void MoveOverDataBlocksOfStream(Stream sourceStream)
        {
            int len;
            //var position = sourceStream.Position;
            //List<int> lenList = new List<int>();            
            while ((len = sourceStream.ReadByte()) > 0)
            {
                sourceStream.Seek(len, SeekOrigin.Current);
            }
        }
        public static byte[] CopyDataBlocks(Stream sourceStream)
        {
            int len;
            var position = sourceStream.Position;
            List<int> lenList = new List<int>();
            int sum = 0;
            while ((len = sourceStream.ReadByte()) > 0)
            {
                lenList.Add(len);
                sum += len;
                sourceStream.Seek(len, SeekOrigin.Current);
            }

            byte[] totalBuffer = new byte[sum];
            sourceStream.Position = position;
            int offset = 0;
            for(int i = 0; i < lenList.Count; i++) 
            {
                int len2 = lenList[i];
                sourceStream.ReadByte();
                sourceStream.ReadAll(totalBuffer, offset, len2);
                offset += len2;
            }
            return totalBuffer;
        }

        public static void CopyDataBlocksToStream1(Stream sourceStream, Stream targetStream)
        {
            int len;
            // the length is on 1 byte, so each data sub-block can't be more than 255 bytes long
            byte[] buffer = new byte[255];
            List<int> lenList = new List<int>();
            while ((len = sourceStream.ReadByte()) > 0)
            {
                sourceStream.ReadAll(buffer, 0, len);
#if LACKS_STREAM_MEMORY_OVERLOADS
                targetStream.Write(buffer, 0, len);
#else
                targetStream.Write(buffer, 0, len);
#endif
            }
        }

        public static async Task<GifColor[]> ReadColorTableAsync(Stream stream, int size)
        {
            int length = 3 * size;
            byte[] bytes = new byte[length];
            await stream.ReadAllAsync(bytes, 0, length).ConfigureAwait(false);
            GifColor[] colorTable = new GifColor[size];
            for (int i = 0; i < size; i++)
            {
                byte r = bytes[3 * i];
                byte g = bytes[3 * i + 1];
                byte b = bytes[3 * i + 2];
                colorTable[i] = new GifColor(r, g, b);
            }
            return colorTable;
        }

        public static GifColor[] ReadColorTable(Stream stream, int size)
        {
            int length = 3 * size;
            byte[] bytes = new byte[length];
            stream.ReadAll(bytes, 0, length);
            GifColor[] colorTable = new GifColor[size];
            for (int i = 0; i < size; i++)
            {
                byte r = bytes[3 * i];
                byte g = bytes[3 * i + 1];
                byte b = bytes[3 * i + 2];
                colorTable[i] = new GifColor(r, g, b);
            }
            return colorTable;
        }

        public static bool IsNetscapeExtension(GifApplicationExtension ext)
        {
            return ext.ApplicationIdentifier == "NETSCAPE"
                && GetString(ext.AuthenticationCode) == "2.0";
        }

        public static ushort GetRepeatCount(GifApplicationExtension ext)
        {
            if (ext.Data.Length >= 3)
            {
                return BitConverter.ToUInt16(ext.Data, 1);
            }
            return 1;
        }

        public static Exception UnknownBlockTypeException(int blockId)
        {
            return new UnknownBlockTypeException("Unknown block type: 0x" + blockId.ToString("x2"));
        }

        public static Exception UnknownExtensionTypeException(int extensionLabel)
        {
            return new UnknownExtensionTypeException("Unknown extension type: 0x" + extensionLabel.ToString("x2"));
        }

        public static Exception InvalidBlockSizeException(string blockName, int expectedBlockSize, int actualBlockSize)
        {
            return new InvalidBlockSizeException(
                $"Invalid block size for {blockName}. Expected {expectedBlockSize}, but was {actualBlockSize}");
        }

        public static Exception InvalidSignatureException(string signature)
        {
            return new InvalidSignatureException("Invalid file signature: " + signature);
        }

        public static Exception UnsupportedVersionException(string version)
        {
            return new UnsupportedGifVersionException("Unsupported version: " + version);
        }

        public static string GetString(byte[] bytes)
        {
            return GetString(bytes, 0, bytes.Length);
        }

        public static string GetString(byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }
    }
}
