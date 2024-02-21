using _24HourSurvival.GUI;
using Kronus_Neural.NEAT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace _24HourSurvival.Models
{
    public class Creature : Creature_Model
    {
        protected Vector2 LastKnownFoodPos;

        protected enum State { Wander, Feed, Sleep }
        protected State _state = State.Wander;
        public readonly string Gene_Sequence = string.Empty;

        public Creature(string ID, string obj_name, Texture2D texture, Vector2 position, int met_base = 1, int enr_base = 1, int reg_base = 2) : base(ID, obj_name, texture, position, met_base, enr_base, reg_base)
        {        
            this.OnLeftClick += () => {
                Debug.WriteLine("Health: " + this.health);
                Debug.WriteLine("Hunger: " + this.hunger);
                Debug.WriteLine("Energy: " + this.energy);

                MainGui.SetNewUnitSelected(this);
                MainGui.Update_Info("Score: " + this.score);
            };

            target_pos = SimpleSurvival.spawner.position;
            this.Gene_Sequence = this.GeneSequence();
        }

        public string get_state()
        {
            return this._state.ToString();
        }        

        /// <summary>
        /// this function runs inputs through the neural networks and determines the state
        /// </summary>
        public void DetermineState()
        {
            var outp = SimpleSurvival.survival_sim.nets[this.id].Process(
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
                for (int i = 0; i < SimpleSurvival.world_map.Length; i++)
                {
                    for (int j = 0; j < SimpleSurvival.world_map[i].Length; j++)
                    {
                        if (SimpleSurvival.world_map[i][j].object_name == "GrassTile" && Vector2.Distance(this.position, SimpleSurvival.world_map[i][j].position) < move_radius && Vector2.Distance(this.position, SimpleSurvival.world_map[i][j].position) > 64)
                        {
                            tiles.Add(SimpleSurvival.world_map[i][j]);
                        }
                    }
                }

                if(tiles.Count > 0)
                    target_pos = tiles[SimpleSurvival.survival_sim.r.Next(0, tiles.Count - 1)].position;
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
            for (int f = 0; f < SimpleSurvival.Food.Count; f++)
            {
                var dist = Vector2.Distance(SimpleSurvival.Food[f].position, this.position);
                if (dist < smell_radius || dist < sight_radius)
                {
                    sensed_food.Add(SimpleSurvival.Food[f]);
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
            for (int f = 0; f < SimpleSurvival.Food.Count; f++)
            {
                var dist = Vector2.Distance(SimpleSurvival.Food[f].position, this.position);
                if (dist < smell_radius || dist < sight_radius)
                {
                    sensed_food.Add(SimpleSurvival.Food[f]);
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
            Handle_Hunger(gt);

            Handle_Sleep(gt);

            ResetOverage();

            Handle_Score(gt);

            if (score < 0)
            {
                score = 0;
            }

            // once every second brain activates
            if (next_update <= 0)
            {
                this.DetermineState();
            }
        }

        public string GeneSequence()
        {
            string rtnval = string.Empty;

            if (SimpleSurvival.survival_sim.nets.ContainsKey(this.id))
            {
                var net = SimpleSurvival.survival_sim.nets[this.id];
                if (net != null)
                {
                    List<Neuron> neurons = new List<Neuron>();
                    foreach (var n in net.Input_Neurons)
                    {
                        neurons.Add(n.Value);
                    }

                    foreach (var n in net.Output_Neurons)
                    {
                        neurons.Add(n.Value);
                    }

                    foreach (var n in net.Hidden_Neurons)
                    {
                        neurons.Add(n.Value);
                    }

                    for (int i = 0; i < neurons.Count; i++)
                    {
                        var s = SimpleSurvival.codons[int.Parse(neurons[i].gene_id)];
                        rtnval += s;
                    }
                }
            }

            return rtnval;
        }

        private void Handle_Score(GameTime gt)
        {
            if(hunger <= 0 || energy <= 0)
                this.score -= (float)gt.ElapsedGameTime.TotalSeconds / 10;

            if (SimpleSurvival.survival_sim.nets.ContainsKey(this.id))
            {
                SimpleSurvival.survival_sim.nets[this.id].current_fitness = score;
            }
        }

        private void Handle_Hunger(GameTime gt)
        {
            // if starving
            if (this.hunger <= 0.0f)
            {
                this.health -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // if not starving, reduce the hunger level
            if (this.hunger > 0.0f)
            {
                this.hunger -= (float)gt.ElapsedGameTime.TotalSeconds * actual_metablolism;
            }
        }

        private void Handle_Sleep(GameTime gt)
        {
            if (_state != State.Sleep)
            {
                // reduce energy while awake
                if (this.energy > 0.0f)
                {
                    this.energy -= (float)gt.ElapsedGameTime.TotalSeconds * actual_energy_use;
                }

                // if there is no energy, then put creature to sleep, cannot move with no energy;
                if (this.energy <= 0)
                {
                    //this._state = State.Sleep;
                    this.actual_metablolism += (int)gt.ElapsedGameTime.TotalSeconds / 10;
                }
            }

            if (this._state == State.Sleep)
            {
                // sleep and recover
                if (this.energy < base_attribute_value)
                {
                    this.energy += (float)gt.ElapsedGameTime.TotalSeconds * actual_regen_rate;
                    this.score += (float)gt.ElapsedGameTime.TotalSeconds;
                }

                // wake up and roam
                if (energy >= base_attribute_value && actual_metablolism > metabolism_base)
                {
                    this.actual_metablolism = metabolism_base;
                }
            }
        }

        /// <summary>
        /// if over n set to n, if less than 0 set to 0
        /// </summary>
        private void ResetOverage()
        {
            // ensure all values are within thresholds.
            if (this.health > base_attribute_value)
            {
                this.health = base_attribute_value;
            }
            else if (this.health < 0)
                this.health = 0;

            if (this.energy > base_attribute_value)
            {
                this.energy = base_attribute_value;
            }
            else if (this.energy < 0)
                this.energy = 0;

            if (this.hunger > base_attribute_value)
            {
                this.hunger = base_attribute_value;
            }
            else if (this.hunger < 0)
            {
                this.hunger = 0;
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
            foreach (var item in SimpleSurvival.Food)
            {
                if (!item.consumed && this.hunger < base_attribute_value)
                {
                    var cre_rect = new Rectangle(this.position.ToPoint(), new Point(32, 32));
                    var food_rect = new Rectangle(item.position.ToPoint(), new Point(32, 32));
                    if (cre_rect.Intersects(food_rect))
                    {
                        item.consumed = true;
                        this.score++;
                        this.hunger += calories_from_food;

                        if (this.health < base_attribute_value)
                        {
                            this.health += calories_from_food * 0.5f;
                        }

                        if (this.energy < base_attribute_value)
                        {
                            this.energy += 2;
                        }
                    }
                }
            }
        }
    }
}
