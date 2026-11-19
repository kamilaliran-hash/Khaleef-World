using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>A pet that hops along behind its owner. Purely cosmetic.</summary>
    public class PetFollower : MonoBehaviour
    {
        public Transform owner;
        public float followDistance = 1.3f;
        public float speed = 6f;
        float hop;

        /// <summary>Creates (or replaces) the pet for the given owner based on the equipped Pet item.</summary>
        public static PetFollower Spawn(Transform owner, AvatarData data)
        {
            var existing = GameObject.Find("Pet_" + owner.GetInstanceID());
            if (existing != null) Destroy(existing);

            var cfg = GameConfig.Instance;
            var item = cfg != null ? cfg.FindItem(data.Get(CosmeticSlot.Pet)) : null;
            if (item == null) return null;

            var go = new GameObject("Pet_" + owner.GetInstanceID());
            go.transform.position = owner.position - owner.forward * 1.2f + owner.right * 0.6f;
            AvatarBuilder.BuildPet(item.shape, item.primary, item.secondary, go.transform);
            var pet = go.AddComponent<PetFollower>();
            pet.owner = owner;
            return pet;
        }

        void Update()
        {
            if (owner == null) { Destroy(gameObject); return; }
            Vector3 target = owner.position - owner.forward * followDistance + owner.right * 0.6f;
            Vector3 flat = target - transform.position;
            flat.y = 0;
            float dist = flat.magnitude;

            if (dist > 8f) { transform.position = target; return; }   // teleport if left far behind

            Vector3 pos = transform.position;
            if (dist > 0.15f)
            {
                pos += flat.normalized * Mathf.Min(dist, speed * Time.deltaTime * Mathf.Clamp01(dist));
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 10f * Time.deltaTime);
                hop += Time.deltaTime * 12f;
            }
            else hop = 0f;

            pos.y = Mathf.Lerp(pos.y, owner.position.y, 10f * Time.deltaTime);
            transform.position = pos;
            var model = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (model != null) model.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(hop)) * 0.15f, 0);
        }
    }
}
