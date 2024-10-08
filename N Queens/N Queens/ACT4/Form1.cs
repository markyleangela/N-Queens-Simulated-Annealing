using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        double T = 20;
        double CoolingRate = 0.9;
        Random r = new Random();
        SixState startState;
        SixState currentState;
        SixState nextState;
        int moveCounter;

        //bool stepMove = true;

        (int, Point)[,] hTable;
        List<Point> bMoves;
        Object chosenMove;

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;
            bMoves = new List<Point>();
            startState = randomSixState();
            currentState = new SixState(startState);

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }
        private SixState randomSixState()
        {
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private double getProbability(double deltaE, double T) {
            return Math.Exp(-deltaE / T);
        }


        private void updateUI()
        {
            //pictureBox1.Refresh();
            pictureBox2.Refresh();

            //label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label4.Text = "Moves: " + moveCounter;

            //bMoves = getBestMoves(hTable.);

            listBox1.Items.Clear();
            foreach (Point move in bMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bMoves.Count > 0) chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }



        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;
            
            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }
            
            return attackers;
        }

        private (int, Point)[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            (int, Point)[,] hStates = new (int, Point)[n, n];

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = (getAttackingPairs(possible), new Point(i, j));
                }
            }

            return hStates;
        }

        private List<Point> getBestMoves(int[,] heuristicTable)
        {
            List<Point> bestMoves = new List<Point>();
            int bestHeuristicValue = heuristicTable[0, 0];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable[i, j])
                    {
                        bestHeuristicValue = heuristicTable[i, j];
                        bestMoves.Clear();
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    } else if (bestHeuristicValue == heuristicTable[i,j])
                    {
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                }
            }
            label5.Text = "Possible Moves (H="+bestHeuristicValue+")";
            return bestMoves;
        }

        private Object chooseMove(List<Point> possibleMoves)
        {
            int arrayLength = possibleMoves.Count;
            Random r = new Random();
            int randomMove = r.Next(arrayLength);

            return possibleMoves[randomMove];
        }

        private void executeMove(Point move)
        {
            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = currentState.Y[i];
            }
            currentState.Y[move.X] = move.Y;
            moveCounter++;

            chosenMove = null;
            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(currentState) > 0)
            {
                hTable = getHeuristicTableForPossibleMoves(currentState);
                (int, Point) chosenPoint = hTable[r.Next(n), r.Next(n)];

                if (chosenPoint.Item1 < getAttackingPairs(currentState))
                {
                    bMoves.Clear();
                    bMoves.Add(chosenPoint.Item2);
                    chosenMove = chosenPoint.Item2;
                    executeMove((Point)chosenMove);
                }
                else
                {
                    double deltaE = (double)chosenPoint.Item1 - (double)getAttackingPairs(currentState);
                    double acceptanceProbability = getProbability(deltaE, T);
                    double randomNum = r.NextDouble();
                    if (randomNum < acceptanceProbability)
                    {
                        bMoves.Clear();
                        bMoves.Add(chosenPoint.Item2);
                        chosenMove = chosenPoint.Item2;
                        executeMove((Point)chosenMove);
                    }
                    T *= CoolingRate;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState = new SixState(startState);

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(currentState) > 0)
            {
                hTable = getHeuristicTableForPossibleMoves(currentState);
                (int, Point) chosenPoint = hTable[r.Next(n), r.Next(n)];

                if (chosenPoint.Item1 < getAttackingPairs(currentState))
                {
                    bMoves.Clear();
                    bMoves.Add(chosenPoint.Item2);
                    chosenMove = chosenPoint.Item2;
                    executeMove((Point)chosenMove);
                }
                else
                {
                    double deltaE = (double)chosenPoint.Item1 - (double)getAttackingPairs(currentState);
                    double acceptanceProbability = getProbability(deltaE, T);
                    double randomNum = r.NextDouble();
                    if (randomNum < acceptanceProbability)
                    {
                        bMoves.Clear();
                        bMoves.Add(chosenPoint.Item2);
                        chosenMove = chosenPoint.Item2;
                        executeMove((Point)chosenMove);
                    }
                    T *= CoolingRate;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }        
    }
}
