﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErksUnityLibrary
{
    public class TypeWriter : MonoBehaviour
    {
        public Text targetText;

        public TextAudioType textAudioType;
        public enum TextAudioType { None, Mumbling, Typing, VoiceOver }
        public bool typeDurationSameAsVoiceOver = false;
        private float voiceOverDuration = 0f;

        public AudioSource audioSource;
        public List<AudioClip> typeClips;

        public TextSkipType textSkipType = TextSkipType.SlowFastSkip;
        public enum TextSkipType { SlowFastSkip, SlowSkip }

        public bool pauseGame = true;
        public float waitTime = 0.1f;
        public float waitTimeFast = 0.01f;
        private float currentWaitTime;

        public float specialCharsTimeMuliplier = 5f;
        public List<string> specialChars;

        private string text;
        private string finalText;

        private const string inputButton = "Fire1";

        private bool isTyping = false;

        private System.Action callback;

        private void Awake()
        {
            if (!audioSource)
                audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            CheckTyping();
        }

        private void CheckTyping()
        {
            if (isTyping)
            {
                CheckSpeechPlayback();

                if (Input.GetButtonDown(inputButton))
                {
                    if (textSkipType == TextSkipType.SlowSkip)
                    {
                        Skip();
                    }
                    else
                    {
                        if (currentWaitTime == waitTime)
                        {
                            currentWaitTime = waitTimeFast;
                        }
                        else
                        {
                            if (currentWaitTime == waitTimeFast)
                            {
                                Skip();
                            }
                        }
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown(inputButton))
                {
                    ClearText();

                    if (pauseGame)
                        Time.timeScale = 1f;

                    if (callback != null)
                    {
                        callback();
                        callback = null;
                    }                        
                }
            }
        }

        private void Skip()
        {
            //Debug.Log("skip text");
            StopAllCoroutines();
            targetText.text += text;
            text = "";
            OnTypingEnded();
        }

        public void SetText(string text)
        {
            if (pauseGame)
                Time.timeScale = 0f;

            currentWaitTime = waitTime;
            this.text = text;
            ClearText();
            isTyping = true;
            finalText = text;
            StartCoroutine(Type());
        }

        public void SetText(string text, AudioClip voiceOver)
        {
            CheckVoiceOver(voiceOver);
            SetText(text);
        }

        public void ClearText()
        {
            //Debug.Log("clear text");
            targetText.text = "";
        }

        IEnumerator Type()
        {
            if (text.Length > 0)
            {
                string firstChar = text[0].ToString();
                targetText.text += firstChar;
                CheckTypeSound();

                if(voiceOverDuration != 0f)
                    yield return new WaitForSecondsRealtime(voiceOverDuration / finalText.Length);
                else
                {
                    if (specialChars.Contains(firstChar))
                    {
                        yield return new WaitForSecondsRealtime(currentWaitTime * specialCharsTimeMuliplier);
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(currentWaitTime);
                    }
                }

                text = text.Substring(1);
                StartCoroutine(Type());
            }
            else
            {
                OnTypingEnded();
            }
        }

        private void OnTypingEnded()
        {
            isTyping = false;

            if (audioSource)
                audioSource.Stop();
        }

        private void CheckSpeechPlayback()
        {
            if (textAudioType == TextAudioType.Mumbling)
            {
                if (audioSource != null)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.PlayRandomVolumePitch(typeClips.GetRandomItem());
                    }
                }
            }
        }

        private void CheckVoiceOver(AudioClip voiceOver)
        {
            if (textAudioType == TextAudioType.VoiceOver)
            {
                if (audioSource)
                {
                    if (typeDurationSameAsVoiceOver)
                        voiceOverDuration = voiceOver.length;

                    audioSource.clip = voiceOver;
                    audioSource.Play();
                }
            }
        }

        private void CheckTypeSound()
        {
            if (textAudioType == TextAudioType.Typing)
            {
                if (audioSource)
                {
                    audioSource.PlayOneShotRandomVolumePitch(typeClips.GetRandomItem());
                }
            }
        }
    }
}
