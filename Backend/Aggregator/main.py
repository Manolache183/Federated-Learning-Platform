import routes

if __name__ == "__main__":
    print("Aggregator started")
    app = routes.getApp()
    app.run(host="0.0.0.0", port=5000, debug=True)

