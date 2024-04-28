using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractableType
    {
     _switch,
     _moveable
    }

    [Header("Interaction Data")]
    [SerializeField] private InteractableType interactionType;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;

    [Header("Switches & Button")]
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private GameObject toggleable;
    [SerializeField] private bool state = false;

    [Header("Moveables")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool held = false;
    private Rigidbody2D carryingRB;
    [SerializeField] private float stabalizationSpeed = 0.5f;
    private float originalGravity;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (held)
        {
            rb.velocity = carryingRB.velocity;
        }
    }

    public void Interact(GameObject interacter)
    {
        switch (interactionType)
        {
            //switches and buttons
            case InteractableType._switch:

                //toggle state
                state = !state;
                toggleable.SetActive(state);

                if (state)
                {
                    spriteRenderer.sprite = activatedSprite;
                }
                else
                {
                    spriteRenderer.sprite = baseSprite;
                }

                break;
            
            //movable objects
            case InteractableType._moveable:

                if (held)
                {
                    held = false;
                    gameObject.transform.parent = null;
                    rb.gravityScale = originalGravity;
                    carryingRB = null;

                }
                else
                {
                    held = true;
                    gameObject.transform.parent = interacter.transform;
                    gameObject.transform.localPosition = new Vector3(0.5f, 0f);
                    originalGravity = rb.gravityScale;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector3.zero;
                    carryingRB = gameObject.transform.parent.GetComponentInParent<Rigidbody2D>();
                }

                break;
        }
    }

    public InteractableType GetInteractionType()
    {
        return interactionType;
    }
}
