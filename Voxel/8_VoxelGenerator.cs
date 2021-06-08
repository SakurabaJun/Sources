using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class VoxelGenerator : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Vector3Int size = Vector3Int.one;
    [SerializeField] float voxelSize = 1;
    [SerializeField] bool useUnityPhysics = false;


    Entity voxelEntity;
    EntityManager entityManager;
    BlobAssetStore blobAssetStore;
    GameObjectConversionSettings settings;

    // Start is called before the first frame update
    void Start()
    {
        if (useUnityPhysics)
        {
            //DOTS
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            blobAssetStore = new BlobAssetStore();
            settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);

            voxelEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);
            //DOTS
        }

        Vector3 pos = gameObject.transform.position;

        //zyxを一つずつ生成する
        for(int y = 0; y < size.y; y++)
        {
            for (int z = 0; z < size.z; z++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    //Unity physicsを使用する場合はentityManager.Instantiateでインスタンス化する
                    if (useUnityPhysics)
                    {
                        Entity voxel = entityManager.Instantiate(voxelEntity);
                        Translation voxelTranslation = new Translation
                        {
                            Value = new float3(pos.x + x * voxelSize, pos.y + y * voxelSize, pos.z + z * voxelSize)
                        };
                        entityManager.SetComponentData(voxel, voxelTranslation);
                    }
                    else
                    {
                        Vector3 p = new Vector3(pos.x + x * voxelSize, pos.y + y * voxelSize, pos.z + z * voxelSize);
                        Instantiate(prefab, p, Quaternion.identity);
                    }
 
                }
            }
        }

    }

    private void OnDisable()
    {
        //（DOTSの場合は必須）
        if (useUnityPhysics)
        {
            blobAssetStore.Dispose();
        }
    }
}
