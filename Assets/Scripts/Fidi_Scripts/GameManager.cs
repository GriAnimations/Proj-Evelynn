using System.Collections;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace Fidi_Scripts
{
    public class GameManager : MonoBehaviour
    {
        // Singleton pattern

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            CreateResponseManager();
            DontDestroyOnLoad(gameObject);
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

        public IEnumerator SetAllowSpeaking()
        {
            yield return new WaitForSeconds(2.5f);
            FindObjectOfType<ResponseManager>().allowedToSpeak = true;
        }
    }
}