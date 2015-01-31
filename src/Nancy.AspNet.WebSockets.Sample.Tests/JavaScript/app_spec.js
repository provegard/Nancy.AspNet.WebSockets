///<reference path="jasmine.js" />
///<reference path="~/../NancyWebsockets/Content/app.js" />

describe("application functions", function () {

    beforeEach(function () {
        var canvas = document.createElement("CANVAS");
        canvas.width = 100;
        canvas.height = 100;
        var body = document.getElementsByTagName("BODY")[0];
        body.appendChild(canvas);
        this.canvas = canvas;

        // Fill with white so that we can test stroke drawing
        var ctx = canvas.getContext("2d");
        ctx.fillStyle = "rgb(255,255,255)";
        ctx.fillRect(0, 0, 100, 100);

        this.dispatchMouseEvent = function (type, x, y) {
            var offs = canvas.topLeftOffset();
            var event = new MouseEvent(type, {
                cancelable: true,
                bubbles: true,
                view: window,
                clientX: x + offs.x,
                clientY: y + offs.y
            });
            canvas.dispatchEvent(event);
        }
    });

    afterEach(function () {
        this.canvas.parentNode.removeChild(this.canvas);
    });

    describe("Stroke (class)", function() {
        it("should expose beginX", function() {
            var s = new APP.Stroke(5, 6, 7, 8);
            expect(s.beginX).toBe(5);
        });
        it("should expose beginY", function () {
            var s = new APP.Stroke(5, 6, 7, 8);
            expect(s.beginY).toBe(6);
        });
        it("should expose endX", function () {
            var s = new APP.Stroke(5, 6, 7, 8);
            expect(s.endX).toBe(7);
        });
        it("should expose endY", function () {
            var s = new APP.Stroke(5, 6, 7, 8);
            expect(s.endY).toBe(8);
        });
    });

    describe("detectStrokes", function() {

        it("should convert mouse down - move to a single stroke", function() {
            var callback = jasmine.createSpy("callback");
            APP.detectStrokes(this.canvas, callback);

            this.dispatchMouseEvent("mousedown", 10, 5);
            this.dispatchMouseEvent("mousemove", 20, 25);

            expect(callback).toHaveBeenCalledWith(new APP.Stroke(10, 5, 20, 25));
        });

        it("should convert mouse down - move - move to two separate strokes", function () {
            var callback = jasmine.createSpy("callback");
            APP.detectStrokes(this.canvas, callback);

            this.dispatchMouseEvent("mousedown", 10, 5);
            this.dispatchMouseEvent("mousemove", 20, 25);

            callback.calls.reset();

            this.dispatchMouseEvent("mousemove", 30, 35);

            expect(callback).toHaveBeenCalledWith(new APP.Stroke(20, 25, 30, 35));
        });

        it("should stop tracking movement after mouse up", function () {
            var callback = jasmine.createSpy("callback");
            APP.detectStrokes(this.canvas, callback);

            this.dispatchMouseEvent("mousedown", 10, 5);
            this.dispatchMouseEvent("mousemove", 20, 25);

            callback.calls.reset();

            this.dispatchMouseEvent("mouseup", 20, 25);
            this.dispatchMouseEvent("mousemove", 30, 35);

            expect(callback).not.toHaveBeenCalled();
        });

    });

    describe("drawStrokes", function () {

        it("should draw a stroke with a starting point", function () {
            APP.drawStrokes(this.canvas, [new APP.Stroke(5, 5, 10, 5)]);
            var context = this.canvas.getContext("2d");
            var imageData = context.getImageData(5, 5, 5, 1).data;
            expect(imageData[0]).not.toBe(255);
        });

        it("should not draw a stroke beyond its finishing point", function () {
            APP.drawStrokes(this.canvas, [new APP.Stroke(5, 5, 10, 5)]);
            var context = this.canvas.getContext("2d");
            var imageData = context.getImageData(11, 5, 1, 1).data;
            expect(imageData[0]).toBe(255);
        });

        it("should draw multiple strokes", function () {
            APP.drawStrokes(this.canvas, [
                new APP.Stroke(5, 5, 10, 5),
                new APP.Stroke(15, 5, 20, 5)
            ]);
            var context = this.canvas.getContext("2d");
            var imageData = context.getImageData(15, 5, 5, 1).data;
            expect(imageData[0]).not.toBe(255);
        });

        it("should not connect separate strokes", function () {
            APP.drawStrokes(this.canvas, [
                new APP.Stroke(5, 5, 10, 5),
                new APP.Stroke(15, 5, 20, 5)
            ]);
            var context = this.canvas.getContext("2d");
            var imageData = context.getImageData(11, 5, 1, 1).data;
            expect(imageData[0]).toBe(255);
        });
    });
});