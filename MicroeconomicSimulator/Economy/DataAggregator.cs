using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroeconomicSimulator.Economy
{
    class DataAggregator
    {
        public enum DataType
        {
            MoneySupply, 

            Population, 
            AverageAge, 
            AverageAssets, 
            AverageTraits,

            NumberOfCompanies, 
            AverageCapitalLevel, 
            AverageCapitalization, 

            End
        }
        //For now, data is stored in Lists
        public List<List<double>>[] EnumeratedData;
        public List<string>[] DataLabels;

        public DataAggregator()
        {
            EnumeratedData = new List<List<double>>[(int)DataType.End];
            DataLabels = new List<string>[(int)DataType.End];
            for (int i = 0; i < (int)DataAggregator.DataType.End; i++)
            {
                DataLabels[i] = new List<string>();
            }
            DataLabels[(int)DataType.MoneySupply].Add("Total");
            DataLabels[(int)DataType.MoneySupply].Add("People");
            DataLabels[(int)DataType.MoneySupply].Add("Holding Companies");
            DataLabels[(int)DataType.MoneySupply].Add("Businesses");
            DataLabels[(int)DataType.Population].Add("Total");
            DataLabels[(int)DataType.Population].Add("Total Employed");
            DataLabels[(int)DataType.Population].Add("Worker");
            DataLabels[(int)DataType.Population].Add("Entrepreneur");
            DataLabels[(int)DataType.AverageAge].Add("Everyone");
            DataLabels[(int)DataType.AverageAge].Add("Workers");
            DataLabels[(int)DataType.AverageAge].Add("Entrepreneurs");
            DataLabels[(int)DataType.AverageAge].Add("Unemployed");
            DataLabels[(int)DataType.AverageAssets].Add("People");
            DataLabels[(int)DataType.AverageAssets].Add("Holding Companies");
            DataLabels[(int)DataType.AverageAssets].Add("Producers");
            DataLabels[(int)DataType.AverageAssets].Add("Suppliers");
            DataLabels[(int)DataType.AverageTraits].Add("Skill Level");
            DataLabels[(int)DataType.AverageTraits].Add("Productivity");
            DataLabels[(int)DataType.NumberOfCompanies].Add("Businesses");
            DataLabels[(int)DataType.NumberOfCompanies].Add("Holding Companies");
            DataLabels[(int)DataType.NumberOfCompanies].Add("Producers");
            DataLabels[(int)DataType.NumberOfCompanies].Add("Suppliers");
            DataLabels[(int)DataType.AverageCapitalLevel].Add("Producers");
            DataLabels[(int)DataType.AverageCapitalLevel].Add("Suppliers");
            DataLabels[(int)DataType.AverageCapitalization].Add("Producers");
            DataLabels[(int)DataType.AverageCapitalization].Add("Suppliers");
            for (int i = 0; i < (int)DataAggregator.DataType.End; i++)
            {
                EnumeratedData[i] = (new List<List<double>>());                
                for (int j = 0; j < DataLabels[i].Count; j++)
                {
                    EnumeratedData[i].Add(new List<double>());
                }
            }
        }

        public void UpdateData()
        {
            double PeopleMoneySum = 0.0;
            double DynastyMoneySum = 0.0;
            double CompanyMoneySum = 0.0;

            EnumeratedData[(int)DataType.Population][0].Add(Manager.People.Count);
            int EmployedSum = 0;
            int SelfEmployedSum = 0;
            int NumDynastySum = 0;
            double WorkerAgeSum = 0.0;
            double EntrepreneurAgeSum = 0.0;
            double OtherAgeSum = 0.0;
            double AssetSum = 0.0;
            double DynastyAssetSum = 0.0;
            double SkillSum = 0.0;
            double ProductivitySum = 0.0;
            for (int i = 0; i < Manager.People.Count; i++)
            {
                PeopleMoneySum += Manager.People[i].Money;
                if (Manager.People[i].Employer != null)
                {
                    EmployedSum++;
                    WorkerAgeSum += Manager.People[i].Age;
                }
                else if (Manager.People[i].Dynasty != null)
                {
                    SelfEmployedSum++;
                    DynastyMoneySum += Manager.People[i].Dynasty.Money;
                    NumDynastySum++;
                    EntrepreneurAgeSum += Manager.People[i].Age;
                    DynastyAssetSum += Manager.People[i].Dynasty.SurplusInventory * Math.Pow(1.5, Manager.People[i].Dynasty.SurplusQuality);
                }
                else
                {
                    OtherAgeSum += Manager.People[i].Age;
                }
                AssetSum += Manager.People[i].Assets;
                SkillSum += Manager.People[i].SkillLevel;
                ProductivitySum += Manager.People[i].Productivity;
            }
            EnumeratedData[(int)DataType.Population][1].Add(EmployedSum + SelfEmployedSum);
            EnumeratedData[(int)DataType.Population][2].Add(EmployedSum);
            EnumeratedData[(int)DataType.Population][3].Add(SelfEmployedSum);
            if (EmployedSum > 0)
            {
                EnumeratedData[(int)DataType.AverageAge][1].Add(WorkerAgeSum / EmployedSum);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageAge][1].Add(0);
            }
            if (SelfEmployedSum > 0)
            {
                EnumeratedData[(int)DataType.AverageAge][2].Add(EntrepreneurAgeSum / SelfEmployedSum);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageAge][2].Add(0);
            }
            if (Manager.People.Count - EmployedSum - SelfEmployedSum > 0)
            {
                EnumeratedData[(int)DataType.AverageAge][3].Add(OtherAgeSum / (Manager.People.Count - EmployedSum - SelfEmployedSum));
            }
            else
            {
                EnumeratedData[(int)DataType.AverageAge][3].Add(0);
            }
            if (Manager.People.Count > 0)
            {
                EnumeratedData[(int)DataType.AverageAge][0].Add((WorkerAgeSum + EntrepreneurAgeSum + OtherAgeSum) / Manager.People.Count);
                EnumeratedData[(int)DataType.AverageAssets][0].Add(AssetSum / Manager.People.Count);
                EnumeratedData[(int)DataType.AverageTraits][0].Add(SkillSum / Manager.People.Count);
                EnumeratedData[(int)DataType.AverageTraits][1].Add(ProductivitySum / Manager.People.Count);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageAge][0].Add(0);
                EnumeratedData[(int)DataType.AverageAssets][0].Add(0);
                EnumeratedData[(int)DataType.AverageTraits][0].Add(0);
                EnumeratedData[(int)DataType.AverageTraits][1].Add(0);
            }
            if (NumDynastySum > 0)
            {
                EnumeratedData[(int)DataType.AverageAssets][1].Add(DynastyAssetSum / NumDynastySum);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageAssets][1].Add(0);
            }

            List<Company> Companies = new List<Company>();
            Manager.Businesses.GetAllCompanies(ref Companies);
            EnumeratedData[(int)DataType.NumberOfCompanies][0].Add(Companies.Count);
            EnumeratedData[(int)DataType.NumberOfCompanies][1].Add(NumDynastySum);
            double ProducerCapitalLevelSum = 0.0;
            double ProducerCapitalSum = 0.0;
            double ProducerAssetSum = 0.0;
            int NumProducerSum = 0;
            double SupplierCapitalLevelSum = 0.0;
            double SupplierCapitalSum = 0.0;
            double SupplierAssetSum = 0.0;
            int NumSupplierSum = 0;
            for (int i = 0; i < Companies.Count; i++)
            {
                CompanyMoneySum += Companies[i].Money;
                if (Companies[i].CapitalLevel >= 1.0)
                {
                    ProducerCapitalLevelSum += Companies[i].CapitalLevel;
                    ProducerCapitalSum += Companies[i].Capital;
                    ProducerAssetSum += Companies[i].Inventory * Companies[i].Effectiveness;
                    NumProducerSum++;
                }
                else
                {
                    SupplierCapitalLevelSum += Companies[i].CapitalLevel;
                    SupplierCapitalSum += Companies[i].Capital;
                    SupplierAssetSum += Companies[i].Inventory;
                    NumSupplierSum++;
                }
            }

            EnumeratedData[(int)DataType.NumberOfCompanies][2].Add(NumProducerSum);
            EnumeratedData[(int)DataType.NumberOfCompanies][3].Add(NumSupplierSum);
            if (NumProducerSum > 0)
            {
                EnumeratedData[(int)DataType.AverageCapitalLevel][0].Add(ProducerCapitalLevelSum / NumProducerSum);
                EnumeratedData[(int)DataType.AverageCapitalization][0].Add(ProducerCapitalSum / NumProducerSum);
                EnumeratedData[(int)DataType.AverageAssets][2].Add(ProducerAssetSum / NumProducerSum);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageCapitalLevel][0].Add(0);
                EnumeratedData[(int)DataType.AverageCapitalization][0].Add(0);
                EnumeratedData[(int)DataType.AverageAssets][2].Add(0);
            }
            if (NumSupplierSum > 0)
            {
                EnumeratedData[(int)DataType.AverageCapitalLevel][1].Add(SupplierCapitalLevelSum / NumSupplierSum);
                EnumeratedData[(int)DataType.AverageCapitalization][1].Add(SupplierCapitalSum / NumSupplierSum);
                EnumeratedData[(int)DataType.AverageAssets][3].Add(SupplierAssetSum / NumSupplierSum);
            }
            else
            {
                EnumeratedData[(int)DataType.AverageCapitalLevel][1].Add(0);
                EnumeratedData[(int)DataType.AverageCapitalization][1].Add(0);
                EnumeratedData[(int)DataType.AverageAssets][3].Add(0);
            }

            EnumeratedData[(int)DataType.MoneySupply][0].Add(PeopleMoneySum + DynastyMoneySum + CompanyMoneySum);
            EnumeratedData[(int)DataType.MoneySupply][1].Add(PeopleMoneySum);
            EnumeratedData[(int)DataType.MoneySupply][2].Add(DynastyMoneySum);
            EnumeratedData[(int)DataType.MoneySupply][3].Add(CompanyMoneySum);
        }
    }
}
