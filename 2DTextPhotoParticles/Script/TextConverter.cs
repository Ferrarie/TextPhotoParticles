using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextConverter
{
    public Texture2D TextToTexture(Font customFont,string text, int space, Color textColor)
    {
        Texture2D textTexture;
        Texture2D fontTexture = (Texture2D)customFont.material.mainTexture;
        Texture2D convertFontTexture;
        convertFontTexture = duplicateTexture(fontTexture);

        char[] charText = text.ToCharArray();

        bool isRotation = false;

        int textWidth = 0;
        int textHeight = 0;

        int charOffset = 0;

        calcTextSize(customFont, text,space, convertFontTexture,out textWidth, out textHeight);

        textTexture = new Texture2D(textWidth, textHeight);

        Color[] emptyColor = new Color[textWidth * textHeight];
        for(int i=0;i<emptyColor.Length;i++)
        {
            emptyColor[i] = new Color(0, 0, 0, 0);
        }
        textTexture.SetPixels(emptyColor);

        for (int c = 0; c < charText.Length; c++)
        {
            isRotation = false;

            CharacterInfo info;
            customFont.GetCharacterInfo(charText[c], out info);

            int cx = (int)(convertFontTexture.width * info.uvTopLeft.x);
            int cy = (int)(convertFontTexture.height * info.uvTopLeft.y);
            int cw = (int)(convertFontTexture.width * info.uvBottomRight.x);
            int ch = (int)(convertFontTexture.height * info.uvBottomRight.y);

            if (cx > cw)
            {
                int n = cw;
                cw = cx;
                cx = n;
                isRotation = true;
            }
            if (cy > ch)
            {
                int n = ch;
                ch = cy;
                cy = n;
                isRotation = true;
            }

            cw = cw - cx;
            ch = ch - cy;


            Color[] fontColor = convertFontTexture.GetPixels(cx, cy, cw, ch);


            if (isRotation == false)
            {
                //上下颠倒
                for (int j = -1; j < ch - 1; j++)
                {
                    for (int i = 0; i < cw; i++)
                    {
                        Color color = textColor;
                        color.a = fontColor[(j + 1) * cw + i].a;
                        if (color.a != 0)
                            textTexture.SetPixel(charOffset + i, ch - j - 1, color);
                    }
                }
            }
            else
            {

                //向左旋转90度
                for (int j = 0; j < ch; j++)
                {
                    for (int i = 0; i < cw; i++)
                    {
                        Color color = textColor;
                        color.a = fontColor[j * cw + i].a;

                        //tempTex[c].SetPixel(ch - j, i, color);
                        if(color.a!=0)
                            textTexture.SetPixel(charOffset + ch - j, i, color);
                    }
                }

            }

            if (isRotation == false)
                charOffset += cw + space;
            else
                charOffset += ch + space;

            textTexture.Apply();
        }

        return textTexture;
    }

    void calcTextSize(Font customFont,string text,int space,Texture2D convertFontTexture, out int width,out int height)
    {
        bool isRotation = false;
        width = 0;
        height = 0;

        char[] charText = text.ToCharArray();
        for (int c = 0; c < charText.Length; c++)
        {
            CharacterInfo info;
            customFont.GetCharacterInfo(charText[c], out info);

            int cx = (int)(convertFontTexture.width * info.uvTopLeft.x);
            int cy = (int)(convertFontTexture.height * info.uvTopLeft.y);
            //int cw = (int)Mathf.Abs(convertTex.width * info.uvBottomRight.x - cx);
            //int ch = (int)Mathf.Abs(convertTex.height * info.uvBottomRight.y - cy);
            int cw = (int)(convertFontTexture.width * info.uvBottomRight.x);
            int ch = (int)(convertFontTexture.height * info.uvBottomRight.y);

            if (cx > cw)
            {
                int n = cw;
                cw = cx;
                cx = n;
                isRotation = true;
            }
            if (cy > ch)
            {
                int n = ch;
                ch = cy;
                cy = n;
                isRotation = true;
            }

            cw = cw - cx;
            ch = ch - cy;

            if (!isRotation)
            {
                width += cw + space;
                if (ch > height)
                    height = ch;
            }
            else
            {
                width += ch + space;
                if (cw > height)
                    height = cw;
            }

        }
    }

    private Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        //Texture2D readableText = new Texture2D(source.width, source.height,  TextureFormat.Alpha8,false);
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
