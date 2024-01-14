from flask import Flask, request

app = Flask(__name__)

@app.route('/')
def hello_geek():
    return '<h1>Hello from Flask & Docker</h2>'

@app.route('/ping', methods=['POST'])
def ping():
    data = request.data.decode('utf-8')

    if data == "ping":
        return "OK"
    else:
        return "Unknown command"

if __name__ == "__main__":
    print("Worker started")
    app.run(host="0.0.0.0", port=5000, debug=True)
