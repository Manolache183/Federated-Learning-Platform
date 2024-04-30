import tensorflow as tf
import os

def getEnvVar(var):
    if var in os.environ:
        variableValue = os.environ[var]
        print(f"The value of {var} is: {variableValue}")
        return variableValue
    else:
        raise Exception("Environment variable " + var + " not set")
    
numClients = int(getEnvVar("NUM_CLIENTS")) # This will eventually be removed, used to identify which part of the dataset to train on
receivedModels = 0

async def aggregateAndEvaluateAsync():
    globalModel = await aggregateModels()
    evaluateGlobalModel(globalModel)

async def aggregateModels():
    print("TensorFlow version:", tf.__version__)

    globalModel = tf.keras.models.Sequential([
      tf.keras.layers.Flatten(input_shape=(28, 28)),
      tf.keras.layers.Dense(128, activation='relu'),
      tf.keras.layers.Dropout(0.2),
      tf.keras.layers.Dense(10)
    ])

    allClientWeights = []

    for i in range(numClients): # this can be more ellegant maybe? without cloning and all the stuff
        filename = f'uploaded_model_{i}.weights.h5'
        
        clientModel = tf.keras.models.clone_model(globalModel)
        clientModel.load_weights(filename)
        allClientWeights.append(clientModel.get_weights())        

    agregatedWeights = [tf.zeros_like(w) for w in globalModel.get_weights()]
    clientWeightsLen = len(allClientWeights)

    for clientWeights in allClientWeights:
        for i, w in enumerate(clientWeights):
            agregatedWeights[i] += w / clientWeightsLen
    
    globalModel.set_weights(agregatedWeights)

    return globalModel
    
def evaluateGlobalModel(globalModel):
    mnist = tf.keras.datasets.mnist

    _, (x_test, y_test) = mnist.load_data()
    x_test = x_test / 255.0
    
    lossFn = tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True)
    globalModel.compile(optimizer='adam',
                  loss=lossFn,
                  metrics=['accuracy'])    

    globalModel.evaluate(x_test, y_test, verbose=2)

