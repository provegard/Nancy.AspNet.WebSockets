/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
(function (module) {

    function topLeftOffset() {
        var totalOffsetX = 0,
            totalOffsetY = 0,
            currentElement = this;

        do {
            totalOffsetX += currentElement.offsetLeft - currentElement.scrollLeft;
            totalOffsetY += currentElement.offsetTop - currentElement.scrollTop;
        } while (currentElement = currentElement.offsetParent);

        return { x: totalOffsetX, y: totalOffsetY };
    }

    // http://stackoverflow.com/questions/55677/how-do-i-get-the-coordinates-of-a-mouse-click-on-a-canvas-element
    function relMouseCoords(event) {
        var total = this.topLeftOffset(),
            canvasX = event.pageX - total.x,
            canvasY = event.pageY - total.y;
        return { x: canvasX, y: canvasY };
    }
    HTMLCanvasElement.prototype.relMouseCoords = relMouseCoords;
    HTMLCanvasElement.prototype.topLeftOffset = topLeftOffset;

    module.Stroke = function(beginX, beginY, endX, endY) {
        this.beginX = beginX;
        this.beginY = beginY;
        this.endX = endX;
        this.endY = endY;
    }

    module.detectStrokes = function(element, callback) {

        var lastPos, isTracking;

        element.addEventListener("mousedown", startTracking);
        element.addEventListener("mousemove", createStroke);
        element.addEventListener("mouseup", stopTracking);

        function stopTracking() {
            isTracking = false;
        }

        function startTracking(event) {
            lastPos = element.relMouseCoords(event);
            isTracking = true;
        }

        function createStroke(event) {
            if (!isTracking) return;
            var begin = lastPos;
            lastPos = element.relMouseCoords(event);
            callback(new module.Stroke(begin.x, begin.y, lastPos.x, lastPos.y));
        }

    }

    module.drawStrokes = function(element, strokes) {
        var context = element.getContext("2d");
        context.beginPath();
        for (var i = 0, j = strokes.length; i < j; i++) {
            var stroke = strokes[i];
            context.moveTo(stroke.beginX, stroke.beginY);
            context.lineTo(stroke.endX, stroke.endY);
        }
        context.stroke();
    }

})(window.APP || (window.APP = {}));