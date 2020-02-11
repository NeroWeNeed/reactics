using UnityEngine;


public class BattleGridTile : ScriptableObject
{

    [SerializeField]
    private Texture2D texture;

    public Texture2D Texture
    {
        get => texture;
    }

}