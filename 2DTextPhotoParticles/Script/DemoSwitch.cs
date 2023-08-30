using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSwitch : MonoBehaviour {

    public TextPhotoParticles particleTextPhoto;

    public string text1;
    public string text2;

    public Texture2D chineseImage;

    public Texture2D logoImage;
    public Texture2D carImage;
    public Texture2D boyImage;
    public Texture2D flowerImage;
    public Texture2D animalImage;

    Texture2D currentImage = null;
    // Use this for initialization
    void Start () {
        currentImage = animalImage;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10,5,130,35),"Switch Text 1"))
        {
            particleTextPhoto.MakeTextParticle(text1);
            currentImage = null;
        }

        if (GUI.Button(new Rect(10, 40, 130, 35), "Switch Text 2"))
        {
            particleTextPhoto.MakeTextParticle(text2);
            currentImage = null;
        }

        if (GUI.Button(new Rect(10, 80, 130, 35), "Switch Chinese"))
        {
            particleTextPhoto.MakeTextureParticle(chineseImage);
            currentImage = null;

        }

        if (GUI.Button(new Rect(10, 120, 130, 35), "LOGO Photo"))
        {
            particleTextPhoto.MakeTextureParticle(logoImage);
            currentImage = logoImage;
        }

        if (GUI.Button(new Rect(10, 160, 130, 35), "Car Photo"))
        {
            particleTextPhoto.MakeTextureParticle(carImage);
            currentImage = carImage;
        }

        if (GUI.Button(new Rect(10, 200, 130, 35), "Boy Photo"))
        {
            particleTextPhoto.MakeTextureParticle(boyImage);
            currentImage = boyImage;
        }

        if (GUI.Button(new Rect(10, 240, 130, 35), "Flower Photo"))
        {
            particleTextPhoto.MakeTextureParticle(flowerImage);
            currentImage = flowerImage;
        }

        if (GUI.Button(new Rect(10, 280, 130, 35), "Animal Photo"))
        {
            particleTextPhoto.MakeTextureParticle(animalImage);
            currentImage = animalImage;
        }


        if (currentImage!=null)
        {
            GUI.DrawTexture(new Rect(10, Screen.height - 150, 256, 128), currentImage);
        }

        if (GUI.Button(new Rect(10, 350, 130, 35), "Mouse Zoom"))
        {
            particleTextPhoto.mouseInteractive = TextPhotoParticles.enumMouseInteractive.MouseZoom;
        }

        if (GUI.Button(new Rect(10, 390, 130, 35), "Mouse Bounce"))
        {
            particleTextPhoto.mouseInteractive = TextPhotoParticles.enumMouseInteractive.MouseBounce;
        }


        if (GUI.Button(new Rect(10, 450, 130, 35), "Particle Converge"))
        {
            particleTextPhoto.particleMode = TextPhotoParticles.enumParticlesMode.ParticleConverge;
        }

        if (GUI.Button(new Rect(10, 490, 130, 35), "Particle Disperse"))
        {
            particleTextPhoto.particleMode = TextPhotoParticles.enumParticlesMode.ParticleDisperse;
        }




        if (GUI.Button(new Rect(Screen.width - 150,5,130,35),"White Color"))
        {
            particleTextPhoto.fillColorType = TextPhotoParticles.enumTextureColorType.WhiteColor;
        }

        if (GUI.Button(new Rect(Screen.width - 150, 40, 130, 35), "Red Color"))
        {
            particleTextPhoto.fillColorType = TextPhotoParticles.enumTextureColorType.CustomColor;
            particleTextPhoto.customColor = new Color32(255, 0, 0, 255);
        }

        if (GUI.Button(new Rect(Screen.width - 150, 80, 130, 35), "Gray Color"))
        {
            particleTextPhoto.fillColorType = TextPhotoParticles.enumTextureColorType.GrayColor;
        }

        if (GUI.Button(new Rect(Screen.width - 150, 120, 130, 35), "Original Color"))
        {
            particleTextPhoto.fillColorType = TextPhotoParticles.enumTextureColorType.OriginalColor;
        }

        if (GUI.Button(new Rect(Screen.width - 150, 160, 130, 35), "Random Color"))
        {
            particleTextPhoto.fillColorType = TextPhotoParticles.enumTextureColorType.RandomColor;
        }
    }
}
