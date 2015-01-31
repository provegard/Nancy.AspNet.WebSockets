
(function (doc) {
    var CanvasId = "canvas1";

    function initApp() {
        var canvasElement = document.getElementById(CanvasId),
            strokesToSend = [],
            timeoutId;

        if (!canvasElement) {
            throw new Error("Cannot find canvas element with ID " + CanvasId);
        }

        var sendStrokes = function () {
            var strokes = strokesToSend.concat();
            strokesToSend.length = 0;
            WS.sendStrokes(strokes);
        }

        WS.init();

        APP.detectStrokes(canvasElement, function(stroke) {
            APP.drawStrokes(canvasElement, [stroke]);
            strokesToSend.push(stroke);
            //WS.sendStroke(stroke);

            if (timeoutId) {
                clearTimeout(timeoutId);
            }
            timeoutId = setTimeout(sendStrokes, 10);
        });

        WS.onStrokes(function(strokes) {
            APP.drawStrokes(canvasElement, strokes);
        });
    };

    window.onerror = function(e) {
        alert(e);
    }

    doc.addEventListener("DOMContentLoaded", function() {
        initApp();
    });

})(document);

