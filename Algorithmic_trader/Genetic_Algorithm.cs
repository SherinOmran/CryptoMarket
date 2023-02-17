using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithmic_trader
{
    class Genetic_Algorithm
    {
        public static int popsize = 351;

        double crossover_rate = 0.8;
        static int gene_num;
        double mut_rate = 0.5;
        int ind;
       static double[] prices;
        double[] z = new double[] { 10000, 50, 1 };
        int neigh_size = 5;

        /////////////////

        public double[,] weights;


        public Genetic_Algorithm(int gene_number, int IND, double[] Prices)
        {
            gene_num = gene_number;
            ind = IND;
            prices = Prices;
            pop = new double[popsize, gene_num];

        }

        double min = 3.0, max = 120;// 
        double OB_min = 60.0, OB_max = 90.0;    //overbought
        double OS_min = 10.0, OS_max = 40.0;   //oversold
        double roc_min = 0;
        double roc_max = 10;
        int[,] E_dist;

        public double[,] pop;// 

        public double[,] population()
        {
            Decomposition dec = new Decomposition(popsize, 3);
            Random r = new Random(Guid.NewGuid().GetHashCode());
             weights = dec.sys_weight_gen();// dec.rand_weight_gen();
           
            E_dist = dec.E_dist(weights, neigh_size);


            for (int i = 0; i < popsize; i++)
            {


                if (ind == 0)  // WMA with a single parameters n 
                {
                    pop[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                }

                else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                {
                    pop[i, 0] = (max - min) * r.NextDouble() + min;
                    pop[i, 1] = (max - min) * r.NextDouble() + min;
                    pop[i, 3] = (OB_max - OB_min) * r.NextDouble() + OB_min;
                    pop[i, 2] = (OS_max - OS_min) * r.NextDouble() + OS_min;


                }
                else if (ind == 2)
                {
                    

                    pop[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                   
                    pop[i, 1] = (max - min) * r.NextDouble() + min; ;// (max - pop[i, 0]) * r.NextDouble() + pop[i, 0];
                    pop[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                    pop[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min);


                }

                else    // bollinger bands ( 2 lookback periods and multiplier)
                {
                    pop[i, 0] = (max - min) * r.NextDouble() + min;
                    pop[i, 1] = (max - min) * r.NextDouble() + min;
                    pop[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution


                }


            }

            return pop;
           

        }








        ///////////////////////////

        public double[,] boundary_check(double[,] pop)

        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < popsize; i++)
            {
                if (ind == 0)
                {
                    if ((pop[i, 0] < min) || (pop[i, 0] > max)) { pop[i, 0] = Math.Floor((max - min) * r.NextDouble() + min); }

                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (pop[i, j] < min) pop[i, j] = min;
                        else if (pop[i, j] > max) pop[i, j] = max;

                    }


                    if (ind == 1)
                    {
                        if (pop[i, 3] < OB_min) pop[i, 3] = OB_min;
                        else if (pop[i, 3] > OB_max) pop[i, 3] = OB_max;
                        if (pop[i, 2] < OS_min) pop[i, 2] = OS_min;
                        else if (pop[i, 2] > OS_max) pop[i, 2] = OS_max;
                    }


                    

                    else if (ind == 2)
                    {
                        
                  
                        if (pop[i, 2] < roc_min) pop[i, 2] = roc_min;
                        else if (pop[i, 2] > roc_max) pop[i, 2] = roc_max;
                        if ((pop[i, 3] > 0) || (pop[i, 3] > pop[i, 2])) pop[i, 3] = 0;
                        else if ((pop[i, 3]) < -roc_max) pop[i, 3] = -1 * roc_max;

                    }



                    else if (ind == 3)
                    {
                        if (pop[i, 2] < 1) pop[i, 2] = 1;
                        else if (pop[i, 2] > 3) pop[i, 2] = 3;
                        
                    }

                }

            }

            return pop;

        }



        
        public double[] fitness(double[,] pop)
        {

            Decomposition dd = new Decomposition(popsize, 3);
            double[] fitness = new double[popsize];
            Trading_MO tr = new Trading_MO(prices);
            int[,] E_dist = dd.E_dist(weights, neigh_size);
            double[] wt = new double[3]; // weight for each subproblem
            double[] a;
            for (int i = 0; i < popsize; i++)
            {
                for (int s = 0; s < 3; s++)   ///// s is the number of objectives
                {
                    wt[s] = weights[i, s];
                }


                if (ind == 0)
                {
                   
                    a = tr.WMA_trading(Convert.ToInt32(pop[i, 0]));
                    update_ref(z, a);
                    fitness[i] = dd.W_old_Tch(z, a, wt); //

                  

                }
                else if (ind == 1)
                {
                   
                    a = tr.SROC_trading(Convert.ToInt32(pop[i, 0]), Convert.ToInt32(pop[i, 1]), Convert.ToInt32(pop[i, 2]), Convert.ToInt32(pop[i, 3]));
                    update_ref(z, a);
                    fitness[i] = dd.W_old_Tch(z, a, wt);
                   

                }
                else if (ind == 2)   /// sroc
                {
                   
                    a = tr.SROC_trading(Convert.ToInt32(pop[i, 0]), Convert.ToInt32(pop[i, 1]), Convert.ToInt32(pop[i, 2]), Convert.ToInt32(pop[i, 3]));
                    update_ref(z, a);
                    fitness[i] = dd.W_old_Tch(z, a, wt);

                }

                else
                {
                    pop[i, 0] = Math.Floor(pop[i, 0]);
                    pop[i, 1] = Math.Floor(pop[i, 1]);
                    a = tr.Bollinger_trading(Convert.ToInt32(pop[i, 0]), Convert.ToInt32(pop[i, 1]), pop[i, 2]);
                    update_ref(z, a);
                    fitness[i] = dd.W_old_Tch(z, a, wt);
                }


            }
            return fitness;
        }

       


       


        public double[,] crossover2(double[,] selected_pop)
        {

            double[,] child = new double[popsize, gene_num];
            Random x = new Random(); // x--> a random number for selecting the crossover place
            Random z = new Random();

            if (gene_num == 2)
            {
                for (int i = 0; i < popsize-1; i += 2)
                {
                    double a = z.NextDouble();
                    if (a <= crossover_rate)
                    {

                        child[i, 0] = selected_pop[i, 0];
                        child[i + 1, 0] = selected_pop[i + 1, 0];
                        child[i, 1] = selected_pop[i + 1, 1];
                        child[i + 1, 1] = selected_pop[i, 1];
                    }
                    else for (int j = 0; j < 2; j++)
                        {
                            child[i, j] = selected_pop[i, j];
                            child[i + 1, j] = selected_pop[i + 1, j];
                        }

                }
                return child;
            }
            else if (gene_num > 2)
            {
                for (int i = 0; i < popsize-1; i += 2)
                {
                    double a = z.NextDouble();
                    if (a <= crossover_rate)
                    {
                        int y = x.Next(1, gene_num - 1);
                        for (int j = 0; j < y; j++)    //part one
                        {
                            child[i, j] = selected_pop[i, j];
                            child[i + 1, j] = selected_pop[i + 1, j];
                        }
                        for (int j = y; j < gene_num; j++)   // part two
                        {
                            child[i, j] = selected_pop[i + 1, j];
                            child[i + 1, j] = selected_pop[i, j];
                        }
                    }
                    else for (int j = 0; j < gene_num; j++)
                        {
                            child[i, j] = selected_pop[i, j];
                            child[i + 1, j] = selected_pop[i + 1, j];
                        }

                }
            }

                   for(int j = 0; j < gene_num; j++)
                        {
                              child[popsize-1, j] = selected_pop[popsize-1, j];
                
                        }

            return child;


        }


        public void update_ref(double[] z, double[] a)
        {
            if (a[0] > z[0]) z[0] = a[0];
            if (a[1] > z[1]) z[1] = a[1];

        }


        ////////////////////////////////////////////////////////////////////////



        public double[,] Mutate(double[,] pop)
        {
            Random m_random = new Random(Guid.NewGuid().GetHashCode()); // m_random --> the probability for gene mutation

            Random r = new Random(Guid.NewGuid().GetHashCode());  // n --> a random number that replaces the mutated gene 
            int d;
            for (int i = 0; i < popsize; i++) // i--> chromosome

                for (int pos = 0; pos < gene_num; pos++)  // pos--> gene
                {
                    double v = m_random.NextDouble();
                    if (v <= mut_rate)

                    {
                        if (ind == 0)  // 2 parameters n and multiplier
                        {
                            pop[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                        }

                        else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                        {
                            d = r.Next(0, 3);

                            if (d == 0) pop[i, 0] = (max - min) * r.NextDouble() + min;
                            else if (d == 1) pop[i, 1] = (max - min) * r.NextDouble() + min;
                            else if (d == 2) pop[i, 2] = (OB_max - OB_min) * r.NextDouble() + OB_min;
                            else pop[i, 3] = (OS_max - OS_min) * r.NextDouble() + OS_min;



                        }
                        else if (ind == 2)
                        {
                            d = r.Next(0, 3);
                            if (d == 0) pop[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                            else if (d == 1) pop[i, 1] = (max - min) * r.NextDouble() + min; /// 
                            else if (d == 2) pop[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                            else pop[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min); ;      //OS


                            


                        }

                        else    // bollinger bands ( 2 lookback periods and multiplier)
                        {
                            d = r.Next(0, 2);
                            if (d == 0) pop[i, 0] = (max - min) * r.NextDouble() + min;
                            else if (d == 1) pop[i, 1] = (max - min) * r.NextDouble() + min;
                            else pop[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution


                        }



                    }




                }
            pop = boundary_check(pop);
            return pop;
        }


        ///////////////
        



            
        
        public double[,] new_roulettewheel(double[] fitness, double[,]pop)    ///// to get the minimum value as compared to the neighborhood size
        {
            double sum = 0;
            int s;
            Random p = new Random();
            double rand;
            double[] probability = new double[neigh_size];
            double[] fit_ind_prop=new double[neigh_size];
            double[,] selected_pop = new double[popsize, pop.GetLength(1)];
            double fit_sum;
            for (int i=0;i<popsize; i++)
            {
                rand=p.NextDouble();
                fit_sum = 0;
                sum = 0;
                for (int k = 0; k < neigh_size; k++)
                {
                   s = E_dist[i, k];
                    sum = sum + fitness[s];
                }
                for (int k = 0; k <neigh_size; k++)
                {
                    s = E_dist[i, k];
                    probability[k] = fitness[s] / sum;
                    fit_ind_prop[k] =fit_sum+ (1 - probability[k]) / (neigh_size - 1);
                    fit_sum = fit_ind_prop[k];

                    if (rand<fit_ind_prop[k]) {
                        for (int j =0; j < pop.GetLength(1); j++) { selected_pop[i, j] = pop[k, j]; }
                        break;
                            }

                }
                

                
                }
            return selected_pop;
            }
        

          
        //////////

        public string decTobin(int decVal)
        {
            
            int val;
            string a = "";
           
            
            while (decVal >= 1)
            {
                val = decVal / 2;
                a += (decVal % 2).ToString();
                decVal = val;
            }
            string binValue = "";
            for (int i = a.Length - 1; i >= 0; i--)
            {
                binValue = binValue + a[i];
            }
            
            return binValue;
        }




        /////////////////////////

        public double[,] Crossover_twopoint(double[,] selected_pop)
        {
            double[,] child = new double[popsize, gene_num];
            Random x = new Random(); // x--> a random number for selecting the crossover place
                                     
           

            
            for (int i = 0; i < popsize-1; i += 2)
                {
                string S1 = ""; string S2 = "";
                string S3 = "";
                double a = x.NextDouble();
                string N1 = "";
                string N2 = "";
                string Q1 ;
                string Q2 ;
             

                if (a <= crossover_rate)
                {
                    for (int j = 0; j < gene_num; j++)
                    { 
                        Q1 = "";
                        Q2 = "";

                         Q1 =decTobin((int)selected_pop[i, j]);
                         Q2 = decTobin((int)selected_pop[i + 1, j]);
                        if (Q2.Length < 8)
                        {
                            S3 = "";
                            for (int c = 0; c < 8 - Q2.Length; c = c + 1)
                            {
                                S3 = S3 + "0";

                            }
                            Q2 = S3 + Q2;
                            


                        }

                        if (Q1.Length < 8)
                        {
                            S3 = "";
                            for (int c = 0; c < 8 - Q1.Length; c = c + 1)
                            {
                                S3 = S3 + "0";
                            }
                            Q1 = S3 + Q1;
                            
                        }
                        N2 = N2 + Q2;
                        N1 = N1 + Q1;
                    }
                    int y = x.Next(1, gene_num*8/2);
                    int z = x.Next(y+1, (gene_num*8)-1);
                    for (int j = 0; j < y; j++)    //part one
                    {
                        S1 = S1 + N1[j];
                        S2 = S2 + N2[j];
                        
                    }
                    for (int j = y; j < z; j++)   // part two
                    {
                        S1 = S1 + N2[j];
                        S2 = S2 + N1[j];
                        
                    }
                    for (int j = z; j < N1.Length; j++)
                    {
                        S1 = S1 + N1[j];
                        S2 = S2 + N2[j];
                        
                    }
                    for (int j = 0; j < gene_num; j++)
                    {
                        Q1 = "";
                        Q2 = "";


                        for (int v = j * 8; v < j * 8 + 8; v++)
                        {
                            Q1 = Q1 + S1[v];
                            Q2 = Q2 + S2[v];
                        }
                        
                        
                        child[i, j] = Convert.ToInt32(Q1, 2);
                        child[i + 1, j] = Convert.ToInt32(Q2, 2);
                    }
                }
                else for (int j = 0; j < gene_num; j++)
                    {
                        child[i, j] = selected_pop[i, j];
                        child[i + 1, j] = selected_pop[i + 1, j];
                    }

            }
            for (int j = 0; j < gene_num; j++)
            {
                child[popsize - 1,j] = selected_pop[popsize - 1,j];
                }

            return child;
        }

    }

}
