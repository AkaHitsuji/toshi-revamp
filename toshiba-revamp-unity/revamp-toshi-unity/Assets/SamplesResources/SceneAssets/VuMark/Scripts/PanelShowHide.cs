/*===============================================================================
Copyright (c) 2016-2017 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;

public class PanelShowHide : MonoBehaviour
{
    public Animator m_Animator;
    public Image m_Image;
    public Text m_Info;

    public void Hide()
    {
        m_Animator.SetTrigger("HidePanel");
    }

    public void Show(string name, string price, string description, Sprite image, bool isGold)
    {
        m_Animator.ResetTrigger("HidePanel");
        GameObject vuMarkPanel = GameObject.FindWithTag("vumarkpanel");
        string congrats = "Congratulations! You have found the golden ticket! Take a further 20% off!";
        if (isGold) {
            // figure out how to make transparency
            vuMarkPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(191, 162, 0, 203);
            Debug.Log("panel color is: " + vuMarkPanel.GetComponent<UnityEngine.UI.Image>().color);
            m_Info.text = "<color=white>" + congrats + "</color>" + "\n\n" + "<color=black>Item Name: </color>" + name + "\n" +
                        "<color=black>Price: </color>" + price +
                        "\n\n<color=black>Item Description: </color>" +
                description + "\n";
        }

        else {
            // figure out how to make transparency
            vuMarkPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(212, 212, 212, 210);
            Debug.Log("panel color is: " + vuMarkPanel.GetComponent<UnityEngine.UI.Image>().color);
            m_Info.text =
                        "<color=black>Item Name: </color>" + name + "\n" +
                        "<color=black>Price: </color>" + price +
                        "\n\n<color=black>Item Description: </color>" +
                        description + "\n";
        }

        m_Image.sprite = image;

        if (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("ShowAnim"))
        {
            m_Animator.SetTrigger("ShowPanel");
        }
    }

    public void ResetShowTrigger()
    {
        m_Animator.ResetTrigger("ShowPanel");
    }
}
