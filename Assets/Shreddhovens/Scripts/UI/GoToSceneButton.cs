using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GoToSceneButton : MonoBehaviour
{
    [SerializeField] string m_sceneName = "StudioScene";
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => { SceneManager.LoadScene(m_sceneName); });
    }
}
