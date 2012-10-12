using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MicroeconomicSimulator.Economy
{
    class QueueButton
    {
        int QueueAmount;
        Rectangle Location;
        Vector2 TextLocation;

        public QueueButton(int Amount, Vector2 Location)
        {
            QueueAmount = Amount;
            this.Location = new Rectangle((int)Location.X, (int)Location.Y, 100, 25);
            TextLocation = new Vector2(Location.X, Location.Y) + MyGame.Kootenay16.MeasureString("+" + Amount) / 2;
        }

        public void Update(MouseState Input)
        {
            if (Location.Contains(new Point(Input.X, Input.Y)))
            {
                if (Input.LeftButton == ButtonState.Pressed
                    && MyGame.PreviousMouse.LeftButton == ButtonState.Released)
                {
                    Manager.Queue += QueueAmount;
                }
            }
        }

        public void Draw()
        {
            MyGame.spriteBatch.Begin();
            MyGame.spriteBatch.Draw(MyGame.BlankBox, Location, Color.White);
            MyGame.spriteBatch.DrawString(MyGame.Kootenay16, "+" + QueueAmount, new Vector2(Location.X + 1, Location.Y + 1), Color.Black);
            MyGame.spriteBatch.End();
        }
    }
}
