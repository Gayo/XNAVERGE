using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {

    public class ScriptRenderLayer : RenderLayer {
        public RenderLayerDelegate script;
        public Object data; // miscellaneous data storage, in case you need state        

        public ScriptRenderLayer() : base(VERGEMap.FIXED_PARALLAX, "Hook Render") {
            script = new RenderLayerDelegate(VERGEGame.game.call_render_hook);
            data = null;
        }
        public ScriptRenderLayer(RenderLayerDelegate script, String name) : this(script, VERGEMap.FIXED_PARALLAX, name) { }
        public ScriptRenderLayer(RenderLayerDelegate script, bool fixed_pos, String name) : 
            this(script, ( fixed_pos ? VERGEMap.FIXED_PARALLAX : VERGEMap.NEUTRAL_PARALLAX ), name ) { }
        public ScriptRenderLayer(RenderLayerDelegate script, Vector2 parallax, String name) : base(parallax, name) {
            this.script = script;
            data = null;
        }

        public override void Draw() {            
            if (script == null) return;

            Rectangle clipping_rect = VERGEGame.game.camera.rect; // copied by value
            if (parallax == VERGEMap.FIXED_PARALLAX) {
                clipping_rect.X = 0;
                clipping_rect.Y = 0;
            }
            else if (parallax != VERGEMap.NEUTRAL_PARALLAX) {
                clipping_rect.X = (int)(clipping_rect.X * parallax.X);
                clipping_rect.Y = (int)(clipping_rect.Y * parallax.Y);
            }

            script(this, clipping_rect);
        }

    }

    public enum LayerType { Tile, Entity, Script }

    public class EntityLayer : RenderLayer {
        public EntityLayer() : base(VERGEMap.NEUTRAL_PARALLAX, "Entities") {
            _type = LayerType.Entity;
        }

        public override void Draw() { // disregards parallax value (always assumes parallax of (1.0, 1.0))
            SpriteBatch spritebatch = VERGEGame.game.spritebatch;
            Rectangle draw_rect = VERGEGame.game.camera.rect;
            Entity cur_ent;
            Entity[] ents = VERGEGame.game.map.entities;
            int num_ents = VERGEGame.game.map.num_entities;

            spritebatch.Begin(SpriteSortMode.FrontToBack, blending, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-VERGEGame.game.camera.x, -VERGEGame.game.camera.y, 0.0f));

            for (int i = 0; i < num_ents; i++) {
                cur_ent = ents[i];
                if (!cur_ent.deleted && cur_ent.visible && cur_ent.destination.Intersects(draw_rect))
                    cur_ent.Draw();
            }

            spritebatch.End();
        }
    }

    public abstract class RenderLayer {        
        public LayerType type { get { return _type; } }
        protected LayerType _type;
        public bool visible;
        public String name;
        public Vector2 parallax;
        
        public BlendState blending;
        
        public RenderLayer(Vector2 parallax_vector, String layer_name) : this(parallax_vector, layer_name, BlendState.AlphaBlend) { }
        public RenderLayer(Vector2 parallax_vector, String layer_name, BlendState blend_state) {
            name = layer_name;
            visible = true;
            parallax = parallax_vector;
            blending = blend_state;
        }
    
        public RenderLayer(String layer_name) : this(VERGEMap.NEUTRAL_PARALLAX, layer_name) {}

        public abstract void Draw();
        public virtual void DrawBaseLayer() {
            Draw();
        }
    }
}
