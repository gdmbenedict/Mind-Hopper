using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterManager : MonoBehaviour
{
    public enum SelectedCharacter
    {
        player,
        ghost
    }

    private SelectedCharacter selectedCharacter = SelectedCharacter.player;

    [Header("Camera Management")]
    [SerializeField] private Camera camera;
    [SerializeField] private Transform playerCameraHolder;
    [SerializeField] private Transform ghostCameraHolder;

    [Header("PlayerManagement")]
    [SerializeField] private PlayerInput playerInput;

    [Header("GhostManagement")]
    [SerializeField] private Transform ghostSpawnPoint;
    [SerializeField] private GameObject ghost;

    // Start is called before the first frame update
    void Start()
    {
        ghost.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchCharacter()
    {
        if (selectedCharacter == SelectedCharacter.player)
        {
            //set ghost location
            ghost.transform.position = ghostSpawnPoint.position;
            ghost.SetActive(true);

            //disabling player character controls
            playerInput.enabled = false;

            //moving Camera to holder
            camera.transform.parent = ghostCameraHolder;
            camera.transform.position = ghostCameraHolder.position;

            //updating enum
            selectedCharacter = SelectedCharacter.ghost;
        }
        else
        {
            //disabling ghost
            ghost.SetActive(false);

            //enabling player input
            playerInput.enabled = true;

            //moving Camera to holder
            camera.transform.parent = playerCameraHolder;
            camera.transform.position = playerCameraHolder.position;

            //updating enum
            selectedCharacter = SelectedCharacter.player;
        }
    }
}
