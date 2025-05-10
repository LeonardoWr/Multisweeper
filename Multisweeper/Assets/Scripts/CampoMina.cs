using UnityEngine;
using UnityEngine.EventSystems;

public class CampoMina : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Animator anim;
    private bool bandeira;
    private int[] campo = new int[2];
    private GameManager gameManager;
    private bool hover;
    private bool selecionado;
    private bool selecionadoM;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            anim.SetBool("Selecionado", false);
            
            if (!bandeira)
            {
                selecionado = true;
            }
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Middle)
        {
            anim.SetBool("Selecionado", false);
            selecionadoM = true;

            gameManager.selecionarCamposRedor(campo);
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            gameManager.trocarBandeiraCampo(campo);
        }
        
    }
    
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if(hover)
        {
            anim.SetBool("Selecionado", true);
        }

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (selecionado)
            {
                anim.SetBool("Selecionado", false);

                if(!bandeira)
                {
                    gameManager.selecionarCampoMina(campo);
                }
            }
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Middle)
        {
            if (selecionadoM)
            {
                anim.SetBool("Selecionado", false);
                gameManager.removerCamposRedor(campo);
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        anim.SetBool("Selecionado", true);
        hover = true;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        anim.SetBool("Selecionado", false);

        if(selecionado)
        {
            selecionado = false;
        }

        if(selecionadoM)
        {
            selecionadoM = false;
            gameManager.deselecionarCamposRedor(campo);
        }

        hover = false;
    }

    public void setBandeira(bool bandeira)
    {
        this.bandeira = bandeira;
    }

    public void setCampo(int[] campo)
    {
        this.campo = campo;
    }

    public void setGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
