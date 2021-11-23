using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatServeurSocketElfe.Classes;

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;
        Thread m_thCombat;
        string[] m_tableauReception;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener)
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7025);
            m_ServerListener.Start();
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();
 
            lstReception.Items.Clear();
        }

        void AfficheStatNain()
        {
            lblVieNain.Text = "Vie: " + m_nain.Vie;
            lblForceNain.Text = "Force: " + m_nain.Force;
            lblArmeNain.Text = "Arme: " + m_nain.Arme;

            //this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {
            lblVieElfe.Text = "Vie: " + m_elfe.Vie;
            lblForceElfe.Text = "Force: " + m_elfe.Force;
            lblSortElfe.Text = "Sort: " + m_elfe.Sort;

            //this.Update(); // pour s'assurer de l'affichage via le thread
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }     

        private void btnAttente_Click(object sender, EventArgs e)
        {
            // Combat par un thread
            ThreadStart codeThread1 = new ThreadStart(Combat);
            m_thCombat = new Thread(codeThread1);
            m_thCombat.Start();
        }
        public void Combat() 
        {
            // déclarations de variables locales 
            m_client = null;
            string reponseServeur = "aucune";
            string receptionClient = "rien";
            int nbOctetReception;
            byte[] tByteReception = new byte[50];
            ASCIIEncoding textByte = new ASCIIEncoding();
            byte[] tByteEnvoie;

            try
            {
                while(m_nain.Vie > 0 && m_elfe.Vie > 0)
                {
                    //initialisation d'un client (bloquant)
                    m_client = m_ServerListener.AcceptSocket();
                    lstReception.Items.Add("Client branché !");
                    lstReception.Update();
                    Thread.Sleep(500);
                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);
                    lstReception.Items.Add("du client: " + receptionClient);
                    lstReception.Update();

                    // split sur le ';' pour récupérer les données d'un nain
                    m_tableauReception = receptionClient.Split(';');
                   
                    m_nain.Vie = Convert.ToInt32(m_tableauReception[0]);
                    m_nain.Force = Convert.ToInt32(m_tableauReception[1]);
                    m_nain.Arme = m_tableauReception[2];

                    AfficheStatNain();

                    //Exécute Frapper
                    lstReception.Items.Add("Serveur: Frapper l'elfe");
                    lstReception.Update();
                    m_nain.Frapper(m_elfe);

                    //Affiche les données de l'elfe membre
                    AfficheStatElfe();

                    //Exécute LancerSort
                    lstReception.Items.Add("Serveur: Lancer un sort au nain");
                    lstReception.Update();
                    m_elfe.LancerSort(m_nain);

                    //Affiche les données du nain membre et de l'elfe membre
                    AfficheStatNain();
                    AfficheStatElfe();

                    //envoie les données au client sous cette forme:
                    //vieNain;forceNain;armeNain|vieElfe;forceElfe;sortElfe
                    reponseServeur = m_nain.Vie + ";" + m_nain.Force + ";" + m_nain.Arme + ";" + m_elfe.Vie + ";" + m_elfe.Force + ";" + m_elfe.Sort + ";";
                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();

                    tByteEnvoie = textByte.GetBytes(reponseServeur);

                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);

                    //Ferme le socket
                    m_client.Close();

                    //Vérifier s'il y a un gagnant
                    if(m_nain.Vie == 0 || m_elfe.Vie == 0)
                    {
                        if(m_nain.Vie == 0)
                        {
                            MessageBox.Show("Elfe gagnant !");
                            picNain.Image = Image.FromFile("elfe.jpg");
                            btnReset.Enabled = true;
                            return;
                        }

                        if(m_elfe.Vie == 0)
                        {
                            MessageBox.Show("Nain gagnant !");
                            picElfe.Image = Image.FromFile("nain.jpg");
                            btnReset.Enabled = true;
                            return;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                lstReception.Items.Add("Exception: " + ex.Message);
                lstReception.Update();
            }

        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            //m_elfe.Vie = 0;
            //m_nain.Vie = 0;
            try
            {
                m_elfe.Vie = 0;
                m_nain.Vie = 0;
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender,e);
            try
            {
                // il faut avoir un objet TCPListener existant
                // m_ServerListener.Stop();
                m_ServerListener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
