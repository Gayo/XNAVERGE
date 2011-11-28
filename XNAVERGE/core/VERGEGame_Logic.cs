﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace XNAVERGE {
    public partial class VERGEGame {
        public int tick { get { return _tick; } }
        protected int _tick; // Centiseconds since app start. This will overflow if you leave the game on for 248 days.   
        public ScriptBank global;


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            BoundedSpace<Entity>.BoundedElementSet ent_enum;
            Entity ent;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            _tick++;
            input.Update();
            //if (player_controllable && player != null) control_player();
            if (map != null) {
                for (int i = 0; i < map.num_entities; i++) {
                    ent_enum = entity_space.elements_within_bounds(map.entities[i].hitbox, true, map.entities[i]);
                    while (ent_enum.GetNext(out ent))
                    {                        
                        //if (ent != map.entities[i] && ent == player) Console.WriteLine("player hit " + i);
                    }
                    map.entities[i].Update();
                }
            }

            base.Update(gameTime);
        }

        // Tries to find a delegate matching the specified type in either the map or global script banks.
        // Checks map scripts first, then global scripts. Returns null if T is not a delegate or if the
        // script does not exist. Throws AmbiguousMatchException if the script is overloaded within a
        // single script bank.
        // TODO: Maybe allow for multiple global script banks?
        public virtual T script<T>(String name) where T : class {
            T script = VERGEGame.game.map.scripts.get_script<T>(name);
            if (script == null) return global.get_script<T>(name);
            return script;
        }

    }    
}
