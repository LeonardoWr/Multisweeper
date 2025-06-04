using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class BotoesMenuPrincipal : MonoBehaviour
{
    public void mudarCenaMenuPrincipal()
    {
        SceneManager.LoadScene(1);
    }
    
    public void mudarCenaSinglePlayer()
    {
        SceneManager.LoadScene(2);
    }

    public void mudarCenaMultiPlayer()
    {
        SceneManager.LoadScene(3);
    }

    public void fecharJogo()
    {
        Application.Quit();
    }

    public void hostJogo()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void clientJogo()
    {
        NetworkManager.Singleton.StartClient();
    }
}
