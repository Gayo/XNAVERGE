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

    // Sprite contains state information necessary for a sprite instance. Anything that is invariant across
    // different instances of the same sprite is stored in the SpriteBasis to preserve memory.
    public class Sprite {

        public SpriteBasis basis;

        // Use these setters to move the entity, rather than manipulating the hitbox directly, if you want to have it update
        // the draw destination and fractional-pixel coordinates automatically (which you probably do).
        public virtual int x {
            get { return hitbox.X; }
            set {
                destination.X += value - hitbox.X;
                hitbox.X = value;
                _exact_x = value*100;
            }
        }
        public virtual int y {
            get { return hitbox.Y; }
            set {
                destination.Y += value - hitbox.Y;
                hitbox.Y = value;
                _exact_y = value*100;
            }
        }
        public virtual int w { get { return hitbox.Width; } }
        public virtual int h { get { return hitbox.Height; } }
        protected int _exact_x, _exact_y; // these are 100 times the pixel coordinates: the amount 1 speed moves you in 1 tick
        public virtual int exact_x {
            get { return _exact_x; }
            set {
                int pixel_val = value / 100;
                _exact_x = value;
                destination.X += pixel_val - hitbox.X;
                hitbox.X = pixel_val;
            }
        }
        public virtual int exact_y {
            get { return _exact_y; }
            set {
                int pixel_val = value / 100;
                _exact_y = value;                
                destination.Y += pixel_val - hitbox.Y;
                hitbox.Y = pixel_val;                
            }
        }
        
        public virtual int foot { get { return hitbox.Y + hitbox.Height - 1; } } // lower edge of hitbox (used for y-sorting)
        public Rectangle destination, hitbox; // It's not recommended that you twiddle with destination/hitbox directly, but you can!
        public SpriteAnimation cur_animation;
        public int cur_step; // current step within the animation string
        protected float rate; // A multiplier applied to the animation speed. At 1.0, the animation moves at 1 tick per centisecond/game tick. TODO: actually implement this
        public int last_draw_tick; // the last tick at which the sprite was drawn
        public int last_logic_tick; // the last tick at which the sprite's logic was updated
        //public int cur_frame { get { return cur_animation.frame[cur_step]; } }
        public float opacity;
        public bool visible, deleted;
        public bool animating { get { return ((fixed_frame < 0) && !_animation_paused && cur_animation != null); } }
        protected bool _animation_paused;

        protected bool going_backwards; // Used only with the BackAndForth animation style. When true, the animation is reversing.
        protected int fixed_frame; // If fixed_frame is nonnegative, that frame is displayed preferentially.
        protected int time_to_next; // hundredths of ticks until next frame, adjusted by rate

        // Returns the frame the sprite should be displaying now. If time has passed, advance_frame() should be called first.
        public virtual int current_frame {
            get {
                if (fixed_frame >= 0) return fixed_frame;
                if (cur_animation == null) return 0;                
                return cur_animation.frame[cur_step];
            }
        }

        public Sprite(SpriteBasis spr_basis, String anim) : this(spr_basis, anim, 0, 0, false) { }
        public Sprite(SpriteBasis spr_basis, String anim, int x_coord, int y_coord, bool visibility) {
            basis = spr_basis;
            hitbox = new Rectangle(x_coord, y_coord, spr_basis.default_hitbox.Width, spr_basis.default_hitbox.Height);
            destination = new Rectangle(x_coord - spr_basis.default_hitbox.X, y_coord - spr_basis.default_hitbox.Y, spr_basis.frame_width, spr_basis.frame_height);
            opacity = 1.0f;            
            deleted = false;
            rate = 1.0f;
            fixed_frame = -1;
            time_to_next = 0;
            visible = visibility;
            set_animation(anim);
            last_draw_tick = last_logic_tick = VERGEGame.game.tick;
        }

        // move to tile coordinates
        public void move_to_tile(int xc, int yc) {
            x = xc * VERGEGame.game.map.tileset.tilesize;
            y = yc * VERGEGame.game.map.tileset.tilesize;
        }

        // move to pixel coordinates
        public void move_to(int xc, int yc) {
            x = xc;
            y = yc;
        }

        public void shift(int xc, int yc) {
            x += xc;
            y += yc;
        }

        // Like shift, but the values are in hundredths-of-pixels
        public void subpixel_shift(int xc, int yc) {
            exact_x += xc;
            exact_y += yc;
        }

        // Stops the animation and locks it at the specified frame until it's resumed, at which point it will continue
        // with the animation regardless of what frame it had been set to.
        public virtual void set_frame(int frame) {            
            fixed_frame = frame;
        }

        // This just sets animating to true, but I've included it in case it needs to be overloaded by something.
        public virtual void resume_animation() {
            fixed_frame = -1;
        }


        // Adjust the sprite's current frame to account for time passed.
        public virtual void advance_frame() { advance_frame(false); }
        public virtual void advance_frame(bool ignore_delegates) {
            // TODO: delegates            
            if (!visible || !animating) {
                last_draw_tick = VERGEGame.game.tick;
                return;
            }
            if (rate == 1.0f) time_to_next -= 100*(VERGEGame.game.tick - last_draw_tick);
            else time_to_next -= (int)(rate*100*(VERGEGame.game.tick - last_draw_tick));
            while (time_to_next <= 0) {
                cur_step++;
                if (cur_step >= cur_animation.length) {
                    if (cur_animation.style == AnimationStyle.Looping) cur_step = 0;
                    else if (cur_animation.style == AnimationStyle.Once) {
                        _animation_paused = true;
                        cur_step--; // stay at the final step
                        time_to_next = 0;
                        last_draw_tick = VERGEGame.game.tick;
                        return;
                    }
                    else if (cur_animation.style == AnimationStyle.BackAndForth) {
                        going_backwards = !going_backwards;
                        if (going_backwards) {
                            if (cur_animation.length > 1) cur_step -= 2;
                            else cur_step--;
                        }
                        else {
                            if (cur_animation.length > 1) cur_step += 2;
                            else cur_step++;
                        }
                    }
                    else if (cur_animation.style == AnimationStyle.Transition) {
                        cur_animation = cur_animation.transition_to;
                        cur_step = 0;
                    }
                }
                time_to_next += 100*cur_animation.delay[cur_step];
            }
            last_draw_tick = VERGEGame.game.tick;
        }

        // Switches the entity to the specified animation, setting "animating" to true if it was false.
        // Can also be used to reset the animation currently in progress.
        public virtual void set_animation(String name) {
            //Console.WriteLine(name);
            if (String.IsNullOrEmpty(name)) {
                cur_animation = null;
                fixed_frame = -1;
                cur_step = 0;
                time_to_next = 0;
            }
            else {
                set_animation(basis.animations[name]);
                _animation_paused = false;
            }
        }
        public virtual void set_animation(SpriteAnimation anim) {
            cur_step = 0;
            cur_animation = anim;
            time_to_next = anim.delay[0];
            going_backwards = false;
            _animation_paused = false;
        }

        public virtual void Draw() {
            VERGEGame.game.spritebatch.Draw(basis.image, destination, basis.frame_box[current_frame], Color.White, 0, Vector2.Zero, SpriteEffects.None, 1.0f);
        }

        public virtual void Update() {}
    }
}