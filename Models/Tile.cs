using Engine_lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace _24HourSurvival.Models
{
    public class Tile : GameObject
    {
        public Tile(string ID, string obj_name, Texture2D texture, Vector2 position) : base(ID, obj_name, texture, position)
        {

        }
    }
}
