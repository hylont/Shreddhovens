using MusicXml;
using MusicXml.Domain;
using System;
using System.Collections;
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

    [Header("Hands placement")]
    [SerializeField] float m_maxHandSpan = .16f;
    [SerializeField] Lerper m_leftHandTarget, m_rightHandTarget;
    [SerializeField] Vector3 m_handsOffsetPosition = new(-.1f,-.05f,0);
    [SerializeField] Vector3 m_handsOffsetRotationLeft = new(0,0,90);
    [SerializeField] Vector3 m_handsOffsetRotationRight = new(0,180,-90);
    [SerializeField] Transform m_rightHandRestPosition;
    [Header("Fingers placement")]
    [SerializeField] List<Lerper> m_leftHandFingerTargets, m_rightHandFingerTargets;
    [SerializeField] Vector3 m_handsFingersOffsetPosition = new(0, 0, 0);
    [SerializeField] Vector3 m_handsFingersOffsetRotationLeft = new(0, 0, 0);
    [SerializeField] Vector3 m_handsFingersOffsetRotationRight = new(0, 0, 0);

    [Header("Lights")]
    [SerializeField] List<ProjectorGroup> m_groups = new();
    [SerializeField] [Range(0, 5)] int m_nbGroupsActivatedSimultaneously = 2; //ignored from now
    [SerializeField] [Range(0f, 1f)] float m_groupDeactivationChancesAtMeasure = .2f;
    List<ProjectorGroup> m_activatedGroups = new();

    [Header("Debug")]
    [SerializeField] TextMeshProUGUI m_infoLeftHandText;
    [SerializeField] TextMeshProUGUI m_infoRightHandText, m_infoNotesComputedText;

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

            //activate groups randomly
            foreach (ProjectorGroup l_group in m_groups)
            {
                l_group.enabled = UnityEngine.Random.Range(0f, 1f) < m_groupDeactivationChancesAtMeasure;
            }

            //foreach (ProjectorGroup l_group in m_groups)
            //{
            //    if (l_group.enabled == false && m_activatedGroups.Contains(l_group))
            //        m_activatedGroups.Remove(l_group);
            //}

            //int l_nbgroupsToActivate = UnityEngine.Random.Range(0, m_nbGroupsActivatedSimultaneously);

        }
        m_beatText.text = $"Beat {m_beatCount+1}";        
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

        InvokeRepeating(nameof(Count16th), 0f, 60f / (m_bpm * m_timeSignature));

        foreach (ProjectorGroup l_group in m_groups)
        {
            l_group.ActivationDelay = 60f / m_bpm;
            l_group.DestChangeSpeed = 60f / (m_bpm * (m_timeSignature / 2));
            l_group.TargetChangeSpeed = 60f / (m_bpm * (m_timeSignature / 2));
            l_group.FlashInterval = 60f / (m_bpm * m_timeSignature);
        }
    }

    void Count16th()
    {
        if (m_allNotes.ContainsKey(m_16thCount) && m_allNotes[m_16thCount] != null)
        {
            m_notesPlayed.text = "Notes Played" + Environment.NewLine;

            List<PianoKey> l_playedKeys = new();

            foreach (string note in m_allNotes[m_16thCount])
            {
                PianoKey l_playedKey = m_piano.RequestKey(note, 1f);

                if (l_playedKey != null)
                {
                    l_playedKeys.Add(l_playedKey);

                    m_notesPlayed.text += note + " ";

                    print(note+" at "+m_16thCount);

                    if (m_infoNotesComputedText) m_infoNotesComputedText.text += " " + l_playedKey.gameObject.name;
                }
            }

            if (l_playedKeys.Count > 0
            && m_leftHandFingerTargets.Count > 0 && m_rightHandFingerTargets.Count > 0)
            {
                l_playedKeys.Sort();

                //render sorted keys
                if (m_infoNotesComputedText)
                {
                    m_infoNotesComputedText.text = "";
                    foreach (PianoKey l_key in l_playedKeys)
                    {
                        m_infoNotesComputedText.text += $"{l_key.gameObject.name}\n";
                    }
                }

                Vector3 l_firstHandKey = Vector3.zero;
                bool l_needsTwoHands = false;
                int l_fingerIdx = 0;

                List<Vector3> l_currentHandKeys = new();
                List<bool> l_currentHandFingersUsed = new();
                foreach(Lerper _ in m_leftHandFingerTargets) l_currentHandFingersUsed.Add(false);

                //Iterate on each key
                foreach (PianoKey l_key in l_playedKeys)
                {
                    if (l_firstHandKey == Vector3.zero) l_firstHandKey = l_key.transform.position;

                    if (!l_needsTwoHands && l_fingerIdx >= m_leftHandFingerTargets.Count)
                    {
                        Debug.Log("[SCORE HANDS] Exceeded finger count on left hand.");
                        l_needsTwoHands = true;
                        l_firstHandKey = Vector3.zero;

                        SetHandDestination(true, ref l_currentHandKeys, ref l_currentHandFingersUsed);

                        l_fingerIdx = 0;
                    }
                    if (l_needsTwoHands && l_fingerIdx >= m_rightHandFingerTargets.Count)
                    {
                        Debug.LogWarning("[SCORE HANDS] Not enough fingers on right hand ?! Aborting.");

                        SetHandDestination(false, ref l_currentHandKeys, ref l_currentHandFingersUsed);

                        break;
                    }

                    //Add to current hand keys list if the offset to the first key is reachable
                    if (Vector3.Distance(l_key.transform.position, l_firstHandKey) < m_maxHandSpan)
                    {
                        //Set first unused finger on the transorm
                        if (l_needsTwoHands)
                        {
                            m_rightHandFingerTargets[l_fingerIdx].SetDestination(l_key.transform.position + m_handsFingersOffsetPosition);
                            m_rightHandFingerTargets[l_fingerIdx].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationRight);
                            if (m_infoRightHandText) m_infoRightHandText.text =
                                    $"Finger {m_rightHandFingerTargets[l_fingerIdx].name} on {l_key.name}\n";
                        }
                        else
                        {
                            m_leftHandFingerTargets[l_fingerIdx].SetDestination(l_key.transform.position + m_handsFingersOffsetPosition);
                            m_leftHandFingerTargets[l_fingerIdx].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationLeft);
                            if (m_infoLeftHandText) m_infoLeftHandText.text =
                                    $"Finger {m_leftHandFingerTargets[l_fingerIdx].name} on {l_key.name}\n";
                        }
                        l_currentHandKeys.Add(l_key.transform.position);
                        l_currentHandFingersUsed[l_fingerIdx] = true;
                        l_fingerIdx++;
                    }
                    else
                    {
                        //if the offset is too high, switch to right hand
                        Debug.Log("[SCORE HANDS] Exceeded hand span of left hand.");
                        l_needsTwoHands = true;
                        l_firstHandKey = Vector3.zero;

                        SetHandDestination(true, ref l_currentHandKeys, ref l_currentHandFingersUsed);
                    }
                }

                if(l_currentHandKeys.Count > 0)
                {
                    SetHandDestination(!l_needsTwoHands, ref l_currentHandKeys, ref l_currentHandFingersUsed);
                }

                if(!l_needsTwoHands && l_currentHandKeys.Count == 0)
                {
                    m_rightHandTarget.SetDestination(m_rightHandRestPosition.transform.position);
                    m_rightHandTarget.transform.rotation = Quaternion.Euler(m_handsOffsetRotationRight);

                    for (int l_idxFinger = 0; l_idxFinger < m_rightHandFingerTargets.Count; l_idxFinger++)
                    {
                        m_rightHandFingerTargets[l_idxFinger].SetDestination(m_rightHandRestPosition.transform.position + m_handsFingersOffsetPosition);
                        m_rightHandFingerTargets[l_idxFinger].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationRight);
                    }
                }
            }
        }

        if (m_16thCount % 4 == 0)
        {
            PlayBeat();
        }
        m_16thCount++;

        m_16thText.text = $"16th {m_16thCount}";
    }

    void SetHandDestination(bool p_leftHand, ref List<Vector3> p_keysPositions, ref List<bool> p_fingersUsed)
    {
        Vector3 l_averageKeyPos = new(
            p_keysPositions.Average(v => v.x),
            p_keysPositions.Average(v => v.y),
            p_keysPositions.Average(v => v.z) 
        );

        if (p_leftHand)
        {
            m_leftHandTarget.SetDestination(l_averageKeyPos + m_handsOffsetPosition);
            m_leftHandTarget.transform.rotation = Quaternion.Euler(m_handsOffsetRotationLeft);

            for(int l_idxFinger = 0; l_idxFinger < p_fingersUsed.Count; l_idxFinger++)
            {
                if (!p_fingersUsed[l_idxFinger])
                {
                    m_leftHandFingerTargets[l_idxFinger].SetDestination(l_averageKeyPos);
                    m_leftHandFingerTargets[l_idxFinger].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationLeft);
                }
            }
        }
        else
        {
            m_rightHandTarget.SetDestination(l_averageKeyPos + m_handsOffsetPosition);
            m_rightHandTarget.transform.rotation = Quaternion.Euler(m_handsOffsetRotationRight);

            for (int l_idxFinger = 0; l_idxFinger < p_fingersUsed.Count; l_idxFinger++)
            {
                if (!p_fingersUsed[l_idxFinger])
                {
                    m_rightHandFingerTargets[l_idxFinger].SetDestination(l_averageKeyPos);
                    m_rightHandFingerTargets[l_idxFinger].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationRight);
                }
            }
        }

        p_fingersUsed.Clear();
        foreach(Lerper _ in m_leftHandFingerTargets) p_fingersUsed.Add(false);
        p_keysPositions.Clear();
    }
}
