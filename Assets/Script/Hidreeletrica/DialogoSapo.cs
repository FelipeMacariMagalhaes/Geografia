using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class DialogoSapo : MonoBehaviour
{
    public enum Quem { Player, Sapo }

    [System.Serializable]
    public class Fala
    {
        public Quem quemFala;
        [TextArea(2, 4)]
        public string texto;
    }

    [Header("UI")]
    public GameObject painelDialogo;
    public TextMeshProUGUI nomePlayerTexto;
    public TextMeshProUGUI nomeSapoTexto;
    public TextMeshProUGUI falaTexto;

    [Header("Sapo")]
    public GameObject sapoSprite; // arraste o sapo aqui (ou deixe vazio para usar o próprio objeto)

    [Header("Nomes")]
    public string nomePlayer = "Player";
    public string nomeSapo = "Sapo";

    [Header("Dialogo")]
    public Fala[] falas;

    int indice;
    int frameInicio;
    bool dialogoAtivo;

    void OnMouseDown()
    {
        if (dialogoAtivo) return;
        IniciarDialogo();
    }

    void Update()
    {
        if (!dialogoAtivo) return;
        if (Time.frameCount == frameInicio) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            ProximaFala();
    }

    void IniciarDialogo()
    {
        dialogoAtivo = true;
        frameInicio = Time.frameCount;
        indice = 0;
        painelDialogo.SetActive(true);
        MostrarFala();
    }

    void MostrarFala()
    {
        Fala f = falas[indice];

        nomePlayerTexto.text = nomePlayer;
        nomeSapoTexto.text = nomeSapo;

        nomePlayerTexto.gameObject.SetActive(f.quemFala == Quem.Player);
        nomeSapoTexto.gameObject.SetActive(f.quemFala == Quem.Sapo);

        falaTexto.text = f.texto;
    }

    void ProximaFala()
    {
        indice++;
        if (indice >= falas.Length)
        {
            FecharDialogo();
            return;
        }
        MostrarFala();
    }

    void FecharDialogo()
    {
        dialogoAtivo = false;
        painelDialogo.SetActive(false);

        var sr = (sapoSprite != null ? sapoSprite : gameObject).GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.DOFade(0f, 0.5f).OnComplete(() => sr.gameObject.SetActive(false));
    }
}