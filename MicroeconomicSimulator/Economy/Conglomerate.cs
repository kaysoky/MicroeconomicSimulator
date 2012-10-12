using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroeconomicSimulator.Economy
{
    class Conglomerate
    {
        public List<Company> Subsidaries = new List<Company>();
        public double Money = 0.0;
        public double SurplusInventory = 0.0;
        public double SurplusQuality = 0.0;

        public Conglomerate(double[] Location, double Investment)
        {
            Subsidaries.Add(new Company(Location, Investment));
            Manager.Businesses.InsertCompany(Subsidaries.Last<Company>());
        }

        public void Update()
        {
            for (int i = 0; i < Subsidaries.Count; i++)
            {
                Subsidaries[i].Update();
                //Grab some small portion of leftover money
                if (Subsidaries[i].Employees.Count <= 0)
                {
                    if (Subsidaries[i].Money > 1.0)
                    {
                        Subsidaries[i].Money--;
                        Money++;
                    }
                    else 
                    {
                        Money += Subsidaries[i].Money;
                        Subsidaries[i].Money = 0.0;
                        SurplusQuality = (SurplusQuality * SurplusInventory + Subsidaries[i].CapitalLevel * Subsidaries[i].Inventory)
                            / (SurplusInventory + Subsidaries[i].Inventory);
                        SurplusInventory += Subsidaries[i].Inventory;
                        Subsidaries.RemoveAt(i);
                    }
                }
                else if (Subsidaries[i].Money > 0.0)
                {
                    Money += 0.01 * Subsidaries[i].Money;
                    Subsidaries[i].Money *= 0.99;
                    if (Subsidaries[i].CapitalLevel <= SurplusQuality)
                    {
                        Subsidaries[i].Inventory += SurplusInventory * 0.25;
                        SurplusInventory *= 0.75;
                    }
                }
            }
            //With enough money, acquire or start more Companies
            if (Money > 100 * Subsidaries.Count + 10)
            {
                Subsidaries.Add(new Company(Manager.GetRandomLocation(), Money));
                Manager.Businesses.InsertCompany(Subsidaries.Last<Company>());
                Money = 0.0;
            }
        }
    }
}
