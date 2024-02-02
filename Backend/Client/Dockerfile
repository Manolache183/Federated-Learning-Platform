# Use the official Python image as the base image
FROM tensorflow/tensorflow

# Set the working directory in the container
WORKDIR /app

# Install dependencies
RUN pip3 install --upgrade pip
RUN pip3 install fastapi uvicorn requests

# Copy the local code into the container at /app
COPY ./* /app/

# Expose the port the app runs on
EXPOSE 5000

# Define the command to run your application
CMD ["uvicorn", "routes:app", "--host", "0.0.0.0", "--port", "5000", "--reload"]
