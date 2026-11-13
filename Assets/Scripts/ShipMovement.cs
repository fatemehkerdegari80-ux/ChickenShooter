using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField, Min(0f)] private float moveSpeed = 6f;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private SpriteRenderer shipSprite;

    private void OnEnable()
    {
        moveAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
    }

    private void Start()
    {
        if (gameCamera == null || shipSprite == null)
        {
            Debug.LogError("ShipMovement: connect Game Camera and Ship Sprite.", this);
            enabled = false;
            return;
        }

        GameManager state = GameManager.Instance;

        if (state != null && state.HasShipPosition)
        {
            Vector2 savedPosition = state.ShipViewportPosition;

            float depth = gameCamera.WorldToViewportPoint(
                transform.position).z;

            Vector3 restoredPosition = gameCamera.ViewportToWorldPoint(
                new Vector3(savedPosition.x, savedPosition.y, depth));

            restoredPosition.z = transform.position.z;
            transform.position = ClampPosition(restoredPosition);
        }

        SavePosition();
    }

    private void Update()
    {
        if (moveAction == null)
            return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector2 direction = Vector2.ClampMagnitude(input, 1f);

        Vector3 movement = new Vector3(direction.x, direction.y, 0f);
        Vector3 nextPosition = transform.position
            + movement * (moveSpeed * Time.deltaTime);

        transform.position = ClampPosition(nextPosition);
        SavePosition();
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float halfHeight = gameCamera.orthographicSize;
        float halfWidth = halfHeight * gameCamera.aspect;

        Vector3 center = gameCamera.transform.position;
        Vector3 shipHalfSize = shipSprite.bounds.extents;

        position.x = Mathf.Clamp(
            position.x,
            center.x - halfWidth + shipHalfSize.x,
            center.x + halfWidth - shipHalfSize.x);

        position.y = Mathf.Clamp(
            position.y,
            center.y - halfHeight + shipHalfSize.y,
            center.y + halfHeight - shipHalfSize.y);

        return position;
    }

    private void SavePosition()
    {
        if (GameManager.Instance == null)
            return;

        Vector3 position = gameCamera.WorldToViewportPoint(
            transform.position);

        GameManager.Instance.SaveShipPosition(
            new Vector2(position.x, position.y));
    }
}