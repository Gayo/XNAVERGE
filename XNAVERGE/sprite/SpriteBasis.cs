﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // This class contains all sprite data that stays constant across different instances of the same sprite.
    // The constructor, which loads a new sprite from a .CHR file, is in SpriteBasis_Loader.vc.
    public partial class SpriteBasis {
        public Texture2D image;
        public int num_frames { get { return _num_frames; } }
        public int frame_width { get { return _frame_width; } }
        public int frame_height { get { return _frame_height; } }
        public int frames_per_row { get { return _per_row; } }
        public int frames_per_column { get { return _per_column; } }
        protected int _num_frames, _frame_height, _frame_width, _per_row, _per_column;
        public Rectangle default_hitbox;
        public Rectangle[] frame_box;
        public Dictionary<String, SpriteAnimation> animations;
    }
}