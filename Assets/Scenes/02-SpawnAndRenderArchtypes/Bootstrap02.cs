using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;

public class Bootstrap02:MonoBehaviour {
   
    private static EntityManager entityManager;
    private static RenderMesh entityRenderer;

    private static EntityArchetype entityTemplate; //Can create an entity using the archtype, which speeds up some stuff under the hood (how/where in mem the entity is stored, so it's by it's friends and can be processed fast)
    
    
    public void Awake() { 
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        //create any archtypes we are going to be using?
        entityTemplate = entityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(RenderMesh), typeof(LocalToWorld)); 
    }

    public void Start() {
        MeshFilter f = GameObject.FindObjectOfType<MeshFilter>(); 
        entityRenderer.mesh = f.sharedMesh; 
       

        entityRenderer.material = GameObject.FindObjectOfType<MeshRenderer>().material;
        entityRenderer.receiveShadows = true;
        entityRenderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;

        int numToSpawn = 50000;

        for(int c = 0; c < numToSpawn; c++) { 
            Entity e = entityManager.CreateEntity(entityTemplate); //create the entity from the archtype template (x50).  Since we create from template, we don't need to add components and then set them, just set them?
            float r = 50f;
            entityManager.SetComponentData<Translation>(e, new Translation { Value = new float3(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r)) }); //assign some values
            entityManager.SetComponentData<Rotation>(e, new Rotation { Value = UnityEngine.Random.rotation });

            entityManager.SetSharedComponentData<RenderMesh>(e, entityRenderer); 
        }
    }
    
}
