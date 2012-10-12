using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace MicroeconomicSimulator.Economy
{
    class Manager
    {
        public static int CurrentTurn = 0;
        public static int Queue = 100;
        public static bool PauseRequest = false;
        public static bool IsPaused = false;

        public static List<Person> People = new List<Person>();
        double FractionalPeople = 0.0;
        public static double FractionalDynasties = 5.0;
        public static Company_KDTree Businesses;
        public static DataAggregator Statistics;

        public Manager()
        {
            for (int i = 0; i < 100; i++)
            {
                People.Add(new Person(GetRandomLocation()));
            }
            Businesses = new Company_KDTree(-1, new Company[0]);
            Statistics = new DataAggregator();
        }

        public void Update(Object notUsed)
        {
            while (true)
            {
                if (Queue > 0)
                {
                    while (PauseRequest)
                    {
                        IsPaused = true;
                        Thread.Sleep(100);
                    }
                    IsPaused = false;

                    CurrentTurn++;

                    double AverageAge = 0.0;
                    for (int i = 0; i < People.Count; i++)
                    {
                        People[i].Update();
                        AverageAge += People[i].Age;
                    }
                    if (People.Count > 0)
                    {
                        AverageAge /= People.Count;
                        AverageAge = 1.0 + 0.01 * Math.Abs(AverageAge - 30);
                        FractionalPeople += People.Count * (Math.Pow(Math.E, 0.00025 / AverageAge) - 1.0);
                        while (FractionalPeople > 1.0)
                        {
                            FractionalPeople--;
                            People.Add(new Person(GetRandomLocation()));
                        }
                    }
                    Businesses = new Company_KDTree(Businesses);

                    Statistics.UpdateData();

                    Queue--;
                }
                else
                {
                    IsPaused = true;
                    Thread.Sleep(250);
                    //Queue += 10;
                }
            }
        }

        public static double[] GetRandomLocation()
        {
            return new double[] { MyGame.random.NextDouble(), MyGame.random.NextDouble(), MyGame.random.NextDouble() };
        }
    }
}
