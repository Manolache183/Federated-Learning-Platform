# Use the official Gradle image to create a build artifact
FROM gradle:jdk8 AS build

# Set the working directory
WORKDIR /home/gradle/src

# Copy the source code into the Docker image
COPY . .

# Build the project and create a JAR file
RUN gradle build --no-daemon

# Use OpenJDK to run the app
FROM openjdk:8-jre-slim

# Set the working directory
WORKDIR /app

# Copy the JAR file from the build stage
COPY --from=build /home/gradle/src/build/libs/*.jar /app/app.jar

# Expose the port the application runs on
EXPOSE 8080

# Run the application
CMD ["java", "-jar", "/app/app.jar"]