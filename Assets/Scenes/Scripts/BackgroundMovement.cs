using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    public GameObject player;
    public float movementSpeed; // Prędkość poruszania tła
    public float followSpeed; // Szybkość poruszania się tła w kierunku gracza

    private Vector3 targetPosition; // Pozycja docelowa tła

    void Update()
    {
        // Obliczanie nowej pozycji docelowej tła
        targetPosition = player.transform.position;

        // Poruszanie tłem w kierunku nowej pozycji docelowej z zadaną szybkością
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Dodatkowe poruszanie tła
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f) * movementSpeed;
        transform.position += movement * Time.deltaTime;
    }
}
