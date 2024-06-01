import json
from typing import List, Tuple
import base64
from firebase_admin import credentials, initialize_app, storage

class FirebaseStorageService:
	def __init__(self):
		cred = credentials.Certificate("federated-learning-platform-secrets.json")
		initialize_app(cred, {'storageBucket': 'federated-learning-platf-c15c0.appspot.com'})
		
		self.bucket = storage.bucket()

	def downloadClientModels(self) -> List[Tuple[int, List[bytes]]]:
		blobs = self.bucket.list_blobs(prefix="clientModels/mnist/client_mnist_model_")
		parameters = []
		for blob in blobs:
			print("Blob name: " + blob.name)
			content = blob.download_as_string()
			parameters_dict=json.loads(content)
			# Maybe change this to include actual examples count
			num_examples = 1
			model_weights = [base64.b64decode(parameters_dict[key]) for key in sorted(parameters_dict.keys())]
			parameters.append((num_examples, model_weights))
		return parameters
			
	def uploadModel(self, model_name, parameters: List[bytes]):
		model_path = f"aggregatedModels/mnist/{model_name}"
		blob = self.bucket.blob(model_path)

		parameters_dict = {f"layer{i}": base64.b64encode(param).decode('utf-8') for i, param in enumerate(parameters)}
		content = json.dumps(parameters_dict)

		blob.upload_from_string(content) # this should be "upload from file"

