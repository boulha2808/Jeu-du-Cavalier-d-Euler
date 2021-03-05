using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cavalier_haroun_et_romain_v2
{
    public partial class Form2 : Form
    {
        private const int TAILLE = 12;
        private const int TAILLE_BOUTON = 72;
        private int coup = 0;
        private Button[,] btnGrille = new Button[TAILLE, TAILLE];
        private List<Memoire> listeCoupSimulation;
        private List<int> listePas;
        private List<int> listeIntervalle;
        private Image imgPion= Image.FromFile("Images\\pion.png");

        private static int[,] echec = new int[12, 12];
        private static int[] depi = new int[] { 2, 1, -1, -2, -2, -1, 1, 2 };
        private static int[] depj = new int[] { 1, 2, 2, 1, -1, -2, -2, -1 };
        private int nb_fuite, min_fuite, lmin_fuite = 0;
        private int i, j, k, l;
        private int intervalle;
        private int pas = 1;
        private bool simulationLancee;
        private bool clickPlateau;
        private Form1 f1;

        public Form2()
        {
            InitializeComponent();
            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            f1 = new Form1();
            listeCoupSimulation = new List<Memoire>();
            listePas = new List<int>();
            listeIntervalle = new List<int>();
            GenererGrille();
            MiseEnformeGrille();
            initialiseEchec();
            radioButton1.Checked = true;
            textBox1.Text="0,2";
            this.Text = "Mode simulation";
            Image imgFond = Image.FromFile("Images\\Fond2.jpg");
            this.BackgroundImage = imgFond;
            this.Height = imgFond.Height;
            this.Width = imgFond.Width;
        }

        public void Grid_Button_Click(object sender, EventArgs e)
        {
            clickPlateau = true;
            MiseEnformeGrille();
            Button boutonClique = (Button)sender;
            Point location = (Point)boutonClique.Tag;
            int x = location.X;
            int y = location.Y;
            MemoriseCoupSimulation(x, y);
            initialiseEchec();
            MarqueCoupJoue(x, y);
        }

        public void MarqueCoupJoue(int x, int y)
        {
            btnGrille[x, y].Font = new Font("Arial", 26);
            btnGrille[x, y].Image = imgPion;
        }


        private void initialiseEchec()
        {
            for (i = 0; i < 12; i++)
                for (j = 0; j < 12; j++)
                {
                    if (i < 2 | i > 9 | j < 2 | j > 9)
                        echec[i, j] = -1;
                    else
                        echec[i, j] = 0;
                }
        }

        public void GenererGrille()
        {
            
            panel1.Height = panel1.Width;
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    btnGrille[i, j] = new Button();
                    btnGrille[i, j].Height = TAILLE_BOUTON;
                    btnGrille[i, j].Width = TAILLE_BOUTON;
                    btnGrille[i, j].FlatStyle = FlatStyle.Flat;
                    btnGrille[i, j].Click += Grid_Button_Click;
                    panel1.Controls.Add(btnGrille[i, j]);
                    btnGrille[i, j].Location = new Point(i * TAILLE_BOUTON, j * TAILLE_BOUTON);
                    btnGrille[i, j].Text = "";
                    btnGrille[i, j].Tag = new Point(i, j);
                    if (i < 2 || j < 2 || i > 9 || j > 9)
                        btnGrille[i, j].Visible = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)//PAUSE
        {
            panel1.Enabled = false;
            if (valideSimulationLancee())
            MessageBox.Show(this, "une pause s'impose ! On reprend quand tu cliques sur OK !",
                              "Pause de la simulation", MessageBoxButtons.OK,
                              MessageBoxIcon.Information,
                              MessageBoxDefaultButton.Button1);
            panel1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)//RELANCER
        {
            if (valideSimulationLancee())
            {
                intervalle = listeIntervalle[0];
                pas = listePas[0];
                initialiseEchec();
                if(clickPlateau)
                SimulationDepuis(listeCoupSimulation[listeCoupSimulation.Count - 2].GetLigne(), listeCoupSimulation[listeCoupSimulation.Count - 2].GetColonne(), intervalle);
                else
                    SimulationDepuis(listeCoupSimulation[listeCoupSimulation.Count - 1].GetLigne(), listeCoupSimulation[listeCoupSimulation.Count - 1].GetColonne(), intervalle);
            }
        }

        private void button3_Click(object sender, EventArgs e)//LANCER
        {
            simulationLancee = true;
            button3.Enabled = false;
            panel1.Enabled = false;
            listePas.Clear();
            listeIntervalle.Clear();
            initialiseEchec();
            if (VerifSaisieIntervalle())
            {
                if(clickPlateau)
                    SimulationDepuis(listeCoupSimulation[listeCoupSimulation.Count - 1].GetLigne(), listeCoupSimulation[listeCoupSimulation.Count - 1].GetColonne(), intervalle);
                else
                {
                    CoupHasard();
                    SimulationDepuis(listeCoupSimulation[listeCoupSimulation.Count - 1].GetLigne(), listeCoupSimulation[listeCoupSimulation.Count - 1].GetColonne(), intervalle);
                }

                listePas.Add(pas);
                listeIntervalle.Add(intervalle);
                button3.Enabled = true;
                clickPlateau = false;
            }
            panel1.Enabled = true;
        }

        private bool VerifSaisieIntervalle()
        {
            String saisie = textBox1.Text.Replace(".", ",");
            Double secondes;
            if (Double.TryParse(saisie, out secondes))
            {
                intervalle = (int)(secondes * 1000);
                return true;
            }
            else if (textBox1.Text.Equals(""))
            {
                textBox1.Text = "0,2";
                intervalle = 200;
                return true;
            }
            else MessageBox.Show(this, "On a dit des secondes !",
                  "Erreur de saisie", MessageBoxButtons.OK,
                  MessageBoxIcon.Exclamation,
                  MessageBoxDefaultButton.Button1);
            textBox1.Text = "";
            button3.Enabled = true;
            return false;
        }

        private void CoupHasard()
        {
            Random r = new Random();
            int x = r.Next(2, 10);
            int y = r.Next(2, 10);
            MemoriseCoupSimulation(x, y);
        }

        public void CoupSimulation(int x, int y, int coupAffiche)
        {
                for (int i = 0; i < TAILLE; i++)
                    for (int j = 0; j < TAILLE; j++) {
                        btnGrille[i, j].Image = null;
                        btnGrille[i, j].Enabled = true;
                    }
                Application.DoEvents();

            if (coupAffiche > 0)
            {
                btnGrille[x, y].Font = new Font("Arial", 26);
                btnGrille[x, y].Text = coupAffiche.ToString();
                btnGrille[x, y].BackColor = Color.IndianRed;
                btnGrille[x, y].ForeColor = Color.White;
                btnGrille[x, y].Image = imgPion;
                Application.DoEvents();
            }

        }


        private void radioButton1_Click(object sender, EventArgs e)
        {
            pas = 1;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            pas = 5;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            pas = 1;
            textBox1.Text = "0";
        }

        public void MiseEnformeGrille()
        {
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < TAILLE; j++)
                {
                    btnGrille[i, j].Text = "";
                    btnGrille[i, j].Image = null;
                    if ((i % 2 == 1 && j % 2 == 1) || (i % 2 == 0 && j % 2 == 0))
                        btnGrille[i, j].BackColor = Color.White;
                    else
                        btnGrille[i, j].BackColor = Color.Black;
                }
            }
        }

        private void MemoriseCoupSimulation(int x, int y)
        {
            Memoire m = new Memoire(x, y);
            listeCoupSimulation.Add(m);
        }

        private void ParcourSimulation(int ii, int jj, int ml)
        {
            int x, y;
            x = listeCoupSimulation[listeCoupSimulation.Count-1].GetLigne();
            y = listeCoupSimulation[listeCoupSimulation.Count - 1].GetColonne();

            ++coup;
            i = ii; j = jj;
            echec[i, j] = 1;

            for (k = 2; k <= 64; k++)
            {
                for (l = 0, min_fuite = 11; l < 8; l++)
                {
                    ii = i + depi[l]; jj = j + depj[l];

                    nb_fuite = echec[ii, jj] != 0 ? 10 : Fuite(ii, jj);

                    if (nb_fuite < min_fuite)
                    {
                        min_fuite = nb_fuite;
                        lmin_fuite = l;
                    }
                }
                if (min_fuite == 9 & k != 64)
                {
                    MessageBox.Show(this, "Impasse",
                               "Impasse", MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
                    break;
                }
                i += depi[lmin_fuite];
                j += depj[lmin_fuite];
                echec[i, j] = k;
                ++coup;
                if(radioButton2.Checked)
                afficheCoupPasdeCinq(ml);
                else
                {
                    System.Threading.Thread.Sleep(ml);
                    CoupSimulation(i, j, coup);
                }

            }
            MemoriseCoupSimulation(x, y);
        }

        private bool valideSimulationLancee()
        {
            if (simulationLancee)
                return true;
            else
                MessageBox.Show(this, "Aucune simulation n'a été lancé",
                  "Déjà la pause !", MessageBoxButtons.OK,
                  MessageBoxIcon.Warning,
                  MessageBoxDefaultButton.Button1);
            return false;
        }

        private void afficheCoupPasdeCinq(int ml)
        {
            if (coup % pas == 0){
                if (listeCoupSimulation.Count > 0){
                    System.Threading.Thread.Sleep(ml);
                    CoupSimulation(i, j, coup);
                    for (int k = 0; k < listeCoupSimulation.Count; ++k)
                        CoupSimulation(listeCoupSimulation[k].GetLigne(), listeCoupSimulation[k].GetColonne(), coup - listeCoupSimulation.Count + k);
                    listeCoupSimulation.Clear();
                }
                else
                {
                    System.Threading.Thread.Sleep(ml);
                    CoupSimulation(i, j, coup);
                }

            }
            else
                MemoriseCoupSimulation(i, j);
            if (64 - coup < pas)
            {
                for (int k = 0; k < listeCoupSimulation.Count; ++k)
                    CoupSimulation(listeCoupSimulation[k].GetLigne(), listeCoupSimulation[k].GetColonne(), coup - listeCoupSimulation.Count + k);
                CoupSimulation(i, j, coup);
                listeCoupSimulation.Clear();
            }
        }

        static int Fuite(int i, int j)
        {
            int n, l;

            for (l = 0, n = 8; l < 8; l++)
                if (echec[i + depi[l], j + depj[l]] != 0)
                    n--;

            return (n == 0) ? 9 : n;
        }

        private void SimulationDepuis(int x, int y , int ms)
        {
            MiseEnformeGrille();
            Application.DoEvents();
            coup = 0;
            btnGrille[x, y].BackColor = Color.IndianRed;
            btnGrille[x, y].Font = new Font("Arial", 26);
            btnGrille[x, y].Text = "1";
            ParcourSimulation(x, y, ms);
            MessageBox.Show(this, "Et voilà ! Simple, n'est-ce-pas ?",
                   "Démonstration terminée", MessageBoxButtons.OK,
                   MessageBoxIcon.Question,
                   MessageBoxDefaultButton.Button1);
            MiseEnformeGrille();
        }

    }
}
