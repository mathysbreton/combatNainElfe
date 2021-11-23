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
using CombatClientSocketNaIn.Classes;


namespace CombatClientSocketNaIn
{
    public partial class frmClienSocketNain : Form
    {
        Random m_r;
        Elfe m_elfe;
        Nain m_nain;
        Socket client;
        string[] m_tableauReception;
        public frmClienSocketNain()
        {
            InitializeComponent();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(0, 3));
            picNain.Image = m_nain.Avatar;
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString(); ;
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;
            lstReception.Items.Clear();
            m_elfe = new Elfe(1, 0, 0);
            picElfe.Image = m_elfe.Avatar;
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnFrappe.Enabled = true;
            Reset();
        }

        private void btnFrappe_Click(object sender, EventArgs e)
        {
            byte[] tByteEnvoie;
            byte[] tByteReceptionClient = new byte[50];
            int nbOctetReception;
            string reponse;
            string str;
            ASCIIEncoding textByte = new ASCIIEncoding();

            try
            {
                //création d'un objet socket et connexion
                client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Parse("127.0.0.1"), 7025);
                lstReception.Items.Add("assurez-vous que le serveur est démarré et en attente d'un client");
                lstReception.Update();

                //vérifier si l'objet socket est connecté
                if (client.Connected)
                {
                    //envoie les données du nain sous cette forme:
                    //vieNain;forceNain;armeNain

                    str = m_nain.Vie + ";" + m_nain.Force + ";" + m_nain.Arme + ";";

                    lstReception.Items.Add("Client: \r\nTransmet..." + str);
                    lstReception.Update();
                    tByteEnvoie = textByte.GetBytes(str);

                    //transmission
                    client.Send(tByteEnvoie);
                    Thread.Sleep(500);

                    //réception
                    //reçoit les données sous cette forme:
                    //vieNain;forceNain;armeNain;vieElfe;forceElfe;sortElfe;

                    lstReception.Items.Add("Client: réception des données du serveur");
                    lstReception.Update();

                    nbOctetReception = client.Receive(tByteReceptionClient);
                    reponse = Encoding.ASCII.GetString(tByteReceptionClient);

                    lstReception.Items.Add("\r\nReception..." + reponse);
                    lstReception.Update();

                    //Split sur le string de réception pour afficher les
                    //nouvelles stat du nain et de l'elfe 
                    m_tableauReception = reponse.Split(';');

                    //réception des données du nain
                    m_nain.Vie = Convert.ToInt32(m_tableauReception[0]);
                    m_nain.Force = Convert.ToInt32(m_tableauReception[1]);
                    m_nain.Arme = m_tableauReception[2];

                    //réception des données de l'elfe
                    m_elfe.Vie = Convert.ToInt32(m_tableauReception[3]);
                    m_elfe.Force = Convert.ToInt32(m_tableauReception[4]);
                    m_elfe.Sort = Convert.ToInt32(m_tableauReception[5]);

                    //affichage des données du nain
                    lblVieNain.Text = "Vie: " + m_nain.Vie;
                    lblForceNain.Text = "Force: " + m_nain.Force;
                    lblArmeNain.Text = "Arme: " + m_nain.Arme;

                    //affichage des données de l'elfe
                    lblVieElfe.Text = "Vie: " + m_elfe.Vie;
                    lblForceElfe.Text = "Force: " + m_elfe.Force;
                    lblSortElfe.Text = "Sort: " + m_elfe.Sort;

                    //tester et afficher le gagnant

                    //tester


                    //Vérifier s'il y a un gagnant
                    if (m_nain.Vie == 0 || m_elfe.Vie == 0)
                    {
                        if (m_nain.Vie == 0)
                        {
                            MessageBox.Show("Elfe gagnant !");
                            picNain.Image = Image.FromFile("elfe.jpg");
                            return;
                        }

                        if (m_elfe.Vie == 0)
                        {
                            MessageBox.Show("Nain gagnant !");
                            picElfe.Image = Image.FromFile("nain.jpg");
                            return;
                        }
                    }

                }
                //fermeture de l'objet socket
                client.Close();
                lstReception.Items.Add("Déconnecté");
                lstReception.Update();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception: " + ex);
            }
            btnFrappe.Enabled = true;
            btnReset.Enabled = true;
        }
    }
}
