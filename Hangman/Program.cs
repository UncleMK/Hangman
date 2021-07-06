using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Hangman
{
    class Program
    {
        static void Main(string[] args)
        {
            string pat = @"countries_and_capitals.txt";
            string outS;
            string wrPat = @"hightscore.txt";

            List<string> counCap = new List<string>();
            List<string> hscore = new List<string>();
            int minTime = 99999, minTry = 9999;

            try
            {
                using StreamReader sr = new StreamReader(wrPat, System.Text.Encoding.Default);
                string line;
                while ((line = sr.ReadLine()) != null) hscore.Add(line);
                sr.Close();
            }
            catch(Exception) { }

            using (StreamReader sr = new StreamReader(pat, System.Text.Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] alls = line.Split('|');
                        outS = alls[0].Trim() + '|' + alls[1].Trim();
                        counCap.Add(outS);
                    }
                }

            string[] pics = {"  +---+\n  |   |\n      |\n      |\n      |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n      |\n      |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n  |   |\n      |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n /|   |\n      |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n /|\\  |\n      |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n /|\\  |\n /    |\n      |\n=========",
                            "  +---+\n  |   |\n  O   |\n /|\\  |\n / \\  |\n      |\n=========" };

            var rand = new Random();
            int max = counCap.Count + 1;
            int maxLife = 7;
            int pLife = maxLife;
            bool game = true, pwin, found;
 
            while (game)
            {
                game = false;
                pwin = false;
                List<char> usLet = new List<char>();
                int num = rand.Next(max);
                string[] ss = counCap[num].Split('|');
                string capit = ss[1];
                string countr = ss[0];
                string inp;
                int countLet = 0;

                char[] capR = capit.ToCharArray();
                char[] capN = new char[capR.Length];
                for (int i = 0; i < capR.Length; i++)
                {
                    if (capR[i] == ' ') capN[i] = ' ';
                    else capN[i] = '_';
                }

                if (hscore.Count > 0)
                {
                    string[] line = hscore[^1].Split("|");
                    minTime = int.Parse(line[2]);
                    minTry = int.Parse(line[3]);
                }

//                Console.WriteLine(capit);
                Stopwatch stopWatch = new Stopwatch(); ;

                while (pLife > 0 && !pwin)
                {
                    //міряєм час
                    stopWatch.Start();
                    found = false;
                    Console.WriteLine($"You have {pLife} lives: ");
                    if (pLife < maxLife) Console.WriteLine(pics[maxLife - pLife - 1]);
                    Console.WriteLine(Konvch(capN));
                    if (usLet.Count>0) Console.WriteLine("not in word: " + String.Join(", ", usLet));
                    if (pLife == 1) Console.WriteLine($"The capital of {countr}");
                    Console.WriteLine("Write letter or the whole word: ");
                    inp = Console.ReadLine().ToLower();

                    if (inp.Length > 1)
                    {
                        if (inp != capit.ToLower()) pLife -= 2;
                        else
                        {
                            pwin = true;
                            stopWatch.Stop();
                            for (int i = 0; i < capR.Length; i++) capN[i] = capR[i];
                        }
                    }
                    else if (inp.Length == 1)
                    {
                        char inpt = char.Parse(inp);
                        if (!usLet.Contains(inpt))
                        {
                            bool f = false;
                            foreach (char c in capN)
                            {
                                if (inp.Equals(c.ToString().ToLower())) f = true;
                            }
                            if (!f) countLet++;
                        }
                        if (!usLet.Contains(inpt))
                        {
                            for (int i = 0; i < capR.Length; i++)
                            {
                                    if (inp.Equals(capR[i].ToString().ToLower()))
                                    {
                                        capN[i] = capR[i];
                                        found = true;
                                    }                                
                            }
                            if (found)
                            {
                                string rezult = Konvch(capN);
                                if (rezult == capit)
                                {
                                    pwin = true;
                                    stopWatch.Stop();
                                }
                            }
                            else
                            {
                                pLife--;
                                usLet.Add(inpt);
                            }
                        }
                        else Console.WriteLine("You have already write this letter");
                    }
                }
                Console.WriteLine(Konvch(capN));
                if (pwin)
                {
                    var rTime = stopWatch.ElapsedMilliseconds / 1000;
                    Console.WriteLine($"You guessed the capital after {countLet} letters. It took you {rTime} seconds");

                    //якщо потрібний перезапис хайскор
                    if (stopWatch.ElapsedMilliseconds / 1000 < minTime || (stopWatch.ElapsedMilliseconds / 1000 == minTime && countLet < minTry) || hscore.Count < 10)
                        {
                        Console.WriteLine("Input your name");
                        string nAme = Console.ReadLine();

                        //записуємо таблицю рекордів
                        string hs = nAme + "|" + DateTime.Now + "|" + rTime + "|" + countLet + "|" + capit;
                        if (hscore.Count < 10) hscore.Add(hs);
                        else hscore[9] = hs;
                        SortHS(hscore);
                        //записуємо на диск
                        using StreamWriter sw = new StreamWriter(wrPat, false, System.Text.Encoding.Default);
                        foreach (string s in hscore) sw.WriteLine(s);
                        sw.Close();
                    }
                    ShowHighScore(hscore);
                    Console.WriteLine("Do you want to play again? Yes|/No?");
                }
                else
                {
                    Console.WriteLine("You didn't guess the capital, do you want to play again? Yes|/No?");
                }

                string cont = "";
                while (!cont.ToLower().Equals("yes") && !cont.ToLower().Equals("no"))
                {
                    cont = Console.ReadLine().ToLower();
                }
                if (cont.ToLower().Equals("yes"))
                {
                    game = true;
                    pLife = maxLife;
                }
            }
        }

        private static void SortHS(List<string> hscore)
        {
            bool sorted = true;
            while (sorted)
            {
                sorted = false;
                for (int i = hscore.Count - 1; i >= 1; i--)
                {
                    string[] line1 = hscore[i].Split("|");
                    string[] line0 = hscore[i - 1].Split("|");
                    //порівнюємо час, або якщо час = то тоді кількість букв
                    if (int.Parse(line0[2]) > int.Parse(line1[2]) || (int.Parse(line0[2]) == int.Parse(line1[2]) && (int.Parse(line0[3]) > int.Parse(line1[3])))){
                        string tmp = hscore[i];
                        hscore[i] = hscore[i - 1];
                        hscore[i - 1] = tmp;
                        sorted = true;
                    }
                }

            }
        }
        private static string Konvch(char[] c)
        {
            string outp = "";
            foreach (char d in c) outp += d;
            return outp;
        }
        private static void ShowHighScore(List<string> hs)
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("|{0,15} |{1,20} |{2,5} |{3,3} |{4,-17}|", "Name", "Date", "Time", "Try", "Capital");
            Console.WriteLine("----------------------------------------------------------------------");
            int max = 10;
            if (hs.Count < 10) max = hs.Count;
            for (int i = 0; i < max; i++)
            {
                string[] line = hs[i].Split("|");
                Console.WriteLine("|{0,15} |{1,20} |{2,5} |{3,3} |{4,-17}|", line[0], line[1], line[2], line[3], line[4]);
            }
            Console.WriteLine("----------------------------------------------------------------------");
        }
    }
}
