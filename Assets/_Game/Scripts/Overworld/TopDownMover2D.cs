using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Overworld
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownMover2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private Animator animator;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _lastMove = Vector2.down;

        public Vector2 Position => _rb.position;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            _rb.velocity = _moveInput * moveSpeed;
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            var speed = _moveInput.sqrMagnitude;
            if (speed > 0.01f)
            {
                _lastMove = _moveInput.normalized;
            }

            animator.SetFloat("MoveX", _moveInput.x);
            animator.SetFloat("MoveY", _moveInput.y);
            animator.SetFloat("Speed", speed);
            animator.SetFloat("LastMoveX", _lastMove.x);
            animator.SetFloat("LastMoveY", _lastMove.y);
        }
    }
}
