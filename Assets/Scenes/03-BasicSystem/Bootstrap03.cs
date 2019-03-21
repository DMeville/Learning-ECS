using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using System;
using Unity.Jobs;
using Unity.Burst;

namespace Bootstrap03 { 
    public class Bootstrap03:MonoBehaviour {

        private static EntityManager entityManager;
        private static RenderMesh entityRenderer;

        private static EntityArchetype entityTemplate;


        public void Awake() {
            entityManager = World.Active.GetOrCreateManager<EntityManager>();
            entityTemplate = entityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(RenderMesh), typeof(LocalToWorld), typeof(MovementSpeed)); //added movement speed comp to archetype

        }

        public void Start() {
            MeshFilter f = GameObject.FindObjectOfType<MeshFilter>();
            entityRenderer.mesh = f.sharedMesh;


            entityRenderer.material = GameObject.FindObjectOfType<MeshRenderer>().material;
            entityRenderer.receiveShadows = true;
            entityRenderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;

            int numToSpawn = 50000;

            for(int c = 0; c < numToSpawn; c++) {
                Entity e = entityManager.CreateEntity(entityTemplate);
                float r = 50f;
                entityManager.SetComponentData<Translation>(e, new Translation { Value = new float3(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r)) });
                entityManager.SetComponentData<Rotation>(e, new Rotation { Value = UnityEngine.Random.rotation });

                entityManager.SetComponentData<MovementSpeed>(e, new MovementSpeed { speed = UnityEngine.Random.value }); //setting movementspeed comp data to random val

                entityManager.SetSharedComponentData<RenderMesh>(e, entityRenderer);
            }
        }

    }

    //new stuff below, mostly.  Adding a few lines in the spawn code

    [Serializable]
    public struct MovementSpeed:IComponentData { //component for storing the speed data, and acts as a tag so we can grab all components with MovementSpeed and Translate to know those ones should be processed by the below MovementSpeedSystem
        public float speed;
    }

    //do we need to register this system somewhere? I didn't and pressed play and things were moving, so I guess it doesn't need to be registered somewhere?
    //How does it know to run this system? Is it because it's shareing a file with Bootstrap3? Let's extract it to it's own classes and try... NOPE. Extracted it out of this class and into it's own class, not on any objects and things still moved.
    //How do I disable a system? JobComponentSystem objects have a .enabled that we can turn off to disable the system.  Where can we get a ref to this system from outside code?


    public class MovementSpeedSystem:JobComponentSystem {
        [BurstCompile]
        struct MovementSpeedJob:IJobProcessComponentData<Translation, MovementSpeed> { //this system creates a job (that runs on multiple threads) and executes the method on all entities with Translation and MovementSpeed components

            public float DeltaTime; //this isn't necessary, but shows how we can have additional data that we want to use in execute?

            public void Execute(ref Translation translation, [ReadOnly] ref MovementSpeed movementSpeed) {
                //translation.Value = translation.Value + (math.up() * movementSpeed.speed*DeltaTime); //Version 1 test

                //what other things can we access from here?
                //Time.deltaTime but then for every execute we have to access it, so I think we only want to do stuff with the data we have
                //eg, use nothing but the stuff in translation and movementspeed components?
                //eg, modify the data in translation however we want using the movementspeed data?

                //or we can store other data we need, like Time.deltaTime outside the job so it's only acessed ONCE (and every execution should use the same deltaTime anyways?)

                translation.Value = translation.Value + (math.up() * movementSpeed.speed * DeltaTime); //Version 2 test
            }
        }


        //Where is this called from? Internally? 
        //oh so maybe this is being  called internally every update, and every update it's creating a new MovementSpeedJob (which has the filter Translations and MovementSpeed components)
        //and then scheduling that job so that it runs every frame?

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            var job = new MovementSpeedJob() {
                DeltaTime = Time.deltaTime //assigning this so we can access it in the job?
            };

            return job.Schedule(this, inputDeps);    //why do we pass in (this)? What is inputDeeps ("dependsOn")? doesn't seem to have any public properties so idk

        }
    }
}

