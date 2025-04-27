using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private const int QUANTIDADE_MINAS = 40;

    private TMP_Text timer;
    private float tempoRestante = 600;
    private TMP_Text[,] textosCampos = new TMP_Text[15, 18];
    private bool[,] minas = new bool[15, 18];

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
        TMP_Text[] botoesCamposEncontrados = GameObject.Find("BotoesCampos").GetComponentsInChildren<TMP_Text>();

        int linha = 0;
        int coluna = 0;

        foreach (TMP_Text texto in botoesCamposEncontrados)
        {
            
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
}
