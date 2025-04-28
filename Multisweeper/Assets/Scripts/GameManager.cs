using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    static readonly Dictionary<int, Color32> textosCampoMinasMap = new Dictionary<int, Color32>
    {
        { 1, new Color32(233, 223, 0, 255)},
        { 2, new Color32(233, 143, 0, 255)},
        { 3, new Color32(233, 92, 0, 255)},
        { 4, new Color32(233, 0, 0, 255)},
        { 5, new Color32(44, 50, 190, 255)},
        { 6, new Color32(44, 184, 190, 255)},
        { 7, new Color32(44, 190, 74, 255)},
        { 8, new Color32(190, 44, 170, 255)}
    };

private const int QUANTIDADE_MINAS = 40;

    private TMP_Text timer;
    private float tempoRestante = 600;
    private TMP_Text[,] textosCampos = new TMP_Text[15, 18];
    private bool[,] minas = new bool[15, 18];
    private bool[,] camposDesbloqueados = new bool[15, 18];
    private GameObject[,] camposMinas = new GameObject[15, 18];

    void Start()
    {
        GameObject timerObject = GameObject.Find("Timer");
        timer = timerObject.GetComponent<TMP_Text>();
        preencherTextosCampos();
        criarMinas();
        preencherComponentesCampos();
    }

    private void preencherTextosCampos()
    {
        TMP_Text[] textosCamposEncontrados = GameObject.Find("CamposText").GetComponentsInChildren<TMP_Text>();

        int linha = 0;
        int coluna = 0;

        foreach (TMP_Text texto in textosCamposEncontrados)
        {
            texto.enabled = false;
            textosCampos[coluna, linha] = texto;

            coluna++;

            if (coluna == 15)
            {
                coluna = 0;
                linha++;
            }
        }
    }

    private void criarMinas()
    {
        for(int i = 0; i < QUANTIDADE_MINAS; i++)
        {
            int coluna = Random.Range(0, 14);
            int linha = Random.Range(0, 18);
            
            while(minas[coluna, linha])
            {
                coluna = Random.Range(0, 14);
                linha = Random.Range(0, 18);
            }

            minas[coluna, linha] = true;
        }
    }

    private void preencherComponentesCampos()
    {
        Transform[] botoesCamposEncontrados = GameObject.Find("BotoesCampos").GetComponentsInChildren<Transform>();

        int linha = 0;
        int coluna = -1;

        foreach (Transform botao in botoesCamposEncontrados)
        {
            if(linha < 18 && coluna >= 0)
            {
                GameObject objetoBotao = botao.gameObject;

                int[] campo = { coluna, linha };

                objetoBotao.AddComponent<CampoMina>();
                objetoBotao.GetComponent<CampoMina>().setCampo(campo);
                objetoBotao.GetComponent<CampoMina>().setGameManager(this);

                camposMinas[coluna, linha] = objetoBotao;
            }

            coluna++;

            if (coluna == 15)
            {
                coluna = 0;
                linha++;
            }
        }
    }

    void Update()
    {
        atualizarTempo();
    }

    private void atualizarTempo()
    {
        if (tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;
            timer.text = Mathf.FloorToInt(tempoRestante + 1).ToString();
        }
    }

    public void selecionarCampoMina(int[] campo)
    {
        if (minas[campo[0], campo[1]])
        {
            Debug.Log("PERDEU KKKKKKKKKKKKKK");
        } else
        {
            verificarMinasProximas(campo);
        }
    }

    private void verificarMinasProximas(int[] campo)
    {
        int quantidadeMinas = 0;

        int[] ix = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] iy = { -1, 0, 1, -1, 1, -1, 0, 1 };

        int coluna = campo[0];
        int linha = campo[1];

        for (int i = 0; i < 8; i++)
        {
            int colunaVerificar = coluna + iy[i];
            int linhaVerificar = linha + ix[i];

            if (campoValido(colunaVerificar, linhaVerificar) && minas[colunaVerificar, linhaVerificar])
            {
                quantidadeMinas++;
            }
        }

        desbloquearCampo(campo, quantidadeMinas);

        if (quantidadeMinas == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                int colunaVerificar = coluna + iy[i];
                int linhaVerificar = linha + ix[i];

                int[] campoVerificar = { colunaVerificar, linhaVerificar };

                if (campoValido(colunaVerificar, linhaVerificar))
                {
                    verificarMinasProximas(campoVerificar);
                }
            }
        }
    }

    private bool campoValido(int colunaVerificar, int linhaVerificar)
    {
        return (colunaVerificar >= 0) && (colunaVerificar < 15) && (linhaVerificar >= 0) && (linhaVerificar < 18) && !camposDesbloqueados[colunaVerificar, linhaVerificar];
    }

    private void desbloquearCampo(int[] campo, int quantidadeMinas)
    {
        if(quantidadeMinas != 0)
        {
            textosCampos[campo[0], campo[1]].enabled = true;
            textosCampos[campo[0], campo[1]].text = quantidadeMinas.ToString();
            textosCampos[campo[0], campo[1]].overrideColorTags = true;
            textosCampos[campo[0], campo[1]].color = textosCampoMinasMap[quantidadeMinas];
        }

        camposDesbloqueados[campo[0], campo[1]] = true;
        Destroy(camposMinas[campo[0], campo[1]]);
    }
}
