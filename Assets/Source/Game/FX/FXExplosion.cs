using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXExplosion : MonoBehaviour
{
    public Color color = Color.white;

    public float multiply = 1f;

    public bool destroy = false;

    private Color tColor = Color.black;

    private float tMultiply = 1f;

    private SpriteRenderer tRenderer;

    private MaterialPropertyBlock block;
    private int colorID;

    void Start()
    {
        tRenderer = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
        colorID = Shader.PropertyToID("_GlowColor");
    }

    void Update()
    {
        if (multiply != tMultiply)
        {
            tMultiply = multiply;
            tColor.r = color.r * tMultiply;
            tColor.g = color.g * tMultiply;
            tColor.b = color.b * tMultiply;
            tColor.a = 0f;

            block.SetColor(colorID, tColor);
            tRenderer.SetPropertyBlock(block);
        }

        if(destroy)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
