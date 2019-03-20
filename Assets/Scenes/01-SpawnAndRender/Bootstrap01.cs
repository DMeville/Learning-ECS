using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;

public class Bootstrap:MonoBehaviour {
    //doesn't have to be a monobehaviour, but needs to be run before the scene starts?

    private static EntityManager entityManager;
    private static RenderMesh entityRenderer; //this can be shared by OBJECTS THAT ARE THE SAME (mesh, colour maybe?) (used to be MeshInstanceRenderer)

    private static EntityArchetype entityTemplate; //Can create an entity using the archtype, which speeds up some stuff under the hood (how/where in mem the entity is stored, so it's by it's friends and can be processed fast)
    
    public static Mesh mesh;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] //nothing special or important here, just triggering a method to run at a specific time (startup before scene load). 
    //these seem to run even if the BootStrap behaviour isn't attached to anything. Could we change this to Awake/Start?
    public static void Initialize() {

        //create our entity manager 
        //create any archtypes we are going to be using?

        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        entityTemplate = entityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(RenderMesh), typeof(LocalToWorld)); //TransformMatrix is not used anymore? Any entity with pos/rot/scale gets one automatically under the hood?
    }



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void SceneStartup() {
        //entityRenderer = new RenderMesh(); //this may or may not be necessary?
        MeshFilter f = GameObject.FindObjectOfType<MeshFilter>(); //find the gameobject with the mesh we want to use for our entities
        entityRenderer.mesh = f.sharedMesh; //using .mesh here instead of .sharedMesh works in the  editor, but crashes builds!!
       

        entityRenderer.material = GameObject.FindObjectOfType<MeshRenderer>().material;
        entityRenderer.receiveShadows = true;
        entityRenderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;

        int numToSpawn = 50000;

        //NativeArray<Entity> el = new NativeArray<Entity>(50, Allocator.Temp); //create an list to store the entities we are about to create
        for(int c = 0; c < numToSpawn; c++) { 
            Entity e = entityManager.CreateEntity(entityTemplate); //create the entity from the archtype template (x50).  Since we create from template, we don't need to add components and then set them, just set them?
            float r = 50f;
            entityManager.SetComponentData<Translation>(e, new Translation { Value = new float3(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r)) }); //assign some values
            entityManager.SetComponentData<Rotation>(e, new Rotation { Value = UnityEngine.Random.rotation });

            entityManager.SetSharedComponentData<RenderMesh>(e, entityRenderer); //setting this to the entityRenderer component *should* render it? Need to somehow set entityRenderer.mesh though. Not sure where the entry point is
        }
    }
}
