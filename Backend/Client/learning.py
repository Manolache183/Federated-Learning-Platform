# Eventually the data should be received through the backend web server, as a global model

import tensorflow as tf
import requests
import os

def getEnvVar(var):
    if var in os.environ:
        variableValue = os.environ[var]
        print(f"The value of {var} is: {variableValue}")
        return variableValue
    else:
        raise Exception("Environment variable " + var + " not set")
    
clientID = float(getEnvVar("CLIENT_ID")) # This will eventually be removed, used to identify which part of the dataset to train on
numClients = float(getEnvVar("NUM_CLIENTS")) # This will eventually be removed, used to identify which part of the dataset to train on

async def sendModelToAggregatorAsync():
    model = await trainAsync();
    filename = "my_model.weights.h5"
    model.save_weights(filename)
    
    url = "http://aggregator:5000/upload_model";
    
    with open(filename, "rb") as file:
        files = {'file': (filename, file)}
        data = {"client_id": int(clientID)}
        r = requests.post(url, files=files, data=data)
    
    if (r.status_code == 200):
        print("Aggregator received my model")
    elif (r.status_code == 202):
        print("Aggregator is started to aggregate all the models")
    else:
        print("Aggregator returned an error: " + r.text)

async def trainAsync():
    print("TensorFlow version:", tf.__version__)

    mnist = tf.keras.datasets.mnist

    (x_train, y_train), _ = mnist.load_data()
    
    #extract only the required part of the training data
    trainSetLength = len(x_train)
    start = clientID * trainSetLength / numClients
    end = min((clientID + 1) * trainSetLength / numClients, trainSetLength)
    
    start = int(start)
    end = int(end)
    x_train = x_train[start:end]
    y_train = y_train[start:end]
    x_train = x_train / 255.0
    
    model = tf.keras.models.Sequential([
      tf.keras.layers.Flatten(input_shape=(28, 28)),
      tf.keras.layers.Dense(128, activation='relu'),
      tf.keras.layers.Dropout(0.2),
      tf.keras.layers.Dense(10)
    ])

    lossFn = tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True)

    model.compile(optimizer='adam',
                  loss=lossFn,
                  metrics=['accuracy'])

    model.fit(x_train, y_train, epochs=5)
    
    print("Training complete")
    
    return model
    

    
        