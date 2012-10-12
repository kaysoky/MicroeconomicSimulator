using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroeconomicSimulator.Economy
{
    class Company
    {
        public List<Person> Employees = new List<Person>();
        public double[] Location;
        public double Capital = 1.0;
        public double CapitalLevel = 0.0;
        public double ProfitGoal;
        public double Money;
        public double Salary;
        public double Inventory = 1.0;
        public double Price;
        public double Effectiveness = 0.0;
        public double EffectPriceRatio;

        /// <summary>
        /// Stores the distance to another Company for use by other methods
        /// </summary>
        public double DistanceRegister = 0.0;

        public Company(double[] Location, double Money)
        {
            this.Location = Location;
            ProfitGoal = 1.0 + 0.25 * MyGame.random.NextDouble();
            this.Money = Money;
            this.Salary = 1.0 + ProfitGoal + MyGame.random.NextDouble();
            Price = ProfitGoal;
        }

        public void Update()
        {
            //Businesses with a higher capital level need lower level inputs to make their outputs
            if (CapitalLevel > 1.0)
            {
                //Search for a bunch of potential suppliers
                List<Company> Suppliers = new List<Company>();
                DistanceRegister = double.PositiveInfinity;
                Manager.Businesses.FindClosestBusinesses(this
                    , ref Suppliers, ref DistanceRegister, MyGame.random.Next(5, 10)
                    , CapitalLevel - 1.0, CapitalLevel - 0.75);
                double productionMax = Capital;
                double originalMoney = Money;
                for (int i = 0; i < Suppliers.Count && productionMax > 0.0; i++)
                {
                    double units = Money / Suppliers[i].Price;
                    if (units > Suppliers[i].Inventory)
                    {
                        units = Suppliers[i].Inventory;
                    }
                    if (units * (CapitalLevel - Suppliers[i].CapitalLevel + 1.0) > productionMax) 
                    {
                        units = productionMax / (CapitalLevel - Suppliers[i].CapitalLevel + 1.0);
                    }
                    Money -= units * Suppliers[i].Price;
                    Suppliers[i].Inventory -= units;
                    Suppliers[i].Money += units * Suppliers[i].Price;
                    productionMax -= units * (CapitalLevel - Suppliers[i].CapitalLevel + 1.0);
                }
                //Save the number of producable units before continuing
                productionMax = Capital - productionMax;
                //Next update each employee
                for (int i = 0; i < Employees.Count; i++)
                {
                    double Production = Employees[i].Productivity * (1.0 + 2 / Math.PI * Math.Atan(Employees[i].SkillLevel - CapitalLevel));
                    Production /= CapitalLevel > 1.0 ? CapitalLevel : 1.0;
                    if (Money >= Salary * Production)
                    {
                        Money -= Salary * Production;
                        Employees[i].Money += Salary * Production;
                        Employees[i].SkillLevel *= 1.0001;
                        Capital += Production;
                    }
                    else
                    {
                        //When money is insufficient, a worker is laid off
                        Employees[i].Employer = null;
                        Employees.RemoveAt(i);
                        break;
                    }
                }
                if (productionMax > 0)
                {
                    Price = (Price * Inventory + (originalMoney - Money) * ProfitGoal) / (Inventory + productionMax);
                    Inventory += productionMax;
                    Effectiveness = Math.Pow(1.5, CapitalLevel);
                    EffectPriceRatio = Effectiveness / Price;
                }

                Capital *= Math.Pow(Math.E, -0.025 * CapitalLevel);
            }
            else
            {
                //Items of level lower than 1.0 are produced with labor only
                double originalMoney = Money;
                for (int i = 0; i < Employees.Count; i++)
                {
                    double Production = Employees[i].Productivity * (1.0 + 2 / Math.PI * Math.Atan(Employees[i].SkillLevel - CapitalLevel));
                    Production /= CapitalLevel > 1.0 ? CapitalLevel : 1.0;
                    if (Money >= Salary * Production)
                    {
                        Money -= Salary * Production;
                        Employees[i].Money += Salary * Production;
                        Capital += Production;
                    }
                    else
                    {
                        //When money is insufficient, a worker is laid off
                        Employees[i].Employer = null;
                        Employees.RemoveAt(i);
                        break;
                    }
                }
                if (Capital > 0)
                {
                    Price = (Price * Inventory + (originalMoney - Money) * ProfitGoal) / (Inventory + Capital);
                    Inventory += Capital;
                    Effectiveness = 0.0;
                    EffectPriceRatio = 0.0;
                }

                Capital *= Math.Pow(Math.E, -0.05);
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

        public void QueryForEmployment(Person Candidate)
        {
            double Chance = 0.1;
            if (Candidate.SkillLevel > CapitalLevel)
            {
                Chance *= 1.0 + 0.1 * (Candidate.SkillLevel - CapitalLevel);
            }
            else
            {
                Chance -= 0.5 * (CapitalLevel - Candidate.SkillLevel);
            }
            if (Candidate.Productivity > 1)
            {
                Chance += 0.05 * Math.Log(Candidate.Productivity);
            }
            if (MyGame.random.NextDouble() > 1.0 - Chance)
            {
                Candidate.Employer = this;
                Employees.Add(Candidate);
                RedefineProduction();
            }
        }

        /// <summary>
        /// Recalculates values for CapitalLevel and IndustrialSector
        /// </summary>
        public void RedefineProduction()
        {
            double CapitalLevel = 0.0;
            double ProductivitySum = 0.0;
            for (int i = 0; i < Employees.Count; i++)
            {
                CapitalLevel += Employees[i].SkillLevel * Employees[i].Productivity;
                ProductivitySum += Employees[i].Productivity;
            }
            if (ProductivitySum > 0.0)
            {
                CapitalLevel /= ProductivitySum;
                this.CapitalLevel = CapitalLevel;
            }
        }
    }
}
