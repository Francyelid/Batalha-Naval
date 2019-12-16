using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Chat
{
    /// <summary>
    /// Tela Principal do Jogo
    /// </summary>
    public partial class Form1 : Form
    {
        #region Variaveis Globais
        /// <summary>
        /// Variaveis globais do programas. Dentre elas temos as instancias das 5 embarcações possiveis pelo jogador, o Jogo atual, o socket para transmissão das mensagens e etc;
        /// </summary>
        Socket socket;
        EndPoint endPointLocal, endPointRemote;
        Jogo game;
        Barco b1 = new Barco() { Orientacao = true, Tamanho = 1 };
        Barco b2 = new Barco() { Orientacao = true, Tamanho = 2 };
        Barco b3 = new Barco() { Orientacao = true, Tamanho = 3 };
        Barco b4 = new Barco() { Orientacao = true, Tamanho = 4 };
        Barco b5 = new Barco() { Orientacao = true, Tamanho = 5 };
        int UltimoBarco = 0;
        #endregion

        #region Inicialização
        /// <summary>
        /// Inicialização do Socket e dos IPs
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            txtIP_1.Text = GetLocal();
            txtIP_2.Text = GetLocal();
        }

        public string GetLocal()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        #endregion

        #region Recepção de Mensagens do Inimigo
        /// <summary>
        /// Responsavel por Receber as Menssagens do jogador Adversário.
        /// </summary>
        /// <param name="asyncResult"></param>
        public void MessageCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int tam = socket.EndReceiveFrom(asyncResult, ref endPointRemote);

                if (tam > 0)
                {

                    byte[] receiveData = (byte[])asyncResult.AsyncState;

                    ASCIIEncoding ecg = new ASCIIEncoding();

                    string receiveMessage = ecg.GetString(receiveData); //Menssagem Recebida;

                    #region Tratamento Da Mensagem Recebida
                    if (receiveMessage.Contains("!@#$%Venceu"))//Mensagem de Vitoria
                    {
                        MessageBox.Show("Ganhou Ka!@#$%lho");
                        FormClose(this);

                    } else if (receiveMessage.Contains("!@#$% - Acertou ")) { //Informação de que Acertou um Barco

                        var aux = receiveMessage.Replace("!@#$% - Acertou : ", "");
                        var x = Convert.ToInt32(aux.Split('x')[0]);
                        var y = Convert.ToInt32(aux.Split('x')[1]);

                        var butaos = new List<Button>();
                        
                        for (int i = 0; i < groupJogador2.Controls.Count; i++)
                        {
                            butaos.Add((Button)groupJogador2.Controls[i]);
                        }

                        var botao = butaos.FirstOrDefault(q => q.Name == x + "x" + y);
                        CorBotão (botao , Color.Green);


                    } else if (receiveMessage.Contains("!@#$% - Errou ")) //Informação de que Errou um Barco
                    {

                        var aux = receiveMessage.Replace("!@#$% - Errou : ", "");
                        var x = Convert.ToInt32(aux.Split('x')[0]);
                        var y = Convert.ToInt32(aux.Split('x')[1]);

                        var butaos = new List<Button>();

                        for (int i = 0; i < groupJogador2.Controls.Count; i++)
                        {
                            butaos.Add((Button)groupJogador2.Controls[i]);
                        }

                        var botao = butaos.FirstOrDefault(q => q.Name == x + "x" + y);
                        CorBotão(botao, Color.Purple);

                    }
                    else if (receiveMessage.Contains("#?!,.Comando: ")) // Informação de um Ataque
                    {
                        EnableGroup(groupJogador2, true);
                        var butaos = new List<Button>();

                        var aux = receiveMessage.Replace("#?!,.Comando: ", "");
                        var x = Convert.ToInt32(aux.Split('x')[0]);
                        var y = Convert.ToInt32(aux.Split('x')[1]);

                        for (int i = 0; i < groupJogador1.Controls.Count; i++)
                        {
                            butaos.Add((Button)groupJogador1.Controls[i]);
                        }

                        var botaoSelecionado = butaos.FirstOrDefault(w => w.Name == x + "x" + y);
                        CorBotão(botaoSelecionado ,Color.Red);

                        if (game.Matriz_Jogo[x, y] == 1)
                        {

                            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

                            byte[] msg = enc.GetBytes("!@#$% - Acertou : " + x + "x" + y); //Envia para o Inimigo que ele acertou o ataque

                            socket.Send(msg);
                        }
                        else
                        {
                            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

                            byte[] msg = enc.GetBytes("!@#$% - Errou : " + x + "x" + y); //Envia para o Inimigo que ele errou o ataque

                            socket.Send(msg);
                        }

                        var ListaButaos = new List<Button>();

                        for (int a = 0; a < groupJogador1.Controls.Count; a++)
                        {
                            ListaButaos.Add((Button)groupJogador1.Controls[a]);
                        }
                        bool status = true;
                        for (int i = 0; i < Math.Sqrt(game.Matriz_Jogo.Length); i++)
                        {
                            for (int j = 0; j < Math.Sqrt(game.Matriz_Jogo.Length); j++)
                            {
                                if (game.Matriz_Jogo[i, j] == 1 && ListaButaos.FirstOrDefault(p => p.Name == (i + "x" + j)).BackColor != Color.Red)
                                {
                                    status = false;
                                }
                            }
                        }
                        if (status)
                        {
                            MessageBox.Show("Você perdeu!");
                            try
                            {
                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

                                byte[] msg = enc.GetBytes("!@#$%Venceu"); //Envia para o Inimigo que ele venceu o Jogo

                                socket.Send(msg);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            this.Close();
                        }
                    }
                    else
                    {
                        MensagemTexto(txtName_2.Text + " disse: " + receiveMessage); //Escreve a menssagem do inimigo
                    }

                    #endregion
                }

                byte[] buffer = new byte[1500];

                socket.BeginReceiveFrom(buffer, 0, buffer.Length, 
                                        SocketFlags.None, ref endPointRemote, 
                                        new AsyncCallback(MessageCallBack), buffer);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public delegate void Mensagem(string msg);

        public delegate void CorBotao(Button botao, Color cor);

        public delegate void EnableGrupo(GroupBox grupo, bool status);

        public delegate void FecharForm(Form form);
        public void MensagemTexto(string msg)
        {
            if (listMessage.InvokeRequired)
            {
                listMessage.Invoke(new Mensagem(MensagemTexto), msg);

            }
            else {
                listMessage.Items.Add(msg);
            }
        }
        public void CorBotão(Button botao, Color cor)
        {
            if (botao.InvokeRequired)
            {
                botao.Invoke(new CorBotao(CorBotão), botao, cor);
            }
            else
            {
                botao.BackColor = cor;
            }
        }
        public void EnableGroup(GroupBox grupo, bool status)
        {
            if (groupBox1.InvokeRequired)
            {
                grupo.Invoke(new EnableGrupo(EnableGroup), grupo, status);
            }
            else
            {
                grupo.Enabled = status;
            }
        }
        public void FormClose(Form form)
        {
            if (groupBox1.InvokeRequired)
            {
                form.Invoke(new FecharForm(FormClose),form);
            }
            else
            {
                form.Close();
            }
        }

        #endregion

        #region Inicializar Conexão
        /// <summary>
        /// Realiza a Conexão com o Inimigo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            txtIP_1.Enabled = false;
            txtIP_2.Enabled = false;
            txtName_1.Enabled = false;
            txtName_2.Enabled = false;
            txtPort_1.Enabled = false;
            txtPort_2.Enabled = false;
            groupJogador1.Enabled = false;
            
            try
            {
                endPointLocal = new IPEndPoint(IPAddress.Parse(txtIP_1.Text), Convert.ToInt32(txtPort_1.Text));
                socket.Bind(endPointLocal);

                endPointRemote = new IPEndPoint(IPAddress.Parse(txtIP_2.Text), Convert.ToInt32(txtPort_2.Text));
                socket.Connect(endPointRemote);

                byte[] buffer = new byte[1500];

                socket.BeginReceiveFrom(buffer, 0, buffer.Length,
                                        SocketFlags.None, ref endPointRemote,
                                        new AsyncCallback(MessageCallBack), buffer);

                btnConnect.Enabled = false;
                txtMessage.Enabled = true;
                txtMessage.Focus();

                btnSend.Enabled = true;

                groupJogador2.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Load Do Form Inicial
        /// <summary>
        /// Carregamento Inicial do Form. Esta Parta é responsavel por carregar os componentes visuais, como os botões, e fazer a inicialização da classe Jogo.
        /// Além disso, esta fução cria os eventos nos botões
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            #region Inicializa Variavel Game
            int[,] resposta = new int[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    resposta[i, j] = 0;
                }
            }

            game = new Jogo(txtName_1.Text, txtName_2.Text, resposta, resposta);
            #endregion

            var coluna_1 = (groupJogador1.Width - 20) / 10;
            var altura_1 = (groupJogador1.Height - 25) / 10;
            var coluna_2 = (groupJogador2.Width - 20) / 10;
            var altura_2 = (groupJogador2.Height - 25) / 10;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    #region Cria Botoes do Jogador e do Inimigo
                    var novoBotao1 = new Button()
                    {
                        Name = i + "x" + j,
                        BackColor = Color.Gray,
                        Location = new Point(10 + (i * coluna_1), (15 + (j * altura_1))),
                        Width = coluna_1,
                        Height = altura_1

                    };

                    var novoBotao2 = new Button()
                    {
                        Name = i + "x" + j,
                        BackColor = Color.Gray,
                        Location = new Point(10 + (i * coluna_2), (15 + (j * altura_2))),
                        Width = coluna_2,
                        Height = altura_2

                    };
                    #endregion

                    #region Evento no Botão do Jogador
                    novoBotao1.Click += (s, elemento) => {

                        Button botao = (Button)s;

                        Barco barcoSelecionado;

                        var ListaButaos = new List<Button>();

                        for (int a = 0; a < groupJogador1.Controls.Count; a++)
                        {
                            ListaButaos.Add((Button)groupJogador1.Controls[a]);
                        }

                        #region Verifica o Barco Selecionado
                        switch (UltimoBarco) 
                        {
                            case 1:
                                barcoSelecionado = b1;
                                break;
                            case 2:
                                barcoSelecionado = b2;
                                break;
                            case 3:
                                barcoSelecionado = b3;
                                break;
                            case 4:
                                barcoSelecionado = b4;
                                break;
                            case 5:
                                barcoSelecionado = b5;
                                break;
                            default:
                                return;
                        }
                        #endregion

                        if (!barcoSelecionado.Ativo)
                            return;

                        var aux = new int[10,10];
                      
                        for(int a = 0; a < 10; a++)
                        {
                            for (int b = 0; b < 10; b++)
                            {
                                aux[a, b] = game.Matriz_Jogo[a, b];
                            
                            }
                        }

                        #region Verifica se Barco pode ser colocado neste local
                        bool erro = false;
                        var x = Convert.ToInt32(botao.Name.Split('x')[0]);
                        var y = Convert.ToInt32(botao.Name.Split('x')[1]);

                        for (int w = 0; w < barcoSelecionado.Tamanho; w++)
                        {
                            if (barcoSelecionado.Orientacao)
                            {
                                if (Math.Sqrt(aux.Length) <= (y + w ))
                                {
                                    erro = true;
                                }
                                else if ( aux[x, y + w] == 0)
                                {
                                    aux[x, y + w] = 1;
                                }
                                else 
                                {
                                    erro = true;
                                }
                            }
                            else 
                            {
                                if (Math.Sqrt(aux.Length) <= (x + w ))
                                {
                                    erro = true;
                                }
                                else if ( aux[x+w, y ] == 0)
                                {
                                    aux[x+w, y ] = 1;
                                }
                                else
                                {
                                    erro = true;
                                }
                            }
                        }

                        if (!erro)
                        {
                            game.Matriz_Jogo = aux;

                            for (int a = 0; a < 10; a++)
                            {
                                for (int b = 0; b < 10; b++)
                                {
                                    if (game.Matriz_Jogo[a, b] == 1)
                                    {
                                        var Butao = ListaButaos.FirstOrDefault(k => k.Name == a + "x" + b);
                                        Butao.BackColor = Color.Blue;
                                        barcoSelecionado.Ativo = false;
                                    }

                                }
                            }

                        }
                        else 
                        {
                            MessageBox.Show("Não pode colocar barco ai.");
                        }
                        #endregion

                    };

                    #endregion

                    #region Evento no Botao do Inimigo
                    novoBotao2.Click += (s , elemento) => {

                        var botaoClicado = (Button)s;
                        if (botaoClicado.BackColor != Color.Green && botaoClicado.BackColor != Color.Purple)
                        {
                            try
                            {
                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                byte[] msg = enc.GetBytes("#?!,.Comando: " + botaoClicado.Name);
                                socket.Send(msg);
                                groupJogador2.Enabled = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }

                    };

                    #endregion

                    groupJogador1.Controls.Add(novoBotao1);
                    groupJogador2.Controls.Add(novoBotao2);

                }

            }
        }
        #endregion

        #region Seta Ultimo Barco Clicado
        private void Barco1_Click(object sender, EventArgs e)
        {
            UltimoBarco = 1;
        }

        private void Barco2_Click(object sender, EventArgs e)
        {
            UltimoBarco = 2;
        }

        private void Barco3_Click(object sender, EventArgs e)
        {
            UltimoBarco = 3;
        }

        private void Barco4_Click(object sender, EventArgs e)
        {
            UltimoBarco = 4;
        }

        private void Barco5_Click(object sender, EventArgs e)
        {
            UltimoBarco = 5;
        }
        #endregion

        #region Alterar Orientação dos Barcos
        private void Barco1_DoubleClick(object sender, EventArgs e)
        {
            b1.Orientacao = !b1.Orientacao;
            lblBarco1.Text = new string(lblBarco1.Text.Reverse().ToArray());
        }

        private void Barco2_DoubleClick(object sender, EventArgs e)
        {
            b2.Orientacao = !b2.Orientacao;
            lblBarco2.Text = new string(lblBarco2.Text.Reverse().ToArray());
        }

        private void Barco3_DoubleClick(object sender, EventArgs e)
        {
            b3.Orientacao = !b3.Orientacao;
            lblBarco3.Text = new string(lblBarco3.Text.Reverse().ToArray());
        }

        private void Barco4_DoubleClick(object sender, EventArgs e)
        {
            b4.Orientacao = !b4.Orientacao;
            lblBarco4.Text = new string(lblBarco4.Text.Reverse().ToArray());
        }

        private void Barco5_DoubleClick(object sender, EventArgs e)
        {
            b5.Orientacao = !b5.Orientacao;
            lblBarco5.Text = new string(lblBarco5.Text.Reverse().ToArray());
        }

        #endregion

        #region Enviar Menssagem Texto
        /// <summary>
        /// Envia Menssagem para o Oponente
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

                byte[] msg = enc.GetBytes(txtMessage.Text);

                socket.Send(msg);

                listMessage.Items.Add(txtName_1.Text + " disse:" + txtMessage.Text);
                txtMessage.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        #endregion

    }
}
