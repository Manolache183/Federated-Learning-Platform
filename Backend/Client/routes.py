from fastapi import FastAPI
import learning
import asyncio

app = FastAPI()

@app.get('/ping')
def ping():
    return {"message": "OK"}

# Eventually the communication will happen through the backend web server, not directly to the aggregator
@app.post('/train_and_send', status_code=202)
async def train_and_send_model():
    train_task = asyncio.create_task(learning.sendModelToAggregatorAsync())
    return {"message": "Model started to train"}
