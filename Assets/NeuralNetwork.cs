//https://www.youtube.com/watch?v=aircAruvnKk

using System;
using System.Collections;
using System.Collections.Generic;
using UE = UnityEngine;

public class NeuralNetwork {
    public Node[][] Network;

    public float[] GetOutput(float[] input) {
        if (input.Length != Network[0].Length)
            throw new Exception("Input length does not match network width!");
        for (int i = 0; i < input.Length; i++)
            Network[0][i].value = input[i];
        
        for (int x = 1; x < Network.Length; x++) {
            for (int y = 0; y < Network[x].Length; y++) {
                float value = 0;
                for (int i = 0; i < Network.Length; i++)
                    value += Network[x-1][i].value * Network[x][y].weights[i];
                Network[x][y].value = (float)(1/(1+Math.Pow(Math.E, -(value + Network[x][y].bias))));
            }
        }

        float[] output = new float[Network.GetLength(1)];
        for (int i = 0; i < Network[Network.Length-1].Length; i++)
            output[i] = Network[Network.Length-1][i].value;
        return output;
    }

    //Generate random network
    public void GenerateNetwork(int[] size, int weightRange, int biasRange) {
        /*  int[], min length of 2.
            Each entry defines a new layer's size.  */
        
        if (size.Length < 2)
            throw new Exception("Network needs at least two layers!");

        System.Random rand = new System.Random();
        Network = new Node[size.Length][];
        for (int x = 0; x < size.Length; x++) {
            Network[x] = new Node[size[x]];
            for (int y = 0; y < size[x]; y++) {
                Node node = new Node();
                Network[x][y] = node;
                if (0 < x) {
                    //node.predecessors = Network[x-1];
                    node.weights = new float[Network[x-1].Length];
                    for (int i = 0; i < node.weights.Length; i++)
                        node.weights[i] = rand.Next(-weightRange*100, weightRange*100)/100f;
                }
                node.bias = rand.Next(-biasRange*100, biasRange*100)/100f;
            }
        }
    }

    //Mutate network
    public void Mutate() {
        int netsize = 0;
        for (int i = 1; i < Network.Length; i++)
            netsize += Network[i].Length;
        int mutateCount = (int)Math.Ceiling(UE.Random.Range(0, netsize * 0.2f));
        
        for (int i = 0; i < mutateCount; i++) {
            int x = UE.Random.Range(1,Network.Length);
            int y = UE.Random.Range(0, Network[x].Length);
            int z = UE.Random.Range(-1, Network[x][y].weights.Length);
            if (z == -1)
                Network[x][y].bias += UE.Random.Range(-1f, 1f);
            else
                Network[x][y].weights[z] += UE.Random.Range(-1f, 1f);
        }
    }

    //Save/load network
    public string Save(string filePath) {
        string[] layers = new string[Network.Length];

        for(int i = 0; i < layers.Length; i++) {
            layers[i] = UE.JsonUtility.ToJson(Network[i]);
        }
        string json = UE.JsonUtility.ToJson(layers);
        


        /*
        Turn each layer into string
        serialize list of strings
        */


        System.IO.File.WriteAllText(filePath, json);
        //System.IO.File.WriteAllLines(filePath, lines);
        return json;
    }

    /*
    public void Load(string filePath) {
        string jsonValue = System.IO.File.ReadAllText(filePath);
        Network = JsonConvert.DeserializeObject<Node[][]>(jsonValue);
    }*/

    protected class Layer {

    }

    public class Node {
        public float[] weights;
        public float bias;
        public float value;
    }
}