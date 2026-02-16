using System.Collections.Generic;
using UnityEngine;

namespace Project.Systems
{
    public static class EnemyRegistry
    {
        private static readonly HashSet<EnemyHealth> Enemies = new HashSet<EnemyHealth>();

        public static void Register(EnemyHealth enemy)
        {
            if (enemy == null)
            {
                return;
            }

            Enemies.Add(enemy);
        }

        public static void Unregister(EnemyHealth enemy)
        {
            if (enemy == null)
            {
                return;
            }

            Enemies.Remove(enemy);
        }

        public static EnemyHealth FindClosest(Vector3 position, float maxDistance)
        {
            var maxSqrDistance = maxDistance * maxDistance;
            var bestSqrDistance = float.PositiveInfinity;
            EnemyHealth closest = null;

            foreach (var enemy in Enemies)
            {
                if (enemy == null || enemy.IsDead || !enemy.CanBeTargeted)
                {
                    continue;
                }

                var sqrDistance = (enemy.transform.position - position).sqrMagnitude;
                if (sqrDistance > maxSqrDistance || sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = sqrDistance;
                closest = enemy;
            }

            return closest;
        }
    }
}
