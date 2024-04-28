using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private Interactable heldInteractable = null;
    [SerializeField] private List<Interactable> interactables = new List<Interactable>();

    [Header("Interaction Management")]
    [SerializeField] private Collider2D interactionArea;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.GetComponent<Interactable>())
        {
            interactables.Add(other.gameObject.GetComponent<Interactable>());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Interactable>())
        {
            if(heldInteractable == other.gameObject.GetComponent<Interactable>())
            {
                heldInteractable.Interact(gameObject);
                heldInteractable = null;
            }

            interactables.Remove(other.gameObject.GetComponent<Interactable>());
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && interactables.Count > 0)
        {
            if (heldInteractable == null)
            {
                interactables.First().Interact(gameObject);

                if (interactables.First().GetInteractionType() == Interactable.InteractableType._moveable)
                {
                    heldInteractable = interactables.First();
                }
            }
            else
            {
                heldInteractable.Interact(gameObject);
                heldInteractable = null;
            }


        }
    }
}
