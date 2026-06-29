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
            Vector3 worldPos = fogParticles.transform.TransformPoint(particles[i].position);

            foreach (Collider zone in blockedZones)
            {
                Vector3 closest = zone.ClosestPoint(worldPos);
                float dist = Vector3.Distance(closest, worldPos);

                if (dist < 0.01f)
                {
                    particles[i].remainingLifetime = 0f;
                    break;
                }
            }
        }

        fogParticles.SetParticles(particles, count);
    }
}