using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Algorithmic_trader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        double[] prices;
        double[] tst_prices;
        List<double> Prices = new List<double>();
        int iter = 50;
        int popsize = 351;
        int runs = 1;

        public double[] DB_read()
        {
            D_B db = new D_B();
            db.db_read("newEthereum", "3/1/2017", "3/1/2019");  
            prices = db.p.ToArray();
            return prices;
        }

        public double[] Tst_DB_read()
        {
            D_B db = new D_B();
            db.db_read("newEthereum", "3/2/2019", "3/1/2021");
            tst_prices = db.p.ToArray();
            return tst_prices;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size =  popsize;
            PSO p = new PSO(s_size, 1, 0, prices);
            double[,] best_pos = new double[iter, 1];
            double[,] ndbest = new double[50000, 1];
            double[] fit;
            double[,] non_dominated = new double[50000, 3];
            List<int> ND_index = new List<int>();

            double[,] Best_pos = new double[s_size, 1];
            double[] N_D = new double[3];
            string[] results = new string[7];
            double[] tst_results;
            string[] cols = new string[7] { "pos", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "No. of days");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            
           
            int c;
            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

            double[,] position = new double[50000, 1];
            for (int z = 0; z < runs; z++)  //
            {
                Non_dominated N = new Non_dominated(prices, 0, 1);
                p = new PSO(s_size, 1, 0, prices);
                p.initialize();
                c = 0;
                weights = dec.sys_weight_gen();
                E_dist = dec.E_dist(weights, neigh_size);
                for (int i = 0; i < iter; i++)
                {
   
                    p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos;
                    p.boundary_check();
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { position[x, 0] = Best_pos[x - N.count, 0]; }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;

                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.WMA_trading(Convert.ToInt32(N.ndbest[j, 0]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);

                    tst_results = Tst.WMA_trading(Convert.ToInt32(N.ndbest[j, 0]));
                    results[0] = N.ndbest[j, 0].ToString();
                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 1] = non_dominated[j, x].ToString();
                        results[x + 4] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            List<double> temp = new List<double>();
            Genetic_Algorithm g = new Genetic_Algorithm(1, 0, prices);
            double[,] pop;
            double[] f;
            double[] bestfit = new double[runs];
            double[] N_D = new double[3]; ;
            string[] results = new string[7];
            double[] tst_results;
            double[,] ndbest = new double[500000, 1];
            double[,] non_dominated = new double[500000, 3];
            int c;
            double[,] position = new double[500000, 1];
            Trading_MO t = new Trading_MO(prices);
            string[] cols = new string[7] { "pos", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "No. of days");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            Trading_MO Tst;

            Stack<int> ND_stack;
            int[,] intermediary = new int[popsize, 4];
            for (int z = 0; z < runs; z++)
            {
                Non_dominated N = new Non_dominated(prices, 0, 1);
                ND_stack = new Stack<int>();
                pop = g.population();

                for (int i = 0; i < iter; i++)
                {
                    f = g.fitness(pop);
                    bestfit[z] = f[f.Length - 1];
                    pop = g.new_roulettewheel(f, pop);
                    pop = g.Crossover_twopoint(pop);
                    pop = g.Mutate(pop);

                    int stop;


                    if (i == 0)
                    { position = pop; }
                    else
                    {
                        position = N.ndbest;
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { position[x, 0] = pop[x - N.count, 0]; }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);

                }



                c = N.count;

                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.WMA_trading((int)N.ndbest[j, 0]);
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);

                    tst_results = Tst.WMA_trading((int)N.ndbest[j, 0]);
                    results[0] = N.ndbest[j, 0].ToString();
                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 1] = non_dominated[j, x].ToString();
                        results[x + 4] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }


            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();

            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;

            double[,] best_pos = new double[iter, 1];
            double[,] ndbest = new double[50000, 1];
            double[] fit;
            double[,] non_dominated = new double[50000, 3];
            List<int> ND_index = new List<int>();

            double[,] Best_pos = new double[s_size, 1];
            double[] N_D = new double[3];
            string[] results = new string[7];
            double[] tst_results;
            string[] cols = new string[7] { "pos", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "No. of days");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
           
            int c;
            double[,] weights = new double[popsize, 3];
            int neigh_size = 3;
            int[,] E_dist;

            double[,] position = new double[50000, 1];
            for (int z = 0; z < runs; z++)  
            {
                PSO p = new PSO(s_size, 1, 0, prices);
                Non_dominated N = new Non_dominated(prices, 0, 1);
                Decomposition dec = new Decomposition(popsize, 3);
                p.initialize();

                c = 0;
                for (int i = 0; i < iter; i++)
                {
                   

                    if (i % 2 == 0)
                    { weights = dec.sys_weight_gen(); }
                    else { weights = dec.rand_weight_gen(); }

                    E_dist = dec.E_dist(weights, neigh_size);

                    p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos;
                    p.boundary_check();
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                     
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { position[x, 0] = Best_pos[x - N.count, 0]; }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;

                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.WMA_trading(Convert.ToInt32(N.ndbest[j, 0]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);

                    tst_results = Tst.WMA_trading(Convert.ToInt32(N.ndbest[j, 0]));
                    results[0] = N.ndbest[j, 0].ToString();
                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 1] = non_dominated[j, x].ToString();
                        results[x + 4] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;
            PSO p = new PSO(s_size, 4, 1, prices);
            double[,] best_pos = new double[iter, 4];

            double[] fit;
            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 4];
            double[] N_D = new double[3];
            string[] results = new string[10];
            double[] tst_results;
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "Look_back 1");
            dataGridView1.Columns.Add("pos", "Look_back 2");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            Stack<int> ND_stack;
            int[,] intermediary = new int[s_size, 4];
            int c;
            //iter = 10;
            double[,] position = new double[50000, 4];
            int dim = 4;
            double[] trial_fit;

            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

            for (int z = 0; z < runs; z++)
            {
                weights = dec.sys_weight_gen();
                E_dist = dec.E_dist(weights, neigh_size);
                List<int> ND_index = new List<int>();
                ND_stack = new Stack<int>();
                p.initialize();
                Non_dominated N = new Non_dominated(prices, 1, 4);
                for (int i = 0; i < iter; i++)
                {

                    trial_fit = p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos; 
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;

                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Genetic_Algorithm g = new Genetic_Algorithm(4, 1, prices);
            double[,] pop;
            double[] f;
            int dim = 4;
            double[] bestfit = new double[runs];
            double[] N_D = new double[3];
            string[] results = new string[10];
            double[] tst_results;
            Trading_MO t = new Trading_MO(prices);
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            double[,] ndbest = new double[50000, dim];
            double[,] non_dominated = new double[50000, 3];
            int c;
            double[,] position = new double[50000, dim];
            dataGridView1.Columns.Add("pos", "Look_back 1");
            dataGridView1.Columns.Add("pos", "Look_back 2");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            Trading_MO Tst;
            int[] intermediary = new int[4];
            for (int z = 0; z < runs; z++)
            {
               
                Non_dominated N = new Non_dominated(prices, 1, dim);
                pop = g.population();
                for (int i = 0; i < iter; i++)
                {
                    f = g.fitness(pop);
                    bestfit[z] = f[f.Length - 1];
                    g.new_roulettewheel(f, pop);
                    pop = g.crossover2(pop);
                    pop = g.Mutate(pop);
                    pop = g.boundary_check(pop);

                    int stop;
                    if (i == 0)
                    { position = pop; }
                    else
                    {
                        position = N.ndbest;
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { for (int y = 0; y < dim; y++) { position[x, y] = pop[x - N.count, y]; } }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);

                }




                c = N.count;
                for (int j = 0; j < c; j++)

                {
                    t = new Trading_MO(prices);
                    N_D = t.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);

                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;
            PSO p = new PSO(s_size, 4, 1, prices);
            double[,] best_pos = new double[iter, 4];

            double[] fit;
            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 4];
            double[] N_D = new double[3];
            string[] results = new string[10];
            double[] tst_results;
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "Look_back 1");
            dataGridView1.Columns.Add("pos", "Look_back 2");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            
            int[,] intermediary = new int[s_size, 4];
            int c;
           
            double[,] position = new double[50000, 4];
            int dim = 4;
            double[] trial_fit;

            double[,] weights = new double[popsize, 3];
            int neigh_size = 3;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

            for (int z = 0; z < runs; z++)
            {

                
                p.initialize();
                Non_dominated N = new Non_dominated(prices, 1, 4);
                for (int i = 0; i < iter; i++)
                {
                    if (i % 2 == 0)
                    { weights = dec.sys_weight_gen(); }
                    else { weights = dec.rand_weight_gen(); }
                    E_dist = dec.E_dist(weights, neigh_size);
                    trial_fit = p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos; 
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;

                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.stoch_RSI_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;
            PSO p = new PSO(s_size, 4, 2, prices);
            double[,] best_pos = new double[iter, 4];

            double[] fit;

            double[,] ndbest = new double[50000, 4];

            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 4];
            double[] N_D = new double[3];
            string[] results = new string[10];
            double[] tst_results;
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "N_EMA");
            dataGridView1.Columns.Add("pos", "N_ROC");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
           
            int[] intermediary = new int[4];
           
           
            int c;
         
            double[,] position = new double[50000, 4];
            int dim = 4;
            double[] trial_fit;
            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

           

            for (int z = 0; z < runs; z++)
            {
                
                p.initialize();
                Non_dominated N = new Non_dominated(prices, 2, 4);
                weights = dec.sys_weight_gen();
                E_dist = dec.E_dist(weights, neigh_size);
                for (int i = 0; i < iter; i++)
                {

                    trial_fit = p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos; 
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                       
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }


                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }


        }

        private void button7_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            List<double> temp = new List<double>();
            Genetic_Algorithm g = new Genetic_Algorithm(4, 2, prices);
           
            double[,] pop;
            double[] f;
            int dim = 4;
          
            double[] bestfit = new double[runs];
            double[] N_D = new double[3]; ;
            string[] results = new string[10];
            double[] tst_results;
            double[,] ndbest = new double[50000, dim];
            double[,] non_dominated = new double[50000, 3];
            int c;
            double[,] position = new double[50000, dim];
            Trading_MO t = new Trading_MO(prices);
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "N_EMA");
            dataGridView1.Columns.Add("pos", "N_ROC");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            Trading_MO Tst;


            
            int[,] intermediary = new int[popsize, dim];
            for (int z = 0; z < runs; z++)
            {
                Non_dominated N = new Non_dominated(prices, 2, dim);
               
                pop = g.population();
                

                for (int i = 0; i < iter; i++)
                {
                    f = g.fitness(pop);
                    bestfit[z] = f[f.Length - 1];
                    pop = g.new_roulettewheel(f, pop);
                    pop = g.crossover2(pop); 
                    pop = g.Mutate(pop);

                    int stop;


                    if (i == 0)
                    { position = pop; }
                    else
                    {
                        position = N.ndbest;
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { for (int y = 0; y < dim; y++) { position[x, y] = pop[x - N.count, y]; } }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);

                }



                c = N.count;
                for (int j = 0; j < c; j++)

                {
                    t = new Trading_MO(prices);
                    N_D = t.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);

                }


            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            
            int s_size = popsize;
            


            PSO p = new PSO(s_size, 4, 2, prices);
            double[,] best_pos = new double[iter, 4];

            double[] fit;

            double[,] ndbest = new double[50000, 4];

            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 4];
            double[] N_D = new double[3];
            string[] results = new string[10];
            double[] tst_results;
            string[] cols = new string[10] { "pos 1", "pos 2", "pos3", "pos4", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "N_EMA");
            dataGridView1.Columns.Add("pos", "N_ROC");
            dataGridView1.Columns.Add("pos", "OB");
            dataGridView1.Columns.Add("pos", "OS");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
           
            int[] intermediary = new int[4];
            
            
            int c;
            //iter = 10;
            double[,] position = new double[50000, 4];
            int dim = 4;
            double[] trial_fit;
            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);


            for (int z = 0; z < runs; z++)
            {
                
               
                
                p.initialize();
                Non_dominated N = new Non_dominated(prices, 2, 4);
               
                for (int i = 0; i < iter; i++)
                {
                    
                    if (i % 2 == 0)
                    { weights = dec.sys_weight_gen(); }
                    else { weights = dec.rand_weight_gen(); }

                    E_dist = dec.E_dist(weights, neigh_size);

                    trial_fit = p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos; 
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                       
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }

                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.SROC_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), Convert.ToInt32(N.ndbest[j, 2]), Convert.ToInt32(N.ndbest[j, 3]));
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                    results[3] = Convert.ToInt32(N.ndbest[j, 3]).ToString();

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 4] = non_dominated[j, x].ToString();
                        results[x + 7] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;
            PSO p = new PSO(s_size, 3, 3, prices);
            double[,] best_pos = new double[iter, 3];

            double[] fit;
            p.initialize();
            double[,] ndbest = new double[50000, 3];
            double[,] position = new double[50000, 3];
            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 3];
            double[] N_D = new double[3];
            string[] results = new string[9];
            double[] tst_results;
            string[] cols = new string[9] { "pos 1", "pos 2", "pos3", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "Look_back_1");
            dataGridView1.Columns.Add("pos", "Look_back_2");
            dataGridView1.Columns.Add("pos", "Multiplier");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            
            int[] intermediary = new int[4];
            int dim = 3;
            int c;
            
            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

            for (int z = 0; z < runs; z++)
            {
                
                Non_dominated N = new Non_dominated(prices, 3, 3);

                p.initialize();
                weights = dec.sys_weight_gen();
                E_dist = dec.E_dist(weights, neigh_size);
                for (int i = 0; i < iter; i++)
                {
                   
                    
                    p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos;
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                        
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }

                    

                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();


                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 3] = non_dominated[j, x].ToString();
                        results[x + 6] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Genetic_Algorithm g = new Genetic_Algorithm(3, 3, prices);
            double[,] pop;
            double[] f;
            
            double[] bestfit = new double[runs];
            double[] N_D = new double[3];
            string[] results = new string[9];
            double[] tst_results;
            double[,] ndbest = new double[50000, 3];
            double[,] position = new double[50000, 3];
            double[,] non_dominated = new double[50000, 3];
            int dim = 3;
            Trading_MO t = new Trading_MO(prices);
            string[] cols = new string[9] { "pos 1", "pos 2", "pos3", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "Look_back_1");
            dataGridView1.Columns.Add("pos", "Look_back_2");
            dataGridView1.Columns.Add("pos", "Multiplier");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            Trading_MO Tst;
            
            int[] intermediary = new int[4];
            int c;

            for (int z = 0; z < runs; z++)
            {
              
                Non_dominated N = new Non_dominated(prices, 3, 3);
                pop = g.population();

                for (int i = 0; i < iter; i++)
                {
                    f = g.fitness(pop);
                    bestfit[z] = f[f.Length - 1];
                    pop = g.new_roulettewheel(f, pop);
                    pop = g.crossover2(pop);
                    pop = g.Mutate(pop);

                    int stop;


                    if (i == 0)
                    { position = pop; }
                    else
                    {
                        position = N.ndbest;
                        
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        { for (int y = 0; y < dim; y++) { position[x, y] = pop[x - N.count, y]; } }
                    }



                    N.calc_nondominated(position, N.count, N.count + popsize);

                }



                c = N.count;
                for (int j = 0; j < c; j++)

                {
                    t = new Trading_MO(prices);
                    N_D = t.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();
                   

                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 3] = non_dominated[j, x].ToString();
                        results[x + 6] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);

                }

            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Tst_DB_read();
            DB_read();
            Trading_MO t = new Trading_MO(prices);
            Trading_MO Tst;
            int s_size = popsize;
            PSO p = new PSO(s_size, 3, 3, prices);
            double[,] best_pos = new double[iter, 3];

            double[] fit;
            p.initialize();
            
            double[,] ndbest = new double[50000, 3];
            double[,] position = new double[50000, 3];
            double[,] non_dominated = new double[50000, 3];
            double[,] Best_pos = new double[s_size, 3];
            double[] N_D = new double[3];
            string[] results = new string[9];
            double[] tst_results;
            string[] cols = new string[9] { "pos 1", "pos 2", "pos3", "RoI", "SR", "trades", "Tst_RoI", "Tst_SR", "Tst_trades" };
            dataGridView1.Columns.Add("pos", "Look_back_1");
            dataGridView1.Columns.Add("pos", "Look_back_2");
            dataGridView1.Columns.Add("pos", "Multiplier");
            dataGridView1.Columns.Add("ROI", "RoI");
            dataGridView1.Columns.Add("SR", "Sortino_Ratio");
            dataGridView1.Columns.Add("trades", "trades");
            dataGridView1.Columns.Add("Tst_RoI", "Tst_RoI");
            dataGridView1.Columns.Add("Tst_SR", "Tst_SR");
            dataGridView1.Columns.Add("Tst_trades", "Tst_trades");
            
            int[] intermediary = new int[4];
            int dim = 3;
            int c;
            //runs = 1;
            double[,] weights = new double[popsize, 3];
            int neigh_size = 5;
            int[,] E_dist;
            Decomposition dec = new Decomposition(popsize, 3);

            for (int z = 0; z < runs; z++)
            {
                
                Non_dominated N = new Non_dominated(prices, 3, 3);

                p.initialize();
                
                weights = dec.rand_weight_gen();
                E_dist = dec.E_dist(weights, neigh_size);
                for (int i = 0; i < iter; i++)
                {
                   

                    if (i % 2 == 0)
                    { weights = dec.sys_weight_gen(); }
                    else { weights = dec.rand_weight_gen(); }
                    E_dist = dec.E_dist(weights, neigh_size);
                    p.fitness(weights, E_dist);
                    p.pos_vel_update();
                    fit = p.globalbest_cost;
                    Best_pos = p.pos;
                    int stop;


                    if (i == 0)
                    { position = Best_pos; }
                    else
                    {
                        position = N.ndbest;
                        
                        stop = N.count + popsize;
                        for (int x = N.count; x < stop; x++)
                        {
                            for (int y = 0; y < dim; y++) { position[x, y] = Best_pos[x - N.count, y]; }
                        }
                    }


                    N.calc_nondominated(position, N.count, N.count + popsize);
                }

                c = N.count;



                for (int j = 0; j < c; j++)

                {

                    t = new Trading_MO(prices);
                    N_D = t.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    non_dominated[j, 0] = N_D[0];
                    non_dominated[j, 1] = N_D[1];
                    non_dominated[j, 2] = N_D[2];
                    Tst = new Trading_MO(tst_prices);
                    tst_results = Tst.Bollinger_trading(Convert.ToInt32(N.ndbest[j, 0]), Convert.ToInt32(N.ndbest[j, 1]), N.ndbest[j, 2]);
                    results[0] = Convert.ToInt32(N.ndbest[j, 0]).ToString();
                    results[1] = Convert.ToInt32(N.ndbest[j, 1]).ToString();
                    results[2] = Convert.ToInt32(N.ndbest[j, 2]).ToString();


                    for (int x = 0; x < N_D.GetLength(0); x++)
                    {

                        results[x + 3] = non_dominated[j, x].ToString();
                        results[x + 6] = tst_results[x].ToString();




                    }
                    dataGridView1.Rows.Add(results);
                }
            }
        }
    }
    }

