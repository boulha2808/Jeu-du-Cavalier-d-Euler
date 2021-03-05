using System;
using System.Collections;
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
    public partial class Form1 : Form
    {
        private const int TAILLE = 12;
        private int coup = 0;
        private int max;
        private int coupAnnules;
        private int cptMemoireLigne = 0;
        private Button[,] btnGrille = new Button[TAILLE, TAILLE];
        private List<Memoire> MemoireCoup;
        private Image imgPion = Image.FromFile("Images\\pion.png");
        private Image imgCheck = Image.FromFile("Images\\old_Check-vert.png");
        private static int[,] echec = new int[12, 12];
        private static int[] depi = new int[] { 2, 1, -1, -2, -2, -1, 1, 2 };
        private static int[] depj = new int[] { 1, 2, 2, 1, -1, -2, -2, -1 };
        private int nb_fuite, min_fuite, lmin_fuite = 0;
        private int i, j, k, l;
        public Form2 f2;
        

        public Form1()
        {
            InitializeComponent();
            GenererGrille();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            MemoireCoup = new List<Memoire>();
            this.Text = "Jeu du Cavalier";
            button1.Text = "ANNULER\n le dernier coup\n (" + coupAnnules.ToString() + "/5)";
            button1.Font = new Font("Arial", 13);
            button2.Text = "REJOUER";
            button2.Font = new Font("Arial", 13);
            button3.Text = "JOUER \n à partir d'une case tirée au hasard";
            button3.Font = new Font("Arial", 13);
            button4.Text = "Jouer la solution";
            button4.Font = new Font("Arial", 13);
            button5.Text = "MODE SIMULATION";
            button5.Font = new Font("Arial", 13);
            label1.Font = new Font("Arial", 13);
            label2.Font = new Font("Arial", 13);
            label1.Text = "";
            label2.Text = "";
            button6.BackColor = Color.White;
            button7.BackColor = Color.Black;
            button8.BackColor = Color.IndianRed;
            Image imgFond= Image.FromFile("Images\\Fond2.jpg");
            this.BackgroundImage = imgFond;
            this.Height = imgFond.Height;
            this.Width = imgFond.Width;
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            MiseEnformeGrille();
            CacheBoutonsCouleurs();
            initialiseEchec();
        }

        private void LienWikipédiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://fr.wikipedia.org/wiki/Probl%C3%A8me_du_cavalier";
            System.Diagnostics.Process.Start(targetURL);

        }

        private void AstuceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = "La méthode à utiliser, basée sur une heuristique due à Euler, consiste à choisir comme case de fuite, en partant de l'étape N, la case de l'étape N+1 qui, à l'étape N+2, présente le MINIMUM de case de fuites possibles. Si l'on applique cette méthode dès le départ, cela revient à choisir n’importe quelle case comme case de départ. Avec cette méthode, on est sûr (selon Euler !) de parcourir l'ensemble de l'échiquier.";
            string title = "Astuce";
            MessageBox.Show(message, title, MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1);
        }


        private void A_ProposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = "Réalisé par Romain Bourges et Haroun Bouloudani";
            string title = "A propos";
            MessageBox.Show(this, message,
                            title, MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1);

        }



        public void Grid_Button_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button4.Enabled = true;
            this.coup++;
            if (coup == 1)
                this.choisirLesCouleursToolStripMenuItem.Enabled = false;
            if(coup>1)
                button1.Enabled = true;

            Button boutonClique = (Button)sender;
            Point location = (Point)boutonClique.Tag;

            int x = location.X;
            int y = location.Y;

            MemoriseCoup(x, y);

            if (max <= coup)
                max = coup;
            MarquerProchainsCoupsAutorises(x, y);
            MarqueCoupJoue(x, y);
            FinDePartie();
        }

        public void Button1_Click(object sender, EventArgs e)//ANNULER
        {
            
            coup--;
            coupAnnules += 1;
            if (coupAnnules > 5)
            {
                MessageBox.Show(this, "Vous ne pouvez plus revenir en arrière",
                   "Attention", MessageBoxButtons.OK,
                   MessageBoxIcon.Exclamation,
                   MessageBoxDefaultButton.Button1);
                coup++;
            }

            else
            {
                if (cptMemoireLigne - coupAnnules < 0 || coup <= 0)
                {
                    MessageBox.Show(this, "Vous ne pouvez plus revenir plus loin que le point de départ",
                        "Attention", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                    coupAnnules--;
                    coup++;
                }
                else
                {
                    InitialiseProchainCoup();
                    int ligneActuelle = MemoireCoup.Last().GetLigne();
                    int colonneActuelle = MemoireCoup.Last().GetColonne();
                    EffaceMiseEnformeBouton(ligneActuelle, colonneActuelle);
                    MemoireCoup.Remove(MemoireCoup.Last());


                    int lignePrecedente = MemoireCoup[(MemoireCoup.Count - 1)].GetLigne();
                    int colonnePrecedente = MemoireCoup[MemoireCoup.Count - 1].GetColonne();
                    btnGrille[lignePrecedente, colonnePrecedente].Enabled = true;
                    MarquerProchainsCoupsAutorises(lignePrecedente, colonnePrecedente);
                    MarqueCoupJoue(lignePrecedente, colonnePrecedente);
                    button1.Text = "ANNULER\n le dernier coup\n ( " + coupAnnules.ToString()  + "/5)";

                }
            }
        }

        private void Button2_Click(object sender, EventArgs e) //REJOUER
        {
            button1.Enabled = false;
            coup = 0;
            coupAnnules = 0;
            label1.Text = "Score : " + coup.ToString() + "/64";
            label2.Text = "Record : " + max.ToString() + "/64";
            button1.Text = "ANNULER\n le dernier coup\n (" + coupAnnules.ToString() + "/5)";
            MemoireCoup.Clear();
            
            this.choisirLesCouleursToolStripMenuItem.Enabled = true;
            for (int l = 0; l < TAILLE; l++)
            {
                for (int c = 0; c < TAILLE; c++)
                {
                    btnGrille[l, c].Text = "";
                    btnGrille[l, c].Enabled = true;
                    btnGrille[l, c].Image = null;
                    this.coup = 0;
                    MiseEnformeGrille();
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)//REJOUER CASE HASARD
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button4.Enabled = true;
            coup = 0;
            coupAnnules = 0;
            label1.Text = "Score : " + coup.ToString() + "/64";
            label2.Text = "Record : " + max.ToString() + "/64";
            button1.Text = "ANNULER\n le dernier coup\n (" + coupAnnules.ToString() + "/5)";
            MemoireCoup.Clear();

            coup++;
            Random r = new Random();
            int x = r.Next(2, 10);
            int y = r.Next(2, 10);
            MemoriseCoup(x, y);

            if (max <= coup)
                max = coup;

            InitialiseProchainCoup();
            MiseEnformeGrille();
            MarquerProchainsCoupsAutorises(x, y);
            InitialiseCheck();
            MarqueCoupJoue(x, y);
            FinDePartie();
            Application.DoEvents();
        }

        private void Button4_Click(object sender, EventArgs e) // DEMONSTRATION
        {
            button1.Enabled = false;
            panel1.Enabled = false;
            if (coup > 0){
                MiseEnformeGrille();
                Application.DoEvents();
                int ligne = MemoireCoup[0].GetLigne();
                int colonne = MemoireCoup[0].GetColonne();
                coup = 0;
                btnGrille[ligne, colonne].BackColor = GetCouleurCoup();
                btnGrille[ligne, colonne].Text = "1";
                ParcoursDemo(ligne, colonne, 800);
                MessageBox.Show(this, "Et voilà ! Simple, n'est-ce-pas ?",
                       "Démonstration terminée", MessageBoxButtons.OK,
                       MessageBoxIcon.Question,
                       MessageBoxDefaultButton.Button1);
                MiseEnformeGrille();
            }else
                MessageBox.Show(this, "Jouez au moins une case, vous pouvez y arriver !",
                       "Démonstration", MessageBoxButtons.OK,
                       MessageBoxIcon.Exclamation,
                       MessageBoxDefaultButton.Button1);
            panel1.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            f2 = new Form2();
            f2.Show();
        }

        private void casesPairesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK && validCouleurSelectionnePaire(cd.Color))
            {
                button6.BackColor = cd.Color;
                MiseEnformeGrille();
            }
            else
                MessageBox.Show(this, "La couleur est déjà utilisée",
                               "Couleur", MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
        }

        private void impaireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            if (cd.ShowDialog() == DialogResult.OK && validCouleurSelectionneImpaire(cd.Color))
            {
                button7.BackColor = cd.Color;
                MiseEnformeGrille();
            }

            else
                MessageBox.Show(this, "La couleur est déjà utilisée",
                               "Couleur", MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
        }

        private void CasesJouéesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            if (cd.ShowDialog() == DialogResult.OK && validCouleurSelectionneCoup(cd.Color))
                button8.BackColor = cd.Color;
            else
                MessageBox.Show(this, "La couleur est déjà utilisée",
                               "Couleur", MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
        }


        private bool validCouleurSelectionneImpaire(Color c)
        {
            return (button6.BackColor==c||button8.BackColor==c) ? false : true;
        }

        private bool validCouleurSelectionnePaire(Color c)
        {
            return (button7.BackColor == c || button8.BackColor == c) ? false : true;
        }

        private bool validCouleurSelectionneCoup(Color c)
        {
            return (button6.BackColor == c || button7.BackColor == c) ? false : true;
        }

        private void CacheBoutonsCouleurs()
        {
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
        }



        public void GenererGrille()
        {
            int tailleButton = panel1.Width / 12;
            panel1.Height = panel1.Width;
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    btnGrille[i, j] = new Button();
                    btnGrille[i, j].Height = tailleButton;
                    btnGrille[i, j].Width = tailleButton;
                    btnGrille[i, j].FlatStyle = FlatStyle.Flat;
                    btnGrille[i, j].Click += Grid_Button_Click;
                    panel1.Controls.Add(btnGrille[i, j]);
                    btnGrille[i, j].Location = new Point(i * tailleButton, j * tailleButton);
                    btnGrille[i, j].Text = "";
                    btnGrille[i, j].Tag = new Point(i, j);
                    if (i < 2 || j < 2 || i > 9 || j > 9)
                    {
                        btnGrille[i, j].Visible = false;
                    }

                }
            }
        }

        public void MarqueCoupJoue(int x, int y)
        {
            btnGrille[x, y].Font = new Font("Arial", 26);
            btnGrille[x, y].Text = coup.ToString();
            btnGrille[x, y].Image = imgPion;
            btnGrille[x, y].Enabled = false;
            label1.Text = "Réussite : " + coup.ToString() + "/64";
            label2.Text = "Score max : " + max.ToString() + "/64";
        }

        public void CoupDemo(int x, int y)
        {
            for (int i = 0; i < TAILLE; i++){
                for (int j = 0; j < TAILLE; j++) { 
                    btnGrille[i, j].Image = null;
                    btnGrille[i, j].Enabled = true;
                }
            }
            Application.DoEvents();

            btnGrille[x, y].Font = new Font("Arial", 26);
            btnGrille[x, y].Text = coup.ToString();
            btnGrille[x, y].BackColor = GetCouleurCoup();
            btnGrille[x, y].ForeColor = Color.White;
            btnGrille[x, y].Image = imgPion;
            Application.DoEvents();
        }

        public void EffaceMiseEnformeBouton(int x, int y)
        {
            btnGrille[x, y].Text = "";
            if ((x % 2 == 1 && y % 2 == 1) || (x % 2 == 0 && y % 2 == 0))
                btnGrille[x, y].BackColor = GetCouleurPaire();
            else
                btnGrille[x, y].BackColor = GetCouleurImpaire();

        }

        public void MiseEnformeGrille()
        {
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < TAILLE; j++)
                {
                    btnGrille[i, j].Text = "";
                    btnGrille[i, j].Image=null;
                    if ((i % 2 == 1 && j % 2 == 1) || (i % 2 == 0 && j % 2 == 0))
                        btnGrille[i, j].BackColor = GetCouleurPaire();
                    else
                        btnGrille[i, j].BackColor = GetCouleurImpaire();
                }
            }
        }

        public void MarquerProchainsCoupsAutorises(int x, int y)
        {
            InitialiseProchainCoup();
            btnGrille[x + 2, y + 1].Enabled = true;
            btnGrille[x + 2, y - 1].Enabled = true;
            btnGrille[x - 2, y + 1].Enabled = true;
            btnGrille[x - 2, y - 1].Enabled = true;
            btnGrille[x + 1, y + 2].Enabled = true;
            btnGrille[x + 1, y - 2].Enabled = true;
            btnGrille[x - 1, y + 2].Enabled = true;
            btnGrille[x - 1, y - 2].Enabled = true;
            MiseEnFormeCoupsPrecedents();
            InitialiseCheck();
        }

        private void InitialiseCheck()
        {
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < TAILLE; j++)
                {
                    if (btnGrille[i, j].Enabled)
                        btnGrille[i, j].Image = imgCheck;
                }
            }
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



        private void InitialiseProchainCoup()
        {
            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < TAILLE; j++)
                {
                    btnGrille[i, j].Image = null;
                    btnGrille[i, j].Enabled = false;
                }
            }
        }



        private void MiseEnFormeCoupsPrecedents()
        {
            for (int i = 0; i < TAILLE; i++){
                for (int j = 0; j < TAILLE; j++)
                    if (!btnGrille[i, j].Text.Equals(""))
                    {
                        btnGrille[i, j].BackColor = GetCouleurCoup();
                        btnGrille[i, j].Enabled = false;
                    }
                       
            }
        }



        private void MemoriseCoup(int x, int y)
        {
            Memoire m = new Memoire(x, y);
            MemoireCoup.Add(m);
            cptMemoireLigne += 1;
        }



        private void FinDePartie()
        {
            int cases_possibles = 64;
            for (int i = 2; i < 10; i++)
            {
                for (int j = 2; j < 10; j++)
                {
                    if (btnGrille[i, j].Enabled == false)
                    {
                        cases_possibles--;
                        if (cases_possibles < 1)
                        {

                            MessageBox.Show(this, "Vous avez perdu au " + coup + "ème coup",
                                               "Dommage", MessageBoxButtons.OK,
                                               MessageBoxIcon.Question,
                                               MessageBoxDefaultButton.Button1);
                        }

                    }
                }

            }
        }



        private Color GetCouleurPaire()
        {
            return button6.BackColor;
        }

        private Color GetCouleurImpaire()
        {
            return button7.BackColor;
        }

        private Color GetCouleurCoup()
        {
            return button8.BackColor;
        }

        private void ParcoursDemo(int ii, int jj, int ml)
        {
            ++coup;
            i = ii ; j = jj ;
            echec[i, j] = 1;

            for (k = 2; k <= 64; k++)
            {
                for (l = 0, min_fuite = 11; l < 8; l++)
                {
                    ii = i + depi[l]; jj = j + depj[l];

                    if (echec[ii, jj] != 0)
                        nb_fuite = 10;
                    else nb_fuite = Fuite(ii, jj);

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
                CoupDemo(i, j);
                System.Threading.Thread.Sleep(ml);
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


    }


    public class Memoire
    {
        private int x;
        private int y;

        public Memoire(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetLigne()
        {
            return x;
        }
        public int GetColonne()
        {
            return y;
        }


    }

}



