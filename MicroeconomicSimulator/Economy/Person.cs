using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroeconomicSimulator.Economy
{
    class Person
    {
        public double SkillLevel, Productivity, Age, Contentness, Money, Assets;
        public double[] Location;
        public Conglomerate Dynasty;
        public Company Employer;

        public Person(double[] Location)
        {
            this.Location = Location;
            SkillLevel = 1.5 * MyGame.random.NextDouble();
            Productivity = 1.0 + MyGame.random.NextDouble();
            Age = 15.0 + 15.0 * MyGame.random.NextDouble();
            Contentness = 0.0;
            Money = 100.0;
            Assets = 0.0;
        }

        public void Update()
        {
            Age += 0.01;

            //Assets slowly degrade and turn into money
            Assets *= Math.Pow(Math.E, -0.001 * SkillLevel);
            Money += Assets * (Math.Pow(Math.E, 0.0001 * SkillLevel) - 1.0);
            //Search through the closest businesses for the best bargains and opportunities
            List<Company> Shops = new List<Company>();
            double Distance = double.PositiveInfinity;
            Manager.Businesses.FindClosestBusinesses(this, ref Shops, ref Distance, MyGame.random.Next(5, 10));
            if (Dynasty == null)
            {
                if (Employer == null)
                {
                    //Look for a job or try to start one
                    if (Shops.Count > 0)
                    {
                        Shops[MyGame.random.Next(Shops.Count)].QueryForEmployment(this);
                        if (Employer == null)
                        {
                            Manager.FractionalDynasties += 0.05 * SkillLevel;
                            if (Manager.FractionalDynasties > 1.0 && Money > 10)
                            {
                                Manager.FractionalDynasties--;
                                Dynasty = new Conglomerate(Location, Money * 0.95);
                                Money *= 0.05;
                            }
                        }
                    }
                    else
                    {
                        //With nothing nearby, someone must be an entrepreneur
                        Manager.FractionalDynasties += 0.25 * SkillLevel;
                        if (Manager.FractionalDynasties > 1.0 && Money > 10)
                        {
                            Manager.FractionalDynasties--;
                            Dynasty = new Conglomerate(Location, Money * 0.95);
                            Money *= 0.05;
                        }
                    }
                }
                else
                {
                    //Consider quitting the job and starting a new Dynasty
                    if (MyGame.random.NextDouble() > 0.9)
                    {
                        Manager.FractionalDynasties += 0.01 * SkillLevel;
                        if (Manager.FractionalDynasties > 1.0 && Money > 10)
                        {
                            Manager.FractionalDynasties--;
                            Dynasty = new Conglomerate(Location, Money * 0.95);
                            Money *= 0.05;
                            Employer.Employees.Remove(this);
                            Employer = null;
                        }
                    }
                }
            }
            else
            {
                Dynasty.Update();
                if (Dynasty.Subsidaries.Count > 0)
                {
                    double SelfSalary = SkillLevel + Productivity;
                    if (Dynasty.Money > SelfSalary)
                    {
                        Dynasty.Money -= SelfSalary;
                        Money += SelfSalary;
                    }
                    else
                    {
                        Dynasty.Money *= 0.5;
                        Money += Dynasty.Money;
                    }
                }
                else
                {
                    Money += Dynasty.Money;
                    Contentness += Dynasty.SurplusInventory * Math.Pow(1.5, Dynasty.SurplusQuality);
                    Dynasty = null;
                }
            }

            if (Assets > Math.E)
            {
                Contentness -= Productivity / Math.Log(Assets);
            }
            else
            {
                Contentness -= Productivity;
            }
            //Iterate through the list of nearby Companies and buy goods with available money
            for (int i = 0; i < Shops.Count && Money > 0.0; i++)
            {
                if (Shops[i].Inventory > 0.0 && Shops[i].Effectiveness > 0.0)
                {
                    double units = Money / Shops[i].Price;
                    if (units > Shops[i].Inventory)
                    {
                        units = Shops[i].Inventory;
                    }
                    Money -= units * Shops[i].Price;
                    Shops[i].Inventory -= units;
                    Shops[i].Money += units * Shops[i].Price;
                    Assets += units * Shops[i].Effectiveness / 10.0;
                    Contentness += units * Shops[i].Effectiveness;
                }
            }
            //Evaluate the effects of Contentness on Productivity
            if (Contentness > 1)
            {
                Productivity += 0.001;
            }
            else if (Contentness < 0)
            {
                Productivity *= Math.Pow(Math.E, -0.001);
                Contentness /= 2;
            }
        }

        public double DistanceTo(Company Other)
        {
            double Distance = 0.0;
            for (int i = 0; i < Location.Length; i++)
            {
                Distance += Math.Pow(Location[i] - Other.Location[i], 2);
            }
            Other.DistanceRegister = Math.Sqrt(Distance);
            return Other.DistanceRegister;
        }
    }
}
