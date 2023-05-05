using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using KSP.Localization;

namespace Biomatic
{
    
    class KIRI : MonoBehaviour
    {

        void GetAudioClip()
        {
            string Txt2Late = Localizer.Format("#BIO-desc");
            string googleUrl = "http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=1024&client=tw-ob&q=+" + Txt2Xlate + "&tl=En-gb";

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(googleUrl, AudioType.MPEG))
            {
                yield return www.Send();

                if (www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            MyClip.Play();
        }

        public void  PlayTranslation()
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://www.my-server.com/audio.ogg", AudioType.OGGVORBIS))
            {
                yield return www.Send();

                if (www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }

        private interface IEnumerator
        {
        }
    }

}
