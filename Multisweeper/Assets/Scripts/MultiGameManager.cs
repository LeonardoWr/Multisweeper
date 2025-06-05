using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MultiGameManager : NetworkBehaviour
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

    static readonly int[] ix = { -1, -1, -1, 0, 0, 1, 1, 1 };
    static readonly int[] iy = { -1, 0, 1, -1, 1, -1, 0, 1 };

    private const int QUANTIDADE_MINAS = 40;
    private const int QUANTIDADE_CAMPOS = 270;
    private const int QUANTIDADE_CAMPOS_NAO_MINAS = QUANTIDADE_CAMPOS - QUANTIDADE_MINAS;

    private int bandeiraRestantes;
    private bool[,] bandeiras;
    private bool[,] camposDesbloqueados;
    private GameObject[,] camposMinas = new GameObject[15, 18];
    private int[,] contagemMinasRondando;
    private bool iniciou;
    private int quantidadeBlocosDescobertos;
    private int quantidadeBlocosDescobertosAdv;
    private TMP_Text quantidadeBlocosDescobertosTxt;
    private TMP_Text quantidadeBandeirasRestantes;
    private bool[,] minas;
    private bool perdeu;
    private bool primeiroCampoDescoberto;
    private float tempoRestante = 600;
    private TMP_Text[,] textosCampos = new TMP_Text[15, 18];
    private int vidas;
    private Animator[] vidasAnim = new Animator[3];

    public override void OnNetworkSpawn()
    {
        if(IsLocalPlayer)
        {
            GameObject quantidadeBlocosDescobertosObject = GameObject.Find("TxtBlocoDescoberto");
            quantidadeBlocosDescobertosTxt = quantidadeBlocosDescobertosObject.GetComponent<TMP_Text>();
            quantidadeBlocosDescobertosTxt.text = "0";
            quantidadeBlocosDescobertos = 0;
            vidasAnim = GameObject.Find("Vidas").GetComponentsInChildren<Animator>();
            vidas = 3;
            GameObject quantidadeBandeirasRestantesObject = GameObject.Find("TxtBandeirasRestantes");
            quantidadeBandeirasRestantes = quantidadeBandeirasRestantesObject.GetComponent<TMP_Text>();
            preencherTextosCampos();
            preencherComponentesCampos();
            inicializacaoCampo();
            preencherTextosCamposAdv();
        }
    }

    private void inicializacaoCampo()
    {
        quantidadeBandeirasRestantes.text = QUANTIDADE_MINAS.ToString();
        bandeiraRestantes = QUANTIDADE_MINAS;
        primeiroCampoDescoberto = true;
        bandeiras = new bool[15, 18];
        camposDesbloqueados = new bool[15, 18];
        contagemMinasRondando = new int[15, 18];
        minas = new bool[15, 18];
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

    private void preencherComponentesCampos()
    {
        Transform[] botoesCamposEncontrados = GameObject.Find("BotoesCampos").GetComponentsInChildren<Transform>();

        int linha = 0;
        int coluna = -1;

        foreach (Transform botao in botoesCamposEncontrados)
        {
            if (linha < 18 && coluna >= 0)
            {
                GameObject objetoBotao = botao.gameObject;

                int[] campo = { coluna, linha };

                objetoBotao.AddComponent<MultiCampoMina>();
                objetoBotao.GetComponent<MultiCampoMina>().setCampo(campo);
                objetoBotao.GetComponent<MultiCampoMina>().setGameManager(this);

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

    private void preencherTextosCamposAdv()
    {
        TMP_Text[] textosCamposEncontrados = GameObject.Find("CamposAdvText").GetComponentsInChildren<TMP_Text>();

        int linha = 0;
        int coluna = 0;

        foreach (TMP_Text texto in textosCamposEncontrados)
        {
            texto.enabled = false;
            coluna++;

            if (coluna == 15)
            {
                coluna = 0;
                linha++;
            }
        }
    }

    [ClientRpc]
    public void inicializarJogoClientRpc()
    {
        GameObject menuConexao = GameObject.Find("MenuConexao");
        iniciou = true;

        if (menuConexao != null)
        {
            GameObject.Find("MenuConexao").SetActive(false);
        }
    }

    void Update()
    {
        if (IsServer && iniciou)
        {
            atualizarTempo();
        }
    }

    private void atualizarTempo()
    {
        if (tempoRestante > 0 && !perdeu)
        {
            tempoRestante -= Time.deltaTime;
            atualizarTempoClientRpc(tempoRestante);
        }
        else if(vidas != 0)
        {
            mostrarTelaFinalTempoClientRpc();
        }
    }

    [ClientRpc]
    private void atualizarTempoClientRpc(float tempoRestante)
    {
        TMP_Text timer = GameObject.Find("Timer").GetComponent<TMP_Text>();
        timer.text = Mathf.FloorToInt(tempoRestante + 1).ToString();
    }

    public void selecionarCampoMina(int[] campo)
    {
        if (!camposDesbloqueados[campo[0], campo[1]])
        {
            if (IsLocalPlayer)
            {
                if (minas[campo[0], campo[1]])
                {
                    vidas--;

                    configurarEstadoAnimVidas();

                    if (vidas == 0)
                    {
                        perdeu = true;
                        mostrarTelaFinalVidaClientRpc();
                    }
                }
                else
                {
                    if (primeiroCampoDescoberto)
                    {
                        criarMinas(campo);
                        primeiroCampoDescoberto = false;
                    }

                    verificarMinasProximas(campo);
                }

                atualizarGraficosCampoAdversarioServerRpc(Serialize(camposDesbloqueados), Serialize(contagemMinasRondando), quantidadeBlocosDescobertos, vidas);
            }
        }
    }

    private void configurarEstadoAnimVidas()
    {
        switch (vidas)
        {
            case 1:
                vidasAnim[0].SetBool("Vazia", false);
                vidasAnim[1].SetBool("Vazia", true);
                vidasAnim[2].SetBool("Vazia", true);
                break;
            case 2:
                vidasAnim[0].SetBool("Vazia", false);
                vidasAnim[1].SetBool("Vazia", false);
                vidasAnim[2].SetBool("Vazia", true);
                break;
            case 3:
                vidasAnim[0].SetBool("Vazia", false);
                vidasAnim[1].SetBool("Vazia", false);
                vidasAnim[2].SetBool("Vazia", false);
                break;
            default:
                vidasAnim[0].SetBool("Vazia", true);
                vidasAnim[1].SetBool("Vazia", true);
                vidasAnim[2].SetBool("Vazia", true);
                break;
        }
    }

    private void criarMinas(int[] campo)
    {
        for (int i = 0; i < QUANTIDADE_MINAS; i++)
        {
            int coluna = Random.Range(0, 14);
            int linha = Random.Range(0, 18);

            while (minas[coluna, linha] || (coluna == campo[0] && linha == campo[1]) || verificarCliqueCamposRedor(coluna, linha, campo))
            {
                coluna = Random.Range(0, 14);
                linha = Random.Range(0, 18);
            }

            minas[coluna, linha] = true;
        }
    }

    private bool verificarCliqueCamposRedor(int coluna, int linha, int[] campo)
    {
        int colunaCampo = campo[0];
        int linhaCampo = campo[1];

        for (int i = 0; i < 8; i++)
        {
            int colunaVerificar = colunaCampo + iy[i];
            int linhaVerificar = linhaCampo + ix[i];

            if (campoValido(colunaVerificar, linhaVerificar) && coluna == colunaVerificar && linha == linhaVerificar)
            {
                return true;
            }
        }

        return false;
    }

    private void verificarMinasProximas(int[] campo)
    {
        int quantidadeMinas = 0;
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

        contagemMinasRondando[campo[0], campo[1]] = quantidadeMinas;
        desbloquearCampo(campo, quantidadeMinas);
        quantidadeBlocosDescobertos++;
        quantidadeBlocosDescobertosTxt.text = quantidadeBlocosDescobertos.ToString();

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

        verificarReinicioCampo();
    }

    private bool campoValido(int colunaVerificar, int linhaVerificar)
    {
        return campoValidoSelecao(colunaVerificar, linhaVerificar) && !camposDesbloqueados[colunaVerificar, linhaVerificar];
    }

    private void desbloquearCampo(int[] campo, int quantidadeMinas)
    {
        if (quantidadeMinas != 0)
        {
            textosCampos[campo[0], campo[1]].enabled = true;
            textosCampos[campo[0], campo[1]].text = quantidadeMinas.ToString();
            textosCampos[campo[0], campo[1]].overrideColorTags = true;
            textosCampos[campo[0], campo[1]].color = textosCampoMinasMap[quantidadeMinas];
        }

        camposDesbloqueados[campo[0], campo[1]] = true;
        camposMinas[campo[0], campo[1]].GetComponent<Animator>().SetBool("Desabilitado", true);
    }

    private void verificarReinicioCampo()
    {
        if (quantidadeBlocosDescobertos % QUANTIDADE_CAMPOS_NAO_MINAS == 0)
        {
            inicializacaoCampo();
            reiniciarEstadoCampos();
            reiniciarTextosCampos();
        }
    }

    private void reiniciarEstadoCampos()
    {
        foreach (GameObject campo in camposMinas)
        {
            campo.GetComponent<MultiCampoMina>().setBandeira(false);
            campo.GetComponent<Animator>().SetBool("Desabilitado", false);
            campo.GetComponent<Animator>().SetBool("Selecionado", false);
            campo.GetComponent<Animator>().SetBool("Bandeira", false);
        }
    }

    private void reiniciarTextosCampos()
    {
        foreach (TMP_Text textoCampo in textosCampos)
        {
            textoCampo.color = new Color32(255, 255, 255, 255);
            textoCampo.text = "";
            textoCampo.enabled = false;
        }
    }

    public void trocarBandeiraCampo(int[] campo)
    {
        if (!camposDesbloqueados[campo[0], campo[1]] && IsLocalPlayer)
        {
            if (bandeiras[campo[0], campo[1]])
            {
                bandeiras[campo[0], campo[1]] = false;
                camposMinas[campo[0], campo[1]].GetComponent<MultiCampoMina>().setBandeira(false);
                camposMinas[campo[0], campo[1]].GetComponent<Animator>().SetBool("Bandeira", false);
                bandeiraRestantes++;
            }
            else if (bandeiraRestantes > 0)
            {
                bandeiras[campo[0], campo[1]] = true;
                camposMinas[campo[0], campo[1]].GetComponent<MultiCampoMina>().setBandeira(true);
                camposMinas[campo[0], campo[1]].GetComponent<Animator>().SetBool("Bandeira", true);
                bandeiraRestantes--;
            }

            quantidadeBandeirasRestantes.text = bandeiraRestantes.ToString();
        }
    }

    public void selecionarCamposRedor(int[] campo)
    {
        if (IsLocalPlayer)
        {
            int coluna = campo[0];
            int linha = campo[1];

            for (int i = 0; i < 8; i++)
            {
                int colunaVerificar = coluna + iy[i];
                int linhaVerificar = linha + ix[i];

                if (campoValidoSelecao(colunaVerificar, linhaVerificar))
                {
                    camposMinas[colunaVerificar, linhaVerificar].GetComponent<Animator>().SetBool("Selecionado", true);
                }
            }
        }
    }

    public void deselecionarCamposRedor(int[] campo)
    {
        if (IsLocalPlayer)
        {
            int coluna = campo[0];
            int linha = campo[1];

            for (int i = 0; i < 8; i++)
            {
                int colunaVerificar = coluna + iy[i];
                int linhaVerificar = linha + ix[i];

                if (campoValidoSelecao(colunaVerificar, linhaVerificar))
                {
                    camposMinas[colunaVerificar, linhaVerificar].GetComponent<Animator>().SetBool("Selecionado", false);
                }
            }
        }
    }

    private bool campoValidoSelecao(int colunaVerificar, int linhaVerificar)
    {
        return colunaVerificar >= 0 && colunaVerificar < 15 && linhaVerificar >= 0 && linhaVerificar < 18 && !camposDesbloqueados[colunaVerificar, linhaVerificar];
    }

    public void removerCamposRedor(int[] campo)
    {
        if (IsLocalPlayer)
        {
            deselecionarCamposRedor(campo);

            if (contagemMinasRondando[campo[0], campo[1]] > 0 && contarBandeirasProximas(campo) == contagemMinasRondando[campo[0], campo[1]])
            {
                int coluna = campo[0];
                int linha = campo[1];

                for (int i = 0; i < 8; i++)
                {
                    int colunaVerificar = coluna + iy[i];
                    int linhaVerificar = linha + ix[i];

                    if (campoValido(colunaVerificar, linhaVerificar) && !bandeiras[colunaVerificar, linhaVerificar])
                    {
                        int[] campoVerificar = { colunaVerificar, linhaVerificar };
                        selecionarCampoMina(campoVerificar);
                    }
                }
            }

            atualizarGraficosCampoAdversarioServerRpc(Serialize(camposDesbloqueados), Serialize(contagemMinasRondando), quantidadeBlocosDescobertos, vidas);
        }
    }

    private int contarBandeirasProximas(int[] campo)
    {
        int quantidadeBandeiras = 0;

        int coluna = campo[0];
        int linha = campo[1];

        for (int i = 0; i < 8; i++)
        {
            int colunaVerificar = coluna + iy[i];
            int linhaVerificar = linha + ix[i];

            if (campoValido(colunaVerificar, linhaVerificar) && bandeiras[colunaVerificar, linhaVerificar])
            {
                quantidadeBandeiras++;
            }
        }

        return quantidadeBandeiras;
    }

    [ServerRpc]
    private void atualizarGraficosCampoAdversarioServerRpc(byte[] camposDesbloqueados, byte[] contagemMinasRondando, int quantidadeBlocosDescobertos, int vidas)
    {
        if (!IsLocalPlayer)
        {
            atualizarCamposAdversario(camposDesbloqueados);
            atualizarTextosAdversario(contagemMinasRondando);
            atualizarQuantidadeBlocosDescobertosAdversario(quantidadeBlocosDescobertos);
            atualizarVidasAdversario(vidas);
        }

        atualizarGraficosCampoAdversarioClientRpc(camposDesbloqueados, contagemMinasRondando, quantidadeBlocosDescobertos, vidas);
    }

    [ClientRpc]
    private void atualizarGraficosCampoAdversarioClientRpc(byte[] camposDesbloqueados, byte[] contagemMinasRondando, int quantidadeBlocosDescobertos, int vidas)
    {
        if (!IsLocalPlayer && !IsServer)
        {
            atualizarCamposAdversario(camposDesbloqueados);
            atualizarTextosAdversario(contagemMinasRondando);
            atualizarQuantidadeBlocosDescobertosAdversario(quantidadeBlocosDescobertos);
            atualizarVidasAdversario(vidas);
        }
    }

    private void atualizarCamposAdversario(byte[] camposDesbloqueados)
    {
        bool[,] camposDesbloquadosObj = Deserialize<bool[,]>(camposDesbloqueados);

        Transform[] botoesCamposEncontrados = GameObject.Find("BotoesAdvCampos").GetComponentsInChildren<Transform>();

        int linha = 0;
        int coluna = -1;

        foreach (Transform botao in botoesCamposEncontrados)
        {
            if (linha < 18 && coluna >= 0)
            {
                botao.gameObject.GetComponent<Animator>().SetBool("Desabilitado", camposDesbloquadosObj[coluna, linha]);
            }

            coluna++;

            if (coluna == 15)
            {
                coluna = 0;
                linha++;
            }
        }
    }

    private void atualizarTextosAdversario(byte[] contagemMinasRondando)
    {
        int[,] contagemMinasRondandoObj = Deserialize<int[,]>(contagemMinasRondando);

        TMP_Text[] textosCamposEncontrados = GameObject.Find("CamposAdvText").GetComponentsInChildren<TMP_Text>();

        int linha = 0;
        int coluna = 0;

        foreach (TMP_Text texto in textosCamposEncontrados)
        {
            if(contagemMinasRondandoObj[coluna, linha] > 0)
            {
                texto.enabled = true;
                texto.text = contagemMinasRondandoObj[coluna, linha].ToString();
                texto.overrideColorTags = true;
                texto.color = textosCampoMinasMap[contagemMinasRondandoObj[coluna, linha]];
            } 
            else
            {
                texto.enabled = false;
                texto.color = new Color32(255, 255, 255, 255);
                texto.text = "";
                texto.enabled = false;
            }

            coluna++;

            if (coluna == 15)
            {
                coluna = 0;
                linha++;
            }
        }
    }

    private void atualizarQuantidadeBlocosDescobertosAdversario(int quantidadeBlocosDescobertos)
    {
        TMP_Text quantidadeBlocosDescobertosAdvTxt = GameObject.Find("TxtBlocoDescobertoAdv").GetComponent<TMP_Text>();
        quantidadeBlocosDescobertosAdvTxt.text = quantidadeBlocosDescobertos.ToString();

        quantidadeBlocosDescobertosAdv = quantidadeBlocosDescobertos;
    }

    private void atualizarVidasAdversario(int vidas)
    {
        Animator[] vidasAdvAnim = GameObject.Find("VidasAdv").GetComponentsInChildren<Animator>();

        switch (vidas)
        {
            case 1:
                vidasAdvAnim[0].SetBool("Vazia", false);
                vidasAdvAnim[1].SetBool("Vazia", true);
                vidasAdvAnim[2].SetBool("Vazia", true);
                break;
            case 2:
                vidasAdvAnim[0].SetBool("Vazia", false);
                vidasAdvAnim[1].SetBool("Vazia", false);
                vidasAdvAnim[2].SetBool("Vazia", true);
                break;
            case 3:
                vidasAdvAnim[0].SetBool("Vazia", false);
                vidasAdvAnim[1].SetBool("Vazia", false);
                vidasAdvAnim[2].SetBool("Vazia", false);
                break;
            default:
                vidasAdvAnim[0].SetBool("Vazia", true);
                vidasAdvAnim[1].SetBool("Vazia", true);
                vidasAdvAnim[2].SetBool("Vazia", true);
                break;
        }
    }

    [ClientRpc]
    private void mostrarTelaFinalTempoClientRpc()
    {
        GameObject gameOverScreen = GameObject.Find("GameOverScreen").transform.GetChild(0).gameObject;

        gameOverScreen.SetActive(true);

        if(quantidadeBlocosDescobertos > quantidadeBlocosDescobertosAdv)
        {
            GameObject.Find("TxtGameOver").GetComponent<TMP_Text>().text = "Você Ganhou!";
        }
        else if(quantidadeBlocosDescobertos < quantidadeBlocosDescobertosAdv)
        {
            GameObject.Find("TxtGameOver").GetComponent<TMP_Text>().text = "Você Perdeu!";
        }
        else
        {
            GameObject.Find("TxtGameOver").GetComponent<TMP_Text>().text = "Vocês Empataram!";
        }

        NetworkManager.Singleton.Shutdown();
    }

    [ClientRpc]
    private void mostrarTelaFinalVidaClientRpc()
    {
        GameObject gameOverScreen = GameObject.Find("GameOverScreen").transform.GetChild(0).gameObject;
        gameOverScreen.SetActive(true);

        perdeu = true;

        if (!IsLocalPlayer)
        {
            GameObject.Find("TxtGameOver").GetComponent<TMP_Text>().text = "Você Ganhou!";
        }
        else
        {
            GameObject.Find("TxtGameOver").GetComponent<TMP_Text>().text = "Você Perdeu!";
        }
        
        NetworkManager.Singleton.Shutdown();
    }

    private static byte[] Serialize(int[,] toSerialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, toSerialize);

        return ms.ToArray();
    }

    private static byte[] Serialize(bool[,] toSerialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, toSerialize);
        
        return ms.ToArray();
    }

    private static T Deserialize<T>(byte[] toDeserialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(toDeserialize);

        return (T)bf.Deserialize(ms);
    }
}
