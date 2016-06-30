using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public sealed class Serialization {
    public static void CreateDirectory(string directory) { //Creates directory at the given path if it doesn't exist
        if(!Directory.Exists(directory)) //Makes sure that the directory doesn't exist
            Directory.CreateDirectory(directory); //Creates directory
    }
        
    public static void SaveXML(object obj, string directory, string fileName) { //Saves an XML file
        string path = directory + fileName; //Combines both paths
        CreateDirectory(directory); //Creates directory if it doesn't exist

        XmlSerializer serializer = new XmlSerializer(obj.GetType()); //Creates the XML serializer

        using(FileStream stream = File.Open(path, FileMode.Create)) //Automatically closes the file after it's done being used
            serializer.Serialize(stream, obj); //Serializes data to the file
    }

    public static LoadType LoadFromXML<LoadType>(LoadType type, string directory, string fileName) { //Loads XML file if it exists. If not, creates a new one
        string path = directory + fileName; //Combines both paths
        CreateDirectory(directory); //Creates directory if it doesn't exist

        if(File.Exists(path)) { //Makes sure XML file exists
            XmlSerializer serializer = new XmlSerializer(typeof(LoadType)); //Creates the XML serializer

            using(FileStream stream = File.Open(path, FileMode.Open)) //Automatically closes the file after it's done being used
                return (LoadType)serializer.Deserialize(stream); //Deserializes the data and returns it
        } else {
            SaveXML(type, directory, fileName); //Creates a new XML file

            return type;
        }
    }

    //Serializes/Deserializes a list to/from an XML file
    public static List<ListType> ListAsXML<ListType>(List<ListType> list, bool serializing, string directory, string fileName, string rootName = null) {
        string path = directory + fileName; //Combines both paths
        CreateDirectory(directory); //Creates directory if it doesn't exist

        List<System.Type> types = new List<System.Type>();

        foreach(ListType obj in list) { //Loops through every item in the list
            System.Type type = obj.GetType();

            if(!types.Contains(type)) //If types list doesn't have the current object's type on it
                types.Add(type); //Add current object's type to the list
        };

        XmlSerializer serializer; //Initialize serializer

        if(rootName == null) //If root name wasn't given
            serializer = new XmlSerializer(typeof(List<ListType>), null, types.ToArray(), null, null); //Create with default root name
        else
            serializer = new XmlSerializer(typeof(List<ListType>), null, types.ToArray(), new XmlRootAttribute(rootName), null); //Create with given root name

        /* Serilization */
        if(serializing) {
            using(FileStream stream = File.Open(path, FileMode.Create)) //Automatically closes the file after it's done being used
                serializer.Serialize(stream, list); //Serializes data to the file

            return list;
        }
        /* Deserilization */
        else {
            if(File.Exists(path)) { //Makes sure XML file exists
                using(FileStream stream = File.Open(path, FileMode.Open)) //Automatically closes the file after it's done being used
                    return (List<ListType>)serializer.Deserialize(stream); //Deserializes the data and returns it
            } else {
                using(FileStream stream = File.Open(path, FileMode.Create)) //Automatically closes the file after it's done being used
                    serializer.Serialize(stream, list); //Serializes data to the file

                return list;
            }
        }
    }
}
