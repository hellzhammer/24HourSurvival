using _24HourSurvival.GUI;
using Kronus_Neural.NEAT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace _24HourSurvival.Models
{
    public class Creature : Creature_Model
    {
        protected Vector2 LastKnownFoodPos;

        public readonly string Gene_Sequence = string.Empty;

        public Creature(string ID, string obj_name, Texture2D texture, Vector2 position, int met_base = 1, int enr_base = 1, int reg_base = 2) : base(ID, obj_name, texture, position, met_base, enr_base, reg_base)
        {        
            this.OnLeftClick += () => {
                Debug.WriteLine("Health: " + this.health);
                Debug.WriteLine("Hunger: " + this.hunger);
                Debug.WriteLine("Energy: " + this.energy);

                MainGui.SetNewUnitSelected(this);
                MainGui.Update_Info("Score: " + this.score);
                BrainViewer.BuildNodeLayout(this);
            };

            target_pos = SimpleSurvival.spawner.position;
            this.Gene_Sequence = this.GeneSequenceNeurons();
        }

        public string get_state()
        {
            return this._state.ToString();
        }        

        /// <summary>
        /// this function runs inputs through the neural networks and determines the state
        /// </summary>
        public void DetermineState(GameTime gt)
        {
            Vector2 foodPos = Vector2.Zero;
            if (this.creatureType == CreatureType.Vegetarian)
            {
                var sensefood = FoodSense();
                foodPos = sensefood;
            }
            else{
                var sensecreature = CreatureSense();
                foodPos = sensecreature;
            }
            
            
            var outp = SimpleSurvival.survival_sim.nets[this.id].Process(
                new double[] 
                { 
                    foodPos.X,
                    foodPos.Y,
                    this.position.X,
                    this.position.Y,
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
            else
            {
                // discipline for lack of decisivness;
                //this.score -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void Update(GameTime gt)
        {
            if (creatureType == CreatureType.Vegetarian)
            {
                FoodSense();
            }
            else
            {
                CreatureSense();
            }

            UpdateCreatureVitals_Base_AI(gt);

            if (_state == State.Wander)
            {
                if (speed != 60)
                {
                    speed = 60;
                }
                WanderBehaviour(gt);
            }
            else if (_state == State.Sleep)
            {
                // do stuff?
            }
            else if (_state == State.Feed)
            {
                if (this.creatureType == CreatureType.Vegetarian)
                {
                    CanEatFoodCheck();
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
                else
                {
                    HuntBehaviour(gt);
                }
            }

            base.Update(gt);
        }

        private void HuntBehaviour(GameTime gt)
        {
            if (speed != 100)
            {
                speed = 100;
            }
            CanEatCreatureCheck();
            var v = CreatureSense();
            if (v != Vector2.Zero)
            {
                LastKnownFoodPos = v;
                target_pos = v;
            }
            else
            {
                if (this._state != State.Sleep)
                {
                    WanderBehaviour(gt);
                }
            }

            MoveToTarget(gt);
        }

        private Vector2 CreatureSense()
        {
            List<Creature> sensed_creature = new List<Creature>();
            for (int f = 0; f < SimpleSurvival.creatures.Count; f++)
            {
                var dist = Vector2.Distance(SimpleSurvival.creatures[f].position, this.position);
                if (dist < smell_radius || dist < sight_radius)
                {
                    sensed_creature.Add(SimpleSurvival.creatures[f]);
                }
            }

            if (sensed_creature.Count > 0)
            {
                // find the absolute closest food item
                var closest_food = sensed_creature[0];
                var closest_dist = Vector2.Distance(sensed_creature[0].position, this.position);
                for (int i = 1; i < sensed_creature.Count; i++)
                {
                    var n_dist = Vector2.Distance(sensed_creature[i].position, this.position);
                    if (n_dist < closest_dist)
                    {
                        closest_dist = n_dist;
                        closest_food = sensed_creature[i];
                    }
                }

                return closest_food.position;
            }
            else
            {
                return Vector2.Zero;
            }
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
            var overeat = Handle_Hunger(gt);

            var oversleep = Handle_Sleep(gt);

            if (overeat || oversleep)
            {
                this.score -= 0.000025f;
            }

            ResetOverage();

            Handle_Score(gt);

            if (score < 0)
            {
                score = 0;
            }

            // once every second brain activates
            if (next_update <= 0)
            {
                this.DetermineState(gt);
            }
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

        private bool Handle_Hunger(GameTime gt)
        {
            // if starving
            if (this.hunger <= 0.0f)
            {
                this.health -= (float)gt.ElapsedGameTime.TotalSeconds * 10;
                this.score -= (float)gt.ElapsedGameTime.TotalSeconds * 10;
            }

            // if not starving, reduce the hunger level
            if (this.hunger > 0.0f)
            {
                this.hunger -= (float)gt.ElapsedGameTime.TotalSeconds * actual_metablolism;
            }

            bool rtnval = false;
            if (this.hunger > 75.0f && this._state == State.Feed)
            {
                rtnval = true;
                //this.score -= (float)gt.ElapsedGameTime.TotalSeconds / 10;
            }

            return rtnval;
        }

        private bool Handle_Sleep(GameTime gt)
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
                    this.score -= (float)gt.ElapsedGameTime.TotalSeconds / 10;
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

                bool rtnval = false;
                if (energy >= base_attribute_value)
                {
                    rtnval = true;
                    //this.score -= (float)gt.ElapsedGameTime.TotalSeconds / 10;
                }
                return rtnval;
            }

            return false;
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
                            this.health += calories_from_food * actual_regen_rate;
                        }

                        if (this.energy < base_attribute_value)
                        {
                            this.energy += 2;
                        }
                    }
                }
            }
        }

        public CreatureSaveModel SaveModel()
        {
            return new CreatureSaveModel()
            {
                lastFoodSourceX = (int)this.LastKnownFoodPos.X,
                lastFoodSourceY = (int)this.LastKnownFoodPos.Y,
                genesequence = this.Gene_Sequence,
                CreatureType = (int)this.creatureType,
                CreatureState = (int)this._state,
                actual_energy_use = this.actual_energy_use,
                actual_metablolism = this.actual_metablolism,
                actual_regen_rate = this.actual_regen_rate,
                base_attribute_value = this.base_attribute_value,
                score = this.score,
                hunger = this.hunger,
                energy = this.energy,
                health = this.health,
                calories_from_food = this.calories_from_food,
                smell_radius = this.smell_radius,
                move_radius = this.move_radius,
                sight_radius = this.sight_radius,
                energy_use_base = this.energy_use_base,
                metabolism_base = this.metabolism_base,
                regen_rate_base = this.regen_rate_base,
                speed = this.speed,
                stop_distance = this.stop_distance,
                targetX = (int)this.target_pos.X,
                targetY = (int)this.target_pos.Y,
                id = this.id,
                thisX = this.position.X,
                thisY = this.position.Y
            };
        }

        private void CanEatCreatureCheck()
        {
            foreach (var item in SimpleSurvival.creatures)
            {
                bool canuse = false;
                if (
                    string.IsNullOrWhiteSpace(SimpleSurvival.survival_sim.nets[item.id].species_id) 
                    || SimpleSurvival.survival_sim.nets[item.id].species_id != SimpleSurvival.survival_sim.nets[this.id].species_id
                    )
                {
                    canuse = true;
                }
                if (
                    /*item.creatureType == CreatureType.Vegetarian 
                    &&*/ this.hunger < base_attribute_value 
                    && canuse
                    )
                {
                    var cre_rect = new Rectangle(this.position.ToPoint(), new Point(32, 32));
                    var food_rect = new Rectangle(item.position.ToPoint(), new Point(32, 32));
                    if (cre_rect.Intersects(food_rect))
                    {
                        item.health = 0;
                        this.score++;
                        this.hunger += calories_from_food;

                        if (this.health < base_attribute_value)
                        {
                            this.health += calories_from_food * actual_regen_rate;
                        }

                        if (this.energy < base_attribute_value)
                        {
                            this.energy += 2 * actual_regen_rate;
                        }
                    }
                }
            }
        }
    }
}
