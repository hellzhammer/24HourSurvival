using _24HourSurvival.GUI;
using Engine_lib;
using Kronus_Neural.NEAT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace _24HourSurvival.Models
{
    public class Creature_Model : GameObject
    {
        public enum CreatureType
        {
            Carnivore,
            Vegetarian
        }
        protected CreatureType creatureType = CreatureType.Vegetarian;

        protected enum State { Wander, Feed, Sleep }
        protected State _state = State.Wander;

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

        public readonly float smell_radius = 450;
        public readonly float sight_radius = 250;
        public readonly float move_radius = 650;

        protected bool draw_radial = false;

        protected int metabolism_base = 1;
        protected int energy_use_base = 1;
        protected int regen_rate_base = 2;

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

        public CreatureType GetCreatureType()
        {
            return this.creatureType;
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

        public string GeneSequenceNeurons()
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
                        var in_gene = SimpleSurvival.genes[int.Parse(neurons[i].gene_id)];
                        this.metabolism_base += in_gene.metabolism;
                        this.energy_use_base += in_gene.energy_use;
                        this.regen_rate_base += in_gene.regen_rate;

                        if (in_gene.isCarnivore)
                        {
                            if (SimpleSurvival.survival_sim.r.NextDouble() <= 0.20)
                            {
                                this.creatureType = CreatureType.Carnivore;
                            }
                            else
                            {
                                this.creatureType = CreatureType.Vegetarian;
                            }
                        }
                        else
                        {
                            this.creatureType = CreatureType.Vegetarian;
                        }

                        if (metabolism_base < 1)
                        {
                            metabolism_base = 1;
                        }
                        if (energy_use_base < 1)
                        {
                            energy_use_base = 1;
                        }
                        if (regen_rate_base < 1)
                        {
                            regen_rate_base = 1;
                        }                        

                        var s = SimpleSurvival.codons[int.Parse(neurons[i].gene_id)];
                        rtnval += s;
                    }
                }
            }

            return rtnval;
        }

        /*public string GeneSequenceConnections()
        {
            string rtnval = string.Empty;

            if (SimpleSurvival.survival_sim.nets.ContainsKey(this.id))
            {
                var net = SimpleSurvival.survival_sim.nets[this.id];
                foreach (var connection in net.All_Connections)
                {
                    rtnval += SimpleSurvival.codons[int.Parse(connection.Value.Input)];
                    var in_gene = SimpleSurvival.genes[int.Parse(connection.Value.Input)];
                    this.metabolism_base += in_gene.metabolism;
                    this.energy_use_base += in_gene.energy_use;
                    this.regen_rate_base += in_gene.regen_rate;

                    if (in_gene.isCarnivore)
                    {
                        if (SimpleSurvival.survival_sim.r.NextDouble() >= 0.99)
                        {
                            this.creatureType = CreatureType.Carnivore;
                        }
                        else
                        {
                            this.creatureType = CreatureType.Vegetarian;
                        }
                    }
                    else
                    {
                        this.creatureType = CreatureType.Vegetarian;
                    }

                    rtnval += SimpleSurvival.codons[int.Parse(connection.Value.Output)];
                    var out_gene = SimpleSurvival.genes[int.Parse(connection.Value.Output)];
                    this.metabolism_base += out_gene.metabolism;
                    this.energy_use_base += out_gene.energy_use;
                    this.regen_rate_base += out_gene.regen_rate;

                    if (in_gene.isCarnivore)
                    {
                        if (SimpleSurvival.survival_sim.r.NextDouble() >= 0.99)
                        {
                            this.creatureType = CreatureType.Carnivore;
                        }
                        else
                        {
                            this.creatureType = CreatureType.Vegetarian;
                        }
                    }
                    else
                    {
                        this.creatureType = CreatureType.Vegetarian;
                    }

                    if (metabolism_base < 1)
                    {
                        metabolism_base = 1;
                    }
                    if (energy_use_base < 1)
                    {
                        energy_use_base = 1;
                    }
                    if (regen_rate_base < 1)
                    {
                        regen_rate_base = 1;
                    }
                    
                }
            }

            return rtnval;
        }*/
    }
}
