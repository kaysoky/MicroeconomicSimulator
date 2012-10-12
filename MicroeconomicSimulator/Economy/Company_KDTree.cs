using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroeconomicSimulator.Economy
{
    class Company_KDTree
    {
        protected int SplitDimensionIndex;
        bool isLeafNode = false;
        protected double SplitPosition;
        protected Company_KDTree LessEqualNode;
        protected Company_KDTree GreaterNode;
        protected Company[] Constituents;

        /// <summary>
        /// Organizes Companies by X Y Z coordinates
        /// More efficient to recreate a tree rather than update it
        /// </summary>
        /// <param name="ParentSplitDimensionIndex">Outside of this constructor, inputs should be -1</param>
        /// <param name="UnitList">Should be converted from a List</param>
        public Company_KDTree(int ParentSplitDimensionIndex, Company[] CompanyList)
        {
            SplitDimensionIndex = (ParentSplitDimensionIndex + 1) % 3;
            if (CompanyList.Length > 2)
            {
                SortByDimension(ref CompanyList, SplitDimensionIndex);
                int MedianIndex = CompanyList.Length / 2;
                SplitPosition = CompanyList[MedianIndex].Location[SplitDimensionIndex];
                Company[] LowerList = new Company[MedianIndex + 1];
                Company[] UpperList = new Company[CompanyList.Length - MedianIndex - 1];
                Array.Copy(CompanyList, 0, LowerList, 0, LowerList.Length);
                Array.Copy(CompanyList, MedianIndex + 1, UpperList, 0, UpperList.Length);
                LessEqualNode = new Company_KDTree(SplitDimensionIndex, LowerList);
                GreaterNode = new Company_KDTree(SplitDimensionIndex, UpperList);
            }
            else
            {
                Constituents = CompanyList;
                isLeafNode = true;
            }
        }
        /// <summary>
        /// Reconstitutes the given tree into a new one (balances it)
        /// </summary>
        /// <param name="OldTree"></param>
        public Company_KDTree(Company_KDTree OldTree)
        {
            List<Company> CompanyList = new List<Company>();
            OldTree.GetAllCompanies(ref CompanyList);
            Company[] CompanyArray = CompanyList.ToArray();

            SplitDimensionIndex = 0;
            if (CompanyArray.Length > 2)
            {
                SortByDimension(ref CompanyArray, SplitDimensionIndex);
                int MedianIndex = CompanyArray.Length / 2;
                SplitPosition = CompanyArray[MedianIndex].Location[SplitDimensionIndex];
                Company[] LowerList = new Company[MedianIndex + 1];
                Company[] UpperList = new Company[CompanyArray.Length - MedianIndex - 1];
                Array.Copy(CompanyArray, 0, LowerList, 0, LowerList.Length);
                Array.Copy(CompanyArray, MedianIndex + 1, UpperList, 0, UpperList.Length);
                LessEqualNode = new Company_KDTree(SplitDimensionIndex, LowerList);
                GreaterNode = new Company_KDTree(SplitDimensionIndex, UpperList);
            }
            else
            {
                Constituents = CompanyArray;
                isLeafNode = true;
            }
        }
        public void GetAllCompanies(ref List<Company> AppendList)
        {
            if (isLeafNode)
            {
                for (int i = 0; i < Constituents.Length; i++)
                {
                    if (Constituents[i].Money > 0)
                    {
                        AppendList.Add(Constituents[i]);
                    }
                }
            }
            else
            {
                LessEqualNode.GetAllCompanies(ref AppendList);
                GreaterNode.GetAllCompanies(ref AppendList);
            }
        }

        public void InsertCompany(Company Newbie)
        {
            if (isLeafNode)
            {
                Array.Resize<Company>(ref Constituents, Constituents.Length + 1);
                Constituents[Constituents.Length - 1] = Newbie;
            }
            else
            {
                if (Newbie.Location[SplitDimensionIndex] > SplitPosition)
                {
                    GreaterNode.InsertCompany(Newbie);
                }
                else
                {
                    LessEqualNode.InsertCompany(Newbie);
                }
            }
        }
        public void FindClosestBusinesses(Person Searcher
            , ref List<Company> CurrentClosest, ref double MaxDistance, int NumCompanies)
        {
            if (isLeafNode)
            {
                for (int i = 0; i < Constituents.Length; i++)
                {
                    if (Searcher.DistanceTo(Constituents[i]) <= MaxDistance)
                    {
                        if (CurrentClosest.Count == 0)
                        {
                            CurrentClosest.Add(Constituents[i]);
                        }
                        else
                        {
                            for (int j = CurrentClosest.Count - 1; j >= 0; j--)
                            {
                                if (Constituents[i].DistanceRegister > CurrentClosest[j].DistanceRegister)
                                {
                                    CurrentClosest.Insert(j + 1, Constituents[i]);
                                }
                            }
                        }
                    }
                }
                if (CurrentClosest.Count > NumCompanies)
                {
                    CurrentClosest.RemoveRange(NumCompanies, CurrentClosest.Count - NumCompanies);
                    MaxDistance = CurrentClosest[CurrentClosest.Count - 1].DistanceRegister;
                }
            }
            else
            {
                if (Searcher.Location[SplitDimensionIndex] > SplitPosition)
                {
                    GreaterNode.FindClosestBusinesses(Searcher
                        , ref CurrentClosest, ref MaxDistance, NumCompanies);
                    if (Searcher.Location[SplitDimensionIndex] - MaxDistance < SplitPosition)
                    {
                        LessEqualNode.FindClosestBusinesses(Searcher
                            , ref CurrentClosest, ref MaxDistance, NumCompanies);
                    }
                }
                else
                {
                    LessEqualNode.FindClosestBusinesses(Searcher
                        , ref CurrentClosest, ref MaxDistance, NumCompanies);
                    if (Searcher.Location[SplitDimensionIndex] + MaxDistance > SplitPosition)
                    {
                        GreaterNode.FindClosestBusinesses(Searcher
                            , ref CurrentClosest, ref MaxDistance, NumCompanies);
                    }
                }
            }
        }
        public void FindClosestBusinesses(Company Searcher
            , ref List<Company> CurrentClosest, ref double MaxDistance, int NumCompanies
            , double MinCapitalLevel, double MaxCapitalLevel)
        {
            if (isLeafNode)
            {
                for (int i = 0; i < Constituents.Length; i++)
                {
                    if (Constituents[i].CapitalLevel > MinCapitalLevel 
                        && Constituents[i].CapitalLevel < MaxCapitalLevel
                        && Constituents[i].Inventory > 0.0
                        && Constituents[i].Price > 0.0
                        && Searcher.DistanceTo(Constituents[i]) <= MaxDistance)
                    {
                        if (CurrentClosest.Count == 0)
                        {
                            CurrentClosest.Add(Constituents[i]);
                        }
                        else
                        {
                            for (int j = CurrentClosest.Count - 1; j >= 0; j--)
                            {
                                if (Constituents[i].DistanceRegister > CurrentClosest[j].DistanceRegister)
                                {
                                    CurrentClosest.Insert(j + 1, Constituents[i]);
                                }
                            }
                        }
                    }
                }
                if (CurrentClosest.Count > NumCompanies)
                {
                    CurrentClosest.RemoveRange(NumCompanies, CurrentClosest.Count - NumCompanies);
                }
            }
            else
            {
                if (Searcher.Location[SplitDimensionIndex] > SplitPosition)
                {
                    GreaterNode.FindClosestBusinesses(Searcher
                        , ref CurrentClosest, ref MaxDistance, NumCompanies
                        , MinCapitalLevel, MaxCapitalLevel);
                    if (Searcher.Location[SplitDimensionIndex] - MaxDistance < SplitPosition)
                    {
                        LessEqualNode.FindClosestBusinesses(Searcher
                            , ref CurrentClosest, ref MaxDistance, NumCompanies
                            , MinCapitalLevel, MaxCapitalLevel);
                    }
                }
                else
                {
                    LessEqualNode.FindClosestBusinesses(Searcher
                        , ref CurrentClosest, ref MaxDistance, NumCompanies
                        , MinCapitalLevel, MaxCapitalLevel);
                    if (Searcher.Location[SplitDimensionIndex] + MaxDistance > SplitPosition)
                    {
                        GreaterNode.FindClosestBusinesses(Searcher
                            , ref CurrentClosest, ref MaxDistance, NumCompanies
                            , MinCapitalLevel, MaxCapitalLevel);
                    }
                }
            }
        }

        private static void SortByDimension(ref Company[] Unsorted, int DimensionIndex)
        {
            if (Unsorted.Length > 0)
            {
                List<List<Company>> Sorter = new List<List<Company>>();
                for (int i = 0; i < Unsorted.Length; i++)
                {
                    Sorter.Add(new List<Company>());
                    Sorter[i].Add(Unsorted[i]);
                }
                while (Sorter.Count > 1)
                {
                    for (int i = 0; i < Sorter.Count - 1; i++)
                    {
                        for (int a = 0; a < Sorter[i].Count; a++)
                        {
                            for (int b = 0; b < Sorter[i + 1].Count; b++)
                            {
                                if (Sorter[i + 1][b].Location[DimensionIndex] < Sorter[i][a].Location[DimensionIndex])
                                {
                                    Sorter[i].Insert(a++, Sorter[i + 1][b]);
                                    Sorter[i + 1].RemoveAt(b--);
                                }
                            }
                        }
                        Sorter[i].AddRange(Sorter[i + 1]);
                        Sorter.RemoveAt(i + 1);
                    }
                }
                Unsorted = Sorter[0].ToArray();
            }
        }
    }
}
