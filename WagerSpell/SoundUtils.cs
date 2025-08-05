using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace WagerSpell
{
    internal static class SoundUtils
    {

        /// <summary>
        /// Loads sound into audio clip
        /// </summary>
        /// <param name="soundFileName">File name of sound to load</param>
        /// <param name="soundFileType">File type of sound to load</param>
        /// <returns>AudioClip containing sound</returns>
        public static AudioClip LoadSound(string soundFileName, AudioType soundFileType, BepInEx.Logging.ManualLogSource logger = null)
        {

            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioPath = Path.Combine(pluginDir, "Sounds", soundFileName);

            if (!File.Exists(audioPath))
            {
                logger?.LogError($"Could not find sound file: {audioPath}");
                return null;
            }

            string uri = "file://" + audioPath.Replace("\\", "/");

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, soundFileType);
            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.result != UnityWebRequest.Result.Success)
            {
                logger?.LogError($"Failed to load {audioPath}: {www.error}");
                return null;
            }

            logger?.LogInfo($"Successfully loaded {audioPath}");

            return DownloadHandlerAudioClip.GetContent(www);
        }

        /// <summary>
        /// Plays 3D spatial audio at location
        /// </summary>
        /// <param name="position">Position to play audio at</param>
        /// <param name="clip">Audio to play</param>
        public static void PlaySpatialSoundAtPosition(Vector3 position, AudioClip clip)
        {
            if (clip == null)
                return;

            GameObject soundObj = new("WagerSpellAudio");
            soundObj.transform.position = position;

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 1f;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 5f;
            source.maxDistance = 350f;
            source.Play();

           Object.Destroy(soundObj, clip.length + 0.1f);
        }
    }
}
