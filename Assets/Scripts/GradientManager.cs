using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GradientManager 
{
    public static Color Evaluate2(Color colorId1, Color colorId2)
    {
        //Crea un Gradiente de 2 colores y retorna el punto medio 
        return GetColorByGradient(colorId1, colorId2);         
    }
    public static Color Evaluate3(Color colorId1, Color colorId2, Color colorId3)
    {
        //Crea un gradiente de 3 colores y retorna el punto medio
        Color color1 = GetColorByGradient(colorId1, colorId2);
        return GetColorByGradient(color1, colorId3);
    }

    public static Color Evaluate4(int colorId1, int colorId2, int colorId3, int ColorId4)
    {
        //Crea un gradiente de 4 colores y retorna el punto medio
        Color color1 = GetColorByGradient(GetColorById(colorId1), GetColorById(colorId2));
        Color color2 = GetColorByGradient(color1, GetColorById(colorId3));
        return GetColorByGradient(color2, GetColorById(ColorId4));
    }
    private static Color GetColorByGradient(Color color1,Color color2)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color2;
        colorKey[1].time = 1.0f;
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;
        gradient.SetKeys(colorKey, alphaKey);
        return gradient.Evaluate(0.5f);
    }
    public static Color GetColorById(int id)
    {
        switch (id)
        {
            case 0:
                return Color.red;
                //return new Color(6f/255f,96f/255f,156f/255f); //water
                //return new Color(190f/255f, 238f/255f, 236f/255f);  //ice 
            case 1:
                return Color.blue;
                //return new Color(222f/255f,220f/255f,82f/255f);     //sand                           
            case 2:
                return new Color(92f / 255f, 39f / 255f, 10f / 255f); //Dirt                
                //return new Color(0f/255f,1163f/255f,38f/255f); 
            case 3:
                return Color.green;      //grass          
            case 4:
                return new Color(113f / 255f, 117f / 255f, 117f / 255f);  //rock
                //return new Color(190f/255f,238f/255f,236f/255f);  //ice             
        }
        return Color.black;
    }
}
