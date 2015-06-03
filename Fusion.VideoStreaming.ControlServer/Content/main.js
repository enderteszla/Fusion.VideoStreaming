function EventHandler(InstanceID) { this.InstanceID = InstanceID; }
EventHandler.prototype.State = { Left: false, Right: false, Within: true };
EventHandler.prototype.CursorPosition = { X: -1, Y: -1 };
EventHandler.prototype.onContextMenu = function (event) { event.preventDefault(); };

EventHandler.prototype.onKeyDown = function (event) {
    if (this.State.Within) {
        this.report('KeyDown', event.keyCode);
    }
};

EventHandler.prototype.onKeyUp = function (event) {
    if (this.State.Within) {
        this.report('KeyUp', event.keyCode);
    }
};

EventHandler.prototype.onMouseEnter = function (event) {
    this.State.Within = true;
};

EventHandler.prototype.onMouseLeave = function (event) {
    this.State.Left = false;
    this.State.Right = false;
    this.State.Within = false;
};

EventHandler.prototype.onMouseDown = function (event) {
    if (this.State.Within) {
        if (event.button == 2) {
            this.State.Right = true;
            this.report('KeyDown', '501');
        } else {
            this.State.Left = true;
            this.report('KeyDown', '500');
        }
    }
};

EventHandler.prototype.onMouseUp = function (event) {
    if (this.State.Within) {
        if (event.button == 2) {
            this.State.Right = false;
            this.report('KeyUp', '501');
        } else {
            this.State.Left = false;
            this.report('KeyUp', '500');
        }
    }
};

/* EventHandler.prototype.onMouseMove = function (event) {
    if (this.State.Within && (this.State.Left || this.State.Right)) {
        // ...
    }
}; */

EventHandler.prototype.report = function (EventType, Key) {
    $.post('/Signal/' + EventType + '/' + this.InstanceID + '/' + Key + '/' + this.CursorPosition.X + '/' + this.CursorPosition.Y);
};

$.fn.attachEventHandler = function (settings) {
	var Handler = new EventHandler(settings);
	this.on('contextmenu', Handler.onContextMenu.bind(Handler))
//		.on('mousemove', Handler.onMouseMove.bind(Handler))
		.on('mouseup', Handler.onMouseUp.bind(Handler))
		.on('mousedown', Handler.onMouseDown.bind(Handler))
        .on('mouseenter', Handler.onMouseEnter.bind(Handler))
        .on('mouseleave', Handler.onMouseLeave.bind(Handler));
	$(document)
		.on('keyup', Handler.onKeyUp.bind(Handler))
		.on('keydown', Handler.onKeyDown.bind(Handler));
	return this;
};

jQuery(function () {
    $('#PlayButton').on('click', function () {
        $.post('/Signal/prepare', {}, function (Response) {
            // $('<video height="600" width="800" autoplay controls="true" />').append('<source src="' + Response.Endpoint + '" type="video/ogg">').replaceAll($('#PlayButton')).on('loadstart', function () {
            $('<video height="600" width="800" autoplay />').append('<source src="' + Response.Endpoint + '" type="video/ogg">').replaceAll($('#PlayButton')).on('loadstart', function () {
                $(this).get(0).play();
                $.post('/Signal/start/' + Response.InstanceID);
            }).attachEventHandler(Response.InstanceID);
        }, 'json');
    });
});