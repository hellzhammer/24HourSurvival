using _24HourSurvival.GUI;
using Engine_lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _24HourSurvival.Models
{
    public class Creature_Model : GameObject
    {
        protected float score = 0.0f;
        public float hunger = 100;
        public float health = 100;
        public float energy = 100;

        protected float base_attribute_value = 100;
        protected float calories_from_food = 50;

        // movement parameters
        public Vector2 target_pos { get; set; }
        protected Vector2 direction = Vector2.Zero;
        public float speed = 60;
        public float stop_distance = 0.8f;

        public readonly float smell_radius = 320;
        public readonly float sight_radius = 256;
        public readonly float move_radius = 320;

        protected bool draw_radial = false;

        protected readonly int metabolism_base = 1;
        protected readonly int energy_use_base = 1;
        protected readonly int regen_rate_base = 2;

        protected int actual_metablolism = 1;
        protected int actual_energy_use = 1;
        protected int actual_regen_rate = 2;

        public Creature_Model(string ID, string obj_name, Texture2D texture, Vector2 position, int met_base, int enr_base, int reg_base) : base(ID, obj_name, texture, position)
        {
            this.metabolism_base = met_base;
            this.energy_use_base = enr_base;
            this.regen_rate_base = reg_base;
            this.actual_energy_use = energy_use_base;
            this.actual_metablolism = metabolism_base;
            this.actual_regen_rate = regen_rate_base;

            this.OnMouseOver += () =>
            {
                this.draw_radial = true;
            };

            this.OnMouseExit += () =>
            {
                this.draw_radial = false;
            };
        }

        public override void Draw(SpriteBatch batch)
        {
            if (this.draw_radial && MainGui.selected_creature != this)
            {
                batch.Draw(
                Engine.CreateCircle(
                    SimpleSurvival._graphics.GraphicsDevice,
                    move_radius / 4,
                    (int)move_radius / 2,
                    (int)move_radius / 2,
                    Color.LightBlue * 0.5f
                    ),

                position,
                null,
                Color.White,
                rotation,
                new Vector2(
                    (move_radius / 2) / 2,
                    (move_radius / 2) / 2
                    ),
                scale,
                SpriteEffects.None,
                1
                );
            }

            batch.Draw(
            this.object_sprite,
            position,
            null,
            Color.White,
            rotation,
            new Vector2(16, 16),
            scale,
            SpriteEffects.None,
            1
            );
        }
    }
}
