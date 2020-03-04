using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ObjectSpawner : MonoBehaviour {
    public int spawnCount = 15;
    public float spawnRadius = 1f;
    public GameObject[] spawnablePrefabs;

    private void Awake() {
        Spawn();
    }

    public void Spawn() {
        // Retrieve spawning area from box collider.
        BoxCollider bc = GetComponent<BoxCollider>();
        bc.enabled = false;

        Transform t = transform;

        for(int i = 0; i < spawnCount; i++) {
            GameObject targetPrefab = spawnablePrefabs[Random.Range(0, spawnablePrefabs.Length)];
            Vector3 targetSpawnPos = GetRandomPointInBox(t, bc);

            // Spawn random object and add random force and torque.
            GameObject objInst = Instantiate(targetPrefab, targetSpawnPos, Random.rotationUniform);
            Rigidbody rigidInst = objInst.GetComponent<Rigidbody>();

            if(rigidInst != null) {
                Vector3 randomForce = Random.insideUnitSphere * 0.01f;
                rigidInst.AddForce(randomForce, ForceMode.Impulse);
                rigidInst.AddTorque(randomForce * 0.1f, ForceMode.Impulse);
            }
        }
    }

    private Vector3 GetRandomPointInBox(Transform alignment, BoxCollider box) {
        Vector3 extent = box.size * 0.5f;
        float x = box.center.x + Random.Range(-extent.x, extent.x);
        float y = box.center.y + Random.Range(-extent.y, extent.y);
        float z = box.center.z + Random.Range(-extent.z, extent.z);
        return alignment.TransformPoint(x, y, z);
    }
}