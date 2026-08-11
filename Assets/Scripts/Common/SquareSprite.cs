using UnityEngine;

namespace HerOClock.Common
{
    /// <summary>
    /// Sprite branco de 1x1 unidade de mundo, criado em tempo de execucao.
    ///
    /// Enquanto o jogo roda com game objects lisos e sem textura, tudo que aparece na
    /// tela e este quadrado pintado por SpriteRenderer.color. Assim nao precisamos de
    /// nenhum arquivo de imagem no projeto ainda.
    /// </summary>
    public static class SquareSprite
    {
        private static Sprite cached;

        public static Sprite Get()
        {
            if (cached != null)
            {
                return cached;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "SquareSpriteTexture";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // Com pixelsPerUnit igual a 1 em uma textura de 1x1, o sprite mede
            // exatamente 1 unidade de mundo. O tamanho final vem da escala do objeto.
            cached = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            cached.name = "SquareSprite";

            return cached;
        }
    }
}
