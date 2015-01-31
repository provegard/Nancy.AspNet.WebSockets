(function (module) {
    var ConnectBtnId = "connectBtn",
        DisconnectBtnId = "disconnectBtn",
        MessagesId = "messages",
        DrawingBoardInputId = "drawingBoardName",
        UserInputId = "userName",
        MessageBtnId = "messageBtn",
        MessageInputId = "messageText";

    var drawingUrl = "ws://" + window.location.host + "/drawing/";
    var currentWs;
    var strokeListeners = [];

    function getElement(id) {
        var e = document.getElementById(id);
        if (!e) throw new Error("Cannot find element with ID " + id);
        return e;
    }

    function sendStrokesToListeners(strokes) {
        for (var i = 0, j = strokeListeners.length; i < j; i++) {
            strokeListeners[i](strokes);
        }
    }

    function connectToDrawingBoard() {
        if (currentWs) return;

        var drawingBoardNameElement = getElement(DrawingBoardInputId),
            userNameElement = getElement(UserInputId),
            drawingBoardName = drawingBoardNameElement.value,
            userName = userNameElement.value;
        if (!drawingBoardName || !userName) {
            alert("Drawing board name and user name are required.");
            return;
        }

        var url = drawingUrl + encodeURIComponent(drawingBoardName) + "?name=" + encodeURIComponent(userName);
        var ws = new WebSocket(url);
        ws.onopen = function () {
            addMessage("Connected!");
            currentWs = ws;
        };

        ws.onmessage = function (e) {
            if (typeof e.data === "string") {
                addMessage(e.data);
            } else {
                // blob
                var reader = new FileReader(e.data);
                reader.addEventListener("loadend", function () {
                    var arr = new Int32Array(reader.result);
                    var strokes = [];
                    for (var i = 0, j = arr.length; i < j; i += 4) {
                        var stroke = new APP.Stroke(arr[i], arr[i + 1], arr[i + 2], arr[i + 3]);
                        strokes.push(stroke);
                    }
                    sendStrokesToListeners(strokes);
                });
                reader.readAsArrayBuffer(e.data);
            }
        };

        ws.onerror = function (e) {
            addMessage("Error: " + e);
        };

        ws.onclose = function(e) {
            addMessage("Closed");
            currentWs = null;
        }
    }

    function disconnectFromDrawingBoard() {
        if (!currentWs) return;

        currentWs.close();
        currentWs = null;
    }

    function sendMessageToServer() {
        var messageText = getElement(MessageInputId),
            msg = messageText.value;
        if (msg && currentWs) {
            currentWs.send(messageText.value);
            messageText.value = "";
        }
    }

    function addMessage(msg) {
        var messages = getElement(MessagesId);
        messages.innerHTML = msg + "<br/>" + messages.innerHTML;
    }

    module.sendStrokes = function(strokes) {
        if (!currentWs) return;

        var arr = new Int32Array(4 * strokes.length);
        for (var i = 0, j = strokes.length; i < j; i++) {
            var stroke = strokes[i];
            arr[i * 4] = stroke.beginX;
            arr[i * 4 + 1] = stroke.beginY;
            arr[i * 4 + 2] = stroke.endX;
            arr[i * 4 + 3] = stroke.endY;
        }
        currentWs.send(arr);
    }

    module.onStrokes = function(listener) {
        strokeListeners.push(listener);
    }

    module.init = function() {
        var connect = getElement(ConnectBtnId),
            disconnect = getElement(DisconnectBtnId),
            sendMessage = getElement(MessageBtnId);

        connect.addEventListener("click", connectToDrawingBoard);
        disconnect.addEventListener("click", disconnectFromDrawingBoard);
        sendMessage.addEventListener("click", sendMessageToServer);
    };

})(window.WS || (window.WS = {}));