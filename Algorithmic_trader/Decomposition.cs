using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithmic_trader
{
    class Decomposition
    {
        int pop, obj;
        public Decomposition(int swarm_size, int n_obj)
        {
            pop = swarm_size;
            obj = n_obj;
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.NextDouble() * (maximum - minimum) + minimum;
        }


        ////////////////// Systematic weight generation ///////////////////////

        public double[,] sys_weight_gen()
        {
            double[,] w = new double[pop, 3];
            int s = 25; //controlling parameter    //
            int i = 0;
            for (int k=0;k<=s;k++)
            {
                for (int H=0;H<=s-k;H++)
                {
                    w[i, 0] = (double)k / (double)s;
                   double d =(double) H / (double)s;
                    w[i, 1] = 1 - (w[i, 0] + d);
                    w[i, 2] = 1 - (w[i, 0] + w[i, 1]);
                    i++;

                }
            }
            return w;
        }

        public double[,] sys_weight_gen(int s)
        {
            double[,] w = new double[pop, 3];
           // int s = 25;// 30;  //controlling parameter    //
            int i = 0;
            for (int k = 0; k <= s; k++)
            {
                for (int H = 0; H <= s - k; H++)
                {
                    w[i, 0] = (double)k / (double)s;
                    double d = (double)H / (double)s;
                    w[i, 1] = 1 - (w[i, 0] + d);
                    w[i, 2] = 1 - (w[i, 0] + w[i, 1]);
                    i++;

                }
            }
            return w;



        }
       

        //////////////////////////// Uniform random weight Generation //////////////////////////////

        public double[,] rand_weight_gen()
        {
            double sum;
            double[,] weight_vec = new double[pop, obj];
            for (int i = 0; i < pop; i++)
            {
                sum = 0;
                for (int j = 0; j < obj; j++)
                {
                    if (j < obj - 1)
                    {
                        weight_vec[i, j] = GetRandomNumber(0, 1 - sum);

                        sum = sum + weight_vec[i, j];
                    }
                    else
                    {
                        weight_vec[i, j] = 1 - sum;
                    }
                }

            }
            return weight_vec;
        }


       


        ///*******************************  Non normalized weighted Tchebycheff   ***************************//

        public double W_old_Tch(double[] z, double[] fitness, double[] W)           //// z is the reference point , W the weights
        {
            double w_Tch;
            double[] a = new double[3];
            

            for (int j = 0; j<fitness.GetLength(0); j++)
            {
                //a[j] = (1 / W[j]) * Math.Abs(fitness[j] - z[j]);   // for inverted weighted Tchebycheff
                a[j] = W[j] * Math.Abs(fitness[j] - z[j]);
                
            }

            
                 w_Tch = a.Max();


            return w_Tch;
        }

        /// 
        /// 
        /// 
        /// ********************************************* Normalized  WTCH  *****************************************************************************
        /// 

        //public double W_Tch(double[] z_min, double[] z_max, double[] fitness, double[] W)
        //{
        //    double w_Tch;
        //    double[] a = new double[3];

        //    if (z_min[2] == 0) { z_min[2] = 1; }

        //    for (int j = 0; j < fitness.GetLength(0); j++)
        //    {

        //        a[j] = W[j] * (Math.Abs(fitness[j] - z_max[j]) / (z_max[j] - z_min[j]));
        //        if (j == 2)
        //        {
        //            a[j] = W[j] * (Math.Abs(z_min[j] - fitness[j]) / (z_max[j] - z_min[j]));


        //        }
        //    }


        //    w_Tch = a.Max();


        //    return w_Tch;
        //}



        /////***************************Normalized AUG_TCH   *************************************************


        public double W_Tch(double[] z_min, double[] z_max, double[] fitness, double[] W)
        {
            double w_Tch;
            double[] a = new double[3];
            double roh = 0.05; // 0.02;
            double sum = 0;
           
            if (z_min[2] == 0) { z_min[2] = 1; }
            for (int j = 0; j < fitness.GetLength(0); j++)
            {

                if (j != 2)
                {
                    a[j] = W[j] * (Math.Abs(fitness[j] - z_max[j]) / (z_max[j] - z_min[j]));
                    sum = sum + (Math.Abs(fitness[j] - z_max[j]) / (z_max[j] - z_min[j]));
                }
                else
                {
                    a[j] = W[j] * (Math.Abs(z_min[j] - fitness[j]) / (z_max[j] - z_min[j]));

                    sum = sum + (Math.Abs(z_min[j] - fitness[j]) / (z_max[j] - z_min[j]));
                }

            }

            w_Tch = a.Max() + roh * sum;


            return w_Tch;
        }





       

        //////////////////


        
////////////// Calculate Euclidean distance

        public int[,] E_dist(double[,] weight, int n)   //  a function too calculate the euclidean distance & the neighbors of each subproblem. Here, n is the neighborhood size
        {
            double[] B = new double[weight.GetLength(0)];
            
            int[,] neighnors = new int[weight.GetLength(0), n];
            double[] A = new double[B.Length];
            double sum = 0;
            int index=0;
            for (int i = 0; i < weight.GetLength(0); i++)
            {
                for (int k = 0; k < weight.GetLength(0); k++)
                {
                    sum = 0;
                    for (int j = 0; j < weight.GetLength(1); j++)
                    {
                        sum = sum + Math.Pow(weight[i, j] - weight[k, j], 2.0);


                    }
                    sum = Math.Sqrt(sum);
                    B[k] = sum;
                    A[k] = sum;

                   

                }
                Array.Sort(A);
                for (int z = 0; z < n; z++)
                {
                    
                    for (int a=0;a<weight.GetLength(0);a++)
                    {
                        if ( B[a]== A[z])
                        {
                            index = a;
                            if ((z > 0) && (index == neighnors[i, z - 1])) { a++; }
                            else
                            {
                                neighnors[i, z] = index;
                                break;
                            }
                        }
                    }

                }


            }
            return neighnors;
        }

      
    }
}
