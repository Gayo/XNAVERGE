using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = XNAVERGE.SpriteBasis;

namespace XNAVERGE.Content {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class SpriteReader : ContentTypeReader<TRead> {
        protected override TRead Read(ContentReader input, TRead nobody_seems_to_know_what_this_argument_is_for) {
            int dim, num_anim;
            byte[] pixels;
            SpriteBasis spr = new SpriteBasis(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            spr.default_hitbox = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            dim = input.ReadInt32();
            Texture2D image = new Texture2D(VERGEGame.game.GraphicsDevice, dim, dim);
            pixels = input.ReadBytes(dim * dim * 4);
            image.SetData(pixels);
            spr.image = image;
            spr.generate_bounding_boxes(0, 0);

            // load animations
            num_anim = input.ReadInt32();
            for (int i = 0; i < num_anim; i++)
                read_animation(input, spr.animations);

            return spr;
        }

        public void read_animation(ContentReader input, Dictionary<String, SpriteAnimation> dict) {
            String name, pattern, transition;
            int style, len;
            int[] frame, delay;
            SpriteAnimation anim;
            name = input.ReadString();
            pattern = input.ReadString();
            style = input.ReadInt32();
            transition = input.ReadString(); // TODO: figure out how to set this up, I guess (maybe better to keep it as a string?)
            len = input.ReadInt32();
            frame = new int[len];
            delay = new int[len];
            for (int i=0;i<len;i++)
                frame[i] = input.ReadInt32();
            for (int i=0;i<len;i++)
                delay[i] = input.ReadInt32();
            anim = new SpriteAnimation(name, pattern, frame, delay, (AnimationStyle)style);
            dict.Add(name, anim);
        }
    }
}
