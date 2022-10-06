using TMPro;
using UnityEngine;
using UnityEngine.UI;
using state;

public class PlayerItem : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI label;
    public Image icon;

    public void Init(Player player)
    {
        Color color = GameScene.Instance.TeamColors[(int)player.team];
        this.player = player;
        label.color = color;
        label.text = player.name;
        icon.color = color;
    }
}
