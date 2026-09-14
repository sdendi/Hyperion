using UnityEngine;

namespace RTS
{
    public class DestinationMarker : MonoBehaviour
    {
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private float startScale = 0.2f;
        [SerializeField] private float targetScale = 1.0f;
        [SerializeField] private Renderer markerRenderer;

        private float timer = 0f;
        private Material materialInstance;
        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (markerRenderer == null)
            {
                markerRenderer = GetComponentInChildren<Renderer>();
            }

            if (markerRenderer != null)
            {
                materialInstance = markerRenderer.material;
            }
        }

        public void Play(Vector3 position)
        {
            transform.position = position + Vector3.up * 0.05f;
            transform.localScale = Vector3.one * startScale;
            gameObject.SetActive(true);
            timer = 0f;
            SetAlpha(1f);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (progress >= 1f)
            {
                gameObject.SetActive(false);
                return;
            }

            float currentScale = Mathf.Lerp(startScale, targetScale, Mathf.Sin(progress * Mathf.PI * 0.5f));
            transform.localScale = new Vector3(currentScale, 0.02f, currentScale);

            float alpha = 1f - progress;
            SetAlpha(alpha);
        }

        private void SetAlpha(float alpha)
        {
            if (materialInstance != null)
            {
                if (materialInstance.HasProperty(ColorProp))
                {
                    Color col = materialInstance.GetColor(ColorProp);
                    col.a = alpha;
                    materialInstance.SetColor(ColorProp, col);
                }
                else if (materialInstance.HasProperty("_Color"))
                {
                    Color col = materialInstance.color;
                    col.a = alpha;
                    materialInstance.color = col;
                }
            }
        }

        private void OnDestroy()
        {
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
        }
    }
}
