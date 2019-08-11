from get_aruco import return_scan
import cv2
from flask import Flask, request, render_template
import json

app = Flask(__name__)

# POST request with image, responce JSON with scan results
@app.route("/", methods = ['POST'])
def hello():
    print(request)
    f = request.files['file']
    f.save("temp")
    
    return json.dump(return_scan(cv2.imread("temp")))

# Run this file from console only! "python server.py"
if __name__ == "__main__":
    app.run(host='0.0.0.0')
