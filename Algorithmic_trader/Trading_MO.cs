using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithmic_trader
{
    class Trading_MO
    {
        double sum;
       static double[] prices;
        double initial_deposite = 100000;
        double[] fit = new double[3];

        
        public Trading_MO(double[] Prices1)
        {
            prices = Prices1;
        }
        public Trading_MO()
        { }
        public double[] sma(int n)
        {
            double[] sma = new double[prices.Length];
           
            for (int i = n - 1; i < prices.Length; i++)
            {
                sum = 0;
                for (int j = i - n + 1; j <= i; j++)
                { sum += prices[j]; }

                sma[i] = sum / n;
            }
            return sma;
        }

        ///////////////////////////////////////////////////////////


        public double[] EMA(int n_ema)
        {
            double sum = 0;
            double currentweight = 0, weightma = 0;
            double[] ema = new double[prices.Length];
            if (n_ema == 0)
            { for (int i = 0; i < prices.Length; i++) { ema[i] = prices[i]; } }
            else
            {
                for (int f = 0; f <= n_ema - 1; f++) { sum += prices[f]; }
                for (int f = 1; f <= n_ema - 1; f++) { ema[f - 1] = 0; }
                ema[n_ema - 1] = sum / n_ema;
                for (int i = n_ema; i <= prices.Length - 1; i++)
                {
                    currentweight = (2 / ((double)n_ema + 1));
                    weightma = 1 - currentweight;
                    ema[i] = (ema[i - 1] * weightma) + (prices[i] * currentweight);
                }
            }
            return ema;


        }

       

        public double[] STD(int n, double[] array)         //// calculate standard deviation
        {
            // sum = 0;
            double dev_sum = 0;
            double diff;
            double[] mean = new double[array.Length];
            double[] S_DEV = new double[array.Length];

            for (int i = n - 1; i < array.Length; i++)
            {
                diff = 0;
                sum = 0;
                for (int j = i - n + 1; j <= i; j++)
                { sum += array[j]; }

                mean[i] = sum / n;
                for (int j = i - n + 1; j <= i; j++)
                {
                    diff = array[j] - mean[i];

                    diff = Math.Pow(diff, 2);
                    dev_sum += diff;
                }
                S_DEV[i] = dev_sum / n;
                S_DEV[i] = Math.Sqrt(S_DEV[i]);



            }
            return S_DEV;


        }

        public double[,] Bollinger(int n_ma, int n_lookback, double multiplier)    // n_ma number of days for MA , number of days for look back period , std multiplier
        {
            double[,] B_bands = new double[prices.Length, 3];  // array here is the prices array
                                                               // B_bands contains middle , upper, and lower bands consequtively.
            double[] middle = sma(n_ma);
            double[] SD = STD(n_lookback, prices);
            for (int i = 0; i < prices.Length; i++)
            {
                B_bands[i, 0] = middle[i];
                B_bands[i, 1] = middle[i] + multiplier * SD[i];
                B_bands[i, 2] = middle[i] - multiplier * SD[i];


            }
            return B_bands;
        }

        //public double[] Bollinger_trading(int n_ma, int n_lookback, double multiplier)
        //{

        //    double[,] bands = Bollinger(n_ma, n_lookback, multiplier);
        //    List<double> ret = new List<double>();
        //    int buy = 0;
        //    int sell = 0;
        //    double deposite = initial_deposite;
        //    double cost = 0, ROI = 0;
        //    int shares = 0;
        //    double buying_price = 0;
        //    double selling_price = 0;
        //    double prof;

        //    for (int i = n_ma; i < prices.Length; i++)
        //    {
        //        if ((prices[i] <= bands[i, 0]) && (buy == 0))
        //        {
        //            buy = 1;
        //            shares = (int)(deposite / prices[i]);
        //            deposite = deposite - shares * prices[i];
        //            cost = cost + shares * prices[i];
        //            buying_price = shares * prices[i];

        //        }
        //        else if ((prices[i] >= bands[i, 1]) && buy == 1)
        //        {
        //            sell = 1;
        //            deposite = deposite + shares * prices[i];
        //            buy = sell = 0;

        //            selling_price = prices[i] * shares;

        //            prof = ((selling_price - buying_price) / buying_price) * 100;
        //            ret.Add(prof);

        //        }
        //        else if (i == prices.Length - 1 && buy == 1 && sell != 1)
        //        {
        //            deposite = deposite + shares * prices[i];
        //            selling_price = prices[i] * shares;

        //            prof = ((selling_price - buying_price) / buying_price) * 100;
        //            ret.Add(prof);
        //        }

        //    }

        //    ROI = ((deposite - initial_deposite) / initial_deposite) * 100;

        //    double SR = Sortino(ret.ToArray());
        //    double PNP = prof_to_nonprof(ret.ToArray());
        //    fit[0] = ROI;
        //    fit[1] = SR;
        //    fit[2] = ret.Count;

        //    return fit;



        //}





        public double[] Bollinger_trading(int n_ma, int n_lookback, double multiplier)
        {

            double[,] bands = Bollinger(n_ma, n_lookback, multiplier);
            List<double> ret = new List<double>();
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            double buying_price = 0;
            double selling_price = 0;
            double prof;

            for (int i = n_ma; i < prices.Length; i++)
            {
             if ((prices[i] <= bands[i, 0]) && (buy == 0))    //   if  ((prices[i] < prices[i - 1]) && (prices[i] <= bands[i, 0] * 1.1) && (buy == 0))          //
                {
                    buy = 1;
                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - shares * prices[i];
                    cost = cost + shares * prices[i];
                    buying_price = shares * prices[i];

                }
                else if ((prices[i] >= bands[i, 1]) && buy == 1)   // else if ((prices[i] > prices[i - 1]) && (prices[i] >= bands[i, 1] * 1.1) && buy == 1)//
                {
                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;
                    // ret.Add(deposite);
                    selling_price = prices[i] * shares;
                    // prof = selling_price - buying_price;
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);

                }
                else if (i == prices.Length - 1 && buy == 1 && sell != 1)
                {
                    deposite = deposite + shares * prices[i];
                    selling_price = prices[i] * shares;
                    // prof = selling_price - buying_price;
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);
                }
                // ret.Add(deposite);
            }
            // ROI = ((deposite - initial_deposite) / cost) * 100;
            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;// cost) * 100

            double SR = Sortino(ret.ToArray());
            double PNP = prof_to_nonprof(ret.ToArray());
            fit[0] = ROI;
            fit[1] = SR;
            fit[2] = ret.Count;

            return fit;



        }




        public double[] RSI(int n_rsi)   //// relative strength index
        {
            double avggain, avgloss, prevgain = 0, prevloss = 0;
            double[] rsi; double[] rs;


            rsi = new double[prices.Length];
            rs = new double[prices.Length];

            
            for (int i = 1; i < n_rsi; i++)
            {

                double diff = 0;
                diff = prices[i] - prices[i - 1];
                if (diff > 0) { prevgain += diff; }
                else if (diff < 0) { prevloss += Math.Abs(diff); }


            }
            rs[n_rsi] = prevgain / prevloss;
            rsi[n_rsi] = 100 - (100 / (1 + rs[n_rsi]));
            prevgain = prevgain / n_rsi;
            prevloss = prevloss / n_rsi;

            for (int i = n_rsi; i < prices.Length - 1; i++)

            {

                double diff = 0, newgain = 0, newloss = 0;
                diff = prices[i + 1] - prices[i];
                if (diff > 0) { newgain = diff; }
                else if (diff < 0) { newloss = Math.Abs(diff); }
                avggain = ((prevgain * (n_rsi - 1)) + newgain) / n_rsi;
                avgloss = ((prevloss * (n_rsi - 1)) + newloss) / n_rsi;
                rs[i + 1] = avggain / avgloss;
                rsi[i + 1] = 100 - (100 / (1 + rs[i + 1]));
                prevgain = avggain;
                prevloss = avgloss;

            }
            return rsi;


        }



        public double[] stoch_RSI(int n_rsi, int n_stoch)    ////// stochastic rsi   
        {
            double max, min;
            double[] rsi = RSI(n_rsi);
            double[] S_RSI = new double[rsi.Length];
            for (int i = 0; i < rsi.Length - n_stoch + 1; i++)
            {
                max = rsi[i];
                min = rsi[i];
                for (int j = i; j < i + n_stoch; j++)
                {

                    if (rsi[j] < min)
                        min = rsi[j];
                    else if (rsi[j] > max)
                        max = rsi[j];

                }
                S_RSI[i] = 100 * (rsi[i] - min) / (max - min);

            }
            return S_RSI;


        }

        public double[] stoch_RSI_trading(int n, int n_stoc, double low_boundary, double upper_boundary)
        {
            double[] S_RSI = stoch_RSI(n, n_stoc);
            List<double> ret = new List<double>();
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            int[] x = new int[S_RSI.Length];   // to calculate the difference for each rsi or stoch - rsi daily values(-1 in the low, +1 above high, and 0 in between the two limits

            int[] d = new int[S_RSI.Length];      //every d =x[i]-x[i-1]  // +1 buy and -1 sell
            double buying_price = 0;
            double selling_price = 0;
            double prof = 0;
            for (int i = n_stoc + 1; i < S_RSI.Length; i++)
            {
                if (S_RSI[i] < low_boundary) x[i] = -1;
                else if (S_RSI[i] > upper_boundary) x[i] = +1;
                else x[i] = 0;
                if (i > n_stoc + 2)
                {
                    d[i] = x[i] - x[i - 1];
                }
                ///////////buy conditions
                if (d[i] == 1 && buy == 0)
                {
                    buy = 1;
                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - (shares * prices[i]);
                    buying_price = shares * prices[i];
                    cost = cost + shares * prices[i];

                }
                /////// sell conditions
                else if (d[i] == -1 && buy != 0)
                {
                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;
                    selling_price = shares * prices[i];
                    
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    
                    ret.Add(prof);
                }

                if ((i == S_RSI.Length - 1) && (buy == 1) && (sell == 0))
                {
                    deposite = deposite + shares * prices[i];
                    selling_price = shares * prices[i];
                  
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    
                    ret.Add(prof);

                    
                }

            }
            double SR = Sortino(ret.ToArray());
            double PNP = prof_to_nonprof(ret.ToArray());
            
            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;

            fit[0] = ROI;
            fit[1] = SR;
            fit[2] = ret.Count;

            return fit;


        }

        public double[] SROC_trading(int n_ema, int n_roc, double OB_ref, double OS_ref)   //////smoothed rate of change   ,   OB (overbought reference (upper ref) ,  OS (oversold reference (lower ref)
        {
            double[] ema = EMA(n_ema);
            double[] es_roc = new double[prices.Length]; // exponentially smoothed rate of change
            double prev_roc;
            List<double> ret = new List<double>();
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            double SR, PNP;
            double buying_price = 0, selling_price = 0, prof = 0;

            for (int i = n_roc; i < prices.Length; i++)
            {
                prev_roc = ema[i - n_roc];
                es_roc[i] = ((ema[i] - prev_roc) / prev_roc) * 100;

            }
            for (int i = n_roc+n_ema-1; i < prices.Length; i++)
            {

                if ((es_roc[i] < es_roc[i - 1]) && (es_roc[i] < OS_ref) && buy == 0)
                {
                    buy = 1;
                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - (shares * prices[i]);
                    cost = cost + shares * prices[i];
                    buying_price = shares * prices[i];


                }
                else if (((es_roc[i] > es_roc[i - 1]) && es_roc[i] > OB_ref) && buy == 1)
                {

                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;
                   
                    selling_price = prices[i] * shares;
                   
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);
                }
                else if ((i == prices.Length - 1) && (buy == 1))
                {
                    deposite = deposite + shares * prices[i];
                   
                    selling_price = prices[i] * shares;
                    
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);

                }
                /////////////////
            }
            SR = Sortino(ret.ToArray());
            PNP = prof_to_nonprof(ret.ToArray());
          
            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;
            double yrs = 365.0 / (double)prices.Length;
            double annual_ret =((Math.Pow((deposite / initial_deposite), yrs)) - 1)*100;
            
                fit[0] = ROI;
                fit[1] = SR;
                fit[2] = ret.Count;
           
           
            return fit;
        }

        /// <summary>
        /// ////////////
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>

        public double[] S_ROC_trading(int n_ema, int n_roc, double OB_ref, double OS_ref)   //////smoothed rate of change   ,   OB (overbought reference (upper ref) ,  OS (oversold reference (lower ref)
        {
            double[] ema = EMA(n_ema);
            double[] es_roc = new double[prices.Length]; // exponentially smoothed rate of change
            double prev_roc;
            List<double> ret = new List<double>();
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            double SR, PNP;
            double buying_price = 0, selling_price = 0, prof = 0;

            for (int i = n_roc; i < prices.Length; i++)
            {
                prev_roc = ema[i - n_roc];
                es_roc[i] = ((ema[i] - prev_roc) / prev_roc) * 100;

            }
            for (int i = n_roc; i < prices.Length; i++)
            {

                if ((es_roc[i] < es_roc[i - 1]) && (es_roc[i] < OS_ref) && buy == 0)
                {
                    buy = 1;
                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - (shares * prices[i]);
                    cost = cost + shares * prices[i];
                    buying_price = shares * prices[i];


                }
                else if (((es_roc[i] > es_roc[i - 1]) && es_roc[i] > OB_ref) && buy == 1)
                {

                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;

                    selling_price = prices[i] * shares;

                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);
                }
                else if ((i == prices.Length - 1) && (buy == 1))
                {
                    deposite = deposite + shares * prices[i];

                    selling_price = prices[i] * shares;

                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);

                }
                /////////////////
            }
            SR = Sortino(ret.ToArray());
            PNP = prof_to_nonprof(ret.ToArray());

            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;
            double yrs = 365.0 / (double)prices.Length;
            double annual_ret = ((Math.Pow((deposite / initial_deposite), yrs)) - 1) * 100;

            fit[0] = ROI;
            fit[1] = SR;
            fit[2] = ret.Count;


            return fit;
        }


        /// <summary>
        /// /////////
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public double[] WMA(int n)  /////weighted moving average
        {
            double sum = 0;

            double[] wma = new double[prices.Length];

            int k;
           
            for (int i = n - 1; i < prices.Length; i++)
            {
                k = 1;
                sum = 0;
                for (int j = i - n + 1; j <= i; j++)
                {


                    sum += prices[j] * k;
                    k++;
                }               
                wma[i] = 2 * sum / (n * (n + 1));
            }
            return wma;


        }

        public double[] WMA_trading(int n_ema)
        {

            int[] s1 = new int[prices.Length];
            int[] f1 = new int[prices.Length];
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            List<double> ret = new List<double>();   // a list of returns
            double buying_price = 0;
            double selling_price = 0;
            double prof = 0;
            double[] wma = WMA(n_ema);   
            f1[0] = 0;

            for (int i = 0; i <= prices.Length - 1; i++)
            {
                if (prices[i] > wma[i]) { s1[i] = 1; }
                else if (prices[i] <= wma[i]) { s1[i] = -1; }
            }
            for (int i = 1; i <= prices.Length - 1; i++) { f1[i] = s1[i] - s1[i - 1]; }
        
            for (int i = 0; i <= prices.Length - 1; i++)
            {
                
                if ((f1[i] == 2) && (buy == 0))
                {
                    buy = 1;
                    
                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - (shares * prices[i]);
                    cost = cost + shares * prices[i];
                    buying_price = shares * prices[i];

                }
                else if ((f1[i] == -2) && (buy != 0))
                {
                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;
                    selling_price = shares * prices[i];
                    prof = ((selling_price - buying_price)/buying_price)*100;
                    ret.Add(prof);

                }
                else if ((i == prices.Length - 1) && (buy == 1))
                {
                    deposite = deposite + shares * prices[i];

                    selling_price = shares * prices[i];
 
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);
                }
            }
            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;

            /////////////////

            double SR = Sortino(ret.ToArray());
            double PNP = prof_to_nonprof(ret.ToArray());
            double trades = ret.Count;
            ////////////////

            fit[0] = ROI;
            fit[1] = SR;
            fit[2] = trades;

            return fit;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="returns"></param>
        /// <returns></returns>
        public double[] WMA_trading(int n_ema,int n_ema2)
        {

            int[] s1 = new int[prices.Length];
            int[] f1 = new int[prices.Length];
            int buy = 0;
            int sell = 0;
            double deposite = initial_deposite;
            double cost = 0, ROI = 0;
            int shares = 0;
            List<double> ret = new List<double>();   // a list of returns
            double buying_price = 0;
            double selling_price = 0;
            double prof = 0;
            double[] wma = WMA(n_ema);
            double[] wma2 = WMA(n_ema2);
            f1[0] = 0;

            for (int i = 0; i <= prices.Length - 1; i++)
            {
                if (wma2[i] > wma[i]) { s1[i] = 1; }
                else if (wma2[i] <= wma[i]) { s1[i] = -1; }
            }
            for (int i = 1; i <= prices.Length - 1; i++) { f1[i] = s1[i] - s1[i - 1]; }

            for (int i = 0; i <= prices.Length - 1; i++)
            {

                if ((f1[i] == 2) && (buy == 0))
                {
                    buy = 1;

                    shares = (int)(deposite / prices[i]);
                    deposite = deposite - (shares * prices[i]);
                    cost = cost + shares * prices[i];
                    buying_price = shares * prices[i];

                }
                else if ((f1[i] == -2) && (buy != 0))
                {
                    sell = 1;
                    deposite = deposite + shares * prices[i];
                    buy = sell = 0;
                    selling_price = shares * prices[i];
                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);

                }
                else if ((i == prices.Length - 1) && (buy == 1))
                {
                    deposite = deposite + shares * prices[i];

                    selling_price = shares * prices[i];

                    prof = ((selling_price - buying_price) / buying_price) * 100;
                    ret.Add(prof);
                }
            }
            ROI = ((deposite - initial_deposite) / initial_deposite) * 100;

            /////////////////

            double SR = Sortino(ret.ToArray());
            double PNP = prof_to_nonprof(ret.ToArray());
            double trades = ret.Count;
            ////////////////

            fit[0] = ROI;
            fit[1] = SR;
            fit[2] = trades;

            return fit;
        }
        


        /// <returns></returns>
        /// 

        public double Sortino(double[] returns)
        {
            double mean, SR, sum = 0;
            List<double> neg_ret = new List<double>();
            double diff, S_DEV = 1, dev_sum = 0;
            int count = 0;
            if (returns.Length == 0) SR = 0; 
            else if (returns.Length > 1)
            {
                for (int i = 0; i < returns.Length; i++)
                {
                    sum = sum + returns[i];
                }
                mean = sum / returns.Length;
                for (int i = 0; i < returns.Length; i++)
                {
                    if (returns[i] < mean)
                    {

                        count++;
                        diff = returns[i] - mean;

                        diff = Math.Pow(diff, 2);
                        dev_sum += diff;
                    }
                    S_DEV = dev_sum / count;
                    S_DEV = Math.Sqrt(S_DEV);
                }
                SR = mean / S_DEV;
            }
            else if (returns[0] > 0) SR = 3;   // single +ve return
            else SR = -3;   // single -ve return
            return SR;

        }

        /// <summary>
        /// /////
        /// </summary>
        /// <param name="returns"></param>
        /// <returns></returns>



        public double prof_to_nonprof(double[] returns)
        {
            //int profitable = 0, non_prof = 0;
            //double prof=0, nonp = 0;
            //double avg_prof = 0, avg_np = 0;
            //// double perc;
            //for (int i = 0; i < returns.Length; i++)
            //{
            //    if (returns[i] > 0)
            //    {
            //        profitable++;
            //        prof = prof + returns[i];
            //    }
            //    else
            //    {
            //        non_prof++;
            //        nonp = nonp + returns[i];
            //    }
            //}
            //avg_prof = prof / profitable;
            //avg_np = nonp / non_prof;
            //if (non_prof != 0)
            //    return avg_prof / avg_np;
            //else return 100;

            ////if (non_prof != 0) return 100.0 * (double)profitable / (double)non_prof;
            ////else return 100;
            double a = returns.Length;
            double trades = 1 / a;
            if (a > 0) return (trades);
            else return 0;


        }
        ////  public double NP_to_MDD() 
        //{// net profit to max drawdown
        //  }








    }
}
