using _24HourSurvival.GUI;
using Engine_lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace _24HourSurvival.Models
{
    public class Creature : GameObject
    {
        private enum State { Wander, Feed, Sleep }
        private State _state = State.Wander;

        private float score = 0.0f;

        private float base_value = 100;
        private float calories_from_food = 25;

        public float hunger = 100;
        public float health = 100;
        public float energy = 100;

        // movement parameters
        public Vector2 target_pos { get; set; }
        private Vector2 direction = Vector2.Zero;
        public float speed = 60;
        public float stop_distance = 0.8f;

        private Vector2 LastKnownFoodPos;

        float smell_radius = 320;
        float sight_radius = 256;
        float move_radius = 320;

        public Creature(string ID, string obj_name, Texture2D texture, Vector2 position) : base(ID, obj_name, texture, position)
        {
            this.OnLeftClick += () => {
                Debug.WriteLine("Health: " + this.health);
                Debug.WriteLine("Hunger: " + this.hunger);
                Debug.WriteLine("Energy: " + this.energy);
                MainGui.SetNewUnitSelected(this);
                MainGui.Update_Info("Creature Score: " + this.score);
            };

            target_pos = Game1.spawner.position;
        }

        public override void Draw(SpriteBatch batch)
        {
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

        /// <summary>
        /// this function runs inputs through the neural networks and determines the state
        /// </summary>
        public void DetermineState()
        {
            var outp = Game1.survival_sim.nets[this.id].Process(
                new double[] 
                { 
                    (double)this.health, 
                    (float)this.energy, 
                    (float)this.hunger
                });

            if (outp[0] > outp[1] && outp[0] > outp[2])
            {
                this._state = State.Wander;
            }
            else if (outp[1] > outp[0] && outp[1] > outp[2])
            {
                this._state = State.Sleep;
            }
            else if (outp[2] > outp[0] && outp[2] > outp[1])
            {
                this._state = State.Feed;
            }
        }

        public override void Update(GameTime gt)
        {
            CanEatFoodCheck();
            UpdateCreatureVitals_Base_AI(gt);

            Sense();

            if (_state == State.Wander)
            {
                WanderBehaviour(gt);
            }
            else if (_state == State.Sleep)
            {
                // do stuff?
            }
            else if (_state == State.Feed)
            {
                var nvect = FoodSense();
                if (nvect == Vector2.Zero)
                {
                    if (this._state != State.Sleep)
                    {
                        WanderBehaviour(gt);
                    }
                }
                else
                {
                    target_pos = nvect;
                    LastKnownFoodPos = nvect;
                }

                MoveToTarget(gt);
            }

            base.Update(gt);
        }

        private void WanderBehaviour(GameTime gt)
        {
            var dist = Vector2.Distance(this.position, target_pos);
            if (dist <= this.stop_distance)
            {
                List<Tile> tiles = new List<Tile>();
                for (int i = 0; i < Game1.world_map.Length; i++)
                {
                    for (int j = 0; j < Game1.world_map[i].Length; j++)
                    {
                        if (Game1.world_map[i][j].object_name == "GrassTile" && Vector2.Distance(this.position, Game1.world_map[i][j].position) < move_radius && Vector2.Distance(this.position, Game1.world_map[i][j].position) > 64)
                        {
                            tiles.Add(Game1.world_map[i][j]);
                        }
                    }
                }

                if(tiles.Count > 0)
                    target_pos = tiles[new Random().Next(0, tiles.Count - 1)].position;
            }
            else
            {
                MoveToTarget(gt);
            }
        }

        private void Sense()
        {           
            // find all nearby food.
            List<Food_Item> sensed_food = new List<Food_Item>();
            for (int f = 0; f < Game1.Food.Count; f++)
            {
                var dist = Vector2.Distance(Game1.Food[f].position, this.position);
                if (dist < smell_radius || dist < sight_radius)
                {
                    sensed_food.Add(Game1.Food[f]);
                }
            }

            if (sensed_food.Count > 0)
            {
                // find the absolute closest food item
                var closest_food = sensed_food[0];
                var closest_dist = Vector2.Distance(sensed_food[0].position, this.position);
                for (int i = 1; i < sensed_food.Count; i++)
                {
                    var n_dist = Vector2.Distance(sensed_food[i].position, this.position);
                    if (n_dist < closest_dist)
                    {
                        closest_dist = n_dist;
                        closest_food = sensed_food[i];
                    }
                }

                LastKnownFoodPos = closest_food.position;
            }

            return;
        }

        private Vector2 FoodSense()
        {
            // find all nearby food.
            List<Food_Item> sensed_food = new List<Food_Item>();
            for (int f = 0; f < Game1.Food.Count; f++)
            {
                var dist = Vector2.Distance(Game1.Food[f].position, this.position);
                if (dist < smell_radius || dist < sight_radius)
                {
                    sensed_food.Add(Game1.Food[f]);
                }
            }

            if (sensed_food.Count > 0)
            {
                // find the absolute closest food item
                var closest_food = sensed_food[0];
                var closest_dist = Vector2.Distance(sensed_food[0].position, this.position);
                for (int i = 1; i < sensed_food.Count; i++)
                {
                    var n_dist = Vector2.Distance(sensed_food[i].position, this.position);
                    if (n_dist < closest_dist)
                    {
                        closest_dist = n_dist;
                        closest_food = sensed_food[i];
                    }
                }

                return closest_food.position;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        public void UpdateCreatureVitals_Base_AI(GameTime gt)
        {
            // if starving, reduce health and score
            if(this.hunger <= 0.0f)
            {
                this.health -= (float)gt.ElapsedGameTime.TotalSeconds;
                this.score -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // if not starving, reduce the hunger level
            if (this.hunger > 0.0f)
            {
                this.hunger -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // handle the state machine

            // if hungry, change the state to find food. 
            if (this._state == State.Wander && this.hunger <= 35)
            {
                //this._state = State.Feed;
                this.DetermineState();
            }
            // ensure creature eats its fill
            else if (this._state == State.Feed && this.hunger > 35 && this.hunger < base_value)
            {
                //this._state = State.Feed;
                this.DetermineState();
            }
            // if not hungry, set back to wander.
            else if(this._state == State.Feed && this.hunger >= base_value)
            {
                //this._state = State.Wander;
                this.DetermineState();
            }

            if (_state != State.Sleep)
            {
                // reduce energy while awake
                if (this.energy > 0.0f)
                {
                    this.energy -= (float)gt.ElapsedGameTime.TotalSeconds;
                }

                // if there is no energy, then put creature to sleep, cannot move with no energy;
                if (this.energy <= 0)
                {
                    //this._state = State.Sleep;
                    this.DetermineState();
                }
            }
            
            if (this._state == State.Sleep)
            {
                // sleep and recover
                if (this.energy < base_value)
                {
                    this.energy += (float)gt.ElapsedGameTime.TotalSeconds * 8;
                }

                // wake up and roam
                if (energy >= base_value)
                {
                    //this._state = State.Wander;
                    this.DetermineState();
                }
            }

            // ensure all values are within thresholds.
            if (this.health > base_value)
            {
                this.health = base_value;
            }
            else if (this.health < 0)
                this.health = 0;

            if (this.energy > base_value)
            {
                this.energy = base_value;
            }
            else if (this.energy < 0)
                this.energy = 0;

            if (this.hunger > base_value)
            {
                this.hunger = base_value;
            }
            else if (this.hunger < 0)
            {
                this.hunger = 0;
            }

            if (score < 0)
            {
                score = 0;
            }
        }

        public float GetScore()
        {
            return this.score;
        }

        public void SetDestination(Vector2 target)
        {
            target_pos = target;
        }

        protected void FaceTarget()
        {
            direction = (target_pos - this.position);
            direction.Normalize();

            rotation = (float)Math.Atan2((double)direction.Y, (double)direction.X) + MathHelper.PiOver2;
        }

        protected void MoveToTarget(GameTime gt)
        {
            this.FaceTarget();
            if (Vector2.Distance(target_pos, position) > stop_distance)
            {
                position += direction * speed * (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        private void CanEatFoodCheck()
        {
            foreach (var item in Game1.Food)
            {
                if (!item.consumed && this.hunger < base_value)
                {
                    var cre_rect = new Rectangle(this.position.ToPoint(), new Point(32, 32));
                    var food_rect = new Rectangle(item.position.ToPoint(), new Point(32, 32));
                    if (cre_rect.Intersects(food_rect))
                    {
                        item.consumed = true;
                        this.score++;
                        this.hunger += calories_from_food;
                        if (this.health < base_value)
                        {
                            this.health += calories_from_food * 0.5f;
                        }

                        if (this.energy < base_value)
                        {
                            this.energy += calories_from_food * 0.5f;
                        }
                    }
                }
            }
        }
    }
}
