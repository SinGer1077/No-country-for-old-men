using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    [SerializeField]
    private float eventDelay;

    [SerializeField]
    private Image winImage;
    [SerializeField]
    private Image loseImage;
    [SerializeField]
    private Image backPanel;
    [SerializeField]
    private TextMeshProUGUI retryButton;
    [SerializeField]
    private Button button;

    private void Start()
    {
        winImage.gameObject.SetActive(false);
        loseImage.gameObject.SetActive(false);
        backPanel.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        button.gameObject.SetActive(false);
    }

    public void Win()
    {
        backPanel.gameObject.SetActive(true);

        winImage.gameObject.SetActive(true);
        winImage.rectTransform.DOAnchorPos3D(new Vector3(0, 300, 0), 1.5f);

        button.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        retryButton.DOColor(new Color(retryButton.color.r, retryButton.color.g, retryButton.color.b, 1.0f), 2.0f);
    }

    public void Lose()
    {
        Invoke("UILoseEvents", eventDelay);
    }

    private void UILoseEvents()
    {
        backPanel.gameObject.SetActive(true);
        backPanel.DOColor(new Color(backPanel.color.r, backPanel.color.g, backPanel.color.b, 0.5f), 1.5f);

        loseImage.gameObject.SetActive(true);
        loseImage.rectTransform.DOAnchorPos3D(new Vector3(0, 300, 0), 1.5f);

        button.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        retryButton.DOColor(new Color(retryButton.color.r, retryButton.color.g, retryButton.color.b, 1.0f), 2.0f);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
