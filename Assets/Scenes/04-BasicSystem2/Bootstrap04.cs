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

namespace Bootstrap04 {
    public class Bootstrap04:MonoBehaviour {

        private static EntityManager entityManager;
        private static RenderMesh entityRenderer;

        private static EntityArchetype entityTemplate;


        public void Awake() {
            entityManager = World.Active.GetOrCreateManager<EntityManager>();
            entityTemplate = entityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(RenderMesh), typeof(LocalToWorld), typeof(MovementSpeed), typeof(RotationSpeed)); //added movement speed comp to archetype

        }

        public void Start() {
            MeshFilter f = GameObject.FindObjectOfType<MeshFilter>();
            entityRenderer.mesh = f.sharedMesh;


            entityRenderer.material = GameObject.FindObjectOfType<MeshRenderer>().material;
            entityRenderer.receiveShadows = true;
            entityRenderer.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;

            int numToSpawn = 50000;

            var random = new Unity.Mathematics.Random(1000);

            for(int c = 0; c < numToSpawn; c++) {
                Entity e = entityManager.CreateEntity(entityTemplate);
                float r = 50f;
                entityManager.SetComponentData<Translation>(e, new Translation { Value = new float3(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r)) });
                entityManager.SetComponentData<Rotation>(e, new Rotation { Value = UnityEngine.Random.rotation });

                entityManager.SetComponentData<MovementSpeed>(e, new MovementSpeed { speed = -UnityEngine.Random.value }); //setting movementspeed comp data to random val
                entityManager.SetComponentData<RotationSpeed>(e, new RotationSpeed { speed = UnityEngine.Random.value, rotationAxis = random.NextFloat3Direction()});

                entityManager.SetSharedComponentData<RenderMesh>(e, entityRenderer);
            }
        }

    }

    [Serializable]
    public struct MovementSpeed:IComponentData { 
        public float speed;
    }

    [Serializable]
    public struct RotationSpeed:IComponentData {
        public float speed;
        public float3 rotationAxis;
    }
   
 
    public class MovementSpeedSystem:JobComponentSystem {
        [BurstCompile]
        struct MovementSpeedJob:IJobProcessComponentData<Translation, MovementSpeed, RotationSpeed, Rotation> { //if we also want to rotate these objects randomly, should that be a different system, or should we use the same system? 
            //I guess try both and profile to see if one is faster than the other?
            //I think the idea is to keep the systems and the data they work on small and modular, so separate systems is probably better?
            //both in the same system is like 0.05ms for 50k entities. Lets see what happens when I split them into two systems
             //SPOLIER: When split into two systems it takes MORE Time, I guess because it has to iterate over all the entities TWICE instead of once to do the same thing?
            public float DeltaTime; 

            public void Execute(ref Translation translation, [ReadOnly] ref MovementSpeed movementSpeed, [ReadOnly] ref RotationSpeed rotSpeed, ref Rotation rot) {
          

                translation.Value = translation.Value + (math.up() * movementSpeed.speed * DeltaTime);
                rot.Value = math.mul(math.normalize(rot.Value), quaternion.AxisAngle(rotSpeed.rotationAxis, rotSpeed.speed * DeltaTime));
                
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
