using _24HourSurvival.Models;
using Engine_lib;
using Engine_lib.UI_Framework;
using Engine_lib.UI_Framework.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace _24HourSurvival.GUI
{
    public class BrainViewer : View
    {
        public static List<List<Button>> Nodes { get; private set; }

        public static void BuildNodeLayout(Creature c)
        {
            Nodes = new List<List<Button>>();
            var structure = SimpleSurvival.survival_sim.nets[c.id].Get_Structure();
            int x = Engine._graphics.GraphicsDevice.DisplayMode.Width - 400;
            int y = 32;
            for (int i = 0; i < structure.Count; i++)
            {
                var _nodes = new List<Button>();
                if (structure[i].Count == 1)
                {
                    var b = new Button(i + "-" + 0, structure[i][0], new Vector2(x + 32, y), 24, 24, Engine._graphics.GraphicsDevice);
                    b.Click += () => { Debug.WriteLine("Click Working!"); };
                    _nodes.Add(b);
                }
                else if (structure[i].Count > 1)
                {
                    for (int j = 0; j < structure[i].Count; j++)
                    {
                        var b = new Button(i + "-" + j, structure[i][j], new Vector2(x, y), 24, 24, Engine._graphics.GraphicsDevice);
                        b.Click += () => { Debug.WriteLine("Click Working!"); };
                        _nodes.Add(b);
                        x += 32;
                    }
                }
                Nodes.Add(_nodes);
                x = Engine._graphics.GraphicsDevice.DisplayMode.Width - 250;
                y += 32;
            }

            return;
        }

        public override void Draw(SpriteBatch sprite, Matrix view)
        {
            if (Nodes != null && Nodes.Count > 0)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    for (int j = 0; j < Nodes[i].Count; j++)
                    {
                        Nodes[i][j].Draw(true, sprite, view, Engine.Game_Font);
                    }
                }
            }

            return;
        }

        public override void Update()
        {
            if (Nodes != null && Nodes.Count > 0)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    for (int j = 0; j < Nodes[i].Count; j++)
                    {
                        Nodes[i][j].Update();
                    }
                }
            }

            return;
        }
    }
}
