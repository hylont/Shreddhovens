using System.Collections.Generic;
using UnityEngine;

public class PianoBuilder : MonoBehaviour
{
    [SerializeField] List<AudioClip> m_pianoKeys = new();
    [SerializeField] GameObject m_whiteKeyPrefab;
    [SerializeField] GameObject m_blackKeyPrefab;
    [SerializeField] float m_keyXOffset = .015f;
    [SerializeField] float m_blackYOffset = .005f;
    [SerializeField] Transform m_keyOrigin;

    public Dictionary<string, PianoKey> m_allKeys { get; private set; } = new();
    // Start is called before the first frame update
    void Start()
    {
        // NOPE TOO DIFFICULT, FILES WILL BE SET IN EDITOR
        //string l_keysDirectory = Path.Combine(Application.streamingAssetsPath, "piano-mp3-master", "piano-mp3");
        //if (Directory.Exists(l_keysDirectory))
        //{
        //    foreach(string l_filePath in Directory.GetFiles(l_keysDirectory))
        //    {
        //        if (l_filePath.EndsWith(".mp3"))
        //        {
        //            Debug.Log($"Found mp3 file : {l_filePath}");
        //        }
        //        else
        //        {
        //            Debug.LogError($"{l_filePath} is not a mp3 folder !");
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.LogError("Key directory doesn't exist !");
        //}

        if (m_pianoKeys.Count == 0) Debug.LogError("No audio clip !");

        CreateKeys();
    }

    void CreateKeys()
    {
        List<AudioClip> l_sortedAudioClips = new();
        List<AudioClip> l_unsortedAudioClips = m_pianoKeys;

        int iteration = 0;
        char l_currentKey = 'A';
        int l_currentOctave = 0;
        bool l_currentKeyIsBemol = false;
        while (l_unsortedAudioClips.Count > 0)
        {
            string l_searchedKey = KeyToText(l_currentKey, l_currentOctave, l_currentKeyIsBemol);

            AudioClip l_nextClip = l_unsortedAudioClips.Find(l_a => l_a.name == l_searchedKey);
            if (l_nextClip != null)
            {
                l_sortedAudioClips.Add(l_nextClip);
                l_unsortedAudioClips.Remove(l_nextClip);

                // if a bemol, remove the bemol
                if (l_currentKeyIsBemol)
                {
                    l_currentKeyIsBemol = false;

                }
                else
                {
                    //not a bemol, go to next note

                    //at the end of the octave ?

                    if (l_currentKey == 'G')
                    {
                        l_currentKey = 'A';
                        l_currentKeyIsBemol = true;
                    }
                    else
                    {
                        l_currentKey = (char)(l_currentKey + 1);

                        if (l_currentKey == 'C')
                        {
                            l_currentOctave++;
                        }

                        //does the next note have a bemol ?
                        if (l_currentKey == 'A' || l_currentKey == 'B' || l_currentKey == 'D' ||
                            l_currentKey == 'E' || l_currentKey == 'G')
                        {
                            l_currentKeyIsBemol = true;
                        }
                    }
                }

                //Debug.Log($"Successfuly added {l_nextClip.name}." +
                //    $"Next : {KeyToText(l_currentKey, l_currentOctave, l_currentKeyIsBemol)}");
            }
            else
            {
                Debug.LogError($"Could not find key {l_searchedKey} !");
            }
            iteration++;
            if (iteration > 10000) break;
        }

        Vector3 l_offset = Vector3.zero;
        foreach (AudioClip l_clip in l_sortedAudioClips)
        {
            GameObject l_newKeyGO = Instantiate(l_clip.name.Contains('b') ? m_blackKeyPrefab : m_whiteKeyPrefab, m_keyOrigin);

            l_newKeyGO.name = l_clip.name;

            PianoKey l_key = l_newKeyGO.AddComponent<PianoKey>();
            l_key.SetClip(l_clip);

            m_allKeys.Add(l_clip.name, l_key);

            l_newKeyGO.transform.Translate(l_offset);
            if (l_clip.name.Contains('b'))
            {
                l_newKeyGO.transform.Translate(m_keyXOffset*.5f, m_blackYOffset, 0);
            }
            else
            {
                l_newKeyGO.transform.Translate(m_keyXOffset,0,0);
                l_offset.x += m_keyXOffset;
            }
        }

        FindFirstObjectByType<ScoreLoader>().OnKeysReady();
    }

    private static string KeyToText(char p_currentKey, int p_currentOctave, bool p_currentKeyIsBemol)
    {
        string l_searchedKey = $"{p_currentKey}";
        if (p_currentKeyIsBemol) l_searchedKey += 'b';
        l_searchedKey += p_currentOctave;
        return l_searchedKey;
    }

    public PianoKey RequestKey(string key, float duration = 1f)
    {
        if (m_allKeys.ContainsKey(key))
        {
            m_allKeys[key].Play(duration);
            return m_allKeys[key];
        }

        Debug.LogError("[BUILDER] Key " + key + " doesn't exist");
        return null;
    }
}
