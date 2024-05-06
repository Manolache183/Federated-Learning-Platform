from firebase_admin import credentials, initialize_app, storage

class FirebaseStorageService:
	def __init__(self):
		cred = credentials.Certificate("federated-learning-platf-c15c0-firebase-adminsdk-slcw0-90585331c4.json")
		initialize_app(cred, {'storageBucket': 'federated-learning-platf-c15c0.appspot.com'})
		
		self.bucket = storage.bucket()

	def downloadClientModels(self):
		blobs = self.bucket.list_blobs(prefix="clientModels/mnist/client_mnist_model_")
		for blob in blobs:
			print("Blob name: " + blob.name)
			# here should be a download happening
			
	def uploadModel(self, modelName):
		modelPath = f"aggregatedModels/mnist/{modelName}"
		blob = self.bucket.blob(modelPath)
		
		content = "This is the content of the model from the Aggregator"
		blob.upload_from_string(content) # this should be "upload from file"


