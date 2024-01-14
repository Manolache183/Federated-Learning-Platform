# Use the official Python image as the base image
FROM python:3.8-slim

# Set the working directory in the container
WORKDIR /app

# Install dependencies
RUN pip install flask requests

# Copy the local code into the container at /app
COPY Worker.py /app/

# Expose the port the app runs on
EXPOSE 5000

# Set the FLASK_APP environment variable
ENV FLASK_APP=Worker.py

# Define the command to run your application
CMD ["flask", "run", "--host", "0.0.0.0"]
