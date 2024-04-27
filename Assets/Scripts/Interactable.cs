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
    [SerializeField] private Rigidbody rb;
    [SerializeField] private bool held = false;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interact(GameObject interacter)
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

                }

                break;
        }
    }
}
