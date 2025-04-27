using UnityEngine;
using UnityEngine.EventSystems;

public class CampoMina : MonoBehaviour, IPointerClickHandler
{
    private GameManager gameManager;
    private int[] campo = new int[2];

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        
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
