using UnityEngine;

public static class Extensions
{
   private static LayerMask layerMask = LayerMask.GetMask("Default");
   public static bool Raycast(this Rigidbody2D rigidbody, Vector2 direction)
   {
      if (rigidbody.bodyType != RigidbodyType2D.Dynamic) {
         return false;
      }

      // colider to detect if grounded. this coding is creating a small (.25f) collider and offsesting it 0.88f, which is just below 0
      float radius = 0.25f;
      float distance = 0.375f;
      
      RaycastHit2D hit = Physics2D.CircleCast(rigidbody.position, radius, direction.normalized, distance, layerMask);
      return hit.collider != null && hit.rigidbody != rigidbody;

   }
   
   // Checks if the transform is facing another transform in a given direction.
   // For example, if you want to check if the player stomps on an enemy, you
   // would pass the player transform, the enemy transform, and Vector2.down.
   public static bool DotTest(this Transform transform, Transform other, Vector2 testDirection)
   {
      Vector2 direction = other.position - transform.position;
      return Vector2.Dot(direction.normalized, testDirection) > 0.25f;
   }
}
