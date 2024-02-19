﻿using _24HourSurvival.GUI;
using _24HourSurvival.Models;
using Engine_lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace _24HourSurvival
{
    public class SimpleSurvival : Engine
    {
        public static bool Pause = false;
        public static bool TrainingAlgorithm = false;

        public static NEAT_Survival_Sim survival_sim { get; set; }

        private MainGui gui { get; set; }
        public readonly int PopMax = 10;

        private CalendarSystem cs;
        public static Tile[][] world_map { get; set; }

        public static Dictionary<string, Texture2D> texture_library { get; set; }
        public static Texture2D foodTexture { get; set; }
        public static Texture2D spawnerTexture { get; set; }
        public static Texture2D creatureTexture { get; set; }

        public static List<Food_Item> Food = new List<Food_Item>();
        public static List<Creature> creatures = new List<Creature>();
        public static Creature_Spawner spawner { get; set; }

        public static Dictionary<int, (string gene_id, int energy_use, int metabolism, int regen_rate)> genes { get; set; }

        public SimpleSurvival() : base("Default")
        {
            genes = new Dictionary<int, (string gene_id, int energy_use, int metabolism, int regen_rate)>();
            var codons = Generate_Codons(102292);
            Random r = new Random(102292);
            for (int i = 0; i < codons.Count; i++)
            {
                genes.Add(i, new (codons[i],
                    r.Next(-1, 4), 
                    r.Next(-1, 4), 
                    r.Next(-1, 4))
                    );
            }

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            camera = new Camera(this.GraphicsDevice.Viewport);

            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            _graphics.ApplyChanges();

            // prepare the galaxy component
            cs = new CalendarSystem(this);
            CalendarSystem.Year = 3000;

            survival_sim = new NEAT_Survival_Sim(3, 3, false, 0.40, 100, 2500);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Game_Font = Content.Load<SpriteFont>("Game_Font");

            texture_library = new Dictionary<string, Texture2D>();
            texture_library.Add("water", Engine.CreateSquare(GraphicsDevice, 32, 32, (s) => Color.Blue));
            texture_library.Add("grass", Engine.CreateSquare(GraphicsDevice, 32, 32, (s) => Color.Green));
            texture_library.Add("hover", Engine.CreateSquare(GraphicsDevice, 32, 32, (s) => Color.Gray));

            spawnerTexture = Engine.CreateCircle(GraphicsDevice, 16, 32, 32, Color.Red);
            foodTexture = Engine.CreateCircle(GraphicsDevice, 16, 32, 32, Color.Pink);
            creatureTexture = Engine.CreateCircle(GraphicsDevice, 16, 32, 32, Color.Black);

            // TODO: use this.Content to load your game content here

            // build the world map
            world_map = BuildMap(200, 200);

            // place the spawner
            int x = new Random().Next(12, world_map[0].Length - 12) * 32;
            int y = new Random().Next(12, world_map.Length - 12) * 32;
            spawner = new Creature_Spawner("1", "Spawner", spawnerTexture, new Vector2(x,y));

            ResetPopulation();

            // create the gui
            gui = new MainGui(
                this.GraphicsDevice,
                Game_Font,
                "Test"
                );
        }

        /// <summary>
        /// creates new batches of creatures, clears old ones.
        /// </summary>
        private void ResetPopulation()
        {
            creatures = new List<Creature>();
            // create first generation of creatures
            foreach (var item in survival_sim.nets)
            {
                SpawnNewCreature(item.Key);
            }
        }

        bool training = false;
        protected override void Update(GameTime gameTime)
        {
            if (Input.KeyUp(Keys.Escape))
            {
                if (MainGui.selected_creature != null)
                    MainGui.SetNewUnitSelected(null);
                else
                    Exit();
            }

            if (CalendarSystem.TotalDays == 2)
                TrainingAlgorithm = true;

            if (!TrainingAlgorithm)
            {
                // TODO: Add your update logic here
                Cleanse_Creatures();
                
                Clear_Eaten_Food();

                camera.Update();

                Update_Creatures(gameTime);

                Update_Food();

                gui.Update();
            }
            else
            {
                if (!training)
                {
                    // set training flag.
                    training = true;

                    // run training algorithm.
                    survival_sim.Run();

                    // reset the calendar.
                    CalendarSystem.Day = 0;
                    CalendarSystem.TotalDays = 0;

                    ResetPopulation();

                    MainGui.SetNewUnitSelected(null);

                    // reset flags.
                    training = false;
                    TrainingAlgorithm = false;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(6, 1, 22, 1));
            this._spriteBatch.Begin(transformMatrix: Camera.main_camera.GetViewMatrix(), blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            // TODO: Add your drawing code here

            // draw map first
            for (int i = 0; i < world_map.Length; i++)
            {
                for (int j = 0; j < world_map[i].Length; j++)
                {
                    world_map[i][j].Draw(this._spriteBatch);
                }
            }

            // then draw the spawner
            spawner.Draw(this._spriteBatch);

            // then drawn the food
            foreach (var item in Food)
            {
                item.Draw(this._spriteBatch);
            }

            // finally draw the creatures
            foreach (var item in creatures)
            {
                item.Draw(this._spriteBatch);
            }

            if (MainGui.selected_creature != null)
            {
                _spriteBatch.Draw(
                Engine.CreateCircle(SimpleSurvival._graphics.GraphicsDevice, MainGui.selected_creature.move_radius, (int)MainGui.selected_creature.move_radius * 2, (int)MainGui.selected_creature.move_radius * 2, Color.Red * 0.5f),
                MainGui.selected_creature.position,
                null,
                Color.White,
                MainGui.selected_creature.rotation,
                new Vector2((MainGui.selected_creature.move_radius * 2) / 2, (MainGui.selected_creature.move_radius* 2) / 2),
                MainGui.selected_creature.scale,
                SpriteEffects.None,
                1
                );
            }

            gui.Draw(_spriteBatch, camera.GetViewMatrix());

            this._spriteBatch.End();
            base.Draw(gameTime);
        }

        private Tile[][] BuildMap(int width, int height)
        {
            int id = 1;
            List<Tile[]> tiles = new List<Tile[]>();
            for (int i = 0; i < 32 * height; i+=32)
            {
                List<Tile> _tiles = new List<Tile>();
                for (int j = 0; j < 32 * width; j+=32)
                {
                    var center = new Vector2((width * 32) / 2, (height * 32) / 2);
                    var n_vector = new Vector2(j, i);

                    var dist = Vector2.Distance(center, n_vector);
                    if (dist < (width * 32) / 2)
                    {
                        _tiles.Add(
                        new Tile(id.ToString(), "GrassTile", texture_library["grass"], n_vector)
                        );
                    }
                    else
                    {
                        _tiles.Add(
                        new Tile(id.ToString(), "WaterTile", texture_library["water"], n_vector)
                        );
                    }

                    id++;
                }
                tiles.Add(_tiles.ToArray());
            }
            return tiles.ToArray();
        }

        System.DateTime next = System.DateTime.Now;
        private void Update_Food()
        {
            if (System.DateTime.Now > next)
            {
                for (int i = 0; i < 20; i++)
                {
                    SpawnFoodItem();
                }

                next = System.DateTime.Now.AddSeconds(10);
            }

            return;
        }

        private void Update_Creatures(GameTime gt)
        {
            foreach (var item in creatures)
            {
                item.Update(gt);
            }

            return;
        }

        private void SpawnFoodItem()
        {
            int x = new System.Random().Next(3, world_map[0].Length - 4) * 32;
            int y = new System.Random().Next(3, world_map.Length - 4) * 32;

            var center = new Vector2((world_map[0].Length * 32) / 2, (world_map.Length * 32) / 2);
            var n_vector = new Vector2(x, y);

            var dist = Vector2.Distance(center, n_vector);
            if (dist < ((world_map[0].Length * 32) / 2) - 128)
            {
                Food.Add(
                new Food_Item((Food.Count + 1).ToString(), "Food", foodTexture, new Vector2(x, y))
                );
            }

            return;
        }

        private void Clear_Eaten_Food()
        {
            List<Food_Item> indexes = new List<Food_Item>();
            for (int i = 0; i < Food.Count; i++)
            {
                if (Food[i].consumed)
                {
                    indexes.Add(Food[i]);
                }
            }
            for (int i = 0; i < indexes.Count; i++)
            {
                if (Food.Contains(indexes[i]))
                {
                    Food.Remove(indexes[i]);
                }
            }

            return;
        }

        private void Cleanse_Creatures()
        {
            List<Creature> indexes = new List<Creature>();
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].health <= 0)
                {
                    // dead creatures cannot carry on with their scores. 
                    // death resets score to 0;
                    if (survival_sim.nets.ContainsKey(creatures[i].id))
                    {
                        survival_sim.nets[creatures[i].id].network_score = 0;
                    }
                    indexes.Add(creatures[i]);
                }
            }
            for (int i = 0; i < indexes.Count; i++)
            {
                if (creatures.Contains(indexes[i]))
                {
                    creatures.Remove(indexes[i]);
                }
            }

            return;
        }

        public static void SpawnNewCreature(string id)
        {
            var c = new Creature(id, "Creature", creatureTexture, spawner.position);

            creatures.Add(c);

            return;
        }

        public List<string> Generate_Codons(int seed = 1000)
        {
            List<string> ids = new List<string>();
            int total_codons = 64;
            bool run = true;
            string[] nucleotides = new string[] { "a", "c", "g", "t" };
            Random r = new Random(seed);

            do
            {
                string Codon = string.Empty;

                // build a new codon of
                // up to 9 different parts
                for (int i = 0; i < 3; i++)
                {
                    string nuc = string.Empty;

                    for (int j = 0; j < 1; j++)
                    {
                        // get the next id at random
                        int id = r.Next(0, nucleotides.Length);

                        // add new nucleotide to string
                        nuc += nucleotides[id];
                    }

                    // add new string to codon
                    Codon += nuc;
                }

                // if the new created codon
                // does not already exist,
                // add it to list
                if (!ids.Contains(Codon))
                {
                    ids.Add(Codon);
                }

                // if codon count is == total
                // codons then we can break.
                if (ids.Count == total_codons)
                {
                    run = false;
                }

            } while (run);

            return ids;
        }
    }
}