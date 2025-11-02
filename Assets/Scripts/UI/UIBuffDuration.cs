using System;
using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UIBuffDuration : MonoBehaviour
{
    [SerializeField] private RectTransform soapbarIcon; // Reference to the soapbar icon GameObject
    [SerializeField] private RectTransform glovesIcon;
    [SerializeField] private RectTransform hairnetIcon;
    [SerializeField, Range(0, 100)] private int slideInSpeed = 5;
    [SerializeField, Range(0, 100)] private int slideOutSpeed = 5;

    [SerializeField, Range(0, 255)] private int maxAlpha = 255;
    [SerializeField, Range(0, 255)] private int minAlpha = 145;
    [SerializeField, Range(1.0f, 100)] private float flashSpeed = 5.0f;
 
    private Image soapbarImage;
    private Image glovesImage;
    private Image hairnetImage;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Image component from the RectTransform
        soapbarImage = soapbarIcon.GetComponent<Image>();
        glovesImage = glovesIcon.GetComponent<Image>();
        hairnetImage = hairnetIcon.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        updateIconEntry();
        updateIconFlash();
        updateIconExit();
    }

    private void updateIconFlash()
    {
        if (soapbarIcon.pivot.x < 3.5 && Movement.invincibilityTimer < 2.5f)
        {
            //Debug.Log(soapbarImage + " " + soapbarImage.color);
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f; // oscillates 0–1
            float alpha = Mathf.Lerp(minAlpha / 255, maxAlpha / 255, t);

            soapbarImage.color = new Color(1, 1, 1, alpha);
        }
        else
        {
            Color c = soapbarImage.color;
            if (c.a < 1)
            {
                Math.Max(c.a += 1 * Time.deltaTime, 1.0f);
                soapbarImage.color = new Color(1, 1, 1, c.a);
            }
        }

        if (glovesIcon.pivot.x < 3.5 && Movement.slowTimer < 2.5f)
        {
            //Debug.Log(soapbarImage + " " + soapbarImage.color);
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f; // oscillates 0–1
            float alpha = Mathf.Lerp(minAlpha / 255, maxAlpha / 255, t);

            glovesImage.color = new Color(1, 1, 1, alpha);
        }
        else
        {
            Color c = glovesImage.color;
            if (c.a < 1)
            {
                Math.Max(c.a += 1 * Time.deltaTime, 1.0f);
                glovesImage.color = new Color(1, 1, 1, c.a);
            }
        }

        if (hairnetIcon.pivot.x < 3.5 && Movement.doublePointsTimer < 2.5f)
        {
            //Debug.Log(soapbarImage + " " + soapbarImage.color);
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f; // oscillates 0–1
            float alpha = Mathf.Lerp(minAlpha / 255, maxAlpha / 255, t);

            hairnetImage.color = new Color(1, 1, 1, alpha);
        }
        else
        {
            Color c = hairnetImage.color;
            if (c.a < 1)
            {
                Math.Max(c.a += 1 * Time.deltaTime, 1.0f);
                hairnetImage.color = new Color(1, 1, 1, c.a);
            }
        }
    }

    private void updateIconEntry()
    {
        if (Invincibility_Powerup.invincibilityActive)
        {
            //play a animation to make the icon slide in if it's out of view
            if (soapbarIcon.pivot.x > 0.5)
            {
                float newX = soapbarIcon.pivot.x - (slideInSpeed * Time.deltaTime);
                soapbarIcon.pivot = new Vector2(Mathf.Max(newX, 0.5f), soapbarIcon.pivot.y);
            }
        }

        if (Slowdown_Glove.slowActive)
        {
            //play a animation to make the icon slide in if it's out of view
            if (glovesIcon.pivot.x > 0.5)
            {
                float newX = glovesIcon.pivot.x - (slideInSpeed * Time.deltaTime);
                glovesIcon.pivot = new Vector2(Mathf.Max(newX, 0.5f), glovesIcon.pivot.y);
            }
        }

        if (Double_Points.doublePointsActive)
        {
            //play a animation to make the icon slide in if it's out of view
            if (hairnetIcon.pivot.x > 0.5)
            {
                float newX = hairnetIcon.pivot.x - (slideInSpeed * Time.deltaTime);
                hairnetIcon.pivot = new Vector2(Mathf.Max(newX, 0.5f), hairnetIcon.pivot.y); //clamp to 0.5
            }
        }
    }
    
    private void updateIconExit()
    {
        if (!Invincibility_Powerup.invincibilityActive)
        {
            //play a animation to make the icon slide out
            if (soapbarIcon.pivot.x < 5)
            {
                float newX = soapbarIcon.pivot.x + (slideOutSpeed * Time.deltaTime);
                soapbarIcon.pivot = new Vector2(Mathf.Min(newX, 5f), soapbarIcon.pivot.y); //clamp to 5
            }
        }

        if (!Slowdown_Glove.slowActive)
        {
            //play a animation to make the icon slide out
            if (glovesIcon.pivot.x < 5)
            {
                float newX = glovesIcon.pivot.x + (slideOutSpeed * Time.deltaTime);
                glovesIcon.pivot = new Vector2(Mathf.Min(newX, 5f), glovesIcon.pivot.y); //clamp to 5
            }
        }
        
        if (!Double_Points.doublePointsActive)
        {
            //play a animation to make the icon slide out
            if (hairnetIcon.pivot.x < 5)
            {
                float newX = hairnetIcon.pivot.x + (slideOutSpeed * Time.deltaTime);
                hairnetIcon.pivot = new Vector2(Mathf.Min(newX, 5f), hairnetIcon.pivot.y); //clamp to 5
            }
        }
    }
}
