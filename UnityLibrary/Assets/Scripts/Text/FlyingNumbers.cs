﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FlyingNumbers : MonoBehaviour
{
    [Header("Position")]
    public Transform targetTransform;
    public bool moveWithTargetTransform = true;
    public Vector3 initialOffset = Vector3.zero;
    public Vector3 finalOffset = new Vector3(0f, 1f, 0f);

    [Header("Movement")]
    public float movementDuration = 1f;
    public float delayBeforeDisappear = 0f;
    public Ease movementEase = Ease.OutCubic;

    [Header("Fading")]
    public bool fade = true;
    public float fadeInDuration = 0.5f;
    public Ease fadeInEase = Ease.InQuart;
    public float fadeOutDuration = 0.5f;
    public Ease fadeOutEase = Ease.OutQuart;

    [Header("Text")]
    public bool useOnlyOneText = false;
    public Text protoText;
    private List<Text> texts = new List<Text>();
    private Text currentText;

    public List<Color> colors;
    private Color currentColor;

    public bool faceCamera = true;

    private float currentScale = 1f;
    private int initalTextSize;

    private void Awake()
    {
        if (!protoText)
            protoText = GetComponentInChildren<Text>();
    }

    // Use this for initialization
    void Start ()
    {
        transform.position = targetTransform.position + initialOffset;

        texts.Add(protoText);
        initalTextSize = protoText.fontSize;

        if(colors.Count > 0)
        {
            currentColor = colors[0];
        }
        else
        {
            currentColor = Color.white;
        }        
	}
	
    public void StartFlying(string text)
    {
        FindAvailableText();

        currentText.transform.localPosition = Vector3.zero;
        currentText.text = text;        
        currentText.fontSize = (int)(initalTextSize * currentScale);

        StartCoroutine(FlyingSequence(currentText));
    }

    private IEnumerator FlyingSequence(Text text)
    {
        yield return new WaitForEndOfFrame();

        if (fade)
        {
            StartCoroutine(FadeSequence(text));
        }
        else
        {
            text.color = currentColor;
        }

        if (moveWithTargetTransform)
        {
            text.transform.DOLocalMove(finalOffset, movementDuration).SetEase(movementEase);
        }
        else
        {
            text.transform.DOMove(finalOffset, movementDuration).SetEase(movementEase);
        }

        yield return new WaitForSeconds(movementDuration);
        yield return new WaitForSeconds(delayBeforeDisappear);

        text.text = "";
    }

    private IEnumerator FadeSequence(Text text)
    {
        Color transparent = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
        text.color = transparent;
        text.DOColor(currentColor, fadeInDuration).SetEase(fadeInEase);

        float fadeDelay = movementDuration + delayBeforeDisappear - fadeInDuration - fadeOutDuration;
        if (fadeDelay < 0f)
            fadeDelay = 0f;

        yield return new WaitForSeconds(fadeInDuration);

        yield return new WaitForSeconds(fadeDelay);

        text.DOColor(transparent, fadeOutDuration).SetEase(fadeOutEase);

        yield return new WaitForSeconds(fadeOutDuration);
    }
    
    public void StartFlying(int number)
    {
        StartFlying(number.ToString());
    }

    public void SetColor(int colorId)
    {
        if(colorId > colors.Count)
        {
            currentColor = colors[colorId];
        }
    }

    public void SetScale(float newScale)
    {
        currentScale = newScale;
    }

    private void FindAvailableText()
    {
        if(useOnlyOneText)
        {
            StopAllCoroutines();
            protoText.DOKill();
            protoText.transform.DOKill();
            currentText = protoText;
            return;
        }

        foreach(Text text in texts)
        {
            if(text.text == "")
            {
                currentText = text;
                return;
            }
        }

        texts.Add(Instantiate(protoText, transform.position, Quaternion.identity, transform));
        currentText = texts[texts.Count - 1];
    }
}
