using UnityEngine;

[CreateAssetMenu(fileName = "BattleGridTile", menuName = "Battle/GridTile", order = 0)]
public class BattleGridTile : ScriptableObject
{

    [SerializeField]
    private Texture2D texture;

    public Texture2D Texture
    {
        get => texture;
    }

}