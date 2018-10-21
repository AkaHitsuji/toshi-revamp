# import logging

from flask import request, jsonify

from revamp import application as app

# logger = logging.getLogger(__name__)

@app.route('/square', methods=['POST'])
def evaluate():
    data = request.get_json()
    app.logger.info("data sent for evaluation {}".format(data))
    inputValue = data.get("input")
    result = inputValue * inputValue
    app.logger.info("My result :{}".format(result))
    return jsonify(result)

