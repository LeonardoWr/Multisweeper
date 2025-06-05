using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class BotoesMenuPrincipal : MonoBehaviour
{
    public void iniciarJogo()
    {
        GameObject[] gameManagers = GameObject.FindGameObjectsWithTag("GameManager");

        foreach(GameObject gameManager in gameManagers)
        {
            gameManager.GetComponent<MultiGameManager>().inicializarJogoClientRpc();
        }
    }

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

        GameObject.Find("Hostear").SetActive(false);
        GameObject.Find("Conectar").SetActive(false);
    }

    public void clientJogo()
    {
        if(NetworkManager.Singleton.ConnectedClients.Count < 2)
        {
            NetworkManager.Singleton.StartClient();

            GameObject.Find("Hostear").SetActive(false);
            GameObject.Find("Conectar").SetActive(false);
        }
    }
}
