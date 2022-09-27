using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
    public GameObject gameOverPopup;
    public GameObject continueGameAfterAdsButton;
    void Start()
    {
        continueGameAfterAdsButton.GetComponent<Button>().interactable = false;
        gameOverPopup.SetActive(false);

        GameEvents.OnGamOver += ShowGameOverPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnGamOver -= ShowGameOverPopup;
    }

    private void ShowGameOverPopup()
    {
        
        gameOverPopup.SetActive(true);
        continueGameAfterAdsButton.GetComponent<Button>().interactable = false;

    }
}
