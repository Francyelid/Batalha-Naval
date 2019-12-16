using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{   
    /// <summary>
    /// A classe Jogo representa as informações sobre o estado atual do jogo.
    /// A Matriz_Jogo é a matriz que guarda o local dos barcos do jogador principal, enquanto a Matriz_Resposta guarda os locais já atacados por ele.
    /// </summary>
    public class Jogo
    {
        public string Jogador_1;
        public string Jogador_2;
        public int[,] Matriz_Jogo = new int [10, 10];
        public int[,] Matriz_Resposta = new int[10, 10];

        public Jogo(string jogador_1, string jogador_2, int[,] matriz_jogo, int[,] matriz_resposta)
        {
            Jogador_1 = jogador_1;
            Jogador_2 = jogador_2;
            Matriz_Jogo = matriz_jogo;
            Matriz_Resposta = matriz_resposta;
        }

    }
}
