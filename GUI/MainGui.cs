using Engine_lib.UI_Framework.Interfaces;
using Engine_lib.UI_Framework;
using Engine_lib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using _24HourSurvival.Models;

namespace _24HourSurvival.GUI
{
    internal class MainGui : View
    {
        private static bool display = true;
        public static Creature selected_creature { get; private set; }

        Label TimeLabel { get; set; }
        Label SimulationNameLabel { get; set; }
        Label PopulationCountLabel { get; set; }
        Label MoreInfoLabel { get; set; }

        static Label info_label { get; set; }
        static Label GeneLabel { get; set; }

        static Box side_panel_background { get; set; }
        static Label unit_health_label { get; set; }
        static Label unit_energy_label { get; set; }
        static Label unit_state_label { get; set; }
        static Label Unit_Species { get; set; }

        Box pause_background { get; set; }
        Button save_button { get; set; }
        Button quit_button { get; set; }

        public MainGui(GraphicsDevice dev, SpriteFont _font, string teamname)
        {
            if (font == null)
                font = _font;

            build(dev, teamname);
            this.Is_Active = true;
        }

        public static void SetNewUnitSelected(Creature creature)
        {
            selected_creature = creature;
        }

        public void UpdateData(string systemscount, string planetcount)
        {
            PopulationCountLabel.Content = systemscount;
            MoreInfoLabel.Content = planetcount;
        }

        private void build(GraphicsDevice dev, string team_name)
        {
            info_label = new Label("update", "Updates", new Vector2(16, 16), 300, 30, dev);
            GeneLabel = new Label("genes", "Todo", new Vector2(16,64), 300, 30, dev);
            info_label.Set_Background(Color.Black, dev);

            Vector2 ui_pos = new Vector2(0, dev.DisplayMode.Height - 120);
            this.background = new Box("background", ui_pos, dev.DisplayMode.Width, 120, dev);
            TimeLabel = new Label("time", "Time: n/a", ui_pos, 500, 30, dev);

            SimulationNameLabel = new Label("gname", team_name, new Vector2(ui_pos.X, ui_pos.Y + 40), 300, 30, dev);
            PopulationCountLabel = new Label("system", "Population", new Vector2(ui_pos.X + 310, ui_pos.Y + 40), 300, 30, dev);
            MoreInfoLabel = new Label("pcount", "** TO DO **", new Vector2(ui_pos.X + 610, ui_pos.Y + 40), 300, 30, dev);

            TimeLabel.Set_Background(Color.Gray, dev);

            side_panel_background = new Box("side_background", new Vector2(16, dev.DisplayMode.Height / 8), 300, 150, dev);

            // instanitate the side panel labels
            unit_health_label = new Label("sel_name_label", "health", new Vector2(20, dev.DisplayMode.Height / 8), 200, 30, dev);
            unit_energy_label = new Label("sel_health_label", "energy", new Vector2(20, (dev.DisplayMode.Height / 8) + 50), 200, 30, dev);
            unit_state_label = new Label("sel_name_label", "hunger", new Vector2(20, (dev.DisplayMode.Height / 8) + 100), 200, 30, dev);
            Unit_Species = new Label("sel_species_label", "species", new Vector2(16, (dev.DisplayMode.Height / 8) + 150), 300, 30, dev);

            pause_background = new Box("pause_background", new Vector2(650, 250), 300, 300, dev);
            save_button = new Button(
                "save", 
                "Save", 
                new Vector2(pause_background.Position.X + 25, pause_background.Position.Y + 20),
                150,
                40,
                dev
                );

            quit_button = new Button(
                "quit",
                "Exit",
                new Vector2(save_button.Position.X, save_button.Position.Y + 50),
                150,
                40,
                dev
                );
        }

        public override void Draw(SpriteBatch sprite, Matrix view)
        {
            if (Is_Active)
            {
                this.background.Draw(true, sprite, view, font);
                this.TimeLabel.Draw(true, sprite, view, font);
                this.SimulationNameLabel.Draw(true, sprite, view, font);
                this.PopulationCountLabel.Draw(true, sprite, view, font);
                this.MoreInfoLabel.Draw(true, sprite, view, font);
                info_label.Draw(true, sprite, view, font);
                if (display)
                {
                    side_panel_background.Draw(true, sprite, view, font);
                    unit_energy_label.Draw(true, sprite, view, font);
                    unit_health_label.Draw(true, sprite, view, font);
                    unit_state_label.Draw(true, sprite, view, font);
                    Unit_Species.Draw(true, sprite, view, font);
                    GeneLabel.Draw(true, sprite, view, font);
                }
            }

            if (SimpleSurvival.Pause_Game)
            {
                pause_background.Draw(true, sprite, view, font);
                save_button.Draw(true, sprite, view, font);
                quit_button.Draw(true, sprite, view, font);
            }
        }

        public static void Update_Info(string update_data)
        {
            info_label.Content = update_data;
            var measure = SimpleSurvival.Game_Font.MeasureString(update_data);
            info_label.background = SimpleSurvival.CreateSquare(
                SimpleSurvival._graphics.GraphicsDevice,
                (int)measure.X,
                (int)measure.Y,
                (s) => Color.Black
                );
        }

        public override void Update()
        {
            if (!SimpleSurvival.Pause_Game)
            {
                if (Is_Active)
                {
                    string hour = CalendarSystem.Current_Hour.ToString();
                    if (hour.Length == 1)
                    {
                        hour = "0" + CalendarSystem.Current_Hour.ToString();
                    }
                    string minute = CalendarSystem.Current_Minute.ToString();
                    if (minute.Length == 1)
                    {
                        minute = "0" + CalendarSystem.Current_Minute.ToString();
                    }
                    string second = Math.Round(CalendarSystem.Current_Second).ToString();
                    if (second.Length == 1)
                    {
                        second = "0" + Math.Round(CalendarSystem.Current_Second).ToString();
                    }

                    string day = CalendarSystem.Day.ToString();
                    if (day.Length == 1)
                    {
                        day = "0" + CalendarSystem.Day.ToString();
                    }
                    string month = CalendarSystem.Month.ToString();
                    if (month.Length == 1)
                    {
                        month = "0" + CalendarSystem.Month.ToString();
                    }

                    this.MoreInfoLabel.Content = "Known Species: " + SimpleSurvival.survival_sim.species.Count;

                    this.TimeLabel.Content =
                        "Time: " + hour + " : " + minute + " : " + second
                        + " -- Date: " + day + " / " + month + " / " + CalendarSystem.Year
                        + " - Epoch: " + SimpleSurvival.survival_sim.epoch;

                    if (selected_creature != null)
                    {
                        GeneLabel.Content = selected_creature.Gene_Sequence;
                        info_label.Content = "Score: " + selected_creature.GetScore().ToString();
                        unit_health_label.Content = "Health: " + Math.Round(selected_creature.health) + " -- Energy: " + Math.Round(selected_creature.energy);
                        unit_energy_label.Content = "Hunger: " + Math.Round(selected_creature.hunger);
                        unit_state_label.Content = "State: " + selected_creature.get_state();
                        if (SimpleSurvival.survival_sim.nets.ContainsKey(selected_creature.id))
                        {
                            if (string.IsNullOrWhiteSpace(SimpleSurvival.survival_sim.nets[selected_creature.id].species_id))
                                Unit_Species.Content = "unknown";
                            else
                                Unit_Species.Content = SimpleSurvival.survival_sim.nets[selected_creature.id].species_id;
                        }
                    }

                    PopulationCountLabel.Content = "Population: " + SimpleSurvival.creatures.Count;
                }
            }
        }
    }
}
