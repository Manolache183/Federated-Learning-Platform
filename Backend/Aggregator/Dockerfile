## Use the official Python image as the base image
#FROM tensorflow/tensorflow
#
## Set the working directory in the container
#WORKDIR /app
#
## Install dependencies
#RUN pip3 install --upgrade pip
#RUN pip3 install fastapi uvicorn python-multipart requests
#RUN pip3 install pika --upgrade
#
## Copy the local code into the container at /app
#COPY ./* /app/
#
## Expose the port the app runs on
#EXPOSE 5000
#
## Define the command to run your application
#CMD ["uvicorn", "routes:app", "--host", "0.0.0.0", "--port", "5000", "--reload"]
#
# Use the official Python image as the base image
FROM tensorflow/tensorflow

ENV PYTHONUNBUFFERED=1 
# Set the working directory in the container
WORKDIR /app

#COPY requirements.txt .
RUN pip3 install --upgrade pip #&& pip install -r requirements.txt
RUN pip3 install pika

# Copy the Python source code into the container
COPY . .

# Expose any necessary ports
# Replace 5672 with the RabbitMQ server port if needed
EXPOSE 5672

# Command to run the Python application
CMD ["python", "main.py"]
