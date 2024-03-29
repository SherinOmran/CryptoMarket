﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithmic_trader
{
    class Non_dominated
    {
        static private int dim;
        static private int ind;
        static double[] pprices;

        public Non_dominated(double[] Prices, int IND, int Dim)
        {
            dim = Dim;
            pprices = Prices;
            ind = IND;

        }
        private double[] Prices;
        private int IND;
        private int Dim;

        //public Non_dominated(double[] Prices, int IND, int Dim)
        //{
        //    this.Dim = Dim;
        //    this.Prices = Prices;
        //    this.IND = IND;
        //    pprices = Prices;

        //}


        public List<double> temp = new List<double>();
        public double[,] ndbest = new double[50000, dim];
        public double[,] ndfit = new double[50000, 3];
        double[] N_D = new double[3];
        int[] intermediary;
        Trading_MO trade = new Trading_MO(pprices);
        Stack<int>  ND_stack = new Stack<int>();
        public int count;
        double[,] non_dominated = new double[50000, 3];
        public void calc_nondominated(double[,] positions, int start, int stop)
        {
            int[] score1 = new int[3];
            int[] score2 = new int[3];
            // count = 0;
            ndbest = new double[50000, dim];
            for (int j = 0; j < stop; j++)
            {
                if (ind == 0)
                {
                    positions[j, 0] = Convert.ToInt32(positions[j, 0]);
                    N_D = trade.WMA_trading((int)positions[j, 0]);
                }
                else if (ind == 1)
                {

                    N_D = trade.stoch_RSI_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), Convert.ToInt32(positions[j, 2]), Convert.ToInt32(positions[j, 3]));
                }
                else if (ind == 2)
                {
                    // N_D=trade.SROC_trading((int)positions[j, 0], (int)positions[j, 1], (int)positions[j, 2], (int)positions[j, 3]);
                    N_D = trade.SROC_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), Convert.ToInt32(positions[j, 2]), Convert.ToInt32(positions[j, 3]));
                }
                else
                {
                    N_D = trade.Bollinger_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), positions[j, 2]);
                }
                non_dominated[j, 0] = N_D[0];
                non_dominated[j, 1] = N_D[1];
                non_dominated[j, 2] = N_D[2];
                // myStack.Push(j);
            }
            count = 0;
            for (int r = 0; r < stop; r++)
            {
                int index = r;// myStack.Pop();


                for (int j = start; j < stop; j++)
                {

                    intermediary = new int[4];
                    int sum = 0;
                    for (int w = 0; w < 3; w++)
                    {
                        if (w < 2)
                        {
                            if (non_dominated[index, w] > non_dominated[j, w]) { score1[w] = 1; score2[w] = -1; } //intermediary[w] = 1; }
                            else if (non_dominated[index, w] < non_dominated[j, w]) { score1[w] = -1; score2[w] = 1; }// intermediary[w] = -1; }
                            else score1[w] = score2[w] = 0;

                        }
                        else
                        {
                            if (non_dominated[index, w] < non_dominated[j, w]) { score1[w] = 1; score2[w] = -1; }// intermediary[w] = 1; }
                            else if (non_dominated[index, w] > non_dominated[j, w]) { score1[w] = -1; score2[w] = 1; }// intermediary[w] = -1; }
                            else score1[w] = score2[w] = 0;//else intermediary[w] = -1;
                        }
                        //intermediary[3] = intermediary[3] + intermediary[w];
                        sum = sum + (score1[w] - score2[w]);
                    }
                    //if (intermediary[3] < -2) { break; }

                    if ((score1[0] != 1) && (score1[1] != 1) && (score1[2] != 1) && (index != j))//&& (score1[0] != score2[0]) && (score1[1] != score2[1]) && (score1[2] != score2[2]))
                    {
                        //break;
                        if ((score1[0] == 0) && (score1[1] == 0) && (score1[2] == 0))
                        {
                            if (j == stop - 1)
                            {
                                for (int y = 0; y < dim; y++)
                                {
                                    ndbest[count, y] = positions[index, y];
                                }
                                count++;
                            }
                            goto equal;
                        }
                        else goto dominated;
                        //if ((score1[0] != score2[0]) && (score1[1] != score2[1]) && (score1[2] != score2[2]))

                    }




                    else if (non_dominated[index, 0] <= 0) { break; }
                    else if (j == stop - 1)
                    { //ND_stack.Push(index);
                      // ND_index.Add(index);

                        for (int y = 0; y < dim; y++)
                        {
                            ndbest[count, y] = positions[index, y];
                        }
                        count++;

                    }


                    equal:;

                }

                dominated:;


            }

        }

        public void calc_nondominated2(double[,] positions, int start, int stop)
        {
            int[] score1 = new int[3];
            int[] score2 = new int[3];
            // count = 0;
            ndbest = new double[50000, dim];
            List<int> domin = new List<int>();
            List<int> eq = new List<int>();
            //double[,] fit;
            for (int j = 0; j < stop; j++)
            {
                if (ind == 0)
                {
                    positions[j, 0] = Convert.ToInt32(positions[j, 0]);
                    N_D = trade.WMA_trading((int)positions[j, 0]);
                }
                else if (ind == 1)
                {
                    

                    N_D = trade.stoch_RSI_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), Convert.ToInt32(positions[j, 2]), Convert.ToInt32(positions[j, 3]));
                }
                else if (ind == 2)
                {
                    // N_D=trade.SROC_trading((int)positions[j, 0], (int)positions[j, 1], (int)positions[j, 2], (int)positions[j, 3]);
                    N_D = trade.SROC_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), Convert.ToInt32(positions[j, 2]), Convert.ToInt32(positions[j, 3]));
                }
                else if (ind==3)
                {
                    N_D = trade.Bollinger_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1]), positions[j, 2]);
                }
                else { N_D = trade.WMA_trading(Convert.ToInt32(positions[j, 0]), Convert.ToInt32(positions[j, 1])); }

                non_dominated[j, 0] = N_D[0];
                non_dominated[j, 1] = N_D[1];
                non_dominated[j, 2] = N_D[2];
                // myStack.Push(j);
            }
            count = 0;
           // non_dominated = fit;
            for (int r = 0; r < stop; r++)
            {
                int index = r;// myStack.Pop();

                for (int a = 0; a < domin.Count; a++) { if (index == domin[a]) goto dominated; }
                for (int w = 0; w < eq.Count; w++) { if (index == eq[w]) goto dominated; }
                for (int j = start; j < stop; j++)
                {

                    intermediary = new int[4];
                    int sum1 = 0; int sum2 = 0;
                    for (int w = 0; w < 3; w++)
                    {
                        if (w < 2)
                        {
                            if (non_dominated[index, w] > non_dominated[j, w]) { score1[w] = 1; score2[w] = -1; } //intermediary[w] = 1; }
                            else if (non_dominated[index, w] < non_dominated[j, w]) { score1[w] = -1; score2[w] = 1; }// intermediary[w] = -1; }
                            else score1[w] = score2[w] = 0;

                        }
                        else
                        {
                            if (non_dominated[index, w] < non_dominated[j, w]) { score1[w] = 1; score2[w] = -1; }// intermediary[w] = 1; }
                            else if (non_dominated[index, w] > non_dominated[j, w]) { score1[w] = -1; score2[w] = 1; }// intermediary[w] = -1; }
                            else score1[w] = score2[w] = 0;//else intermediary[w] = -1;
                        }
                        //intermediary[3] = intermediary[3] + intermediary[w];
                        sum1 = sum1 + score1[w];
                        sum2 = sum2 + score2[w];
                    }
                    //if (intermediary[3] < -2) { break; }
                    if ((score1[0] == -1) && (score1[1] == -1) && (score1[2] == -1)) { goto dominated; }
                    else if ((score2[0] == -1) && (score2[1] == -1) && (score2[2] == -1)){ domin.Add(j); }
                    else if (score1[0] == 0)
                    {
                        if ((score1[1] == 0) && (score1[2] == 0) && (index != j)) { eq.Add(j); }
                        else if ((score1[1] == -1) && score1[2] == -1) { goto dominated; }
                        else if ((score2[1] == -1) && (score2[2] == -1)) { domin.Add(j); }
                        else if ((score2[1] == 0) && (score2[2] == -1)) { domin.Add(j); }
                        else if ((score1[1] == 0) && (score1[2] == -1)) { goto dominated; }
                        else if ((score2[1] == -1) && (score2[2] == 0)) { domin.Add(j); }
                        else if ((score1[1] == -1) && (score1[2] == 0)) { goto dominated; }
                    }
                    else if (score1[1]==0)
                    {
                        if ((score1[0] == -1) && score1[2] == -1) { goto dominated; }
                        else if ((score1[0] == 0) && (score1[2] == -1)) { goto dominated; }
                        else if ((score1[0]==-1) && (score1[2]==0))  {goto dominated; }
                        else if ((score1[0] == -1) && (score1[2] == 0)){ goto dominated; }
                        else if ((score2[0] == -1) && (score2[2] == -1)){ domin.Add(j); }
                        else if ((score2[0] == 0) && (score2[2] == -1)) { domin.Add(j); }
                        else if ((score2[0] == -1) && (score2[2] == 0)) { domin.Add(j); }
                    }
                    else if (score1[2] == 0)
                    { if ((score1[0] == -1) && (score1[1] == -1)) goto dominated;
                        else if ((score1[0] == 0) && (score1[1] == -1)) goto dominated;
                        else if ((score1[0] == -1) && (score1[1] == 0)) goto dominated;
                        else if ((score2[0] == 0) && (score2[1] == -1)) domin.Add(j);
                        else if ((score2[0] == -1) && (score2[1] == 0)) domin.Add(j);
                        else if ((score2[0] == -1) && score2[1] == -1) domin.Add(j);
                    }

                  



                    if (non_dominated[index, 0] <= 0) { break; }
                    if (j == stop - 1)
                    { //ND_stack.Push(index);
                      // ND_index.Add(index);

                        for (int y = 0; y < dim; y++)
                        {
                            ndbest[count, y] = positions[index, y];

                        }
                        for(int a = 0; a < 3; a++) { ndfit[count, a] = non_dominated[index, a]; }
                        count++;

                    }


                    equal:;

                }

                dominated:;


            }

        }

    }
}

