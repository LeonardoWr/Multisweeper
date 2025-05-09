using UnityEngine;
using UnityEngine.SceneManagement;

public class BotoesMenuPrincipal : MonoBehaviour
{
    public void mudarCenaSinglePlayer()
    {
        SceneManager.LoadScene(1);
    }
    
    public void fecharJogo()
    {
        Application.Quit();
    }
}
