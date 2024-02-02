from fastapi import FastAPI, File, UploadFile, Form
from fastapi.responses import JSONResponse
import asyncio
import learning

app = FastAPI()

@app.get("/ping")
async def ping():
    return {"message": "OK from Aggregator"}

@app.post("/upload_model")
async def receive_model(file: UploadFile = File(...), client_id: str = Form(...)):
    i = int(client_id)
    filename = f'uploaded_model_{i}.weights.h5'
    
    with open(filename, "wb") as f:
        f.write(file.file.read())

    learning.receivedModels += 1

    if learning.receivedModels == learning.numClients:
        asyncio.create_task(learning.aggregateAndEvaluateAsync())
        learning.receivedModels = 0
        return {"message": "All models received, starting to agregate"}

    return {"message": "Model received"}
