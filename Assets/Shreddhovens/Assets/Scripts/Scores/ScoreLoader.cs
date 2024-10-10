using MusicXml;
using MusicXml.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreLoader : MonoBehaviour
{
    [SerializeField] PianoBuilder m_piano;
    [SerializeField] AudioClip m_beatClip, m_measureClip;
    [SerializeField] AudioSource m_beatSource, m_measureSource;
    [SerializeField] TextMeshProUGUI m_bpmText, m_beatText, m_16thText, m_measureText, m_nowPlaying, m_notesPlayed;
    [SerializeField] string m_songName = "helloworld";
    [SerializeField] bool m_playMetronome = true;

    public int m_bpm = 60;
    public int m_timeSignature = 4;

    int m_measureCount = 0;
    int m_16thCount = 0;
    int m_beatCount = 0;

    Dictionary<int, List<string>> m_allNotes = new();

    private void Start()
    {
        m_beatSource.clip = m_beatClip;
        m_measureSource.clip = m_measureClip;
    }

    void PlayBeat()
    {
        if (m_playMetronome) m_beatSource.Play();
        m_beatCount++;
        if(m_beatCount%m_timeSignature == 0)
        {
            m_beatCount = 0;
            if(m_playMetronome) m_measureSource.Play();
            m_measureCount++;
            m_measureText.text = $"Measure {m_measureCount}";
        }
        m_beatText.text = $"Beat {m_beatCount}";
    }

    public void OnKeysReady()
    {
        var l_score = MusicXmlParser.GetScore(
            Path.Combine(Application.streamingAssetsPath, "Songs", m_songName+".xml"));

        m_nowPlaying.text = $"{m_songName} by {l_score.Identification.Composer}";

        m_bpmText.text = $"BPM : {m_bpm}";

        foreach (Part part in l_score.Parts)
        {
            for (int idxMeasure = 0; idxMeasure < part.Measures.Count; idxMeasure++)
            {
                int l_currentNote16th = 0;
                //List<string> l_measureNotes = new();
                foreach (MeasureElement element in part.Measures[idxMeasure].MeasureElements)
                {
                    if (element.Type == MeasureElementType.Note)
                    {
                        if (element.Element is not Note l_note) continue;

                        if (!l_note.IsRest && l_note.Pitch != null)
                        {
                            string l_noteStr = $"{l_note.Pitch.Step}{(l_note.Pitch.Alter == -1 ? "b" : "")}{l_note.Pitch.Octave}";

                            //TODO is chord ?

                            if (l_note.Type == "half") l_currentNote16th += 8;
                            else if (l_note.Type == "quarter") l_currentNote16th += 4;
                            else if (l_note.Type == "eighth") l_currentNote16th += 2;
                            else if (l_note.Type == "16th") l_currentNote16th += 1;

                            try
                            {
                                int idxToInsert = idxMeasure * 16 + l_currentNote16th;
                                //TODO add the elements to the existing list if it exists
                                if (m_allNotes.ContainsKey(idxToInsert))
                                {
                                    m_allNotes[idxToInsert].Add(l_noteStr);
                                }
                                else
                                {
                                    m_allNotes.Add(idxToInsert, new() { l_noteStr });
                                }
                            }
                            catch (Exception)
                            {
                                Debug.LogError($"Something went wrong when adding {l_noteStr[0]}(& more) at {idxMeasure * 16 + l_currentNote16th}");
                            }
                        }

                    }
                    else if (element.Type == MeasureElementType.Backup)
                    {
                        Backup l_backup = element.Element as Backup;

                        l_currentNote16th-=l_backup.Duration;
                    }
                    else if (element.Type == MeasureElementType.Forward)
                    {
                        Forward l_forward = element.Element as Forward;

                        l_currentNote16th += l_forward.Duration;
                    }                   
                }
            }
        }

        InvokeRepeating(nameof(Count16th), 0f, 60f / (m_bpm * 4));
    }

    void Count16th()
    {
        if (m_allNotes.ContainsKey(m_16thCount) && m_allNotes[m_16thCount] != null)
        {
            m_notesPlayed.text = "Notes Played" + Environment.NewLine;
            foreach(string note in m_allNotes[m_16thCount])
            {
                m_notesPlayed.text += note + " ";
                m_piano.RequestKey(note, 1f);
            }
        }

        if (m_16thCount % 4 == 0)
        {
            PlayBeat();
        }
        m_16thCount++;

        m_16thText.text = $"16th {m_16thCount}";
    }
}
