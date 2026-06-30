using UnityEngine;

public class FogBoundary : MonoBehaviour
{
    public ParticleSystem fogParticles;
    public Collider[] blockedZones;

    private ParticleSystem.Particle[] particles;

    void LateUpdate()
    {
        if (fogParticles == null || blockedZones == null) return;

        int count = fogParticles.particleCount;
        if (count == 0) return;

        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        fogParticles.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // Thử cả 2 cách lấy position
            Vector3 localPos = particles[i].position;
            Vector3 worldPos = fogParticles.transform.TransformPoint(localPos);

            // Debug vị trí particle đầu tiên
            if (i == 0)
                Debug.Log($"Particle[0] local={localPos} world={worldPos}");

            foreach (Collider zone in blockedZones)
            {
                // Debug bounds của collider
                if (i == 0)
                    Debug.Log($"Zone bounds: center={zone.bounds.center} size={zone.bounds.size}");

                if (zone.bounds.Contains(worldPos) || zone.bounds.Contains(localPos))
                {
                    particles[i].remainingLifetime = 0f;
                    break;
                }
            }
        }

        fogParticles.SetParticles(particles, count);
    }
}