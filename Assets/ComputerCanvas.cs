using MusicXml;
using MusicXml.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ComputerCanvas : MonoBehaviour
{
    [SerializeField] ScoreLoader m_loader;

    [SerializeField] GameObject m_welcomePanel;
    [SerializeField] GameObject m_errorPanel;
    [SerializeField] GameObject m_alreadyPlayingPanel;
    [SerializeField] GameObject m_songPanelOrigin;
    [SerializeField] GameObject m_songPanelPrefab;
    [SerializeField] GameObject m_songLinePrefab;

    [SerializeField] Button m_beginButton;
    [SerializeField] Button m_previousButton;
    [SerializeField] Button m_nextButton;
    [SerializeField] Button m_errorButton;

    [SerializeField] int m_linesPerPanel = 4;

    int m_songsPanelIdx = -1;
    List<GameObject> m_songPanels = new();

    List<(string, string)> m_songs = new();

    [SerializeField] TextMeshProUGUI m_userText;
    [SerializeField] TextMeshProUGUI m_errorText;

    void Start()
    {
        m_userText.text = $"Welcome back, " + Environment.UserName;

        m_beginButton.onClick.AddListener(OpenSongsList);

        m_welcomePanel.SetActive(true);
        m_songPanelOrigin.SetActive(false);
        m_alreadyPlayingPanel.SetActive(false) ;
        m_errorPanel.SetActive(false);
        m_previousButton.gameObject.SetActive(false);
        m_nextButton.gameObject.SetActive(false);
    }

    private void OpenSongsList()
    {
        m_welcomePanel.SetActive(false);
        m_songPanelOrigin.SetActive(true);

        string l_songsPath = Path.Combine(Application.streamingAssetsPath, "Songs");

        GameObject l_currentPanel = null;

        foreach(string l_file in Directory.GetFiles(l_songsPath))
        {
            print(l_file);
            if (l_file.EndsWith(".xml"))
            {
                try
                {
                    Score l_score = MusicXmlParser.GetScore(
                        Path.Combine(l_songsPath, l_file));

                    if (l_score != null)
                    {
                        if(l_currentPanel == null || (l_currentPanel != null && l_currentPanel.transform.childCount >= m_linesPerPanel))
                        {
                            l_currentPanel = Instantiate(m_songPanelPrefab, m_songPanelOrigin.transform);
                            m_songPanels.Add(l_currentPanel);

                            if(m_songsPanelIdx == -1)
                            {
                                RenderSongPanel(0);
                            }
                        }

                        GameObject l_newLine = Instantiate(m_songLinePrefab, l_currentPanel.transform);

                        string l_songName = l_file.Split('\\')[l_file.Split('\\').Length -1];

                        l_newLine.GetComponentsInChildren<TextMeshProUGUI>()[0].text = l_file;
                        l_newLine.GetComponentsInChildren<TextMeshProUGUI>()[1].text = l_score.Identification.Composer;

                        l_newLine.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            m_loader.StartScore(l_score,l_songName);
                            m_songPanelOrigin.SetActive(false);
                            m_alreadyPlayingPanel.SetActive(true);
                        });

                        l_newLine.transform.Translate(new(0, -.05f * m_songs.Count, 0));

                        m_songs.Add((l_file, l_score.Identification.Composer));
                    }
                }
                catch (Exception e)
                {
                    m_errorPanel.SetActive(true);
                    m_errorText.text = $"File {l_file} could is impossible to read :\n"+e.Message;
                }
            }
        }
    }

    void RenderSongPanel(int p_index)
    {
        m_songsPanelIdx = p_index;

        m_previousButton.gameObject.SetActive(m_songsPanelIdx > 0);
        m_nextButton.gameObject.SetActive(m_songsPanelIdx < m_songPanels.Count-2);
    }
}
