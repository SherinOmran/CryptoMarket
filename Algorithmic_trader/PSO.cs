using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithmic_trader
{
    class PSO
    {
        static double[] prices;

        int neigh_size = 20;
        static private int swarm_size;
      
        static private int dim; // no. of variables that need to be optimized
                                
        public double[,] pos = new double[swarm_size, dim]; //position matrix
        public double[,] fit = new double[swarm_size, 3];
        public double[,] personalbest_pos;
        public double[,] globalbest_pos;
        public double[] globalbest_cost; 
        static private int ind;
        public double[,] vel;
        double w = 0.98;//0.729;//
        double c1 = 1.1;// 0.5;//1.1;//9445;  0.5;//
        double c2 = 1.1;// 0.5;//1.1;//0.1;//1.4;//9445; 0.5;//1    0.5;//

        double[] z = new double[] { 10000, 50, 120 };
        double[] z_min = new double[] { 0, 0, 10 };// { 100000, 100000, 100000 };
        double[] z_max = new double[] { 100, 1, 50 };//{ 0, 0, 0 };

        /////////////////

        public double[,] weights;

        ///////////////////////////




        /// Initialization parameters
        /// 
        double min = 3.0, max = 200;// 200;//  minimum and maximum number of analysis days
        double OB_min = 60.0, OB_max = 90.0;    //overbought range
        double OS_min = 10.0, OS_max = 40.0;   //oversold range
        double roc_min = 0;   // rate of change minimum
        double roc_max = 20.0;  // rate of change maximum
        double roc_Smin = 3;
        double roc_Smax = 20.0;

        int[,] E_dist;
        double mut_rate = 0.25;// 0.10;//0.05;
        public double[] personalbest_cost;
       
        public void initialize()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            
           

            for (int i = 0; i < swarm_size; i++)
            {
               
                globalbest_cost[i] = 1000000.00;
                personalbest_cost[i] = 1000000.00;
                if (ind==0)  // WMA with a single  parameters n (number of days)
                    {
                      
                    pos[i, 0] = r.Next((int)min,(int)max);
                    vel[i, 0] = 0;
                       
                        
                    }
                   

                  

                else if(ind==1 )    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochastic rsi  
                    {
                        pos[i, 0] = (max - min) * r.NextDouble() + min;
                        pos[i, 1] = (max - min) * r.NextDouble() + min;
                        pos[i,3]= (OB_max - OB_min) * r.NextDouble() + OB_min;
                        pos[i,2]= (OS_max - OS_min) * r.NextDouble() + OS_min;

                    vel[i, 0] = 0;// 0.1*(max - min) * r.NextDouble() +0.1* min;
                    vel[i, 1] = 0;// 0.1*(max - min) * r.NextDouble() + 0.1*min;
                    vel[i, 2] = 0;// 0.1*(OB_max - OB_min) * r.NextDouble() + 0.1*OB_min;
                    vel[i, 3] = 0;// 0.1*(OS_max - OS_min) * r.NextDouble() + 0.1*OS_min;
                    }
                    else if (ind == 2)  /// SROC Smoothed rate of change
                {


                    pos[i, 0] =  (max - min) * r.NextDouble() + min; //(20 - 3) * r.NextDouble() + 3;//
                    pos[i, 1] = (max - min) * r.NextDouble() + min; //(50 - 3) * r.NextDouble() + 3;// 



                    pos[i, 2] =  (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                    pos[i, 3] =  -1*((roc_max - roc_min) * r.NextDouble() + roc_min);      //OS

                    vel[i, 0] = 0;// 0.1 * (max - min) * r.NextDouble() + 0.1 * min;
                    vel[i, 1] = 0;// 0.1 * (max - min) * r.NextDouble() + 0.1 * min;
                    vel[i, 2] = 0;// 0.1 * (roc_max - roc_min) * r.NextDouble() + 0.1 * roc_min;
                    vel[i, 3] = 0;// -1 * (0.1 * (roc_max - roc_min) * r.NextDouble() + 0.1 * roc_min);

                }

                else if(ind==3)   // bollinger bands ( 2 lookback periods and a multiplier)
                    {
                        pos[i, 0] = (max - min) * r.NextDouble() + min;
                        pos[i, 1] = (max - min) * r.NextDouble() + min; 
                        pos[i, 2] = 2.0* r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution

                    vel[i, 0] = 0;
                    vel[i, 1] = 0;
                    vel[i, 2] = 0;
                    //vel[i, 0] = 0.1 * (max - min) * r.NextDouble() + 0.1 * min;
                    //vel[i, 1] = 0.1 * (max - min) * r.NextDouble() + 0.1 * min;
                    //vel[i, 2] = 0.2 * r.NextDouble() + 0.1;

                }
                if (ind == 4)  // WMA with a single  parameters n (number of days)
                {

                    pos[i, 0] = r.Next((int)min, (int)max);
                    pos[i, 1] = r.Next((int)min, (int)max);
                    vel[i, 0] = 0;
                    vel[i, 1] = 0;


                }
                for (int k = 0; k <= dim - 1; k++)
                {
                    personalbest_pos[i, k] = pos[i, k];
                    globalbest_pos[i,k] = pos[i, k];
                }
               

            }

           

        }
        private void method()
        {
            for (int i = 0; i < swarm_size; i++)
            {
                globalbest_cost[i] = 100000000;
            }
        }
        
        public double[] fitness( double[,] weights,int[,]E_dist)         /////////// 
        {
            
             double[] cost = new double[swarm_size]; //double[] z;

            
            Trading_MO tr = new Trading_MO(prices);
            Decomposition dd = new Decomposition(swarm_size, 3);
            
            double[] wt = new double[3]; // weight for each particle
            double[] a;
            for (int i = 0; i < swarm_size; i++)
            {

                for (int s = 0; s < 3; s++)   ///// s is the number of objectives
                {
                    wt[s] = weights[i, s];
                }

                if (ind == 0)
                {
                    
                    a = tr.WMA_trading(Convert.ToInt32(pos[i, 0]));
                    //update_ref(z, a);
                    //cost[i] =  dd.W_Tch(z, a, wt);
                    
                    update_ref(z_min, z_max, a);
                    cost[i] = dd.W_Tch(z_min, z_max, a, wt);

                }
                else if (ind == 1)
                {
                    a = tr.stoch_RSI_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));
                    //update_ref(z, a);
                    //cost[i] = dd.W_Tch(z, a, wt); 
                    update_ref(z_min, z_max, a);
                    cost[i] = dd.W_Tch(z_min, z_max, a, wt);

                }
                else if (ind == 2)   /// sroc
                {
                    
                    a = tr.SROC_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));
                    //update_ref(z, a);
                    //cost[i] = dd.W_Tch(z, a, wt);
                    update_ref(z_min, z_max, a);
                    cost[i] = dd.W_Tch(z_min, z_max, a, wt);

                }

                else if (ind == 3)  //BB
                {

                    a = tr.Bollinger_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), pos[i, 2]);
                    update_ref(z_min, z_max, a);
                    cost[i] = dd.W_Tch(z_min, z_max, a, wt);
                }
                else
                {
                    a = tr.WMA_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1])); // Double WMA crossover
                    update_ref(z_min, z_max, a);
                    cost[i] = dd.W_Tch(z_min, z_max, a, wt);
                }
                fit[i, 0] = a[0]; fit[i, 1] = a[1]; fit[i, 2] = a[2];
               

                if (cost[i] < personalbest_cost[i])
                {
                    for (int j = 0; j < dim; j++)
                    {
                        personalbest_pos[i, j] = pos[i, j];
                        
                        personalbest_cost[i] = cost[i];
                    }
                }

            }
            update_global_best(E_dist, cost);
           

            return cost;
        }
        void update_global_best(int[,]E_dist,double[]cost)
        {
            for (int i = 0; i < swarm_size; i++)
            {
                for (int k = 0; k < E_dist.GetLength(1); k++)
                {
                    int s = E_dist[i, k];


                    if (cost[s] < globalbest_cost[i])
                    {
                        globalbest_cost[i] = cost[s];
                        for (int j = 0; j < dim; j++)
                        {
                            globalbest_pos[i, j] = pos[s, j];
                        }
                    }

                }
            }
        }

        public void boundary_check()
        
        { Random r = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < swarm_size; i++)
            {
                if (ind == 0)
                {
                    if ((pos[i, 0] < min) || (pos[i, 0] > max)) { pos[i, 0] = Math.Floor((max - min) * r.NextDouble() + min); }
                    
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (pos[i, j] < min) pos[i, j] = min;
                        else if (pos[i, j] > max) pos[i, j] = max;
                       
                    }


                    if (ind == 1)
                    {
                        if (pos[i, 3] < OB_min) pos[i,3 ] = OB_min;
                        else if (pos[i, 3] > OB_max) pos[i, 3] = OB_max;
                        if (pos[i, 2] < OS_min) pos[i, 2] = OS_min;
                        else if (pos[i, 2] > OS_max) pos[i, 2] = OS_max;
                    }


                  

                    else if (ind == 2)
                    {
                        if (pos[i, 2] < roc_min) pos[i, 2] = roc_min;
                        else if (pos[i, 2] > roc_max) pos[i, 2] = roc_max;
                        if ((pos[i, 3] >0)||(pos[i,3]>pos[i,2])) pos[i, 3] = 0;
                        else if ((pos[i, 3]) <- roc_max) pos[i, 3] = -1 * roc_max;

                       
                    }

                    

                    else if (ind == 3)
                    {
                        if (pos[i, 2] < 1) pos[i, 2] = 1;
                        else if (pos[i, 2] > 3) pos[i, 2] = 3;
                       
                    }

                    
                }
            }






        }

        /////////////////////////////////////////position_velocity update without mutation

        //public void pos_vel_update()   // velocity and position update
        //{
        //    Random r1 = new Random(Guid.NewGuid().GetHashCode());
        //    Random r2 = new Random(Guid.NewGuid().GetHashCode());
        //    double R1, R2;
        //    R1 = r1.NextDouble();
        //    R2 = r2.NextDouble();
        //    for (int i = 0; i < swarm_size; i++)
        //    {

        //        for (int j = 0; j < dim; j++)
        //        {


        //            vel[i, j] = (w * vel[i, j] + c1 * R1 * (personalbest_pos[i, j] - pos[i, j]) + c2 * R2 * (globalbest_pos[i,j] - pos[i, j]));

        //            //position update
        //            pos[i, j] = pos[i, j] + vel[i, j];





        //        }


        //    }
        //    boundary_check();
        //}


            /// <summary>
            /// /////////////////////////////////////////////// position_velocity update with mutation
            /// </summary>
        public void pos_vel_update()   // velocity and position update
        {
            Random r1 = new Random(Guid.NewGuid().GetHashCode());
            Random r2 = new Random(Guid.NewGuid().GetHashCode());
            double R1, R2;
            R1 = r1.NextDouble();
            R2 = r2.NextDouble();
            for (int i = 0; i < swarm_size; i++)
            {

                for (int j = 0; j < dim; j++)
                {


                    vel[i, j] = (w * vel[i, j] + c1 * R1 * (personalbest_pos[i, j] - pos[i, j]) + c2 * R2 * (globalbest_pos[i, j] - pos[i, j]));

                    //position update
                    pos[i, j] = pos[i, j] + vel[i, j];





                }


            }
            boundary_check();
        }

        /// <summary>
        /// /////////////

        public void Mutate()
        {
            Random m_random = new Random(Guid.NewGuid().GetHashCode()); // m_random --> the probability for gene mutation

            Random r = new Random(Guid.NewGuid().GetHashCode());  // n --> a random number that replaces the mutated gene 
            int d;
            for (int i = 0; i < swarm_size; i++)
            { // i--> chromosome
                double v = m_random.NextDouble();
                if (v <= mut_rate)

                {
                    d = r.Next(0, dim-1);
                    //for (int p = 0; p < dim; p++)  // pos--> gene
                    //{
                        if (ind == 0)  // 2 parameters n and multiplier
                        {
                            pos[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                        }                        
                       else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                            {
                                //d = r.Next(0, 3);
                                if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                                else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                                else if (d == 2) pos[i, 2] = (OS_max - OS_min) * r.NextDouble() + OS_min;
                                else pos[i, 3] = (OB_max - OB_min) * r.NextDouble() + OB_min;
                            }
                            else if (ind == 2)
                            {
                                //d = r.Next(0, 3);
                                if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                                else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min; /// 
                                else if (d == 2) pos[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                                else pos[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min); ;      //OS
                            }

                            else  if (ind==3)  // bollinger bands ( 2 lookback periods and multiplier)
                            {
                                //d = r.Next(0, 2);
                                if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                                else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                                else pos[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution


                            }
                        else if (ind==4)

                         {
                       if (d==0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                       else if (d==1)  pos[i, 1] = (max - min) * r.NextDouble() + min;
                    }
                        }

                    }
                
           
        
             boundary_check();
            //return pop;
        }

        public void Mutate2(double[]cost,int[,] E_dist, double[,]weights)
        {
            Random m_random = new Random(Guid.NewGuid().GetHashCode()); // m_random --> the probability for gene mutation
            //double[,] old_fit = fit;
            double new_cost=1000; double temp_pos;
            Random r = new Random(Guid.NewGuid().GetHashCode());  // n --> a random number that replaces the mutated gene 
            int d;
            Trading_MO tr = new Trading_MO(prices);
            Decomposition dd = new Decomposition(swarm_size, 3);
            double[] a;
            double[] wt=new double[3];
              
            for (int i = 0; i < swarm_size; i++)
            { // i--> chromosome
                for (int s = 0; s < 3; s++)   ///// s is the number of objectives
                {
                    wt[s] = weights[i, s];
                }
                double v = m_random.NextDouble();
                if (v <= mut_rate)

                {
                    d = r.Next(0, dim-1);
                    temp_pos = pos[i, d];
                    if (ind == 0)  // 2 parameters n and multiplier
                    {
                        pos[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                    }
                    else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else if (d == 2) pos[i, 2] = (OS_max - OS_min) * r.NextDouble() + OS_min;
                        else pos[i, 3] = (OB_max - OB_min) * r.NextDouble() + OB_min;

                        a = tr.stoch_RSI_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));
                        
                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                        
                    }
                    else if (ind == 2)
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min; /// 
                        else if (d == 2) pos[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                        else pos[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min); ;      //OS
                        a = tr.SROC_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }

                    else if (ind==3)   // bollinger bands ( 2 lookback periods and multiplier)
                    {
                        //d = r.Next(0, 2);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else pos[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution
                        a = tr.Bollinger_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), pos[i, 2]);

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);

                    }
                    else if (ind == 4)

                    {
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;

                        a = tr.WMA_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }
                    if (new_cost < cost[i])
                    {
                        cost[i] = new_cost;
                        if (cost[i] < personalbest_cost[i])
                        {
                            for (int j = 0; j < dim; j++)
                            {
                                personalbest_pos[i, j] = pos[i, j];

                                personalbest_cost[i] = cost[i];
                            }
                        }
                    }
                    else pos[i, d] = temp_pos;
                }

            }
            // fitness(wt, e_dist);
            update_global_best(E_dist, cost);

           // boundary_check();
            //return pop;
        }


        ///
        public void Mutate2(double[] cost, int[,] E_dist, double[,] weights,double mut_rate)
        {
            Random m_random = new Random(Guid.NewGuid().GetHashCode()); // m_random --> the probability for gene mutation
            //double[,] old_fit = fit;
            double new_cost = 1000; double temp_pos;
            Random r = new Random(Guid.NewGuid().GetHashCode());  // n --> a random number that replaces the mutated gene 
            int d;
            Trading_MO tr = new Trading_MO(prices);
            Decomposition dd = new Decomposition(swarm_size, 3);
            double[] a;
            double[] wt = new double[3];

            for (int i = 0; i < swarm_size; i++)
            { // i--> chromosome
                for (int s = 0; s < 3; s++)   ///// s is the number of objectives
                {
                    wt[s] = weights[i, s];
                }
                double v = m_random.NextDouble();
                if (v <= mut_rate)

                {
                    d = r.Next(0, dim - 1);
                    temp_pos = pos[i, d];
                    if (ind == 0)  // 2 parameters n and multiplier
                    {
                        pos[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                    }
                    else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else if (d == 2) pos[i, 2] = (OS_max - OS_min) * r.NextDouble() + OS_min;
                        else pos[i, 3] = (OB_max - OB_min) * r.NextDouble() + OB_min;

                        a = tr.stoch_RSI_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);

                    }
                    else if (ind == 2)
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min; /// 
                        else if (d == 2) pos[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                        else pos[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min); ;      //OS
                        a = tr.SROC_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }

                    else if (ind == 3)   // bollinger bands ( 2 lookback periods and multiplier)
                    {
                        //d = r.Next(0, 2);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else pos[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution
                        a = tr.Bollinger_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), pos[i, 2]);

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);

                    }
                    else if (ind == 4)

                    {
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;

                        a = tr.WMA_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }
                    if (new_cost < cost[i])
                    {
                        cost[i] = new_cost;
                        if (cost[i] < personalbest_cost[i])
                        {
                            for (int j = 0; j < dim; j++)
                            {
                                personalbest_pos[i, j] = pos[i, j];

                               
                            }
                            personalbest_cost[i] = cost[i];
                        }
                    }
                    else pos[i, d] = temp_pos;
                }

            }
            // fitness(wt, e_dist);
            update_global_best(E_dist, cost);

            // boundary_check();
            //return pop;
        }

        public void Mutate3(double[] cost, int[,] E_dist, double[,] weights, double mut_rate)
        {
            Random m_random = new Random(Guid.NewGuid().GetHashCode()); // m_random --> the probability for gene mutation
            //double[,] old_fit = fit;
            double new_cost = 1000; double temp_pos;
            Random r = new Random(Guid.NewGuid().GetHashCode());  // n --> a random number that replaces the mutated gene 
            int d;
            Trading_MO tr = new Trading_MO(prices);
            Decomposition dd = new Decomposition(swarm_size, 3);
            double[] a;
            double[] wt = new double[3];

            for (int i = 0; i < swarm_size; i++)
            { // i--> chromosome
                for (int s = 0; s < 3; s++)   ///// s is the number of objectives
                {
                    wt[s] = weights[i, s];
                }
                double v = m_random.NextDouble();
                if (v <= mut_rate)

                {
                    d = r.Next(0, dim - 1);
                    temp_pos = pos[i, d];
                    if (ind == 0)  // 2 parameters n and multiplier
                    {
                        pos[i, 0] = Math.Floor((max - min) * r.NextDouble() + min);

                    }
                    else if (ind == 1)    // 4 parameters 2 lookback periods and 2 levels for OB and OS   // stochrsi  
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else if (d == 2) pos[i, 2] = (OS_max - OS_min) * r.NextDouble() + OS_min;
                        else pos[i, 3] = (OB_max - OB_min) * r.NextDouble() + OB_min;

                        a = tr.stoch_RSI_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);

                    }
                    else if (ind == 2)
                    {
                        //d = r.Next(0, 3);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;       // sroc 
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min; /// 
                        else if (d == 2) pos[i, 2] = (roc_max - roc_min) * r.NextDouble() + roc_min;   // OB 
                        else pos[i, 3] = -1 * ((roc_max - roc_min) * r.NextDouble() + roc_min); ;      //OS
                        a = tr.SROC_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), Convert.ToInt32(pos[i, 2]), Convert.ToInt32(pos[i, 3]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }

                    else if (ind == 3)   // bollinger bands ( 2 lookback periods and multiplier)
                    {
                        //d = r.Next(0, 2);
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;
                        else pos[i, 2] = 2.0 * r.NextDouble() + 1.0;   // where min=1 and max=3   this is the value after substitution
                        a = tr.Bollinger_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]), pos[i, 2]);

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);

                    }
                    else if (ind == 4)

                    {
                        if (d == 0) pos[i, 0] = (max - min) * r.NextDouble() + min;
                        else if (d == 1) pos[i, 1] = (max - min) * r.NextDouble() + min;

                        a = tr.WMA_trading(Convert.ToInt32(pos[i, 0]), Convert.ToInt32(pos[i, 1]));

                        update_ref(z_min, z_max, a);
                        new_cost = dd.W_Tch(z_min, z_max, a, wt);
                    }
                    if (new_cost < cost[i])
                    {
                        cost[i] = new_cost;
                        if (cost[i] < personalbest_cost[i])
                        {
                            for (int j = 0; j < dim; j++)
                            {
                                personalbest_pos[i, j] = pos[i, j];

                                personalbest_cost[i] = cost[i];
                            }
                        }
                    }
                    else pos[i, d] = temp_pos;
                }

            }
            // fitness(wt, e_dist);
            update_global_best(E_dist, cost);

            // boundary_check();
            //return pop;
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="s_size"></param>
        /// <param name="Dim"></param>
        /// <param name="Ind"></param>
        /// <param name="Prices"></param>



        public PSO(int s_size, int Dim, int Ind, double[] Prices)
        {
            swarm_size = s_size;
            ind = Ind;
            dim = Dim;
            prices = Prices;
            pos = new double[swarm_size, dim];
            personalbest_pos = new double[swarm_size, dim];
            globalbest_pos = new double[swarm_size,dim];
            vel = new double[swarm_size, dim];
            globalbest_cost = new double[swarm_size];
            personalbest_cost = new double[swarm_size];
            fit = new double[swarm_size, 3];
    }
        public void update_ref(double[]z,double[]a)
        { if (a[0] > z[0]) z[0] = a[0];
            if (a[1] > z[1]) z[1] = a[1];

        }
        public void update_ref(double[] z_min, double[] z_max, double[] a)
        {
            for (int i = 0; i < 3; i++)

            {
                if (a[i] < z_min[i]) { z_min[i] = a[i]; }
                else if (a[i] > z_max[i]) { z_max[i] = a[i]; }
            }

        }
        public PSO(int s_size, int Dim, int Ind, double[] Prices, double[,]Weights, int[,]Euc_dist)
        {
            swarm_size = s_size;
            ind = Ind;
            dim = Dim;
            prices = Prices;
            pos = new double[swarm_size, dim];
            personalbest_pos = new double[swarm_size, dim];
            globalbest_pos = new double[swarm_size, dim];
            vel = new double[swarm_size, dim];
            globalbest_cost = new double[swarm_size];
            personalbest_cost = new double[swarm_size];
            weights = Weights;
            E_dist = Euc_dist;
        }
    }
}
