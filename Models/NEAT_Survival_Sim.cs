using Kronus_Neural.Activations;
using Kronus_Neural.NEAT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace _24HourSurvival.Models
{
    public class NEAT_Survival_Sim : NEAT_Project
    {
        public Dictionary<string, (int epoch_found, Texture2D species_color)> species_dict { get; set; }
        public Network winning_network { get; private set; }
        public NEAT_Survival_Sim(
            int input_count,
            int output_count,
            bool initFullyConnected,
            double chanceToConnect,
            int pop_max,
            int training_epochs, 
            int seed
            )
        {
            this.Seed = seed;
            this.r = new System.Random(this.Seed);
            this.species_dict = new Dictionary<string, (int epoch_found, Texture2D species_color)>();
            this.max_hidden_nodes = 64 - (input_count + output_count);
            this.mutate_activation = false;
            this.Allowed_Activations = new List<string>();
            this.Allowed_Activations.Add("Sigmoid");
            //"Gaussian", "ReLU", "LeakyReLU", "Sigmoid", "Swish", "SoftPlus", "TanH"

            this.Clone_Elite = true;
            this.nets = new Dictionary<string, NeatNetwork>();
            this.species = new Dictionary<string, Species>();
            this.gene_tracker = new Genetic_Dictionary();

            this.chance_to_make_inital_connection = chanceToConnect;
            this.input_neuron_count = input_count;
            this.output_neuron_count = output_count;
            this.init_fully_connected = initFullyConnected;
            this.PopulationMax = pop_max;
            this.total_epochs = training_epochs;

            this.totalSpeciesCountTarget = 15;

            this.Hidden_Activation_Function = new Sigmoid();
            this.Output_Activation_Function = new Sigmoid();

            this.init_project();
        }

        public override void Run()
        {
            // increment the current epoch
            this.epoch++;

            if (epoch == total_epochs)
            {
                // end the simulation
            }
            else
            {
                // keep running simulation
                string _id = this.Find_Fittest_Network();
                if (nets.ContainsKey(_id))
                {
                    winning_network = nets[_id];
                }

                // run the training algorithm
                this.Train();

                // track all new genes
                foreach (var net in nets)
                {
                    foreach (var connection in net.Value.All_Connections)
                    {
                        if (!this.gene_tracker.Connection_Exists(connection.Key))
                        {
                            this.gene_tracker.Add_Connection(connection.Key, epoch);
                        }
                    }
                    foreach (var node in net.Value.Hidden_Neurons)
                    {
                        if (!this.gene_tracker.Neuron_Exists(node.Key))
                        {
                            this.gene_tracker.Add_Node(node.Key, epoch);
                        }
                    }
                }
            }
        }

        public override void init_project()
        {
            for (int i = 0; i < this.PopulationMax; i++)
            {
                var net = Network_Generator.Generate_New_Network_Neurons(this.input_neuron_count, this.output_neuron_count, this.Hidden_Activation_Function, this.Output_Activation_Function);
                if (this.init_fully_connected)
                {
                    net = Network_Generator.Init_Connections_fully_connected(net, weight_init_min, weight_init_max);
                }
                else
                {
                    net = Network_Generator.Init_Connections_random_connections(net, r, this.chance_to_make_inital_connection, weight_init_min, weight_init_max);
                }
                nets.Add(net.network_id, net);
            }
            return;
        }

        private void Set_Winner(NeatNetwork winner)
        {
            NeatNetwork n = winner.clone();
            n.network_score = winner.network_score;
            n.current_fitness = winner.current_fitness;
            n.current_adjusted_fitness = winner.current_adjusted_fitness;
            this.winning_network = n;
        }
    }
}
