using UnityEngine;

// Contrato común de todo enemigo que patrulla: permite que los sistemas de combate
// (espada del jugador, daño por contacto) funcionen con CUALQUIER tipo de patrulla
// (EnemyPatrol de N puntos, EnemyPatrol3Puntos, futuros enemigos) sin conocer su clase concreta.
public interface IEnemigoMovil
{
    // Invierte el rumbo de la patrulla (media vuelta al golpear al jugador)
    void TurnAround();

    // Retroceso al recibir un golpe de la espada
    void ApplyKnockback(Vector2 direction, float force, float duration);
}
