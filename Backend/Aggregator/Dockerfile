FROM tensorflow/tensorflow

ENV PYTHONUNBUFFERED=1 
WORKDIR /app

#COPY requirements .
RUN pip3 install --upgrade pip #&& pip install -r requirements
RUN pip3 install pika
RUN pip3 install firebase_admin
RUN pip3 install numpy

COPY . .

EXPOSE 5672

CMD ["python", "main.py"]
