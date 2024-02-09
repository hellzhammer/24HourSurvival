using Engine_lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace _24HourSurvival.Models
{
    public class Food_Item : GameObject
    {
        public bool consumed = false;
        public Food_Item(string ID, string obj_name, Texture2D texture, Vector2 position) : base(ID, obj_name, texture, position)
        {
            this.OnMouseOver += () => {
                Debug.WriteLine("Consumed: " + consumed);
            };
        }
    }
}
