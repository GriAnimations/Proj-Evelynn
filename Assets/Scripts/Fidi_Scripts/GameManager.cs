using System.Collections;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace Fidi_Scripts
{
    public class GameManager : MonoBehaviour
    {

        public float timeBeforeSleep;
        // Singleton pattern

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            CreateResponseManager();
            DontDestroyOnLoad(gameObject);
            StartCoroutine(MustHaveDrivenOff());
        }

        [SerializeField] private GameObject OideDreckschleidan;

        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("GameManager").AddComponent<GameManager>();
                }

                return _instance;
            }
        }

        public void ResetResponseManager()
        {
            GameObject responseManager = FindObjectOfType<ResponseManager>().gameObject;

            responseManager.GetComponent<ResponseManager>().StartResetSession();

            Destroy(responseManager);

            StartCoroutine(CreateAfterDelay());
        }

        private IEnumerator MustHaveDrivenOff()
        {
            while (true)
            {
                while (timeBeforeSleep < 300f)
                {
                    timeBeforeSleep += Time.deltaTime;
                    yield return null;
                }
            
                ResetResponseManager();
                
                timeBeforeSleep = 0f;
                yield return null;
            }
        }

        private IEnumerator CreateAfterDelay()
        {
            yield return new WaitForSeconds(2.0f);
            CreateResponseManager();
            StartCoroutine(SetAllowSpeaking());
        }


        public void CreateResponseManager()
        {
            Instantiate(OideDreckschleidan);
        }

        public void DestroyResponseManager()
        {
            GameObject responseManager = FindObjectOfType<ResponseManager>().gameObject;

            Destroy(responseManager);
        }

        private IEnumerator SetAllowSpeaking()
        {
            yield return new WaitForSeconds(3.5f);
            FindObjectOfType<ResponseManager>().allowedToSpeak = true;
        }
    }
}