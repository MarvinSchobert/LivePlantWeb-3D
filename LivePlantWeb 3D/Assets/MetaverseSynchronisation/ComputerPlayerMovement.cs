using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayerMovement : MonoBehaviour
{
    Vector3 mousePos;

    public GameObject firstPersonCamera;
    public GameObject thirdPersonCamera;
    public enum ControlMode
    {
        FirstPerson, ThirdPerson
    }
    public ControlMode m_controlMode = ControlMode.ThirdPerson;

    private void Start()
    {
        mousePos = Input.mousePosition;
        // Cursor.lockState = CursorLockMode.Confined;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchControlMode();
        }
        if (m_controlMode == ControlMode.ThirdPerson)
        {            
            // Translation
            if (Input.GetMouseButton(2) || Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
            {
                Vector3 delta = Input.mousePosition - mousePos;
                thirdPersonCamera.transform.Translate(new Vector3(-delta.x, 0, -delta.y) * Time.deltaTime * thirdPersonCamera.transform.position.y * 0.5f);
            }
            if (Input.GetKey(KeyCode.W))
            {
                // transform.Translate(Vector3.forward * Time.deltaTime * 5);
                GetComponent <CharacterController>().SimpleMove (transform.forward * 5);
            }
            if (Input.GetKey(KeyCode.S))
            {
                // transform.Translate(Vector3.back * Time.deltaTime * 5);
                GetComponent<CharacterController>().SimpleMove(- transform.forward * 5);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(Vector3.up, -75 * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(Vector3.up, 75 * Time.deltaTime);
            }
            // Rotation
            if (Input.GetMouseButton(1))
            {
                Vector3 delta = Input.mousePosition - mousePos;
                thirdPersonCamera.transform.RotateAround(thirdPersonCamera.transform.position - thirdPersonCamera.transform.up * thirdPersonCamera.transform.position.y, Vector3.up, delta.x * 0.1f);
                thirdPersonCamera.transform.RotateAround(thirdPersonCamera.transform.position - thirdPersonCamera.transform.up * thirdPersonCamera.transform.position.y, thirdPersonCamera.transform.right, -delta.y * 0.1f);
            }
            if (Input.mouseScrollDelta.y != 0)
            {
                thirdPersonCamera.transform.Translate(Vector3.down * Input.mouseScrollDelta.y * Time.deltaTime * 45 * Mathf.Clamp (thirdPersonCamera.transform.position.y-3, 1,5));
            }
            mousePos = Input.mousePosition;
        }
        else if (m_controlMode == ControlMode.FirstPerson)
        {
            if (Input.GetKey(KeyCode.W))
            {
                // transform.Translate(Vector3.forward * Time.deltaTime * 5);
                GetComponent<CharacterController>().SimpleMove(transform.forward * 5);
            }
            if (Input.GetKey(KeyCode.S))
            {
                // transform.Translate(Vector3.back * Time.deltaTime * 5);
                GetComponent<CharacterController>().SimpleMove(-transform.forward * 5);
            }
            if (Input.GetKey(KeyCode.A))
            {
                GetComponent<CharacterController>().SimpleMove(-transform.right * 5);
            }
            if (Input.GetKey(KeyCode.D))
            {
                GetComponent<CharacterController>().SimpleMove(transform.right * 5);
            }
            // Mouse Look
            transform.Rotate(Vector3.up, 180 * Time.deltaTime * Input.GetAxis ("Mouse X"));
            firstPersonCamera.transform.Rotate(Vector3.right, - 90 * Time.deltaTime * Input.GetAxis ("Mouse Y"));
            

        }
    }

    public void SwitchControlMode()
    {
        if (m_controlMode == ControlMode.FirstPerson) m_controlMode = ControlMode.ThirdPerson;
        else if (m_controlMode == ControlMode.ThirdPerson) m_controlMode = ControlMode.FirstPerson;
        
        if (m_controlMode == ControlMode.FirstPerson)
        {
            // Kamera aktivieren
            thirdPersonCamera.SetActive(false);
            firstPersonCamera.SetActive(true);
        }
        else
        {
            // Kamera aktivieren
            firstPersonCamera.SetActive(false);
            thirdPersonCamera.SetActive(true);            
        }
    }
}
