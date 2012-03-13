using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public class RenderStack {
        public List<RenderLayer> list;

        public RenderStack() { // Empty renderstack
            list = new List<RenderLayer>();
        }

        public RenderStack(VERGEMap map, String rstring) : this(map, rstring, ',') {} // MAP file renderstrings are always comma-delimited

        public RenderStack(VERGEMap map, String rstring, Char delim) {
            int cur_pos, next_pos, len, layer_number;
            bool added_base_layer = false;
            String str = rstring.Trim().ToUpper();            
            String cur_token;
            RenderLayer cur_layer;
            list = new List<RenderLayer>();
                
            cur_pos = 0;
            len = str.Length;
            while (cur_pos < len) {
                next_pos = str.IndexOf(delim, cur_pos);
                if (next_pos == -1) next_pos = len;
                cur_token = str.Substring(cur_pos, next_pos - cur_pos).Trim();
                //Console.WriteLine(cur_token);
                switch (cur_token) {
                    case "R": // rendering script layer (defaults to hook_render and fixed parallax)
                        cur_layer = new ScriptRenderLayer();
                        list.Add(cur_layer);
                        break;
                    case "E": // entity layer
                        cur_layer = new EntityLayer();
                        list.Add(cur_layer);
                        break;
                    default: // tile layer
                        try {
                            layer_number = Int32.Parse(cur_token);
                            if (layer_number <= 0) throw new Exception();                            
                        }
                        catch (Exception) { throw new MalformedRenderstringException(rstring); } // not a positive integer                        
                        cur_layer = map.tiles[layer_number - 1];
                        if (!added_base_layer) {
                            ((TileLayer)cur_layer).base_layer = true;
                            added_base_layer = true;
                        }
                        list.Add(cur_layer);                        
                        break;                        
                }
                cur_pos = next_pos + 1;
            }
        }

        // sets all layers in the stack to visible
        public void show_all() {
            foreach (RenderLayer layer in list)
                if (layer.type == LayerType.Tile) {
                    layer.visible = true;
                }
        }

        public void Draw() {
            for( int i = 0; i < list.Count; i++ ) {
                if( list[i].visible ) list[i].Draw();                                    
            }
        }
    }

    public class MalformedRenderstringException : Exception {
        public MalformedRenderstringException(String rstring) : base("\"" + rstring + "\" is not a valid renderstring.") {}
    }
}
