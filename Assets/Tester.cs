using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour {
    private void Start() {
        print(new NeuralNetwork().Save("C:/Users/Mac/Desktop/NN.txt"));
    }
}