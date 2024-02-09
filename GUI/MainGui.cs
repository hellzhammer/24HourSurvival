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
        static Creature selected_creature { get; set; }

        Label TimeLabel { get; set; }
        Label SimulationNameLabel { get; set; }
        Label PopulationCountLabel { get; set; }
        Label MoreInfoLabel { get; set; }

        static Label Update_Label { get; set; }

        static Box side_panel_background { get; set; }
        static Label Unit_Health { get; set; }
        static Label Unit_Energy { get; set; }
        static Label Unit_Hunger { get; set; }

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
            Update_Label = new Label("update", "Updates", new Vector2(16, 16), 300, 30, dev);
            Update_Label.Set_Background(Color.Black, dev);

            Vector2 ui_pos = new Vector2(0, dev.DisplayMode.Height - 120);
            this.background = new Box("background", ui_pos, dev.DisplayMode.Width, 120, dev);
            TimeLabel = new Label("time", "Time: n/a", ui_pos, 300, 30, dev);

            SimulationNameLabel = new Label("gname", team_name, new Vector2(ui_pos.X, ui_pos.Y + 40), 300, 30, dev);
            PopulationCountLabel = new Label("system", "Population", new Vector2(ui_pos.X + 310, ui_pos.Y + 40), 300, 30, dev);
            MoreInfoLabel = new Label("pcount", "** TO DO **", new Vector2(ui_pos.X + 610, ui_pos.Y + 40), 300, 30, dev);

            TimeLabel.Set_Background(Color.Gray, dev);

            side_panel_background = new Box("side_background", new Vector2(16, dev.DisplayMode.Height / 8), 300, 150, dev);

            // instanitate the side panel labels
            Unit_Health = new Label("sel_name_label", "health", new Vector2(20, dev.DisplayMode.Height / 8), 200, 30, dev);
            Unit_Energy = new Label("sel_health_label", "energy", new Vector2(20, (dev.DisplayMode.Height / 8) + 50), 200, 30, dev);
            Unit_Hunger = new Label("sel_name_label", "hunger", new Vector2(20, (dev.DisplayMode.Height / 8) + 100), 200, 30, dev);
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
                Update_Label.Draw(true, sprite, view, font);
                if (display)
                {
                    side_panel_background.Draw(true, sprite, view, font);
                    Unit_Energy.Draw(true, sprite, view, font);
                    Unit_Health.Draw(true, sprite, view, font);
                    Unit_Hunger.Draw(true, sprite, view, font);
                }
            }
        }

        public static void Update_Info(string update_data)
        {
            Update_Label.Content = update_data;
            var measure = Game1.Game_Font.MeasureString(update_data);
            Update_Label.background = Game1.CreateSquare(
                Game1._graphics.GraphicsDevice,
                (int)measure.X,
                (int)measure.Y,
                (s) => Color.Black
                );
        }

        public void SetSystemName(string system_name)
        {
            this.PopulationCountLabel.Content = system_name;
        }

        public override void Update()
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

                this.MoreInfoLabel.Content = "** TO DO **";

                this.TimeLabel.Content =
                    "Time: " + hour + " : " + minute + " : " + second
                    + " -- Date: " + day + " / " + month + " / " + CalendarSystem.Year;

                if (selected_creature != null)
                {
                    Unit_Health.Content = "Health: " + Math.Round(selected_creature.health);
                    Unit_Energy.Content = "Energy: " + Math.Round(selected_creature.energy);
                    Unit_Hunger.Content = "Hunger: " + Math.Round(selected_creature.hunger);
                }

                PopulationCountLabel.Content = "Population: " + Game1.creatures.Count;
            }
        }

        public static bool display = true;
        public static void ManagerPanel()
        {
            display = !display;
        }
    }
}
