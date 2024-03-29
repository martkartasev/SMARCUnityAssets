using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace DefaultNamespace
{

    public class ROSPrepareRobot : MonoBehaviour
    {
        GameObject robot;
        [Tooltip("ROBOTNAME<Link Separator>LINKNAME<Link separator>CHILDLINK")]
        public static readonly string linkSeparator = "_";

        public static List<GameObject> GetAllChildrenLinks(GameObject Go)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i< Go.transform.childCount; i++)
             {
                 var child_go = Go.transform.GetChild(i).gameObject;
                 if(child_go.GetComponent<UrdfLink>())
                 {
                    list.Add(child_go);
                    list.AddRange(GetAllChildrenLinks(child_go));
                 } 
             }
             return list;
        }

        private void Rename()
        {
            string baseName = name.Split(" ")[0];
            string num = "0";
            // If there are parens in the name, we use the number inside
            if(name.Contains("(")){
                // NAME (NUM)
                //      ^---- Split("(")
                //       NUM) -> [1]
                //        --^ Split(")")
                //       NUM -> [0]
                num = name.Split("(")[1].Split(")")[0];
            };
            string rosName = baseName + "" + num;
            robot.transform.name = rosName;
            // And we also want to add a prefix to _each link_ in the robot tree
            // such that they all have unique names when working with ROS TF trees. 
            // I've looked into this, and there is basically 2 ways of handling multiple
            // robots of the same kind. Either publish into /robot1/tf and /robot2/tf
            // or publish into /tf/robot1_base_link, /tf/robot1_whatever etc.
            // First option makes it hard to work with shared frames and relative stuff
            // since the trees are completely separate.
            // Second option just requires people to use the robots's names to properly
            // identify anything. I choose nr2.

            // Get all the children links

            var children_links = GetAllChildrenLinks(robot);
            foreach(GameObject child_go in children_links)
            {
                // and rename them to have the same name as a prefix
                child_go.transform.name = rosName+ linkSeparator +child_go.transform.name;
            }

        }

        private void StripURDF()
        {
            // Simply destroy all the simulation stuff from a sibling robot object
            // which is generated by URDF importer.
            // For a reason I could not decipher, the articulation tree is not proper
            // and the thruster parts just fall apart...
            // This shoul ideally be fixed in the xacro or urdf of sam so that
            // when imported, it stays in one piece.
            // At the same time, we do our own sim of sam's dynamics, so none of these
            // sim-related stuff matter anyways.
            // Why not just delete those components manually? So that we can modify and re-import
            // URDF files like this later without having to re-delete and remember this stuff.
            // GetComponentsInChildren is recursive! :D
            foreach(Behaviour childComp in robot.GetComponentsInChildren<Behaviour>()){
                childComp.enabled = false;
            }
            // Also remove any colliders auto-added from the URDF, since they'll cause
            // physics issues if they are inside the vehicle
            foreach(Collider childComp in robot.GetComponentsInChildren<Collider>()){
                childComp.enabled = false;
            }
        }


        // Use Awake so that this stuff is done _before_ any Starts
        // Especially useful for robot-name-related-topic reasons.
        void Awake()
        {
            // Find the auv child by tag, not name, because we change the name...
            robot = Utils.FindDeepChildWithTag(gameObject, "robot");
            if(robot == null){
                Debug.Log("robot GO was null, nothing will work...");
                return;
            }

            Rename();
            StripURDF();

            //TODO dirty hacks these two... v
            if(TryGetComponent<SAMThrusterWiggler>(out var wiggler))
            {
                wiggler.Setup(robot);
            }

            var motion_model_tf = transform.Find("sam_motion_model");
            if(motion_model_tf.gameObject.TryGetComponent<SamActuatorController>(out var cont))
            {
                cont.Setup(robot);
            }
            // ^ hacks

            // Setup the sensors in the "Sensors" child
            // They all need access to the auv object
            // These sensor objects should extend Sensor object
            var sensors_tf = transform.Find("Sensors");
            if(sensors_tf != null)
            {
                for(int i=0; i<sensors_tf.childCount; i++)
                {
                    ISensor[] sensor_scripts = sensors_tf.GetChild(i).gameObject.GetComponents<ISensor>();
                    foreach(ISensor sensor_script in sensor_scripts)
                    {
                        // Avoid running setup if not needed.
                        if(sensor_script == null) continue; 
                        Behaviour b = (Behaviour)sensor_script;
                        if(!b.enabled) continue;
                        if(!b.gameObject.activeInHierarchy) continue;

                        sensor_script.Setup(robot, linkSeparator);
                    }
                }
            }
        }

    }

}