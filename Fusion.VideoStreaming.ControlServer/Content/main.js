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

EventHandler.prototype.onUnload = function (event) {
    this.report('Stop');
};

EventHandler.prototype.report = function (EventType, Key) {
    if (EventType == 'Stop') {
        $.ajax({
            async: false,
            dataType: 'json',
            method: 'POST',
            success: function (Response) {
                return true;
            },
            url: '/Signal/' + EventType + '/' + this.InstanceID
        });
    } else {
        $.post('/Signal/' + EventType + '/' + this.InstanceID + '/' + Key + '/' + this.CursorPosition.X + '/' + this.CursorPosition.Y, {}, function (Response) {
            if(EventType == 'KeyUp' && Key == 27){
                document.location.reload();
            }
        }, 'json');
    }
};

$.fn.attachEventHandler = function (settings) {
	var Handler = new EventHandler(settings);
	this.on('contextmenu', Handler.onContextMenu.bind(Handler))
		.on('mouseup', Handler.onMouseUp.bind(Handler))
		.on('mousedown', Handler.onMouseDown.bind(Handler))
        .on('mouseenter', Handler.onMouseEnter.bind(Handler))
        .on('mouseleave', Handler.onMouseLeave.bind(Handler));
	$(window)
		.on('keyup', Handler.onKeyUp.bind(Handler))
		.on('keydown', Handler.onKeyDown.bind(Handler))
        .on('unload', Handler.onKeyDown.bind(Handler))
        .on('beforeunload', Handler.onKeyDown.bind(Handler));
	return this;
};

jQuery(function () {
    $('#PlayButton').on('click', function () {
        var Time = new Date();
        $.post('/Signal/prepare', {}, function (Response) {
            $('<video height="600" width="800" autoplay />').append('<source src="' + Response.Endpoint + '" type="video/ogg; codecs = theora, vorbis" />').replaceAll($('#PlayButton')).on('canplay', function () {
                var video = $(this).off('canplay').get(0),
                    cTime = (new Date() - Time) / 1000;
                console.log(cTime);
                video.currentTime = cTime;
                $.post('/Signal/start/' + Response.InstanceID);
            }).attachEventHandler(Response.InstanceID);
        }, 'json');
    });
});