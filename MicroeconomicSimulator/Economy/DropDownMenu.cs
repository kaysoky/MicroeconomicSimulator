using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MicroeconomicSimulator.Economy
{
    class DropDownMenu
    {
        Rectangle MenuBar;
        Rectangle DropDownArea;
        Texture2D DropDownText;
        double DropDownTimer = 1.0;

        Graphs.LineGraph DataDisplay;

        public DropDownMenu()
        {
            Vector2 TextSize = MyGame.Kootenay16.MeasureString("Choose Data to View");
            float Width = TextSize.X;
            float Height = 0.0f;
            for (int i = 0; i < (int)DataAggregator.DataType.End; i++)
            {
                TextSize = MyGame.Kootenay16.MeasureString(((DataAggregator.DataType)i).ToString());
                if (TextSize.X + 2 > Width)
                {
                    Width = TextSize.X + 2;
                }
                Height += TextSize.Y + 2;
            }
            MenuBar = new Rectangle((int)(MyGame.GameWindow.Width - Width - 5)
                , 5
                , (int)Width
                , (int)(Height / (int)DataAggregator.DataType.End));
            DropDownArea = new Rectangle(MenuBar.X
                , MenuBar.Y
                , (int)Width
                , (int)Height);
            Height /= (int)DataAggregator.DataType.End;

            RenderTarget2D renderTarget = new RenderTarget2D(MyGame.graphics.GraphicsDevice
                , DropDownArea.Width, DropDownArea.Height, 1, MyGame.graphics.GraphicsDevice.DisplayMode.Format);
            MyGame.graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            MyGame.graphics.GraphicsDevice.Clear(Color.White);

            MyGame.spriteBatch.Begin();
            for (int i = 0; i < (int)DataAggregator.DataType.End; i++)
            {
                MyGame.spriteBatch.DrawString(MyGame.Kootenay16
                    , ((DataAggregator.DataType)i).ToString()
                    , new Vector2(1, Height * i)
                    , Color.Black);
            }
            MyGame.spriteBatch.End();

            MyGame.graphics.GraphicsDevice.SetRenderTarget(0, null);
            MyGame.graphics.GraphicsDevice.Clear(Color.Black);

            DropDownText = renderTarget.GetTexture();
        }

        public void Update(GameTime gameTime, MouseState Input)
        {
            Point Location = new Point(Input.X, Input.Y);
            //Detect mouse on the bar when the menu is closed
            if (DropDownTimer == 1.0)
            {
                if (MenuBar.Contains(Location)
                    && Input.LeftButton == ButtonState.Pressed
                    && MyGame.PreviousMouse.LeftButton == ButtonState.Released)
                {
                    DropDownTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
            //Continue opening the menu
            else if (DropDownTimer > 0.0)
            {
                DropDownTimer -= 10.0 * gameTime.ElapsedGameTime.TotalSeconds;
            }
            //Menu bar completely opened
            else
            {
                if (DropDownArea.Contains(Location))
                {
                    if (Input.LeftButton == ButtonState.Pressed
                        && MyGame.PreviousMouse.LeftButton == ButtonState.Released)
                    {
                        int selectionIndex = (Input.Y - DropDownArea.Y) / MenuBar.Height;
                        if (selectionIndex < (int)DataAggregator.DataType.End)
                        {
                            DataDisplay = new Graphs.LineGraph(" "
                                , ((DataAggregator.DataType)selectionIndex).ToString()
                                , Manager.Statistics.EnumeratedData[selectionIndex]
                                , Manager.Statistics.DataLabels[selectionIndex]);
                        }
                    }
                }
                else
                {
                    if (Input.LeftButton == ButtonState.Pressed
                        && MyGame.PreviousMouse.LeftButton == ButtonState.Released)
                    {
                        DropDownTimer = 1.0;
                    }
                }
            }
        }

        public void Draw()
        {
            if (DataDisplay != null)
            {
                DataDisplay.Draw();
            }

            MyGame.spriteBatch.Begin();
            MyGame.spriteBatch.DrawString(MyGame.Kootenay16
                , "Turn Number: " + Manager.CurrentTurn + " + (" + Manager.Queue + ")"
                , Vector2.One, Color.White);
            //Compressed menu bar only shows a button
            if (DropDownTimer == 1.0)
            {
                MyGame.spriteBatch.Draw(MyGame.BlankBox, MenuBar, Color.White);
                MyGame.spriteBatch.DrawString(MyGame.Kootenay16, "Choose Data to View", new Vector2(MenuBar.X + 1, MenuBar.Y + 1), Color.Black);
            }
            //Menu bar opening in one second
            else if (DropDownTimer > 0.0)
            {
                MyGame.spriteBatch.Draw(DropDownText
                    , new Vector2(MenuBar.X, MenuBar.Y)
                    , new Rectangle(0, 0, DropDownArea.Width, (int)((1.0 - DropDownTimer) * DropDownArea.Height))
                    , Color.White);
            }
            //Menu bar completely opened
            else
            {
                MyGame.spriteBatch.Draw(DropDownText
                   , DropDownArea
                   , Color.White);
                MouseState Input = Mouse.GetState();
                if (DropDownArea.Contains(new Point(Input.X, Input.Y)))
                {
                    MyGame.spriteBatch.Draw(MyGame.BlankBox
                       , new Rectangle(MenuBar.X, DropDownArea.Y + (Input.Y - DropDownArea.Y) / MenuBar.Height * MenuBar.Height
                           , MenuBar.Width, MenuBar.Height)
                       , new Color(Color.Blue, 0.2f));
                }
            }
            MyGame.spriteBatch.End();
        }
    }
}
