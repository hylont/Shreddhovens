using MusicXml;
using MusicXml.Domain;
using SFB;
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
    [Header("Loader")]
    [SerializeField] ScoreLoader m_loader;

    [Header("Child UI panels")]
    [SerializeField] GameObject m_welcomePanel;
    [SerializeField] GameObject m_errorPanel;
    [SerializeField] GameObject m_alreadyPlayingPanel;
    [SerializeField] GameObject m_songPanelOrigin;
    [SerializeField] GameObject m_songPanelPrefab;
    [SerializeField] GameObject m_songLinePrefab;
    [SerializeField] int m_linesPerPanel = 4;

    [Header("Child UI buttons")]
    [SerializeField] Button m_beginButton;
    [SerializeField] Button m_previousButton;
    [SerializeField] Button m_nextButton;
    [SerializeField] Button m_errorButton;
    [SerializeField] Button m_uploadButton;
    
    [Header("Child UI texts")]
    [SerializeField] TextMeshProUGUI m_userText;
    [SerializeField] TextMeshProUGUI m_errorText;

    [Header("Song start listeners")]
    [SerializeField] List<GameObject> m_spotsStartup;
    [SerializeField] TravellingScenario m_scenario;
    [SerializeField] Renderer m_windowRenderer;

    Color m_baseWindowColor;
    bool m_windowIsFading = false;

    int m_songsPanelIdx = -1;
    List<GameObject> m_songPanels = new();

    List<(string, string)> m_songs = new();


    void Start()
    {
        m_scenario.enabled = false;

        m_userText.text = $"Welcome back, " + Environment.UserName;

        m_beginButton.onClick.AddListener(OpenSongsList);

        m_baseWindowColor = m_windowRenderer.material.color;

        m_windowRenderer.material.color = new(m_baseWindowColor.r, m_baseWindowColor.g, m_baseWindowColor.b, .98f);

        m_errorButton.onClick.AddListener(() =>
        {
            m_errorPanel.SetActive(false);
            m_welcomePanel.SetActive(true);
            m_errorButton.gameObject.SetActive(false);
        });

        m_welcomePanel.SetActive(true);
        m_songPanelOrigin.SetActive(false);
        m_alreadyPlayingPanel.SetActive(false) ;
        m_errorPanel.SetActive(false);
        m_uploadButton.gameObject.SetActive(false);
        m_previousButton.gameObject.SetActive(false);
        m_nextButton.gameObject.SetActive(false);

        foreach(GameObject l_spot in m_spotsStartup)
        {
            foreach(Light l_light in l_spot.GetComponentsInChildren<Light>())
            {
                l_light.enabled = false;
            }
        }
    }

    private void Update()
    {
        if(m_windowIsFading)
        {
            Color l_actualColor = m_windowRenderer.material.color;
            if(l_actualColor.a < .15f) m_windowIsFading = false;

            m_windowRenderer.material.color =
                new(m_baseWindowColor.r, m_baseWindowColor.g, m_baseWindowColor.b, 
                Mathf.Lerp(l_actualColor.a, .1f, UnityEngine.Time.deltaTime*.1f));
        }
    }

    private void OpenSongsList()
    {
        foreach(GameObject l_panel in m_songPanels)
        {
            Destroy(l_panel);
        }
        m_songPanels.Clear();

        m_songs.Clear();

        m_welcomePanel.SetActive(false);
        m_songPanelOrigin.SetActive(true);

        string l_songsPath = Path.Combine(Application.streamingAssetsPath, "Songs");

        GameObject l_currentPanel = null;

        foreach(string l_file in Directory.GetFiles(l_songsPath))
        {
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
                        }

                        GameObject l_newLine = Instantiate(m_songLinePrefab, l_currentPanel.transform);

                        string l_songName = l_file.Split('\\')[l_file.Split('\\').Length -1];

                        l_newLine.GetComponentsInChildren<TextMeshProUGUI>()[0].text = l_songName;
                        l_newLine.GetComponentsInChildren<TextMeshProUGUI>()[1].text = l_score.Identification.Composer;

                        l_newLine.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            m_scenario.enabled = true;
                            m_windowIsFading = true;
                            m_loader.StartScore(l_score,l_songName);
                            m_songPanelOrigin.SetActive(false);
                            m_alreadyPlayingPanel.SetActive(true);
                            StartCoroutine(StartLightsCoroutine());
                        });

                        l_newLine.transform.Translate(
                            new(0, -.05f * m_songs.Count + ((m_songPanels.Count-1) * m_linesPerPanel * .05f), 0));

                        m_songs.Add((l_file, l_score.Identification.Composer));
                    }
                }
                catch (Exception e)
                {
                    m_errorPanel.SetActive(true);
                    m_errorText.text = $"File {l_file} could is impossible to read :\n"+e.Message;
                    m_errorButton.gameObject.SetActive(true);
                }
            }
        }

        m_uploadButton.gameObject.SetActive(true);

        m_uploadButton.onClick.AddListener(TryAddNewSong);

        if (m_songPanels.Count > 0)
        {
            RenderSongPanel(0);

            m_previousButton.onClick.AddListener(() =>
            {
                RenderSongPanel(m_songsPanelIdx - 1);
            });

            m_nextButton.onClick.AddListener(() =>
            {
                RenderSongPanel(m_songsPanelIdx + 1);
            });
        }
    }

    private IEnumerator StartLightsCoroutine()
    {
        foreach (GameObject l_spot in m_spotsStartup)
        {
            foreach (Light l_light in l_spot.GetComponentsInChildren<Light>())
            {
                l_light.enabled = true;
            }
            l_spot.GetComponentInChildren<AudioSource>().Play();
            yield return new WaitForSeconds(0.625f * 4); // one measure at 96 BPM, ugly af but no time !
        }
    }

    void TryAddNewSong()
    {
        string[] l_files = StandaloneFileBrowser.OpenFilePanel("Choose one or more songs", "", "xml", true);
        if(l_files.Length > 0)
        {
            foreach(string l_file in l_files)
            {
                string l_songName = l_file.Split('\\')[l_file.Split('\\').Length - 1];
                try
                {
                    string l_dest = Path.Combine(Application.streamingAssetsPath, "Songs", l_songName);
                    File.Copy(l_file, l_dest);
            
                    print("Successfuly copied "+l_file+" to "+l_dest);
                    
                    OpenSongsList();
                }catch(Exception _)
                {

                }
            }

        }
        else
        {
            m_errorPanel.SetActive(true);
            m_errorText.text = "You have selected no song";
            m_errorButton.gameObject.SetActive(true);
        }
    }

    void RenderSongPanel(int p_index)
    {
        m_songsPanelIdx = p_index;

        foreach (GameObject l_panel in m_songPanels)
        {
            l_panel.SetActive(false);
        }

        m_songPanels[m_songsPanelIdx].SetActive(true);

        m_previousButton.gameObject.SetActive(m_songsPanelIdx > 0);
        m_nextButton.gameObject.SetActive(m_songsPanelIdx < m_songPanels.Count-1);
    }
}
