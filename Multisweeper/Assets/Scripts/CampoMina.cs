using UnityEngine;
using UnityEngine.EventSystems;

public class CampoMina : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private int[] campo = new int[2];

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        gameManager.selecionarCampoMina(campo);
    }

    public void setGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    
    public void setCampo(int[] campo)
    {
        this.campo = campo;
    }
}
