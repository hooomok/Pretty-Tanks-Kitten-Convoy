using UnityEngine;

namespace PrettyTanks
{
    public sealed class Vfx : MonoBehaviour
    {
        private static Material material;

        private void OnEnable()
        {
            EventBus.Effect += Effect;
        }

        private void OnDisable()
        {
            EventBus.Effect -= Effect;
        }

        public static Material Material
        {
            get {
                if (!material)
                {
                    material = Resources.Load<Material>("ConfettiMaterial");
                    if (!material)
                    {
                        material = new Material(Shader.Find("Sprites/Default"));
                    }
                }
                return material;
            }
        }

        private void Effect(Vector2 point, string kind)
        {
            var go = new GameObject("FX " + kind);
            if (GameManager.I && GameManager.I.World)
            {
                go.transform.SetParent(GameManager.I.World);
            }
            go.transform.position = point;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.duration = 0.25f;
            main.startLifetime = kind == "smoke" ? 0.8f : 0.55f;
            main.startSpeed = kind == "bomb" ? 6 : kind == "smoke" ? 0.35f : 2;
            main.startSize = kind == "smoke" ? 0.2f : 0.12f;
            main.gravityModifier = 0;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = kind == "fire"    ? new Color(1, 0.5f, 0.22f)
                              : kind == "smoke" ? new Color(0.45f, 0.4f, 0.5f, 0.6f)
                                                : new Color(1, 0.57f, 0.79f);
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)(kind == "bomb"    ? 90
                                                                           : kind == "burst" ? 40
                                                                           : kind == "smoke" ? 2
                                                                                             : 12)) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Material;
            renderer.sortingOrder = 30;
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Sprites;
            sheet.AddSprite(Art.Sprite(kind == "smoke" ? "round" : kind == "hit" ? "star" : "heart"));
            ps.Play();
            Destroy(go, 1.5f);
        }

        public static void Beam(Vector2 a, Vector2 b, Color color)
        {
            var go = new GameObject("Laser beam");
            go.transform.SetParent(GameManager.I.World);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = Material;
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = 0.12f;
            line.positionCount = 2;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
            line.sortingOrder = 22;
            Destroy(go, 0.12f);
        }
    }
}
