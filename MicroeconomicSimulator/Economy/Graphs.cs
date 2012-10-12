using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroeconomicSimulator.Economy
{
    class Graphs
    {
        /// <summary>
        /// Returns the standard deviation
        /// </summary>
        /// <param name="values">Will be destroyed in the process</param>
        public static void CalculateStandardDeviation(double[] values, out double mean, out double stdDev)
        {
            if (values.Length > 1)
            {
                mean = 0.0;
                for (int i = 0; i < values.Length; i++)
                {
                    mean += values[i];
                }
                mean /= values.Length;
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = Math.Pow(values[i] - mean, 2.0);
                }
                stdDev = 0.0;
                for (int i = 0; i < values.Length; i++)
                {
                    stdDev += values[i];
                }
                stdDev = Math.Sqrt(stdDev / (values.Length - 1.0));
            }
            else if (values.Length == 1)
            {
                mean = values[0];
                stdDev = 0.0;
            }
            else
            {
                mean = 0.0;
                stdDev = 0.0;
            }
        }

        public static Color GetRandomColor()
        {
            return new Color((float)MyGame.random.NextDouble()
                , (float)MyGame.random.NextDouble()
                , (float)MyGame.random.NextDouble());
        }

        /// <summary>
        /// Data representation with labels in line format
        /// </summary>
        public class LineGraph
        {
            ResolveTexture2D resolveTarget;
            bool isGraphSaved = false;

            List<GraphTick> Axes;
            List<Curve> Lines;
            Matrix Projection;
            Matrix Camera;
            Matrix World;
            string XAxisTitle;
            string YAxisTitle;

            List<List<double>> dataValues = new List<List<double>>();
            List<string> lineLabels = new List<string>();

            /// <summary>
            /// A collection of objects that represents inputted data in the form of a Line Graph
            /// </summary>
            /// <param name="values">Values to be inputted</param>
            public LineGraph(string XAxisTitle, string YAxisTitle, List<List<double>> values, List<string> lineLabels)
            {
                Manager.PauseRequest = true;
                while (!Manager.IsPaused)
                {
                    System.Threading.Thread.Sleep(10);
                }

                resolveTarget = new ResolveTexture2D(MyGame.graphics.GraphicsDevice
                , MyGame.GameWindow.Width
                , MyGame.GameWindow.Height
                , 1
                , MyGame.graphics.GraphicsDevice.DisplayMode.Format);
                this.XAxisTitle = XAxisTitle;
                this.YAxisTitle = YAxisTitle;
                this.lineLabels = lineLabels;
                Lines = Curve.CreateCurves(values, lineLabels);
                RefreshGraphAxes();
                RefreshGraphImage();

                Manager.PauseRequest = false;
            }

            /// <summary>
            /// Refreshes 'Axes' by finding the minimum and maximum values for the data
            /// </summary>
            private void RefreshGraphAxes()
            {
                int NumEntries = 1;
                float DataMin = float.PositiveInfinity;
                float DataMax = float.NegativeInfinity;
                for (int i = 0; i < Lines.Count; i++)
                {
                    if (Lines[i].DataCount > NumEntries)
                    {
                        NumEntries = Lines[i].DataCount;
                    }
                    if (Lines[i].MinimumY < DataMin)
                    {
                        DataMin = Lines[i].MinimumY;
                    }
                    if (Lines[i].MaximumY > DataMax)
                    {
                        DataMax = Lines[i].MaximumY;
                    }
                }
                float DataRange = DataMax - DataMin;
                Projection = Matrix.CreateOrthographic(1.0f, 1.0f, 0.0f, 10.0f);
                Camera = Matrix.CreateLookAt(new Vector3(0.5f, 0.5f, 1.0f), new Vector3(0.5f, 0.5f, 0.0f), Vector3.UnitY);
                World = Matrix.CreateScale(0.9f / (NumEntries - 1), 0.9f / DataRange, 1.0f)
                    * Matrix.CreateTranslation(0.1f, 0.1f - 0.9f * DataMin / DataRange, 0.0f);
                //Add the main axes
                Axes = new List<GraphTick>();
                Axes.Add(GraphTick.CreateHorizontalAxis(XAxisTitle));
                Axes.Add(GraphTick.CreateVerticalAxis(YAxisTitle));
                //Add the horizontal tick marks
                int numLabels = 2 + (int)(0.45 * MyGame.GameWindow.Width / MyGame.Kootenay16.MeasureString("" + NumEntries).X);
                if (numLabels > NumEntries)
                {
                    numLabels = NumEntries;
                }
                List<string> labels = new List<string>();
                for (int i = 0; i < numLabels; i++)
                {
                    labels.Add("" + Math.Round(i * (double)NumEntries / (numLabels - 1.0), numLabels == NumEntries ? 1 : 0));
                }
                Axes.AddRange(GraphTick.CreateHorizontalAxisLabels(labels));
                //Add the vertical tick marks
                int decimalPlaces = 5 - (int)Math.Log10(DataMax);
                if (decimalPlaces < 0)
                {
                    decimalPlaces = 0;
                }
                else if (decimalPlaces > 5)
                {
                    decimalPlaces = 5;
                }
                numLabels = 2 + (int)(0.1125 * MyGame.GameWindow.Height / MyGame.Kootenay16.MeasureString("" + Math.Round(DataMax, decimalPlaces)).Y);
                labels = new List<string>();
                for (int i = 0; i < numLabels; i++)
                {
                    labels.Add("" + Math.Round(DataMin + i * DataRange / (numLabels - 1), decimalPlaces));
                }
                Axes.AddRange(GraphTick.CreateVerticalAxisLabels(labels));
            }

            /// <summary>
            /// May cause the screen to flash if called during the Draw loop (which it should not be)
            /// </summary>
            void RefreshGraphImage()
            {
                MyGame.graphics.GraphicsDevice.Clear(Color.TransparentBlack);

                MyGame.spriteBatch.Begin();
                foreach (GraphTick tick in Axes)
                {
                    tick.Draw();
                }
                MyGame.spriteBatch.End();

                MyGame.OrdinaryEffect.CurrentTechnique = MyGame.OrdinaryEffect.Techniques["Ordinary"];
                MyGame.OrdinaryEffect.Parameters["ViewXProjection"].SetValue(Camera * Projection);
                MyGame.OrdinaryEffect.Parameters["World"].SetValue(World);
                MyGame.OrdinaryEffect.Begin();
                MyGame.OrdinaryEffect.CurrentTechnique.Passes.First<EffectPass>().Begin();
                for (int i = 0; i < Lines.Count; i++)
                {
                    Lines[i].Draw();
                }
                MyGame.OrdinaryEffect.CurrentTechnique.Passes.First<EffectPass>().End();
                MyGame.OrdinaryEffect.End();

                Curve.DrawLabels(Lines);

                MyGame.graphics.GraphicsDevice.ResolveBackBuffer(resolveTarget);
                isGraphSaved = true;
            }

            public void Draw()
            {
                if (isGraphSaved)
                {
                    MyGame.spriteBatch.Begin();
                    MyGame.spriteBatch.Draw(resolveTarget
                        , new Rectangle(0, 25, MyGame.GameWindow.Width, MyGame.GameWindow.Height - 50)
                        , Color.White);
                    //Debug code to print out were the lines are actually being drawn
                    //MyGame.spriteBatch.DrawString(MyGame.Kootenay16
                    //    , "Data Point: " 
                        //+ Vector3.Transform(Lines[0].Data[Lines[0].Data.Length / 2].Position, World * Camera * Projection).ToString()
                    //    , new Vector2(MyGame.GameWindow.Width / 2, MyGame.GameWindow.Height / 2)
                    //    , Color.White);
                    MyGame.spriteBatch.End();
                }
            }
        }

        /// <summary>
        /// A single Rectangular region with a label
        /// </summary>
        class GraphTick
        {
            Rectangle Area;
            string Label;
            Vector2 LabelPosition;
            float LabelRotation;
            Vector2 LabelOrigin;
            float LabelScale;
            Color Color = Color.White;

            public void Draw()
            {
                MyGame.spriteBatch.Draw(MyGame.BlankBox, Area, Color);
                MyGame.spriteBatch.DrawString(MyGame.Kootenay16
                    , Label, LabelPosition, Color
                    , LabelRotation, LabelOrigin, LabelScale, SpriteEffects.None, 0.0f);
            }

            /// <summary>
            /// Creates a Rectangle across the bottom of the screen with a label
            /// </summary>
            public static GraphTick CreateHorizontalAxis(string axisTitle)
            {
                GraphTick tick = new GraphTick();

                tick.Area = new Rectangle(0, (int)(MyGame.GameWindow.Height * 0.9) - 1
                    , MyGame.GameWindow.Width, 3);
                Vector2 titleSize = MyGame.Kootenay16.MeasureString(axisTitle);
                tick.Label = axisTitle;
                tick.LabelPosition = new Vector2(MyGame.GameWindow.Width / 2, MyGame.GameWindow.Height * 0.95f);
                tick.LabelRotation = 0.0f;
                tick.LabelOrigin = titleSize / 2;
                tick.LabelScale = 1.0f;

                return tick;
            }
            /// <summary>
            /// Creates a Rectangle running through the left of the screen with a label
            /// </summary>
            public static GraphTick CreateVerticalAxis(string axisTitle)
            {
                GraphTick tick = new GraphTick();

                tick.Area = new Rectangle((int)(MyGame.GameWindow.Width * 0.1) - 1, 0
                    , 3, MyGame.GameWindow.Height);
                Vector2 titleSize = MyGame.Kootenay16.MeasureString(axisTitle);
                tick.Label = axisTitle;
                tick.LabelPosition = new Vector2(MyGame.GameWindow.Width * 0.05f, MyGame.GameWindow.Height / 2);
                tick.LabelRotation = -MathHelper.PiOver2;
                tick.LabelOrigin = titleSize / 2;
                tick.LabelScale = 1.0f;

                return tick;
            }
            /// <summary>
            /// Adds tick marks across the bottom axis with corresponding labels
            /// </summary>
            public static List<GraphTick> CreateHorizontalAxisLabels(List<string> labels)
            {
                List<GraphTick> ticks = new List<GraphTick>();
                for (int i = 0; i < labels.Count; i++)
                {
                    GraphTick tick = new GraphTick();

                    tick.Area = new Rectangle((int)(MyGame.GameWindow.Width * (0.1 + i * 0.9 / (labels.Count - 1))) - 1
                        , (int)(MyGame.GameWindow.Height * 0.9) - 6
                        , 2, 5);
                    Vector2 labelSize = MyGame.Kootenay16.MeasureString(labels[i]);
                    float labelDiagonal = (float)Math.Sqrt(Math.Pow(labelSize.X, 2.0) + Math.Pow(labelSize.Y, 2.0));
                    tick.Label = labels[i];
                    tick.LabelPosition = new Vector2(tick.Area.X - labelDiagonal / 2, tick.Area.Y + labelDiagonal);
                    tick.LabelRotation = -MathHelper.PiOver4;
                    tick.LabelOrigin = Vector2.One * labelDiagonal / 2;
                    tick.LabelScale = 1.0f;

                    ticks.Add(tick);
                }
                return ticks;
            }
            /// <summary>
            /// Adds tick marks across the left axis with corresponding labels
            /// </summary>
            public static List<GraphTick> CreateVerticalAxisLabels(List<string> labels)
            {
                List<GraphTick> ticks = new List<GraphTick>();
                for (int i = 0; i < labels.Count; i++)
                {
                    GraphTick tick = new GraphTick();

                    tick.Area = new Rectangle((int)(MyGame.GameWindow.Width * 0.1) + 1
                        , (int)(MyGame.GameWindow.Height * (0.9 - i * 0.9 / (labels.Count - 1))) - 1
                        , 5, 2);
                    Vector2 labelSize = MyGame.Kootenay16.MeasureString(labels[i]);
                    tick.Label = labels[i];
                    tick.LabelPosition = new Vector2(tick.Area.X - labelSize.X - 2, tick.Area.Y);
                    tick.LabelRotation = 0.0f;
                    tick.LabelOrigin = Vector2.Zero;
                    tick.LabelScale = 1.0f;

                    ticks.Add(tick);
                }
                return ticks;
            }
            /// <summary>
            /// Creates a bunch of colored Rectangles bordering labels that correlates to a line on the graph 
            /// on the left quarter of the screen
            /// </summary>
            public static List<GraphTick> CreateLegend(List<Color> colors, List<string> labels)
            {
                List<GraphTick> boxes = new List<GraphTick>();
                for (int i = 0; i < colors.Count && i < labels.Count; i++)
                {
                    GraphTick box = new GraphTick();

                    Vector2 labelSize = MyGame.Kootenay16.MeasureString(labels[i]);
                    box.Area = new Rectangle((int)(MyGame.GameWindow.Width * 0.25 - labelSize.X / 2 - 1)
                        , (int)(MyGame.GameWindow.Height * 0.45 - (colors.Count / 2 + 0.5 - i) * labelSize.Y - 1)
                        , 1, (int)labelSize.Y + 2);
                    box.Color = colors[i];
                    box.Label = labels[i];
                    box.LabelPosition = new Vector2(box.Area.X + 1, box.Area.Y + 1);
                    box.LabelRotation = 0.0f;
                    box.LabelOrigin = Vector2.Zero;
                    box.LabelScale = 1.0f;

                    boxes.Add(box);
                }
                return boxes;
            }
        }

        class Curve
        {
            public VertexPositionColor[] Data;
            GraphTick Label;
            public bool ShouldBeDrawn = true;
            public int DataCount;
            public float MinimumY = float.PositiveInfinity;
            public float MaximumY = float.NegativeInfinity;

            public void Draw()
            {
                if (ShouldBeDrawn)
                {
                    MyGame.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, Data, 0, Data.Length - 1);
                }
            }
            public static void DrawLabels(List<Curve> Lines)
            {
                MyGame.spriteBatch.Begin();
                for (int i = 0; i < Lines.Count; i++)
                {
                    Lines[i].Label.Draw();
                }
                MyGame.spriteBatch.End();
            }

            public static List<Curve> CreateCurves(List<List<double>> Values, List<string> Labels)
            {
                List<Curve> curves = new List<Curve>();
                List<Color> colors = new List<Color>();
                for (int i = 0; i < Values.Count; i++)
                {
                    Curve curve = new Curve();
                    curve.Data = new VertexPositionColor[Values[i].Count];
                    curve.DataCount = Values[i].Count;
                    colors.Add(GetRandomColor());
                    for (int j = 0; j < Values[i].Count; j++)
                    {
                        curve.Data[j] = new VertexPositionColor(
                            new Vector3(j, (float)Values[i][j], 0.0f), colors.Last<Color>());
                        if (Values[i][j] < curve.MinimumY)
                        {
                            curve.MinimumY = (float)Values[i][j];
                        }
                        if (Values[i][j] > curve.MaximumY)
                        {
                            curve.MaximumY = (float)Values[i][j];
                        }
                    }
                    curves.Add(curve);
                }
                List<GraphTick> legend = GraphTick.CreateLegend(colors, Labels);
                for (int i = 0; i < curves.Count && i < legend.Count; i++)
                {
                    curves[i].Label = legend[i];
                }
                return curves;
            }
        }
    }
}
