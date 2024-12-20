using Assets.Shreddhovens.Assets.Scripts.Builder;
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
    [SerializeField] List<ProjectorGroup> m_twinGroups = new();
    [SerializeField] ProjectorGroup m_windowGroup, m_bassGroup, m_highGroup;
    //[SerializeField] [Range(0, 5)] int m_nbGroupsActivatedSimultaneously = 2; //ignored from now
    //[SerializeField] [Range(0f, 1f)] float m_groupDeactivationChancesAtMeasure = .2f;
    List<ProjectorGroup> m_activatedGroups = new();
    [HideInInspector] public PianoKey CenterKey = null;

    [Header("Effects")]
    [SerializeField] BlendShapeMorpher m_timpaniMorpher;
    [SerializeField] List<ParticleSystem> m_flameThrowers = new();
    [SerializeField] int m_minNotesFlamethrowers = 7;
    int m_lastFlamethrowerMeasureIdx = 0;

    [Header("Canvases")]
    [SerializeField] GameObject m_UINotePrefab;
    [SerializeField] GameObject m_UINotesParent;

    [Header("Debug")]
    [SerializeField] TextMeshProUGUI m_infoLeftHandText;
    [SerializeField] TextMeshProUGUI m_infoRightHandText, m_infoNotesComputedText;

    Dictionary<int, List<string>> m_allNotes = new();

    List<UIKey> m_UIKeys = new();

    private void Start()
    {
        m_beatSource.clip = m_beatClip;
        m_measureSource.clip = m_measureClip;
    }

    void PlayBeat()
    {
        m_timpaniMorpher.RequestWeight(50f);
        if (m_playMetronome)
        {
            m_beatSource.volume = (m_beatCount +1) % m_timeSignature * .25f;
            m_beatSource.Play();
        }
        m_beatCount++;
        if(m_beatCount%m_timeSignature == 0)
        {
            m_beatCount = 0;
            if (m_playMetronome)
            {
                m_measureSource.Play();
            }
            m_measureCount++;
            m_measureText.text = $"Measure : {m_measureCount}";

            m_timpaniMorpher.RequestWeight(100f);

            if (m_measureCount % m_timeSignature == 0)
            {
                m_bassGroup.SetNewMaterial();
            }else if (m_measureCount % m_timeSignature == m_timeSignature / 2)
            {
                m_highGroup.SetNewMaterial();
            }

        }
        m_beatText.text = $"Beat : {m_beatCount+1}";        
    }

    public bool StartScore(Score p_score, string p_scoreName)
    {
        if(m_UIKeys == null || m_UIKeys.Count == 0) return false;

        List<UIKey> keys = FindObjectsOfType<UIKey>().ToList();

        m_nowPlaying.text = $"NOW PLAYING : {p_scoreName} " +
            $"{(string.IsNullOrEmpty(p_score.Identification.Composer) ? "" : p_score.Identification.Composer)}";

        m_bpmText.text = $"BPM : {m_bpm}";

        foreach (Part part in p_score.Parts)
        {
            for (int idxMeasure = 0; idxMeasure < part.Measures.Count; idxMeasure++)
            {
                Dictionary<int, int> l_noteIdxByVoice = new();

                foreach (MeasureElement element in part.Measures[idxMeasure].MeasureElements)
                {
                    if (element.Element is Measure)
                    {
                        print("measure !");
                    }

                    if (element.Type == MeasureElementType.Note)
                    {
                        if (element.Element is not Note l_note) continue;

                        if (!l_note.IsRest && l_note.Pitch != null)
                        {
                            if (!l_noteIdxByVoice.ContainsKey(l_note.Voice))
                            {
                                l_noteIdxByVoice.Add(l_note.Voice, 0);
                            }

                            string l_noteStr = GetNoteStr(l_note);

                            try
                            {
                                int idxToInsert = idxMeasure * 16 + l_noteIdxByVoice[l_note.Voice];
                                //TODO add the elements to the existing list if it exists
                                if (m_allNotes.ContainsKey(idxToInsert))
                                {
                                    m_allNotes[idxToInsert].Add(l_noteStr);
                                }
                                else
                                {
                                    m_allNotes.Add(idxToInsert, new() { l_noteStr });
                                }

                                //print("[SCORE LOADER] Inserted " + l_noteStr + " at " + idxToInsert + " (voice "+l_note.Voice+")");

                                GameObject l_UINote = Instantiate(m_UINotePrefab, m_UINotesParent.transform);
                                l_UINote.GetComponentInChildren<TextMeshProUGUI>().text = l_noteStr + "\n" + idxToInsert;
                                l_UINote.name = l_noteStr + "_" + idxToInsert;

                                UIKey l_UIKeyMatch = m_UIKeys.Find(k => k.name == l_noteStr);

                                if (l_UIKeyMatch != null)
                                {
                                    l_UINote.transform.localPosition = new((float)(l_UIKeyMatch.transform.localPosition.x + 0.05681825 * 0.5f), idxToInsert / 3.2f);

                                    l_UINote.GetComponent<UINote>().KeyMatch = l_UIKeyMatch;

                                    //print("Set key " + l_UINote.name + "to (" + l_UIKeyMatch.name + ") " + l_UIKeyMatch.transform.localPosition.x + " ; " + idxToInsert );
                                }
                                else
                                {
                                    Debug.LogWarning("Note " + l_noteStr + " has no corresponding UIKEY !");
                                }

                                if (!l_note.IsChordTone)
                                {
                                    if (l_note.Type == "half") l_noteIdxByVoice[l_note.Voice] += 8;
                                    else if (l_note.Type == "quarter") l_noteIdxByVoice[l_note.Voice] += 4;
                                    else if (l_note.Type == "eighth") l_noteIdxByVoice[l_note.Voice] += 2;
                                    else if (l_note.Type == "16th") l_noteIdxByVoice[l_note.Voice] += 1;
                                }
                            }
                            catch (Exception)
                            {
                                Debug.LogError($"Something went wrong when adding {l_noteStr[0]}(& more) at {idxMeasure * 16 + l_noteIdxByVoice[l_note.Voice]}");
                            }
                        }

                    }
                    else if (element.Type == MeasureElementType.Backup)
                    {
                        Backup l_backup = element.Element as Backup;

                        foreach (int l_voiceIdx in l_noteIdxByVoice.Keys.ToList())
                        {
                            l_noteIdxByVoice[l_voiceIdx] -= l_backup.Duration;
                        }
                    }
                    else if (element.Type == MeasureElementType.Forward)
                    {
                        Forward l_forward = element.Element as Forward;

                        foreach (int l_voiceIdx in l_noteIdxByVoice.Keys.ToList())
                        {
                            l_noteIdxByVoice[l_voiceIdx] -= l_forward.Duration;
                        }
                    }
                }
            }
        }

        InvokeRepeating(nameof(Count16th), 0f, 60f / (m_bpm * m_timeSignature));

        InitGroup(m_windowGroup, true);
        InitGroup(m_bassGroup, true);
        InitGroup(m_highGroup, true);
        //InitGroup(m_twinGroups[0]);
        //InitGroup(m_twinGroups[1]);

        return true;
    }

    void InitGroup(ProjectorGroup p_group, bool p_enableGroupAndProjectors = false)
    {
        p_group.ActivationDelay = 60f / m_bpm;
        p_group.DestChangeSpeed = 60f / (m_bpm * (m_timeSignature / 2));
        p_group.TargetChangeSpeed = 60f / (m_bpm * (m_timeSignature / 2));

        if (!p_enableGroupAndProjectors)
        {
            foreach (AnimatedProjector l_proj in p_group.Projectors)
            {
                l_proj.StopAnimation();
            }

            p_group.enabled = false;
        }
        else
        {
            p_group.enabled = true;
        }
        
        //p_group.FlashInterval = 60f / (m_bpm * m_timeSignature);
    }

    public void OnKeysReady(List<UIKey> p_UIKeys)
    {
        m_UIKeys = p_UIKeys;
    }

    private static string GetNoteStr(Note p_note)
    {
        Note l_noteCopy = p_note;
        bool l_isBemol = false;

        if(l_noteCopy.Pitch.Alter == 1)
        {
            if (l_noteCopy.Pitch.Step == 'G')
            {
                l_noteCopy.Pitch.Step = 'A';
            }
            else
            {
                l_noteCopy.Pitch.Step++;
            }
            l_isBemol = true;
        }else if(l_noteCopy.Pitch.Alter == -1)
        {
            l_isBemol = true;
        }

        return $"" +
            $"{l_noteCopy.Pitch.Step}" +
            $"{(l_isBemol ? "b" : "")}" +
            $"{l_noteCopy.Pitch.Octave}";
    }

    void Count16th()
    {
        if (m_allNotes.ContainsKey(m_16thCount) && m_allNotes[m_16thCount] != null)
        {
            m_notesPlayed.text = "Notes Played :" + Environment.NewLine;

            List<PianoKey> l_playedKeys = new();

            foreach (string note in m_allNotes[m_16thCount])
            {
                PianoKey l_playedKey = m_piano.RequestKey(note, 1f);

                if (l_playedKey != null)
                {
                    l_playedKeys.Add(l_playedKey);

                    m_notesPlayed.text += note + " ";

                    //print(note+" at "+m_16thCount);

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
                int l_nbBassNotes = 0, l_nbHighNotes = 0;
                foreach (PianoKey l_key in l_playedKeys)
                {
                    if(l_key.CompareTo(CenterKey) < 0)
                    {
                        l_nbBassNotes++;
                    }else if(l_key.CompareTo(CenterKey) > 0)
                    {
                        l_nbHighNotes++;
                    }

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
                        //Debug.Log("[SCORE HANDS] Exceeded hand span of left hand.");
                        l_needsTwoHands = true;
                        l_firstHandKey = Vector3.zero;

                        SetHandDestination(true, ref l_currentHandKeys, ref l_currentHandFingersUsed);
                    }
                }

                //flamethrowers
                if(l_playedKeys.Count >= UnityEngine.Random.Range(m_minNotesFlamethrowers, 11) && m_lastFlamethrowerMeasureIdx + 1 < m_measureCount)
                {
                    m_lastFlamethrowerMeasureIdx = m_measureCount;
                    foreach(ParticleSystem p_flames in m_flameThrowers) 
                    {
                        p_flames.Play();
                        p_flames.GetComponentInChildren<ParticleSystem>().Play();
                        p_flames.gameObject.GetComponentInChildren<AudioSource>().Play();
                    }
                }

                if(l_currentHandKeys.Count > 0)
                {
                    StartCoroutine(StartProjectors(m_windowGroup, l_currentHandKeys.Count, 60f / m_bpm ));

                    SetHandDestination(!l_needsTwoHands, ref l_currentHandKeys, ref l_currentHandFingersUsed);
                }
                else
                {
                    StartCoroutine(StartProjectors(m_windowGroup, 0, 60f / m_bpm * m_timeSignature));
                }

                if (!l_needsTwoHands && l_currentHandKeys.Count == 0)
                {
                    m_rightHandTarget.SetDestination(m_rightHandRestPosition.transform.position);
                    m_rightHandTarget.transform.rotation = Quaternion.Euler(m_handsOffsetRotationRight);

                    for (int l_idxFinger = 0; l_idxFinger < m_rightHandFingerTargets.Count; l_idxFinger++)
                    {
                        m_rightHandFingerTargets[l_idxFinger].SetDestination(m_rightHandRestPosition.transform.position - m_handsFingersOffsetPosition);
                        m_rightHandFingerTargets[l_idxFinger].transform.rotation = Quaternion.Euler(m_handsFingersOffsetRotationRight);
                    }                    
                }
                StartCoroutine(StartProjectors(m_bassGroup, l_nbBassNotes, 60f / m_bpm * m_timeSignature));
                StartCoroutine(StartProjectors(m_highGroup, l_nbHighNotes, 60f / m_bpm * m_timeSignature));
            }
        }

        if (m_16thCount % 4 == 0)
        {
            PlayBeat();
        }
        m_16thCount++;

        m_16thText.text = $"16th : {m_16thCount}";
    }

    IEnumerator StartAnimationOfGroup(ProjectorGroup p_group, float p_timeUntilDisable)
    {
        p_group.enabled = false;
        p_group.enabled = true;
        yield return new WaitForSeconds(p_timeUntilDisable);
        p_group.enabled = false;
    }

    IEnumerator StartProjectors(ProjectorGroup p_projectors, int p_nbProjectorsToActivate, float p_timeUntilDisable)
    {
        List<int> l_projectorsIdsPulled = new();
        do
        {
            if(l_projectorsIdsPulled.Count == p_projectors.Projectors.Length)
            {
                print($"[START PROJECTOR] ({p_projectors.name}) Not enough projectors to match notes count");
                break;
            }
            int l_pulledInt = UnityEngine.Random.Range(0, p_projectors.Projectors.Length);
            if (!l_projectorsIdsPulled.Contains(l_pulledInt))
            {
                l_projectorsIdsPulled.Add(l_pulledInt);
            }
        } while (l_projectorsIdsPulled.Count <= p_nbProjectorsToActivate);

        for(int l_idxProj = 0; l_idxProj < p_projectors.Projectors.Length; l_idxProj++)
        {
            if(l_projectorsIdsPulled.Contains(l_idxProj)) p_projectors.Projectors[l_idxProj].StartAnimation();
            else p_projectors.Projectors[l_idxProj].StopAnimation();
        }

        if (p_projectors.m_debug)
        {
            print($"[START PROJECTOR] ({p_projectors.name}) With {p_nbProjectorsToActivate} notes, started {l_projectorsIdsPulled.Count} projectors");
        }

        yield return null;
        //yield return new WaitForSeconds(p_timeUntilDisable);
        //foreach(AnimatedProjector p_projector in p_projectors.Projectors)
        //{
        //    p_projector.StopAnimation();
        //}
    }

    void SetHandDestination(bool p_leftHand, ref List<Vector3> p_keysPositions, ref List<bool> p_fingersUsed)
    {
        if (p_keysPositions.Count == 0) return;

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
