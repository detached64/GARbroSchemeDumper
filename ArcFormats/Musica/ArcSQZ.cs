//! \file       ArcSQZ.cs
//! \date       Sun Oct 09 03:29:54 2016
//! \brief      Musica engine multi-frame images.
//
// Copyright (C) 2016 by morkt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using GameRes.Compression;

namespace GameRes.Formats.Musica
{
    internal class SqzArchive : ArcFile
    {
        public readonly ImageMetaData Info;

        public SqzArchive (ArcView arc, ArchiveFormat impl, ICollection<Entry> dir, ImageMetaData info)
            : base (arc, impl, dir)
        {
            Info = info;
        }
    }

    [Export(typeof(ArchiveFormat))]
    public class SqzOpener : ArchiveFormat
    {
        public override string         Tag { get { return "SQZ"; } }
        public override string Description { get { return "Musica engine animated frames"; } }
        public override uint     Signature { get { return 0x315A5153; } } // 'SQZ1'
        public override bool  IsHierarchic { get { return false; } }
        public override bool     CanCreate { get { return false; } }

        public override ArcFile TryOpen (ArcView file)
        {
            int count = file.View.ReadInt32 (0x10) * 2;
            if (!IsSaneCount (count))
                return null;

            var info = new ImageMetaData
            {
                Width   = file.View.ReadUInt32 (8),
                Height  = file.View.ReadUInt32 (0xC),
                BPP     = 32,
//                BPP = file.View.ReadInt32 (4),
            };
            var base_name = Path.GetFileNameWithoutExtension (file.Name);
            uint index_offset = 0x14;
            var dir = new List<Entry> (count);
            for (int i = 0; i < count; ++i)
            {
                var entry = new Entry {
                    Name = string.Format ("{0}#{1:D4}.tga", base_name, i),
                    Type = "image",
                    Offset = file.View.ReadUInt32 (index_offset),
                    Size   = file.View.ReadUInt32 (index_offset+4),
                };
                if (!entry.CheckPlacement (file.MaxOffset))
                    return null;
                dir.Add (entry);
                index_offset += 8;
            }
            return new SqzArchive (file, this, dir, info);
        }

        public override Stream OpenEntry (ArcFile arc, Entry entry)
        {
            var sqarc = (SqzArchive)arc;
            using (var input = arc.File.CreateStream (entry.Offset, entry.Size))
            using (var zs = new ZLibStream (input, CompressionMode.Decompress))
            {
                var pixels = new byte[sqarc.Info.Width * sqarc.Info.Height * 4];
                zs.Read (pixels, 0, pixels.Length);
                return TgaStream.Create (sqarc.Info, pixels);
            }
        }
    }
}
